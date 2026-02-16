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

La calidad no es un acto, es un hábito automatizado.

### 4.1 Automatización con Husky.NET y Git Hooks
Para garantizar que "lo correcto" sea también "lo fácil", utilizamos **Husky**:

*   **Antes de confirmar (Pre-commit)**: El sistema se auto-corrige. Formateadores automáticos (`dotnet format`) arreglan inconsistencias de estilo (espacios, llaves) en los archivos modificados. Esto evita discusiones triviales en las revisiones de código.

### 4.2 Configuración de Editor Contextual
Reconocemos que el código de producción y el código de prueba tienen necesidades diferentes:

*   **Producción (.editorconfig raíz)**: Prioriza la uniformidad y la documentación pública.
*   **Pruebas (tests/.editorconfig)**: Prioriza la expresividad. Permitimos nombres de métodos con guiones bajos (`Debe_HacerX_Cuando_Y`) porque en los tests, el nombre del método es la documentación del escenario.

### 4.3 Disciplina de "Cero Advertencias"
Tratamos las advertencias del compilador como errores (`TreatWarningsAsErrors`). Una advertencia ignorada hoy es un bug mañana. Esto mantiene la ventana rota cerrada desde el primer día.

## 5. Especificaciones Funcionales (Resumen)

### 5.1 CLI
*   **Argumentos**: `-d` (directorio), `-o` (salida), `-f` (formato).
*   **Smart Defaults**: Si no se especifica nada, asume que quieres analizar "aquí y ahora".

### 5.2 Análisis Semántico (C#)
*   No leemos el código como texto plano; lo entendemos como estructura (AST).
*   Extraemos la **intención** (firmas, atributos, herencia) y métricas objetivas (complejidad, anidamiento) para dar una radiografía real del código.

### 5.3 Análisis Arquitectónico
*   Calculamos métricas fundamentales como la **Inestabilidad** ($I = Ce / (Ca + Ce)$).
*   Esto nos permite visualizar objetivamente qué partes del sistema son el "núcleo estable" y cuáles son las "zonas de cambio", ayudando a tomar decisiones de refactorización basadas en datos.

## 6. Documentación y Comunidad

Este repositorio aspira a ser un ciudadano modelo del ecosistema Open Source, proporcionando:

*   **CONTRIBUTING.md**: Mapa de ruta para colaboradores.
*   **CODE_OF_CONDUCT.md**: Nuestros valores comunitarios.
*   **SECURITY.md**: Política de responsabilidad.
*   **CHANGELOG.md**: Respeto por la historia del proyecto.

---
*Este documento evoluciona junto con nuestro entendimiento del problema y la solución.*
