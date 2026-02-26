# ContextWeaver Specification (SPEC)

## 1. Visión General

**ContextWeaver** es una herramienta de línea de comandos (CLI) diseñada para ingenieros de software y arquitectos. Su propósito principal es transformar cualquier repositorio de código en un **único documento Markdown enriquecido**, optimizado para el consumo por Large Language Models (LLMs) y la colaboración técnica.

Más allá de la herramienta en sí, este proyecto sirve como un **Laboratorio de Ingeniería de Software**, demostrando cómo construir sistemas robustos basándose en **fundamentos, valores y principios**, más allá de seguir ciegamente patrones o reglas rígidas.

## 2. Objetivos del Diseño

1.  **Optimización para Contexto**: Maximizar la señal y reducir el ruido para el procesamiento por IA.
2.  **Excelencia Técnica**: Aplicar rigurosamente los fundamentos de la ingeniería (cohesión, acoplamiento, abstracción).
3.  **Cero Fricción**: Automatizar la consistencia y el estilo para liberar la carga cognitiva del desarrollador.
4.  **Extensibilidad**: Facilitar la evolución del sistema a través de una arquitectura abierta.
5.  **Transparencia**: El sistema explica su estructura a través de su propio diseño.

## 3. Arquitectura del Sistema

El sistema se basa en una **Arquitectura Centrada en el Dominio** (Domain-Centric Architecture).

Independientemente de si la llamas "Clean", "Hexagonal" o "Puertos y Adaptadores", la idea central es la misma: **Proteger el núcleo de la aplicación de los detalles externos**.

### 3.1 La Regla de Oro (Dependency Rule)
El principio fundamental que gobierna este diseño es la dirección de las dependencias:
**Las dependencias siempre apuntan hacia adentro, hacia las políticas de alto nivel.**

`Cli (Detalle) → Engine (Mecanismo) → Core (Dominio)`

*   **ContextWeaver.Core (El Corazón)**:
    *   Aquí residen los conceptos fundamentales y las reglas del negocio (`Modelos`, `Abstracciones`).
    *   No sabe nada del mundo exterior (ni bases de datos, ni CLI, ni sistema de archivos).
    *   Es la parte más estable y reutilizable del sistema.
*   **ContextWeaver.Engine (La Lógica)**:
    *   Implementa los casos de uso y coordina las operaciones.
    *   Usa las abstracciones definidas en el Core para realizar el trabajo "sucio" (analizar archivos, generar reportes).
    *   Es el adaptador principal entre la intención del usuario y los recursos del sistema.
*   **ContextWeaver.Cli (La Entrega)**:
    *   Es solo un mecanismo de entrega. Podría ser una API Web, una GUI o una CLI.
    *   Su única responsabilidad es recibir la entrada del usuario, configurar el sistema (Inyección de Dependencias) y presentar la salida.

### 3.2 Por qué este enfoque?
Al desacoplar el "qué hace" (Core) del "cómo se usa" (Cli) y "cómo funciona" (Engine), logramos:
*   **Testabilidad**: Podemos probar el núcleo sin necesidad de un sistema de archivos real o interacción de usuario.
*   **Mantenibilidad**: Cambios en la CLI no rompen las reglas de negocio.
*   **Evolución**: Si mañana queremos una interfaz web, el Core y Engine no cambian.

### 3.3 Estrategia de Pruebas

Siguiendo la misma filosofía de separación:

| Nivel | Propósito | Enfoque |
| :--- | :--- | :--- |
| **Pruebas de Unidad (Core/Engine)** | Verificar la corrección lógica de componentes aislados. | Rápidas, deterministas, sin efectos secundarios. Usan "dobles de prueba" (test doubles) cuando es necesario. |
| **Pruebas de Integración (E2E)** | Verificar que el sistema ensamblado funciona como un todo. | Realistas, usan el sistema de archivos (en entornos controlados/temporales) para asegurar que la "pegamin" entre componentes es fuerte. |

## 4. Estándares de Ingeniería y Calidad (QA)

La calidad no es un acto, es un hábito automatizado. Hemos implementado una estrategia de **"Defensa en Profundidad"**:

### 4.1 Arquitectura como Código (Architecture Tests)
No confiamos solo en la disciplina humana para mantener la arquitectura. Utilizamos `NetArchTest` para **imponer** las reglas de diseño en cada build:

