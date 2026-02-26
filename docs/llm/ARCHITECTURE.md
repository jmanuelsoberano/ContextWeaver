# ARCHITECTURE — Mapa del proyecto ContextWeaver

## Estructura (resumen)
- `src/ContextWeaver.Core/`     Modelos de dominio y abstracciones (conceptos fundamentales). No sabe nada del exterior.
- `src/ContextWeaver.Engine/`   Lógica de negocio, casos de uso, Analyzers (Roslyn), Reporters. Coordina operaciones base.
- `src/ContextWeaver.Cli/`      Mecanismo de entrega (CLI interactiva/desatendida). Punto de entrada y configuración de Inyección de Dependencias.

## Reglas de dependencia (imports) - CLEAN ARCHITECTURE ESTRICTO
- `ContextWeaver.Core` NO PUEDE importar NADA de `Engine` o `Cli`. Cero dependencias externas (solo BCL local).
- `ContextWeaver.Engine` importa de `Core` para implementar abstracciones y orquestar lógica. Nunca depende de `Cli`.
- `ContextWeaver.Cli` importa de `Engine` y `Core` para instanciar el sistema e interactuar con el usuario.
La dirección siempre es: `Cli (Detalle) → Engine (Mecanismo) → Core (Dominio)`

## Herramientas de control arquitectónico
- Aplicamos `NetArchTest` para hacer pruebas automatizadas de arquitectura. Si rompes las reglas en dependencias, el build del CI fallará.
- `Husky` pre-commits y la flag `TreatWarningsAsErrors` garantizan "Defensa en Profundidad": cero advertencias de compilador.

## Orquestación y estado del Wizard CLI
- `ContextWeaver.Cli` implementa un patrón "State Orchestrator Pipeline" para el Wizard.
- Separación estricta (SRP) en la terminal: navegación interactiva fluida (stack) y manejo de "Volver al paso anterior" (`Back`) consistente para evitar flujos erráticos e imprevistos en prompts anidados.
