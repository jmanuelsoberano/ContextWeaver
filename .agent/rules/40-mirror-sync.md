---
trigger: always_on
---

## REGLA DE SINCRONIZACIÓN OBLIGATORIA (EFECTO ESPEJO)

Tu función crítica en este proyecto es mantener una sincronización perfecta entre tu entorno interno (Anti Gravity/Artefactos) y los archivos locales del proyecto.

CADA VEZ que realices una acción que modifique el estado del proyecto en tu interfaz interna, ESTÁS OBLIGADO a replicar ese mismo cambio en los archivos físicos locales ANTES de responder en el chat.

Aplica este "efecto espejo" bajo las siguientes condiciones:

1. **Tareas:** Si generas, modificas o marcas una tarea como completada en tu gestor interno, debes abrir `.context/tasks.md` y realizar la actualización exacta (ej. marcando `[x]`).
2. **Plan:** Si avanzas de fase, agregas pasos o modificas tu plan de implementación interno, debes editar `.context/plan.md` para que refleje exactamente tu nuevo estado interno.
3. **Workarounds:** Si el entorno o el código nos obliga a tomar una ruta alternativa, aplicar un parche o una solución no convencional, debes registrarlo en tu sistema y simultáneamente documentarlo en `.context/workarounds.md`.

**RESTRICCIÓN DE SALIDA:** Es una violación a tus instrucciones confirmar en el chat que has actualizado tu entorno, plan o tareas sin haber ejecutado la escritura correspondiente en los archivos `.md`. Ambos ecosistemas (tu entorno y los archivos locales) deben ser idénticos al finalizar cada una de tus respuestas.

**Ubicación canónica de los archivos:**
- `.context/plan.md`
- `.context/tasks.md`
- `.context/workarounds.md`
