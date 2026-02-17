# Informe de Revisión y Especificación Técnica - ContextWeaver

**Fecha:** 2026-02-15 (Finalizado)
**Proyecto:** ContextWeaver

---

## 1. Resumen Ejecutivo

ContextWeaver es una herramienta de consola (CLI) diseñada para analizar repositorios de código y generar un reporte consolidado en Markdown. Su objetivo principal es empaquetar el contexto de un proyecto para ser consumido por Modelos de Lenguaje (LLMs).

La arquitectura general es sólida, moderna y sigue buenas prácticas de ingeniería de software en .NET. **Tras la Fase 3, el proyecto se ha dividido en 3 capas estrictas: `Core` (Abstracciones), `Engine` (Lógica) y `Cli` (Entrada), siguiendo la Dependency Rule.** Ahora la herramienta ofrece capacidades avanzadas de diagramación (Mermaid y PlantUML) y un análisis de contexto detallado a nivel de módulo y archivo.

---

## 2. Revisión de Código (Code Review) - Estado Actual

### 2.1. Puntos Fuertes (Positivos)

1.  **Arquitectura Modular y Extensible**:
    -   Uso correcto de **Inyección de Dependencias (DI)** y **Patrón Strategy**.
    -   Fácil extensibilidad para nuevos lenguajes y formatos.
2.  **Análisis Robusto con Roslyn**:
    -   Uso de la API de compilación de .NET para extracción precisa de tipos y relaciones.
3.  **Visualización Avanzada**:
    -   Soporte dual para **Mermaid y PlantUML**.
    -   Diagramas granulares: Global, por Módulo y Contexto de Archivo.
    -   Distinción semántica de tipos (`class`, `interface`, `enum`, `record`, `struct`).
4.  **Rendimiento Optimizado**:
    -   Procesamiento paralelo de archivos utilizando `Parallel.ForEachAsync`.

### 2.2. Estado de Hallazgos Previos

#### ✅ 1. Extracción de Dependencias Incompleta (Cross-File Dependencies)
**Estado:** RESUELTO.
**Solución:** Se implementó una lógica de recolección de tipos en `CSharpFileAnalyzer` que permite identificar tipos del proyecto vs. tipos del sistema. El analizador ahora conecta correctamente dependencias entre archivos y filtra ruido del sistema (.NET framework).

#### ✅ 2. Ejecución Secuencial (Performance)
**Estado:** RESUELTO.
**Solución:** `CodeAnalyzerService` ahora utiliza ejecución paralela para el análisis de archivos, mejorando significativamente el tiempo de procesamiento en proyectos grandes.

#### ✅ 3. Cálculo de Inestabilidad Aproximado
**Estado:** RESUELTO.
**Solución:** Se ha refactorizado `InstabilityCalculator` para utilizar las referencias de tipos reales extraídas por Roslyn (`ClassDependencies` y `DefinedTypes`) en lugar de heurísticas basadas en `usings`. Esto garantiza que solo se cuenten dependencias reales entre componentes.

### 2.3. Nuevas Oportunidades de Mejora (Minor)

#### ✅ 1. Centralización de Lógica de Módulos
**Estado:** RESUELTO.
**Solución:** Se implementó la propiedad computada `ModuleName` en `FileAnalysisResult`, eliminando la duplicación de lógica en `InstabilityCalculator` y `MarkdownReportGenerator`.

#### ✅ 2. Refinamiento en Visualización de Records/Structs
**Estado:** RESUELTO.
**Solución:** Se actualizó `MarkdownReportGenerator` para usar estereotipos de PlantUML (`<<record>>`, `<<struct>>`), mejorando la distinción visual de estos tipos.

---

## 3. Especificación Técnica

