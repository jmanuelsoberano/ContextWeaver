# Contribuir a ContextWeaver

¡Gracias por tu interés en contribuir! 🎉

## Configuración del entorno

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

### Setup

```bash
# 1. Clonar el repositorio
git clone https://github.com/jmanuelsoberano/ContextWeaver.git
cd ContextWeaver

# 2. Restaurar dependencias
dotnet restore

# 3. Verificar que todo compila
dotnet build

# 4. Correr los tests
dotnet test
```

## Arquitectura del proyecto

```
Cli → Engine → Core    (Dependency Rule: nunca al revés)
```

| Proyecto | Qué contiene | Cuándo tocarlo |
|:---|:---|:---|
| `ContextWeaver.Core` | Modelos + Interfaces | Agregar/modificar DTOs o abstracciones |
| `ContextWeaver.Engine` | Analyzers, Reporters, Services | Agregar lógica de análisis o formatos de reporte |
| `ContextWeaver.Cli` | Program.cs + DI wiring | Modificar argumentos CLI o configuración de DI |

> **Regla:** Core no puede importar Engine ni Cli. Engine no puede importar Cli. El compilador lo enforza.

## Principios de diseño

Este proyecto aplica principios fundacionales de ingeniería de software:

- **Separation of Concerns** (Dijkstra, 1974)
- **Information Hiding** (Parnas, 1972)
- **High Cohesion / Low Coupling** (Constantine & Yourdon, 1979)
- **Composition over Inheritance** (GoF, 1994)
- **Dependency Rule** — las dependencias fluyen hacia las abstracciones

## Flujo de contribución

1. **Fork** el repositorio
2. **Crea un branch** desde `main`: `git checkout -b feature/mi-feature`
3. **Haz tus cambios** siguiendo la arquitectura descrita arriba
4. **Asegúrate de que pasa todo:**
   ```bash
   dotnet format --verify-no-changes
   dotnet build --no-incremental
   dotnet test
   ```
5. **Commit** usando [Conventional Commits](https://www.conventionalcommits.org/):
   - `feat: agregar soporte para TypeScript`
   - `fix: corregir cálculo de complejidad ciclomática`
   - `refactor: extraer lógica de filtrado a servicio`
   - `docs: actualizar guía de contribución`
6. **Abre un Pull Request** hacia `main`

## Convenciones de código

- **Estilo:** configurado en `.editorconfig` + StyleCop. Ejecuta `dotnet format` antes de hacer commit.
- **Idioma y Localización:**
  - **Documentación y Comentarios**: Español (excepto Propiedades y Constructores, que deben seguir reglas de StyleCop (English)).
  - **Términos Técnicos**: Mantener en Inglés (ej. "Helper", "Task", "Wrapper").
  - **Identificadores de Código**: Inglés (ej. `CSharpFileAnalyzer`).
  - *Nota*: Para reglas detalladas de IA, consultar `.cursorrules`.
- **Tests:** todo feature nuevo debe incluir tests unitarios o E2E.
- **Namespaces:** deben coincidir con la ubicación del archivo dentro del proyecto.

## ¿Dónde va mi código?

| Quiero... | Proyecto | Carpeta |
|:---|:---|:---|
| Agregar un nuevo analizador (ej. TypeScript) | Engine | `Analyzers/` |
| Agregar un nuevo formato de reporte (ej. XML) | Engine | `Reporters/` |
| Agregar una nueva sección al reporte Markdown | Engine | `Reporters/Sections/` |
| Agregar un nuevo modelo de datos | Core | `Models/` |
| Agregar una nueva abstracción | Core | `Abstractions/` |
| Modificar argumentos CLI | Cli | `Program.cs` |
| Agregar una nueva utilidad de cálculo | Engine | `Utilities/` |

## Reportar bugs

Usa la plantilla de [Bug Report](.github/ISSUE_TEMPLATE/bug_report.md) para reportar problemas.

## Proponer features

Usa la plantilla de [Feature Request](.github/ISSUE_TEMPLATE/feature_request.md) para proponer nuevas funcionalidades.
