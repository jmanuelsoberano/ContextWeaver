## Descripción

Breve descripción de los cambios.
Link al issue relacionado: Fixes # (issue)

## Tipo de cambio

- [ ] Bug fix (fix non-breaking que arregla un problema)
- [ ] Nueva funcionalidad (feature non-breaking que agrega funcionalidad)
- [ ] Breaking change (fix o feature que causaría que funcionalidad existente no funcione como se espera)
- [ ] Documentación (actualización o creación de docs)
- [ ] Refactor / Chore (mantenimiento de código sin cambios funcionales)
- [ ] CI/CD (cambios en pipelines o workflows)

## Checklist

- [ ] Mi código sigue la arquitectura del proyecto (`Cli → Engine → Core`)
- [ ] He agregado tests que cubren mis cambios (Unitarios o E2E)
- [ ] `dotnet format --verify-no-changes` pasa sin errores
- [ ] `dotnet build --no-incremental` compila sin warnings
- [ ] `dotnet test` pasa con todos los tests (100% pass)
- [ ] He actualizado la documentación (README, SPEC) si es necesario
- [ ] Mi código no genera nuevas advertencias de análisis estático (StyleCop/Roslyn)