### 3.1. Visión General del Sistema
ContextWeaver escanea recursivamente un directorio, analiza código (C# vía Roslyn, otros vía texto), calcula métricas y genera un reporte Markdown con diagramas incrustados.

### 3.2. Arquitectura de Componentes

El proyecto se divide en 3 ensamblados (Projectos):

1.  **ContextWeaver.Core**: Abstracciones puras (`IFileAnalyzer`, `IReportGenerator`) y Modelos (`FileAnalysisResult`).
2.  **ContextWeaver.Engine**: Implementación de la lógica (`CodeAnalyzerService`, `CSharpFileAnalyzer`, `MarkdownReportGenerator`).
3.  **ContextWeaver.Cli**: Configuración del Host (`Program.cs`) e Inyección de Dependencias.

#### Diagrama de Clases (Simplificado por Capas)

```mermaid
classDiagram
    namespace Core {
        class IFileAnalyzer { <<interface>> }
        class FileAnalysisResult { <<record>> }
    }
    namespace Engine {
        class CodeAnalyzerService
        class CSharpFileAnalyzer
    }
    namespace Cli {
        class Program
    }

    Program ..> CodeAnalyzerService : DI Resolution
    CodeAnalyzerService --> IFileAnalyzer : Injects
    CSharpFileAnalyzer ..|> IFileAnalyzer : Implements
    CodeAnalyzerService ..> FileAnalysisResult : Uses
```

### 3.3. Nuevas Capacidades de Diagramación

El generador de reportes ha sido enriquecido con las siguientes capacidades:

1.  **Soporte Multi-Formato**: Genera bloques para `mermaid` y `plantuml` simultáneamente.
2.  **Diagramas de Módulo**: Agrupa clases por carpetas de primer nivel (Arquitectura).
3.  **Diagramas de Contexto**: Al inicio de cada archivo, muestra un mini-diagrama con sus dependencias directas (Entrantes y Salientes).
4.  **Semántica de Tipos**:
    -   Detecta y renderiza correctamente `interface` vs `class` en PlantUML.
    -   Usa iconos/colores específicos (e.g., `#Pink` para el archivo actual).

### 3.4. Definición de Datos (Core)

#### `FileAnalysisResult`
DTO extendido para soportar las nuevas funcionalidades:
-   `RelativePath`: Ruta relativa.
-   `LinesOfCode`: Conteo de líneas.
-   `CodeContent`: Código fuente.
-   `DefinedTypes`: Lista de tipos declarados en el archivo.
-   `DefinedTypeKinds`: Diccionario mapeando `NombreTipo -> Kind` (class, interface, enum, etc.).
-   `ClassDependencies`: Lista de relaciones salientes ("Origen -> Destino").
-   `IncomingDependencies`: Lista de relaciones entrantes (calculado post-análisis).
-   `Metrics`: Diccionario flexible.

### 3.5. Requisitos del Entorno
-   **Runtime**: .NET 8.0 o superior.
-   **Dependencias**: `System.CommandLine`, `Microsoft.Extensions.Hosting`, `Microsoft.CodeAnalysis.CSharp`.

---

## 4. Conclusión

ContextWeaver ha evolucionado de una herramienta de concatenación simple a un generador de documentación técnica avanzado. La corrección de la extracción de dependencias y la adición de diagramas detallados (especialmente con soporte PlantUML y distinción de interfaces) lo convierten en una herramienta potente para entender bases de código legacy o complejas rápidamente.
### 4. Análisis Avanzado para LLMs
**Estado:** RESOLVED
**Impacto:** Alto (Mejora la comprensión semántica por parte de la IA).

**Solución Implementada:**
1.  **Enriquecimiento Semántico:**
    -   Se extraen **modificadores** (`sealed`, `abstract`), **interfaces** implementadas y **atributos** clave.
    -   Se renderizan en el `Repo Map` justo debajo de la definición de la clase, proporcionando contexto inmediato sobre el "rol" del código.
2.  **Profundidad de Anidamiento (`MaxNestingDepth`):**
    -   Se calcula la profundidad máxima de bloques lógicos (`if`, `for`, `try`, etc.).
    -   Esta métrica ayuda a identificar complejidad cognitiva que podría dificultar el razonamiento de un LLM.
3.  **Referencias Entrantes Textuales ("Used By"):**
    -   Se lista explícitamente qué otros archivos dependen del archivo actual.
    -   Proporciona contexto instantáneo sobre el impacto de los cambios.

-   `CodeAnalyzerService` muestra múltiples referencias en "Used By".

### 4. Evolución de Usabilidad (Fases 4-7)
**Estado:** COMPLETADO
**Impacto:** Crítico (Adopción de usuario y experiencia de desarrollador).

**Mejoras Implementadas:**
1.  **Wizard Interactivo Mejorado:**
    -   Se implementó un flujo guiado robusto con `Spectre.Console`.
    -   Soporte para selección recursiva de archivos y filtrado por extensión.
    -   **Selección Masiva:** Opción inicial para "Seleccionar Todo/Nada" o usar defaults.
2.  **Granularidad de Diagramas:**
    -   Refactorización de `DependencyGraphSection` y `ModuleDiagramSection` en componentes granulares.
    -   Ahora es posible generar reportes con **solo Mermaid** o **solo PlantUML**, reduciendo ruido.
3.  **Persistencia de Configuración:**
    -   El sistema recuerda la última selección de secciones del usuario en `.contextweaver.json`.
    -   Soporte para perfiles de proyecto (`.contextweaver.json` en raíz del repo analizado).

---

## 5. Conclusión

ContextWeaver ha evolucionado de una herramienta de concatenación simple a un generador de documentación técnica avanzado y altamente usable. La arquitectura basada en plugins (`IReportSection`) demostró su valor al permitir la adición de nuevas capacidades de diagramación y granularidad sin tocar el núcleo. La herramienta está lista para producción.
