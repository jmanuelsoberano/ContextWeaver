using System.Text;
using ContextWeaver.Core;
using ContextWeaver.Interfaces;
using ContextWeaver.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Agregado para construir cadenas

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
        GetAllProjectTypesFromCompilation(_globalCompilation);
    }
    
    private HashSet<string> _allProjectTypes = new();

    private void GetAllProjectTypesFromCompilation(CSharpCompilation compilation)
    {
        // Helper simple para recorrer el namespace global y extraer tipos
        var stack = new Stack<INamespaceSymbol>();
        stack.Push(compilation.GlobalNamespace);

        while (stack.Count > 0)
        {
            var ns = stack.Pop();
            foreach (var member in ns.GetMembers())
            {
                if (member is INamespaceSymbol childNs)
                {
                    stack.Push(childNs);
                }
                else if (member is ITypeSymbol typeSymbol)
                {
                    _allProjectTypes.Add(typeSymbol.Name);
                }
            }
        }
    }

    public async Task<FileAnalysisResult> AnalyzeAsync(FileInfo file)
    {
        if (!_syntaxTrees.TryGetValue(file.FullName, out var tree))
        {
            // Fallback si por alguna razón no se inicializó (ej. archivo nuevo creado durante ejecución, aunque raro en CLI)
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
            
        var complexity = CSharpMetricsCalculator.CalculateCyclomaticComplexity(contentText);
        var publicApiSignatures = ExtractPublicApiSignatures(root);
        var usings = ExtractUsingStatements(root); 
        // Extraer las dependencias de clase usando el modelo semántico (global o local)
        var classDependencies = ExtractClassDependencies(root, semanticModel);

        return new FileAnalysisResult
        {
            LinesOfCode = contentText.Split('\n').Length,
            CodeContent = contentText,
            Language = "csharp",
            Usings = usings,
            ClassDependencies = classDependencies,
            Metrics =
            {
                { "CyclomaticComplexity", complexity },
                { "PublicApiSignatures", publicApiSignatures }
            }
        };
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