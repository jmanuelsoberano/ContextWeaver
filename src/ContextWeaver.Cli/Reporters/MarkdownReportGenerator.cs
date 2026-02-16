using System.Text;
using ContextWeaver.Core;
using ContextWeaver.Interfaces;
using ContextWeaver.Utilities;

namespace ContextWeaver.Reporters;

/// <summary>
///     PATRÓN DE DISEÑO: Concrete Strategy (Estrategia Concreta).
///     Implementación de IReportGenerator que sabe cómo construir un reporte en formato Markdown.
///     PRINCIPIO DE DISEÑO: ALTA COHESIÓN y SRP.
///     Toda la lógica de formato de Markdown reside exclusivamente en esta clase.
/// </summary>
public class MarkdownReportGenerator : IReportGenerator
{
    public string Format => "markdown";

    public string Generate(DirectoryInfo directory, List<FileAnalysisResult> results,
        Dictionary<string, (int Ca, int Ce, double Instability)> instabilityMetrics)
    {
        var reportBuilder = new StringBuilder();
        var sortedResults = results.OrderBy(r => r.RelativePath).ToList();
        // H09: Calcular una sola vez y pasar como parámetro a todos los métodos que lo necesiten.
        var typeKindMap = BuildTypeKindMap(sortedResults);

        reportBuilder.Append(GenerateHeader(directory));
        reportBuilder.Append(GenerateHotspots(sortedResults));
        reportBuilder.Append(GenerateInstabilityReport(instabilityMetrics));
        reportBuilder.Append(GenerateDependencyGraph(sortedResults, typeKindMap));
        reportBuilder.Append(GenerateModuleDiagrams(sortedResults, typeKindMap));
        reportBuilder.Append(GenerateDirectoryTree(sortedResults, directory.Name));
        reportBuilder.Append(GenerateFileContent(sortedResults, typeKindMap));

        return reportBuilder.ToString();
    }

    private string GenerateHeader(DirectoryInfo directory)
    {
        return $"""
                Este archivo es una representación consolidada del código fuente de '{directory.Name}', fusionado en un único documento por ContextWeaver.
                El contenido ha sido procesado para crear un contexto completo para su análisis.

                # Resumen del Archivo

                ## Propósito
                Este archivo contiene una representación empaquetada de los contenidos del repositorio.
                Está diseñado para ser fácilmente consumible por sistemas de IA para análisis, revisión de código u 
                otros procesos automatizados.

                ## Formato del Archivo
                El contenido se organiza de la siguiente manera:
                1. Esta sección de resumen.
                2. Una sección de "Análisis de Hotspots" que identifica archivos clave por métricas.
                3. Una sección de "Análisis de Inestabilidad" que proporciona información arquitectónica.
                4. Un árbol de la estructura de directorios con enlaces clicables a cada archivo.
                5. Múltiples entradas de archivo, cada una de las cuales consta de:
                   - Un encabezado con la ruta del archivo (## Archivo: ruta/al/archivo)
                   - El resumen del "Repo Map" (API pública e importaciones).
                   - El contenido completo del archivo en un bloque de código.

                ## Pautas de Uso
                - Este archivo debe ser tratado como de solo lectura. Cualquier cambio debe realizarse en 
                  los archivos originales del repositorio, no en esta versión empaquetada.
                - Al procesar este archivo, use la ruta del archivo para distinguir entre los diferentes 
                  archivos del repositorio.
                - Tenga en cuenta que este archivo puede contener información sensible. Manéjelo con el mismo 
                  nivel de seguridad que manejaría el repositorio original.

                ## Notas
                - Algunos archivos pueden haber sido excluidos según la configuración de ContextWeaver en `.contextweaver.json`.
                - Los archivos binarios no se incluyen en esta representación empaquetada.
                - Los archivos se ordenan alfabéticamente por su ruta completa para una ordenación consistente.

                """;
    }

