---
trigger: always_on
---

# ContextWeaver — Calidad, Tamaño y Prácticas C# (siempre)

- Modularidad: Mantén responsabilidades limitadas por clase. Alto grado de inyección de dependencias a nivel constructor.
- Zero Warnings: "Una advertencia ignorada hoy es un bug mañana". Trata los warnings como errores estáticos en tu output.
- Diseño sellado: Usa la palabra reservada `sealed` para todas las clases y servicios de lógicas cerradas.
- Colecciones: Privilegia implementaciones Inmutables (`IReadOnlyCollection<T>`, `IReadOnlyList<T>`, arrays con C# 12 `Collection Expressions`) frente a Mutables, a menos que un algoritmo mutable sea demostrablemente indispensable a nivel local dentro de un método.
- Estructura de funciones: Separa código largo en funciones descriptivas que puedan ser fácilmente testeables por los Mocks del sistema. Evita pasar demasiados argumentos separados agrupándolos mediante options/settings records.
- Pruebas expresivas en `.Tests`: Sigue el naming híbrido de `.editorconfig` localmente en pruebas: Ej. `MiClase_ValidarArgumento_LanzaExcepcion()`.