*   **Reglas de Dependencia**: `Core` nunca puede depender de `Engine` o `Cli`. `Engine` nunca puede depender de `Cli`.
*   **Reglas de Diseño**: Las interfaces deben empezar con `I`. Los servicios deben ser `sealed`.
*   **Reglas de Encapsulamiento**: Los modelos de dominio no pueden tener campos públicos.

Si violas estas reglas, el build falla.

### 4.2 Automatización con Husky.NET (Git Hooks)
Para garantizar que "lo correcto" sea inevitable, utilizamos **Husky** con una política de **Tolesancia Cero**:

*   **Pre-commit (Estricto)**:
    1.  **Build Check**: Ejecuta `dotnet build --warnaserror`. Si tu código tiene errores o *warnings* (variables no usadas, etc.), el commit se bloquea.
    2.  **Auto-Format**: Ejecuta `dotnet format`. Si hay problemas de estilo arreglables (espacios), los corrige y los stagea.

### 4.3 Configuración de Editor Contextual
Reconocemos que el código de producción y el código de prueba tienen necesidades diferentes:

*   **Producción (.editorconfig raíz)**: Prioriza la uniformidad y la documentación pública.
*   **Pruebas (tests/.editorconfig)**: Prioriza la expresividad. Permitimos nombres de métodos con guiones bajos (`Debe_HacerX_Cuando_Y`) porque en los tests, el nombre del método es la documentación del escenario.

### 4.3 Disciplina de "Cero Advertencias"
Tratamos las advertencias del compilador como errores (`TreatWarningsAsErrors`). Una advertencia ignorada hoy es un bug mañana. Esto mantiene la ventana rota cerrada desde el primer día.

## 5. Especificaciones Funcionales (Resumen)

### 5.1 CLI y Wizard Interactivo
El Wizard interactivo se rige bajo un patrón de diseño **State Orchestrator Pipeline**. Este patrón se adoptó para favorecer el *Single Responsibility Principle (SRP)* en la terminal.
- **Navegación Histórica**: Otorga al usuario la posibilidad de rectificar decisiones mediante la opción `🔙 [Volver al paso anterior]` o tecleando `<` en *Text Prompts*. El orquestador mantiene una pila (*Stack*) de navegación.
- **Modo Interactivo (Default)**: Guía paso a paso dividida en módulos discretos para la selección de archivos (árbol recursivo), formatos y secciones.
    - **Selección de Archivos**: Permite elegir entre "Todos" o "Selección Manual".
    - **Selección de Secciones**: Permite "Seleccionar Todo", "Nada" o usar preferencias guardadas.
    - **Persistencia**: Recuerda las preferencias del usuario (`.contextweaver.json`) para agilizar ejecuciones futuras.
- **Modo Desatendido (Scriptable)**:
    - Flags robustos: `--all`, `--sections`, `--exclude-sections`.
    - Matching difuso ("fuzzy match") para nombres de secciones (e.g., "mermaid" selecciona los gráficos pertinentes).

### 5.2 Análisis Semántico (C#)
- No leemos el código como texto plano; lo entendemos como estructura (AST).
- Extraemos la **intención** (firmas, atributos, herencia) y métricas objetivas (complejidad, anidamiento) para dar una radiografía real del código.

### 5.3 Análisis Arquitectónico y Diagramación
- **Granularidad de Diagramas**: Generación independiente de gráficos **Mermaid** y **PlantUML**.
    - El usuario puede elegir generar solo uno de los dos formatos para reducir el ruido en el reporte.
- **Métricas Fundamentales**: Cálculo de la **Inestabilidad** ($I = Ce / (Ca + Ce)$) para detectar zonas de cambio.
- **Visualización Jerárquica**:
    - **Grafo de Dependencias**: Vista de pájaro de todo el sistema.
    - **Diagramas de Módulo**: Enfoque en subsistemas específicos (carpetas de primer nivel).
    - **Contexto de Archivo**: Vista microscópica de las dependencias directas de cada archivo.

## 6. Documentación y Comunidad

Este repositorio aspira a ser un ciudadano modelo del ecosistema Open Source, proporcionando:

*   **CONTRIBUTING.md**: Mapa de ruta para colaboradores.
*   **CODE_OF_CONDUCT.md**: Nuestros valores comunitarios.
*   **SECURITY.md**: Política de responsabilidad.
*   **CHANGELOG.md**: Respeto por la historia del proyecto.

---
*Este documento evoluciona junto con nuestro entendimiento del problema y la solución.*
