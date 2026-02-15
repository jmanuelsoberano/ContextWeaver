using System.Text;
using ContextWeaver.Core;
using ContextWeaver.Interfaces;
using ContextWeaver.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace ContextWeaver.Analyzers;

/// <summary>
///     PATRÓN DE DISEÑO: Concrete Strategy (Estrategia Concreta).
///     Esta clase es una implementación específica de IFileAnalyzer para archivos C#.
///     PRINCIPIO DE DISEÑO: Principio de Responsabilidad Única (SRP) de SOLID.
///     La única razón para cambiar esta clase es si cambia la forma en que se analizan los archivos C#.
///     Toda la lógica relacionada con Roslyn y C# está encapsulada aquí (ALTA COHESIÓN).
/// </summary>
public class CSharpFileAnalyzer : IFileAnalyzer
{
    private readonly ILogger<CSharpFileAnalyzer> _logger;
    private CSharpCompilation? _globalCompilation;
    private Dictionary<string, SyntaxTree> _syntaxTrees = new();

    public bool CanAnalyze(FileInfo file)
    {
        return file.Extension.Equals(".cs", StringComparison.OrdinalIgnoreCase);
    }

    public async Task InitializeAsync(IEnumerable<FileInfo> files)
    {
        var csFiles = files.Where(CanAnalyze).ToList();
        var trees = new List<SyntaxTree>();

        foreach (var file in csFiles)
        {
            var content = await File.ReadAllTextAsync(file.FullName);
            var tree = CSharpSyntaxTree.ParseText(content, path: file.FullName); // Path es importante para mapear luego
            trees.Add(tree);
            _syntaxTrees[file.FullName] = tree;
        }

        _globalCompilation = CSharpCompilation.Create("ContextWeaverAnalysis")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(trees);

        // Extraer todos los tipos definidos en el proyecto para el filtro
        // ✅ MEJORA: Solo extraer tipos de los árboles de sintaxis (código fuente), no de las referencias ensambladas.
        foreach (var tree in trees)
        {
            var root = tree.GetRoot();
            foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                _allProjectTypes.Add(typeDecl.Identifier.Text);
            }
        }
    }
    
    // THREAD-SAFETY: _allProjectTypes se llena exclusivamente en InitializeAsync() (secuencial)
    // y se lee en AnalyzeAsync() (paralelo). Esto es seguro porque CodeAnalyzerService.AnalyzeAndGenerateReport()
    // garantiza que InitializeAsync() se completa ANTES de que Parallel.ForEachAsync invoque AnalyzeAsync().
    // Si este orden cambia, convertir a ConcurrentDictionary<string, byte> o similar.
    private readonly HashSet<string> _allProjectTypes = new();

    public CSharpFileAnalyzer(ILogger<CSharpFileAnalyzer> logger)
    {
        _logger = logger;
    }

    public async Task<FileAnalysisResult> AnalyzeAsync(FileInfo file)
    {
        try
        {
            if (!_syntaxTrees.TryGetValue(file.FullName, out var tree))
            {
                var content = await File.ReadAllTextAsync(file.FullName);
                tree = CSharpSyntaxTree.ParseText(content);
            }

            var root = tree.GetRoot();
            var contentText = root.ToFullString();

        // Usar SemanticModel global si está disponible, sino crear uno local.
        SemanticModel semanticModel;
        if (_globalCompilation != null)
        {
            semanticModel = _globalCompilation.GetSemanticModel(tree);
        }
        else
        {
            var compilation = CSharpCompilation.Create("ContextWeaverAnalysis")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);
            semanticModel = compilation.GetSemanticModel(tree);
        }
            
        var complexity = CSharpMetricsCalculator.CalculateCyclomaticComplexity(root);
        var maxNestingDepth = CSharpMetricsCalculator.CalculateMaxNestingDepth(root);
        var publicApiSignatures = ExtractPublicApiSignatures(root);
        var usings = ExtractUsingStatements(root); 
        // Extraer las dependencias de clase usando el modelo semántico (global o local)
        var classDependencies = ExtractClassDependencies(root, semanticModel);

        // Extraer tipos definidos con su semántica enriquecida
        var definedTypes = new List<string>();
        var definedTypeKinds = new Dictionary<string, string>();
        var definedTypeSemantics = new Dictionary<string, TypeSemantics>();

        foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            var name = typeDecl.Identifier.Text;
            definedTypes.Add(name);
            definedTypeKinds[name] = typeDecl.Keyword.Text;
            
            // Semántica: Modificadores
            var modifiers = typeDecl.Modifiers.Select(m => m.Text).ToList();
            
            // Semántica: Interfaces
            var interfaces = new List<string>();
            if (typeDecl.BaseList != null)
            {
                foreach (var unused in typeDecl.BaseList.Types)
                {
                    // Nota: Aquí solo tomamos el texto crudo. Para mayor precisión se podría usar SemanticModel.
                    // Pero para "Repo Map" visual es suficiente y más rápido así.
                    interfaces.Add(unused.ToString());
                }
            }

            // Semántica: Atributos
            var attributes = new List<string>();
            foreach (var attrList in typeDecl.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    attributes.Add($"[{attr.Name}]");
                }
            }

            definedTypeSemantics[name] = new TypeSemantics(modifiers, interfaces, attributes);
        }

        // También incluir Enums (con semántica vacía o básica)
        foreach (var enumDecl in root.DescendantNodes().OfType<EnumDeclarationSyntax>())
        {
            var name = enumDecl.Identifier.Text;
            definedTypes.Add(name);
            definedTypeKinds[name] = "enum";
            // Enums solo suelen tener modificadores (public, internal) y atributos.
            var modifiers = enumDecl.Modifiers.Select(m => m.Text).ToList();
             var attributes = new List<string>();
            foreach (var attrList in enumDecl.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    attributes.Add($"[{attr.Name}]");
                }
            }
            definedTypeSemantics[name] = new TypeSemantics(modifiers, new List<string>(), attributes);
        }

        return new FileAnalysisResult
        {
            LinesOfCode = contentText.Split('\n').Length,
            CodeContent = contentText,
            Language = "csharp",
            Usings = usings,
            ClassDependencies = classDependencies,
            DefinedTypes = definedTypes,
            DefinedTypeKinds = definedTypeKinds,
            DefinedTypeSemantics = definedTypeSemantics,
            Metrics =
            {
                { "CyclomaticComplexity", complexity },
                { "MaxNestingDepth", maxNestingDepth },
                { "PublicApiSignatures", publicApiSignatures }
            }
        };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al analizar {File}. Se retorna resultado parcial.", file.FullName);
            var fallbackContent = await File.ReadAllTextAsync(file.FullName);
            return new FileAnalysisResult
            {
                LinesOfCode = fallbackContent.Split('\n').Length,
                CodeContent = fallbackContent,
                Language = "csharp"
            };
        }
    }

    /// <summary>
    ///     Extrae las firmas de los miembros públicos (clases, métodos, propiedades) del árbol de sintaxis.
    /// </summary>
    private List<string> ExtractPublicApiSignatures(SyntaxNode root)
    {
        var signatures = new List<string>();

        // Visita las declaraciones de clases, structs, interfaces y records
        foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            if (typeDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                var typeSignature = new StringBuilder();
                typeSignature.Append($"{typeDeclaration.Keyword.Text} {typeDeclaration.Identifier.Text}");
                if (typeDeclaration.TypeParameterList != null) typeSignature.Append(typeDeclaration.TypeParameterList);
                if (typeDeclaration.BaseList != null)
                    typeSignature.Append(
                        $" : {string.Join(", ", typeDeclaration.BaseList.Types.Select(t => t.ToString()))}");
                signatures.Add($"- {typeSignature.ToString().Trim()}");

                // Visita los miembros dentro de esta clase/struct/etc.
                foreach (var member in typeDeclaration.Members)
                    if (member.Modifiers.Any(SyntaxKind.PublicKeyword))
                    {
                        var memberSignature = new StringBuilder();

                        if (member is MethodDeclarationSyntax method)
                        {
                            memberSignature.Append(
                                $"{method.ReturnType} {method.Identifier.Text}{method.TypeParameterList}{method.ParameterList}");
                            signatures.Add($"  - {memberSignature.ToString().Trim()}");
                        }
                        else if (member is PropertyDeclarationSyntax property)
                        {
                            memberSignature.Append($"{property.Type} {property.Identifier.Text}");
                            if (property.AccessorList != null)
                                memberSignature.Append(
                                    $" {property.AccessorList.ToString().Replace("\n", "").Replace("\r", "").Replace(" ", "")}"); // Simplificar accesores
                            signatures.Add($"  - {memberSignature.ToString().Trim()}");
                        }
                        else if (member is ConstructorDeclarationSyntax constructor)
                        {
                            memberSignature.Append($"{constructor.Identifier.Text}{constructor.ParameterList}");
                            signatures.Add($"  - {memberSignature.ToString().Trim()}");
                        }
                        // Puedes añadir más tipos de miembros si es necesario (ej. eventos, campos)
                    }
            }

        return signatures;
    }

    /// <summary>
    ///     Extrae las sentencias 'using' del árbol de sintaxis.
    /// </summary>
    private List<string> ExtractUsingStatements(SyntaxNode root)
    {
        return root.DescendantNodes().OfType<UsingDirectiveSyntax>()
            .Select(u => u.Name.ToString())
            .OrderBy(u => u)
            .ToList();
    }


    /// <summary>
    ///     ✅ VERSIÓN CORREGIDA: Extrae dependencias limpias y con sintaxis correcta para 'graph TD'.
    ///     1. Usa '-->' para uso y '-.->' para herencia.
    ///     2. Filtra dependencias a tipos del sistema de .NET (ej. List, Exception).
    ///     3. Evita la creación de enlaces vacíos.
    /// </summary>
    private List<string> ExtractClassDependencies(SyntaxNode root, SemanticModel semanticModel)
    {
        var dependencies = new HashSet<string>();
        var declaredTypeSymbols = root.DescendantNodes()
            .OfType<BaseTypeDeclarationSyntax>()
            .Select(typeSyntax => semanticModel.GetDeclaredSymbol(typeSyntax))
            .Where(symbol => symbol != null)
            .ToList();

        if (!declaredTypeSymbols.Any()) return new List<string>();

        // Crear una lista de los nombres de nuestros propios tipos para poder filtrar.
        var projectTypeNames = new HashSet<string>(declaredTypeSymbols.Select(s => s.Name));

        foreach (var sourceTypeSymbol in declaredTypeSymbols)
        {
            var sourceTypeName = sourceTypeSymbol.Name;

            // --- Análisis de HERENCIA / IMPLEMENTACIÓN ---
            var baseTypes = sourceTypeSymbol.Interfaces.Concat(new[] { sourceTypeSymbol.BaseType });
            foreach (var baseTypeSymbol in baseTypes)
            {
                if (baseTypeSymbol == null || baseTypeSymbol.SpecialType == SpecialType.System_Object) continue;

                var targetTypeName = baseTypeSymbol.Name;
                // ✅ FIX: Solo añadir si el destino es relevante (no es del sistema y no está vacío).
                var targetNs = baseTypeSymbol.ContainingNamespace?.ToDisplayString() ?? "";
                if (!string.IsNullOrWhiteSpace(targetTypeName) && !targetNs.StartsWith("System"))
                    // ✅ FIX: Usar sintaxis de línea punteada para herencia en 'graph TD'
                    dependencies.Add($"{sourceTypeName} -.-> {targetTypeName}");
            }

            var typeDeclarationSyntax = sourceTypeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            if (typeDeclarationSyntax == null) continue;

            // --- Análisis de USO / COMPOSICIÓN ---
            foreach (var typeNode in typeDeclarationSyntax.DescendantNodes().OfType<TypeSyntax>())
            {
                var symbolInfo = semanticModel.GetSymbolInfo(typeNode);
                if (symbolInfo.Symbol is not ITypeSymbol targetTypeSymbol) continue;

                var targetTypeName = targetTypeSymbol.Name;
                var targetNs = targetTypeSymbol.ContainingNamespace?.ToDisplayString() ?? "";

                // ✅ FIX: El filtro principal. Solo nos interesan las dependencias a otros tipos del proyecto.
                // También se excluyen tipos del sistema y se asegura que el nombre no esté vacío.
                // Usamos _allProjectTypes para validar si el destino pertenece al proyecto.
                if (!string.IsNullOrWhiteSpace(targetTypeName) &&
                    _allProjectTypes.Contains(targetTypeName) &&
                    targetTypeName != sourceTypeName)
                    dependencies.Add($"{sourceTypeName} --> {targetTypeName}");
            }
        }

        return dependencies.ToList();
    }
}