    /// <summary>
    ///     Genera la sección de Hotspots, mostrando los top 5 archivos por LOC y por número de imports.
    /// </summary>
    private string GenerateHotspots(List<FileAnalysisResult> results)
    {
        var hotspotsBuilder = new StringBuilder();
        hotspotsBuilder.AppendLine("# 🔥 Análisis de Hotspots");
        hotspotsBuilder.AppendLine();

        // --- Top 5 por Líneas de Código (LOC) ---
        hotspotsBuilder.AppendLine("## 5 Principales Archivos por Líneas de Código (LOC)");
        var topByLoc = results.OrderByDescending(r => r.LinesOfCode).Take(5);
        foreach (var result in topByLoc)
        {
            var headerText = $"File: {result.RelativePath}";
            var anchor = MarkdownHelper.CreateAnchor(headerText);
            hotspotsBuilder.AppendLine($"* **({result.LinesOfCode} LOC)** - [`{result.RelativePath}`](#{anchor})");
        }

        hotspotsBuilder.AppendLine();

        // --- Top 5 por Número de Imports ---
        hotspotsBuilder.AppendLine("## 5 Principales Archivos por Número de Importaciones");
        var topByImports = results
            .Select(r => new
            {
                Result = r,
                ImportCount = r.Usings.Count // <-- Acceso directo a la propiedad Usings.Count
            })
            .Where(x => x.ImportCount > 0)
            .OrderByDescending(x => x.ImportCount)
            .Take(5);

        foreach (var item in topByImports)
        {
            var headerText = $"File: {item.Result.RelativePath}";
            var anchor = MarkdownHelper.CreateAnchor(headerText);
            hotspotsBuilder.AppendLine(
                $"* **({item.ImportCount} Imports)** - [`{item.Result.RelativePath}`](#{anchor})");
        }

        hotspotsBuilder.AppendLine();

        return hotspotsBuilder.ToString();
    }

    private string GenerateInstabilityReport(
        Dictionary<string, (int Ca, int Ce, double Instability)> instabilityMetrics)
    {
        var reportBuilder = new StringBuilder();
        reportBuilder.AppendLine("# 📊 Análisis de Inestabilidad");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine(
            "Esta sección estima la métrica de Inestabilidad (I) para cada módulo de nivel superior (carpeta/proyecto) basándose en sus dependencias (importaciones).");
        reportBuilder.AppendLine("`I = Ce / (Ca + Ce)`");
        reportBuilder.AppendLine("- `Ce` (Eferente): Cuántos otros módulos usa este módulo (apunta hacia afuera).");
        reportBuilder.AppendLine(
            "- `Ca` (Aferente): Cuántos otros módulos dependen de este módulo (apunta hacia adentro).");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("## Resumen de Inestabilidad del Módulo:");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("| Módulo | Ca (Eferente) | Ce (Aferente) | Inestabilidad (I) | Descripción |");
        reportBuilder.AppendLine("|---|---|---|---|---|");

        foreach (var entry in instabilityMetrics.OrderBy(e => e.Key))
        {
            var module = entry.Key;
            var (ca, ce, instability) = entry.Value;
            var description = GetInstabilityDescription(instability);
            reportBuilder.AppendLine($"| `{module}` | {ca} | {ce} | {instability:F2} | {description} |");
        }

        reportBuilder.AppendLine();

        reportBuilder.AppendLine("## Guía de Interpretación:");
        reportBuilder.AppendLine(
            "- `I ≈ 0`: Muy estable (muchos dependen de él; depende poco de otros). A menudo son contratos/interfaces principales.");
        reportBuilder.AppendLine(
            "- `I ≈ 1`: Muy inestable (depende de muchos; pocos o ninguno dependen de él). A menudo son implementaciones concretas como UI/adaptadores.");
        reportBuilder.AppendLine("- `I ≈ 0.5`: Estabilidad intermedia.");
        reportBuilder.AppendLine(
            "Idealmente, los módulos estables deben ser abstractos y los inestables concretos. Evite módulos abstractos muy inestables o módulos concretos muy estables.");
        reportBuilder.AppendLine();

        return reportBuilder.ToString();
    }

