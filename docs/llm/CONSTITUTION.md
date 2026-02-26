# CONSTITUTION — Reglas obligatorias de ContextWeaver

## Prioridad
Si hay conflicto: CONSTITUTION > ARCHITECTURE > `.cursorrules` > resto del repo.

## MUST (obligatorio)
1) Respetar arquitectura centrada en dominio (Ports & Adapters):
   - `Core`: DOMAIN PURO. Totalmente aislado (Ni System.IO invasivo, ni CLI, dependencias mínimas BCL).
   - `Engine`: Detalles técnicos (parsing de AST, llamadas al sistema de archivos mediante puertos/interfaces, generación Markdown/Mermaid/PlantUML).
   - `Cli`: Capa pasiva de presentación en consola. Solo entrada/salida ("dumb layer") y orquestación del Pipeline Wizard/DI.

2) Calidad técnica inflexible (Zero Warnings):
   - Tolerancia cero a advertencias: Todo código debe compilar con cero advertencias (`--warnaserror`).
   - Todos los servicios deben ser `sealed` a menos que estén expresamente diseñados para herencia.
   - Las interfaces obligatoriamente deben empezar con la letra `I`.
   - Nomenclatura C# estricta: PascalCase para clases/métodos, camelCase para locales.

3) Calidad y Pruebas:
   - Respetar la nomenclatura híbrida en pruebas: Para producción se usa PascalCase formal. En proyectos `.Tests` los nombres de pruebas pueden utilizar guiones bajos (ej. `Debe_HacerX_Cuando_Y`) como se indica en `.editorconfig` de `tests`.
   - Todo código nuevo debe justificar explícitamente si requiere inyección de dependencias para facilitar sus pruebas (dobles de prueba).

4) Manejo de estado de la CLI (State Orchestrator Pipeline):
   - Si se edita funcionalidad del Wizard (`WizardCommand`, `WizardOrchestrator`), se debe preservar o mejorar la integridad de la navegación del Stack (Push/Pop/Peek) de la terminal sin acoplamiento accidental entre pasos.

## SHOULD (preferible)
- Favorecer la inmutabilidad y colecciones de solo lectura en constructos del `Core`.
- Usar `record` para estado y objetos de transferencia en lugar de clases ricas mutables cuando aplique.
- Utilizar características recientes de C# (C# 12) como *Collection Expressions* o *Target-typed new* cuando aumenten la legibilidad.

## MAY (permitido)
- Extracciones moderadas (Refactors locales) si reducen el tamaño general y aplican Single Responsibility Principle.

## Formato de respuesta del agente (obligatorio)
1) Plan
2) Archivos a modificar (lista)
3) Implementación (código)
4) Checklist (pass/fail + razón si falla)
