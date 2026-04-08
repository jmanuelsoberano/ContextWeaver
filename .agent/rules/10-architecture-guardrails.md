---
trigger: always_on
---

# ContextWeaver — Arquitectura por capas (siempre)

Respeta el patrón de Arquitectura Centrada en el Dominio (Ports & Adapters) estricta:

- `src/ContextWeaver.Core/**`: Dominio. Modelos, Reglas, Puertos (Interfaces). Cero dependencias externas.
- `src/ContextWeaver.Engine/**`: Mecanismos y lógica de aplicación. Analyzers, Formatters y Reporters. Depende de `Core`.
- `src/ContextWeaver.Cli/**`: Mecanismo de entrega. Control de la terminal interactiva (Wizard). Depende de `Engine` y `Core`.

Reglas de dependencias y aislamiento estructural:
- `ContextWeaver.Core` DEBE ignorar por completo la existencia de `Engine` o de `Cli`. Nunca puede usar lógicas de CLI.
- `ContextWeaver.Engine` puede referenciar e instrumentar objetos de `Core`. No puede importar `Cli`.
- `ContextWeaver.Cli` configura las inyecciones de dependencias instanciando clases de base e interfaces de Core y Engine.

Si detectas un caso que viola esta dirección (`Cli -> Engine -> Core`), estás obligado a priorizar refactorizaciones para corregirlo, ya que el proyecto impone esta regla por `NetArchTest`.