    /// <summary>
    ///     ✅ VERSIÓN CORREGIDA: Genera un gráfico más limpio y con sintaxis correcta.
    /// </summary>
    private string GenerateDependencyGraph(List<FileAnalysisResult> results, Dictionary<string, string> typeKindMap)
    {
        var allDependencies = new HashSet<string>();
        var modules = new Dictionary<string, HashSet<string>>();
        var interfaces = new HashSet<string>();

        foreach (var result in results)
        {
            // ✅ FIX: Usar la propiedad centralizada ModuleName
            var moduleName = result.ModuleName;

            if (!modules.ContainsKey(moduleName)) modules[moduleName] = new HashSet<string>();

            if (result.ClassDependencies != null)
                foreach (var dependency in result.ClassDependencies)
                {
                    var relation = DependencyRelation.Parse(dependency);
                    if (relation == null) continue;

                    allDependencies.Add(dependency);
                    modules[moduleName].Add(relation.Source);

                    // H13: Usar metadata real de Roslyn en vez de heurística de nombre.
                    if (typeKindMap.TryGetValue(relation.Target, out var targetKind) && targetKind == "interface")
                        interfaces.Add(relation.Target);
                }
        }

        if (allDependencies.Count == 0) return string.Empty;

        var graphBuilder = new StringBuilder();
        // ... (el resto del método para construir el string de mermaid se mantiene igual) ...
        graphBuilder.AppendLine("# 📈 Gráfico de Dependencias de Clases");
        graphBuilder.AppendLine();
        graphBuilder.AppendLine(
            "Este gráfico visualiza las relaciones jerárquicas (línea punteada) y de colaboración (línea sólida) entre las clases del proyecto. Renderizado con Mermaid.js.");
        graphBuilder.AppendLine();
        graphBuilder.AppendLine("```mermaid");
        graphBuilder.AppendLine("graph TD;");
        graphBuilder.AppendLine();

        foreach (var module in modules.OrderBy(m => m.Key))
            if (module.Value.Any())
            {
                graphBuilder.AppendLine($"  subgraph {module.Key}");
                foreach (var className in module.Value.OrderBy(n => n)) graphBuilder.AppendLine($"    {className}");
                graphBuilder.AppendLine("  end");
                graphBuilder.AppendLine();
            }

        foreach (var dependency in allDependencies.OrderBy(d => d)) graphBuilder.AppendLine($"  {dependency}");
        graphBuilder.AppendLine();

        if (interfaces.Any())
        {
            graphBuilder.AppendLine("  %% Estilos");
            graphBuilder.AppendLine("  classDef interface fill:#ccf,stroke:#333,stroke-width:2px");
            graphBuilder.AppendLine($"  class {string.Join(",", interfaces)} interface");
        }

        graphBuilder.AppendLine("```");
        graphBuilder.AppendLine();

        // PlantUML Version
        graphBuilder.AppendLine("### Alternativa: PlantUML");
        graphBuilder.AppendLine("```plantuml");
        graphBuilder.AppendLine("@startuml");
        // ✅ FIX: Usar 'class'/'interface'/'enum' según corresponda.
        // Ocultar miembros vacíos para que se vean como cajas simples si no hay detalles.
        graphBuilder.AppendLine("hide empty members");
        
        // typeKindMap ya fue calculado en Generate() y pasado como parámetro.
        graphBuilder.AppendLine();

        foreach (var module in modules.OrderBy(m => m.Key))
            if (module.Value.Any())
            {
                graphBuilder.AppendLine($"package \"{module.Key}\" {{");
                foreach (var className in module.Value.OrderBy(n => n)) 
                {
                     var (keyword, stereotype) = GetPlantUMLMeta(className, typeKindMap);
                     graphBuilder.AppendLine($"  {keyword} {className} {stereotype}");
                }
                graphBuilder.AppendLine("}");
                graphBuilder.AppendLine();
            }

        foreach (var dependency in allDependencies.OrderBy(d => d))
        {
            // Convertir sintaxis Mermaid a PlantUML
            var plantUmlDep = dependency.Replace("-.->", "..>").Replace("-->", "-->");
            graphBuilder.AppendLine($"  {plantUmlDep}");
        }
        graphBuilder.AppendLine("@enduml");
        graphBuilder.AppendLine("```");
        graphBuilder.AppendLine();

        return graphBuilder.ToString();
    }

    /// <summary>
    ///     Genera diagramas separados para cada módulo (carpeta de primer nivel).
    /// </summary>
    private string GenerateModuleDiagrams(List<FileAnalysisResult> results, Dictionary<string, string> typeKindMap)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# 🧩 Diagramas de Módulo");
        sb.AppendLine();
        sb.AppendLine("A continuación se presentan diagramas de dependencia detallados por cada módulo para facilitar la visualización.");
        sb.AppendLine();

        // Agrupar por módulo usando la propiedad centralizada
        var modules = results
            .GroupBy(r => r.ModuleName)
            .OrderBy(g => g.Key);

