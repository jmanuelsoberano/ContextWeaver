---
trigger: always_on
---

# ContextWeaver — Formato de entrega (siempre)

Al responder con cambios de código, SIEMPRE entrega obligatoriamente este formato:

1) Plan (resumen breve)
2) Archivos a modificar (lista de rutas)
3) Implementación (código en diffs o snippets)
4) Checklist (pass/fail + razón si falla)

En `.NET`, si la implementación incluye agregar nuevas clases que no existían previamente en un proyecto, recuerda mencionar brevemente si requerirán ser dadas de alta en la inyección de dependencias `ServiceCollection` que ocurre en `ContextWeaver.Cli/Program.cs` o sus extensions.
