# Changelog

Todos los cambios notables en este proyecto serán documentados en este archivo.

El formato se basa en [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/),
y este proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

## [Unreleased]

### Añadido
- Configuración de **Husky.NET** para hooks de Git (`pre-commit`).
- Documentación de contribución (`CONTRIBUTING.md`) en español.
- Archivos estándar de comunidad: `CODE_OF_CONDUCT.md`, `SECURITY.md`.
- Nuevo proyecto `ContextWeaver.Tests.Shared` para utilidades de prueba compartidas.

### Cambiado
- Refactorización masiva de pruebas: Separación de `ContextWeaver.Tests` en `Core.Tests`, `Engine.Tests` y `E2E.Tests`.
- Actualización de `FullPipelineTests` para usar directorios temporales aislados.
- Consolidación de reglas de `.editorconfig`.

### Eliminado
- Proyecto monolítico `ContextWeaver.Tests`.
