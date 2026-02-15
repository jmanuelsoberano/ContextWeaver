using ContextWeaver.Core;

namespace ContextWeaver.Utilities;

public class InstabilityCalculator
{
    public Dictionary<string, (int Ca, int Ce, double Instability)> Calculate(List<FileAnalysisResult> results)
    {
        var typeToModuleMap = new Dictionary<string, string>();
        var moduleNames = new HashSet<string>();

        // 1. Construir mapa: Tipo -> Módulo
        foreach (var result in results)
        {
            var moduleName = result.ModuleName;
            moduleNames.Add(moduleName);

            if (result.DefinedTypes != null)
            {
                foreach (var type in result.DefinedTypes)
                {
                    // Asumimos unicidad de nombre de tipo por simplicidad en este contexto,
                    // o "último gana" si hay duplicados (partial classes en mismo módulo ok).
                    typeToModuleMap[type] = moduleName;
                }
            }
        }

        var moduleEfferentDependencies = new Dictionary<string, HashSet<string>>();
        
        // Inicializar
        foreach (var module in moduleNames)
        {
            moduleEfferentDependencies[module] = new HashSet<string>();
        }

        // 2. Analizar dependencias de CLASE (Source -> Target)
        foreach (var result in results)
        {
            var sourceModule = result.ModuleName;

            if (result.ClassDependencies != null)
            {
                foreach (var dep in result.ClassDependencies)
                {
                    var relation = DependencyRelation.Parse(dep);
                    if (relation == null) continue;

                    if (typeToModuleMap.TryGetValue(relation.Target, out var targetModule))
                    {
                        if (!string.Equals(sourceModule, targetModule, StringComparison.OrdinalIgnoreCase))
                        {
                            moduleEfferentDependencies[sourceModule].Add(targetModule);
                        }
                    }
                }
            }
        }

        var moduleMetrics = moduleNames.ToDictionary(m => m, m => (Ca: 0, Ce: 0));

        // 3. Calcular Ce (Eferentes) y Ca (Aferentes)
        foreach (var (module, dependencies) in moduleEfferentDependencies)
        {
            // Ce = Número de módulos de los que dependo
            var ce = dependencies.Count;
            var currentMetrics = moduleMetrics[module];
            moduleMetrics[module] = (currentMetrics.Ca, ce);

            // Ca = Número de módulos que dependen de mí
            foreach (var dependentModule in dependencies)
            {
                if (moduleMetrics.ContainsKey(dependentModule))
                {
                    var depMetrics = moduleMetrics[dependentModule];
                    moduleMetrics[dependentModule] = (depMetrics.Ca + 1, depMetrics.Ce);
                }
            }
        }

        // 4. Calcular Inestabilidad (I)
        return moduleMetrics.ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                var (ca, ce) = kvp.Value;
                // I = Ce / (Ca + Ce)
                // Range: [0, 1]. 0 = Muy Estable (Abstracto), 1 = Muy Inestable (Concreto)
                var instability = ca + ce == 0 ? 0.0 : (double)ce / (ca + ce);
                return (ca, ce, instability);
            });
    }
}