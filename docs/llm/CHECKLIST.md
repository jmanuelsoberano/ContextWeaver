# CHECKLIST — Validación antes de entregar (ContextWeaver)

- [ ] Respeté la arquitectura de capas (`ContextWeaver.Core` sin importar `Engine` o `Cli`).
- [ ] Compilé mental o localmente comprobando que no se generaría ningún warning del compilador (`TreatWarningsAsErrors`).
- [ ] Apliqué el modificador `sealed` en todos los nuevos servicios y clases base no diseñadas para derivación.
- [ ] Mantuve el encapsulamiento: no creé `public set` o campos públicos en modelos de dominio que no deben modificarse desde el exterior.
- [ ] Si escribí código nuevo del Wizard, me aseguré de que el State Orchestrator no dependa indebidamente de otros pasos ni rompa la navegación con el botón "Atrás/Back".
- [ ] Respeté el `tests/.editorconfig` para métodos de pruebas unitarias (`Metodo_Escenario_Resultado`).
- [ ] Cumplí con las convenciones de `StyleCop` y agregué comentarios XML a todos mis elementos públicos siguiendo el formato estricto estipulado en `.cursorrules`.
- [ ] Cambios mínimos, concentrados al caso de uso provisto.
- [ ] Archivos y métodos creados cumplen con ser responsabilidades atómicas y fácilmente testeables.
- [ ] Pasan todos los linters y pruebas de arquitectura virtual (`NetArchTest` compatible).