        foreach (var moduleGroup in modules)
        {
            var moduleName = moduleGroup.Key;
            var moduleFiles = moduleGroup.ToList();
            var moduleDependencies = new HashSet<string>();
            var relatedClasses = new HashSet<string>();

            foreach (var file in moduleFiles)
            {
                if (file.ClassDependencies != null)
                {
                    foreach (var dep in file.ClassDependencies)
                    {
                        var relation = DependencyRelation.Parse(dep);
                        if (relation == null) continue;

                        moduleDependencies.Add(dep);
                        relatedClasses.Add(relation.Source);
                        relatedClasses.Add(relation.Target);
                    }
                }
            }

            if (moduleDependencies.Any())
            {
                sb.AppendLine($"## Módulo: {moduleName}");
                sb.AppendLine();
                
                // Mermaid
                sb.AppendLine("### Mermaid");
                sb.AppendLine("```mermaid");
                sb.AppendLine("graph TD;");
                foreach (var dep in moduleDependencies.OrderBy(d => d))
                {
                    sb.AppendLine($"  {dep}");
                }
                sb.AppendLine("```");
                sb.AppendLine();

                // PlantUML
                sb.AppendLine("### PlantUML");
                sb.AppendLine("```plantuml");
                sb.AppendLine($"@startuml {moduleName}");
                sb.AppendLine("hide empty members");
                
                // typeKindMap ya fue calculado en Generate() y pasado como parámetro.

                foreach (var cls in relatedClasses.OrderBy(c => c))
                {
                    var (keyword, stereotype) = GetPlantUMLMeta(cls, typeKindMap);
                    sb.AppendLine($"{keyword} {cls} {stereotype}");
                }
                sb.AppendLine();

                foreach (var dep in moduleDependencies.OrderBy(d => d))
                {
                    var plantUmlDep = dep.Replace("-.->", "..>").Replace("-->", "-->");
                    sb.AppendLine($"  {plantUmlDep}");
                }
                sb.AppendLine("@enduml");
                sb.AppendLine("```");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Genera un pequeño diagrama de contexto para un archivo específico.
    /// </summary>
    /// <summary>
    ///     Genera un pequeño diagrama de contexto para un archivo específico.
    /// </summary>
    private string GenerateFileContextDiagram(FileAnalysisResult result, Dictionary<string, string> typeKindMap)
    {
        // Recopilar conexiones
        var connections = new HashSet<string>();
        
        // Salientes (Outgoing)
        if (result.ClassDependencies != null)
        {
            foreach (var dep in result.ClassDependencies)
            {
                connections.Add(dep);
            }
        }

        // Entrantes (Incoming)
        if (result.IncomingDependencies != null && result.DefinedTypes != null)
        {
            foreach (var incoming in result.IncomingDependencies)
            {
                var myMainType = result.DefinedTypes.FirstOrDefault() ?? Path.GetFileNameWithoutExtension(result.RelativePath);
                var rel = $"{incoming} --> {myMainType}";
                connections.Add(rel);
            }
        }

        if (connections.Count == 0) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("#### Contexto");
        
        // Mermaid
        sb.AppendLine("##### Mermaid");
        sb.AppendLine("```mermaid");
        sb.AppendLine("graph LR;"); // Left-Right para flujos de contexto suele ser mejor
        foreach (var conn in connections.OrderBy(c => c))
        {
            sb.AppendLine($"  {conn}");
        }
        
        // Resaltar el nodo central (tipos de este archivo)
        if (result.DefinedTypes != null)
        {
            foreach(var type in result.DefinedTypes)
            {
                sb.AppendLine($"  style {type} fill:#f9f,stroke:#333,stroke-width:2px");
            }
        }
        sb.AppendLine("```");

        // PlantUML
        sb.AppendLine("##### PlantUML");
        sb.AppendLine("```plantuml");
        sb.AppendLine("@startuml");
        sb.AppendLine("left to right direction");
        sb.AppendLine("hide empty members");

        // Declarar clases participantes
        var participants = new HashSet<string>();
        foreach (var conn in connections)
        {
            var relation = DependencyRelation.Parse(conn);
            if (relation == null) continue;
            participants.Add(relation.Source);
            participants.Add(relation.Target);
        }

        foreach (var p in participants.OrderBy(x => x))
        {
                var (keyword, stereotype) = GetPlantUMLMeta(p, typeKindMap);
                if (result.DefinedTypes != null && result.DefinedTypes.Contains(p))
                    sb.AppendLine($"{keyword} {p} {stereotype} #Pink"); // Resaltar propia
                else
                    sb.AppendLine($"{keyword} {p} {stereotype}");
        }

        foreach (var conn in connections.OrderBy(c => c))
        {
            var pUml = conn.Replace("-->", "-->").Replace("-.->", "..>");
            sb.AppendLine($"  {pUml}");
        }
        sb.AppendLine("@enduml");
        sb.AppendLine("```");

        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    ///     Proporciona una descripción textual de la inestabilidad.
    /// </summary>
    private string GetInstabilityDescription(double instability)
    {
        if (instability <= 0.2) return "Muy estable / Core";
        if (instability >= 0.8) return "Muy inestable / Concreto";
        return "Estabilidad intermedia";
    }

    /// <summary>
    ///     Genera el contenido de todos los archivos con sus respectivas métricas y mapa de repositorio.
    /// </summary>
    private string GenerateFileContent(List<FileAnalysisResult> results, Dictionary<string, string> typeKindMap)
    {
        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine("# Archivos");
        contentBuilder.AppendLine();

        foreach (var result in results)
        {
            contentBuilder.AppendLine($"## File: {result.RelativePath}");
            contentBuilder.AppendLine();

            // ✅ NUEVO: Diagrama de Contexto
            contentBuilder.Append(GenerateFileContextDiagram(result, typeKindMap));

            // ✅ NUEVO (3): Referencias Entrantes Textuales ("Used By")
            if (result.IncomingDependencies != null && result.IncomingDependencies.Any())
            {
                // Agrupamos por archivo origen para no repetir
                var usedByFiles = result.IncomingDependencies
                    .Select(d => DependencyRelation.Parse(d))
                    .Where(r => r != null)
                    .Select(r => r!.Source)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                if (usedByFiles.Any())
                {
                    contentBuilder.AppendLine($"**Used By:** {string.Join(", ", usedByFiles)}");
                    contentBuilder.AppendLine();
                }
            }

            // --- NUEVA SECCIÓN DE REPO MAP ---
            if (result.Metrics.PublicApiSignatures.Any())
            {
                var publicApi = result.Metrics.PublicApiSignatures;
                contentBuilder.AppendLine("### Repo Map: Extraer solo firmas públicas y imports de cada archivo");
                contentBuilder.AppendLine("#### API Publica:");
                foreach (var signature in publicApi) 
                {
                    contentBuilder.AppendLine(signature);
                    
                    // ✅ NUEVO (1): Enriquecimiento Semántico debajo de la definición de clase/tipo
                    // La firma suele ser "- class MyClass : Base". Extraemos el nombre para buscar en el diccionario.
                    var lineTrimmed = signature.TrimStart('-', ' ');
                    var firstWord = lineTrimmed.Split(' ').FirstOrDefault();
                    // Heurística simple: si empieza por class/interface/struct/record/enum, el siguiente es el nombre.
                    if (IsTypeKeyword(firstWord)) 
                    {
                        var parts = lineTrimmed.Split(new[]{' ', ':'}, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            var typeName = parts[1];
                            // Renderizar semántica si existe
                            if (result.DefinedTypeSemantics != null && result.DefinedTypeSemantics.TryGetValue(typeName, out var semantics))
                            {
                                if (semantics.Modifiers.Any()) 
                                    contentBuilder.AppendLine($"    - Modifiers: {string.Join(", ", semantics.Modifiers)}");
                                if (semantics.Attributes.Any()) 
                                    contentBuilder.AppendLine($"    - Attributes: {string.Join(", ", semantics.Attributes)}");
                                if (semantics.Interfaces.Any()) 
                                    contentBuilder.AppendLine($"    - Implements: {string.Join(", ", semantics.Interfaces)}");
                            }
                        }
                    }
                }
                contentBuilder.AppendLine(); // Línea en blanco para separación
            }

            if (result.Usings.Any())
            {
                contentBuilder.AppendLine("#### Imports:");
                foreach (var singleUsing in result.Usings) contentBuilder.AppendLine($"- {singleUsing}");
                contentBuilder.AppendLine(); // Línea en blanco para separación
            }
            // --- FIN NUEVA SECCIÓN ---

            // Información de métricas existente
            contentBuilder.AppendLine("#### Métricas");
            contentBuilder.AppendLine($"* **Lineas de Código (LOC):** {result.LinesOfCode}");
            if (result.Metrics.CyclomaticComplexity.HasValue)
                contentBuilder.AppendLine($"* **CyclomaticComplexity:** {result.Metrics.CyclomaticComplexity.Value}");
            if (result.Metrics.MaxNestingDepth.HasValue)
                contentBuilder.AppendLine($"* **MaxNestingDepth:** {result.Metrics.MaxNestingDepth.Value}");
            contentBuilder.AppendLine();

            // Sección de Código Fuente
            contentBuilder.AppendLine("#### Source Code");
            // Nota: Aquí se muestra el código completo, podrías añadir la lógica para "Fuente: líneas 1-X" si es necesario
            // Por ahora, el "CodeContent" ya tiene todo el código y las "LinesOfCode" te dan el rango.
            contentBuilder.AppendLine("```" + result.Language);
            contentBuilder.AppendLine(result.CodeContent.Trim());
            contentBuilder.AppendLine("```");
            contentBuilder.AppendLine();
        }

        return contentBuilder.ToString();
    }

    #region Directory Tree Generation

    private class TreeNode
    {
        public string Name { get; set; }
        public string? Path { get; set; }
        public SortedDictionary<string, TreeNode> Children { get; } = new();
    }

    /// <summary>
    ///     Genera la sección de estructura de directorios con un formato de árbol avanzado.
    /// </summary>
    private string GenerateDirectoryTree(List<FileAnalysisResult> results, string rootName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Directory Structure");
        sb.AppendLine();

        var root = BuildTree(results);

        // Nodo raíz
        sb.AppendLine($"- {rootName}/");
        AppendDirectoryStructureWithLinks(root.Children.Values, sb, 1);

        sb.AppendLine();
        return sb.ToString();
    }

    private TreeNode BuildTree(List<FileAnalysisResult> results)
    {
        var root = new TreeNode { Name = "" };
        foreach (var result in results)
        {
            var currentNode = root;
            var pathParts = result.RelativePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < pathParts.Length; i++)
            {
                var part = pathParts[i];
                if (!currentNode.Children.ContainsKey(part)) currentNode.Children[part] = new TreeNode { Name = part };
                currentNode = currentNode.Children[part];
                if (i == pathParts.Length - 1) currentNode.Path = result.RelativePath;
            }
        }

        return root;
    }

    private void AppendDirectoryStructureWithLinks(IEnumerable<TreeNode> nodes, StringBuilder sb, int level)
    {
        var indent = new string(' ', level * 4);

        // Directorios primero, luego archivos; ambos ordenados por nombre
        var directories = nodes.Where(n => n.Path == null).OrderBy(n => n.Name);
        var files = nodes.Where(n => n.Path != null).OrderBy(n => n.Name);

        foreach (var dir in directories)
        {
            sb.AppendLine($"{indent}- {dir.Name}/");
            AppendDirectoryStructureWithLinks(dir.Children.Values, sb, level + 1);
        }

        foreach (var file in files)
        {
            var headerText = $"File: {file.Path}";
            var anchor = MarkdownHelper.CreateAnchor(headerText);
            sb.AppendLine($"{indent}- [{file.Name}](#{anchor})");
        }
    }

    private Dictionary<string, string> BuildTypeKindMap(List<FileAnalysisResult> results)
    {
        var map = new Dictionary<string, string>();
        foreach (var result in results)
        {
            if (result.DefinedTypeKinds != null)
            {
                foreach (var kvp in result.DefinedTypeKinds)
                {
                    map[kvp.Key] = kvp.Value;
                }
            }
        }
        return map;
    }

    private (string Keyword, string Stereotype) GetPlantUMLMeta(string typeName, Dictionary<string, string> typeKindMap)
    {
        if (typeKindMap.TryGetValue(typeName, out var kind))
        {
            return kind switch
            {
                "interface" => ("interface", ""),
                "enum" => ("enum", ""),
                "record" => ("class", "<<record>>"),
                "struct" => ("class", "<<struct>>"),
                _ => ("class", "")
            };
        }
        // Fallback: heuristic detection
        if (typeName.StartsWith("I") && typeName.Length > 1 && char.IsUpper(typeName[1])) return ("interface", "");
        return ("class", "");
    }

    private bool IsTypeKeyword(string keyword)
    {
        return keyword == "class" || keyword == "interface" || keyword == "struct" || keyword == "record" || keyword == "enum";
    }

    #endregion
}