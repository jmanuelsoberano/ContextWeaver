---
trigger: always_on
---

# ContextWeaver — Política de idioma (siempre)

Alineado con lo declarado en `.cursorrules`:

- Toda explicación, plan, resumen y texto para el usuario en el chat DEBE estar en español.
- **En Código Fuente:** La documentación XML (`<summary>`, `<param>`, `<returns>`), comentarios en línea (`//`) y directivas de cabecera (`#region`) deben escribirse EN ESPAÑOL, con tono directo.
- NO APLIQUES prefijos generados del inglés (no uses "Gets or sets..." -> "Obtiene o establece..."). Ve directo a la descripción: e.g., "El límite de anidamiento permitido.".
- **EXCEPCIONES ESTRICTAS QUE DEBEN SEGUIR EN INGLÉS:**
  - Nomenclaturas y conceptos de programación: NO traduzcas nombres de patrones ("Wrapper", "Helper", "Service", "Pipeline", "Task", "Mock"). "Envoltorio" o "Ayudante" son incorrectos.
  - Identificadores en C#: Toda clase, interfaz, método, propiedad, atributo, parámetro y variable local del código continuarán en estricto inglés (`Engine`, `Parser`, `Analyze()`, `Execute()`).
- Si detectas inglés por error en la documentación o explicaciones, corrígelo. Si ocurre español en un modificador local por error, conviértelo a inglés (e.g. `int contador` -> `int counter`).
