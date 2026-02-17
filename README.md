# ContextWeaver

[![CI](https://github.com/jmanuelsoberano/ContextWeaver/actions/workflows/ci.yml/badge.svg)](https://github.com/jmanuelsoberano/ContextWeaver/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)

## La Herramienta CLI para el Context Engineering y Análisis Arquitectónico

`ContextWeaver` es una potente herramienta de línea de comandos (.NET Global Tool) diseñada para ingenieros de software,
arquitectos y desarrolladores. Transforma cualquier codebase en un **documento Markdown único, coherente y enriquecido
**, optimizado para el análisis por parte de Large Language Models (LLMs) y la colaboración en equipos.

### ¿Por qué ContextWeaver?

En la era del desarrollo asistido por IA, la calidad del contexto de entrada lo es todo. `ContextWeaver` aborda este
desafío al:

1. **Consolidar el Código**: Combina múltiples archivos de un repositorio en un único documento Markdown, haciendo que
   el consumo por parte de LLMs sea más eficiente y completo.
2. **Optimizar el Contexto**: Filtra directorios y archivos irrelevantes (ej. `node_modules`, `bin`, `obj`), enfocándose
   en el código fuente y los artefactos clave.
3. **Proporcionar un Mapa de Código Inteligente**:
    - **Árbol de Directorios Navegable**: Una representación visual de la estructura del proyecto con enlaces directos a
      cada archivo dentro del documento.
    - **"Repo Map" por Archivo**: Extrae las firmas públicas (API) y las dependencias (`using`/`import`) de cada archivo
      de código, ofreciendo un resumen de alto nivel.
    - **Métricas Clave**: Incluye el conteo de Líneas de Código (LOC) y Complejidad Ciclomática a nivel de archivo.
4. **Visualización Arquitectónica Avanzada**:
    - **Soporte Dual**: Genera diagramas tanto en **Mermaid** como en **PlantUML**.
    - **Granularidad Inteligente**: Crea diagramas de dependencias a nivel global, por módulo y diagramas de contexto específicos por archivo.
    - **Semántica Rica**: Distingue visualmente entre `class`, `interface`, `record`, `struct` y `enum`.
5. **Contexto Semántico Enriquecido para LLMs**:
    - **Taxonomía Automática**: Extrae modificadores (`abstract`, `sealed`), interfaces implementadas y atributos clave para dar contexto inmediato sobre el rol del código.
    - **Detección de Complejidad Cognitiva**: Calcula la métrica `MaxNestingDepth` para alertar sobre lógica profundamente anidada.
    - **Referencias Entrantes ("Used By")**: Lista explícitamente qué archivos dependen del código actual, facilitando el análisis de impacto.
6. **Identificar "Hotspots"**: Destaca automáticamente los 5 archivos con mayor tamaño (LOC) y mayor acoplamiento (
   número de imports), permitiendo enfocar la atención en áreas críticas.
7. **Análisis de Inestabilidad Arquitectónica (Métrica de Robert C. Martin)**: Calcula la métrica de Inestabilidad (I =
   Ce / (Ca + Ce)) a nivel de módulos (carpetas/proyectos) para ayudar a entender la dirección y la salud de las
   dependencias arquitectónicas. Identifica módulos estables (núcleo) e inestables (implementaciones).
8. **Configuración Flexible y por Proyecto**: La herramienta busca un archivo `.contextweaver.json` en el directorio
   analizado para usar configuraciones específicas del proyecto. Si no lo encuentra, utiliza la configuración global por
   defecto, permitiendo una gran adaptabilidad.

### Calidad de Código y Arquitectura

El proyecto se adhiere a estándares estrictos de ingeniería de software:
*   **Clean Architecture**: Separación estricta de responsabilidades (Core, Engine, Cli).
*   **Zero Warnings**: El código compila sin advertencias ni errores (tratados como errores).
*   **100% Test Pass Rate**: Todos los componentes críticos están cubiertos por pruebas unitarias y de integración.
*   **Static Analysis**: Uso intensivo de Roslyn Analyzers y StyleCop para garantizar consistencia.

### Casos de Uso:

- **Context Engineering para IA**: Genera un contexto rico y estructurado para tareas complejas como refactorización
  estratégica, análisis de seguridad preliminar o generación de documentación.
- **Onboarding Acelerado**: Facilita a los nuevos miembros del equipo la comprensión rápida de un codebase complejo.
- **Revisiones de Código y Arquitectura**: Proporciona una visión macro del proyecto para revisiones más informadas y
  basadas en datos.
- **Transferencia de Conocimiento**: Crea artefactos permanentes del estado de un proyecto en un punto específico en el
  tiempo.

### Instalación y Uso:

`ContextWeaver` se distribuye como una .NET Global Tool a través de NuGet.

```bash
# Instalación desde NuGet (próximamente)
dotnet tool install --global ContextWeaver
```

#### Desarrollo Local

Si deseas probar la herramienta sin instalarla globalmente o contribuir al desarrollo:

1.  **Clonar el repositorio**:
    ```bash
    git clone https://github.com/jmanuelsoberano/ContextWeaver.git
    cd ContextWeaver
    ```
2.  **Compilar y Ejecutar**:
    ```bash
    # Ejecutar directamente desde el código fuente
    dotnet run --project src/ContextWeaver.Cli/ContextWeaver.Cli.csproj -- --help
    
    # O para analizar el propio proyecto
    dotnet run --project src/ContextWeaver.Cli/ContextWeaver.Cli.csproj -- -d . -o reporte.md
    ```

#### Uso Básico (Recomendado)

La forma más sencilla de usar la herramienta es navegar hasta el directorio raíz de tu proyecto y ejecutar el comando.
`ContextWeaver` analizará el directorio actual.

```bash
# 1. Navega a tu proyecto
cd C:\ruta\a\tu\repositorio

# 2. Ejecuta el comando (generará un analysis_report.md)
contextweaver
```

#### Uso Explícito

También puedes especificar todas las opciones manualmente desde cualquier ubicación.

```bash
contextweaver --directorio "C:\ruta\a\tu\repositorio" --output "reporte_personalizado.md" --format "markdown"
```

#### Configuración por Proyecto (Opcional)

Para anular la configuración global, crea un archivo `.contextweaver.json` en la raíz del proyecto que deseas analizar.

**Ejemplo de `.contextweaver.json`:**

```json
{
  "AnalysisSettings": {
    "IncludedExtensions": [
      ".cs",
      ".csproj",
      ".yml"
    ],
    "ExcludePatterns": [
      "bin",
      "obj",
      "node_modules",
      "docs"
    ]
  }
}
```

### Arquitectura

El proyecto sigue **Ports & Adapters** con **Dependency Rule** unidireccional:

```
ContextWeaver.Cli → ContextWeaver.Engine → ContextWeaver.Core
```

| Proyecto | Responsabilidad |
|:---|:---|
| **ContextWeaver.Core** | Modelos + Abstracciones (cero dependencias externas). |
| **ContextWeaver.Engine** | Lógica de negocio puro: Analyzers, Reporters, Services, Utilities. |
| **ContextWeaver.Cli** | Punto de entrada, configuración de DI y parseo de argumentos. |

### 🤝 Contribuir

¡Las contribuciones son bienvenidas! Por favor lee las siguientes guías antes de empezar:

- [Guía de Contribución](CONTRIBUTING.md): Cómo configurar el entorno y enviar PRs.
- [Código de Conducta](CODE_OF_CONDUCT.md): Nuestras normas de comunidad.
- [Política de Seguridad](SECURITY.md): Cómo reportar vulnerabilidades.

### 📜 Historial de Cambios

Consulta el [CHANGELOG](CHANGELOG.md) para ver la historia de cambios notable.

### 📄 Licencia

Este proyecto está bajo la Licencia [MIT](LICENSE).
