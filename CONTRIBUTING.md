# Contribuir a ContextWeaver

¡Gracias por tu interés en contribuir! 🎉

## Configuración del entorno

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Git](https://git-scm.com/)

### Setup Inicial

1.  **Clonar el repositorio**:
    ```bash
    git clone https://github.com/jmanuelsoberano/ContextWeaver.git
    cd ContextWeaver
    ```

2.  **Restaurar dependencias y herramientas**:
    Este paso es crucial ya que instala **Husky.NET** (para hooks de git) y otras herramientas locales.
    ```bash
    dotnet tool restore
    ```
    *Nota: Si los hooks no se instalan automáticamente, ejecuta `dotnet husky install`.*

## Flujo de Trabajo y Calidad

### Estándares de Código (.editorconfig)
Utilizamos `dotnet format` para asegurar consistencia.
- **Root `.editorconfig`**: Reglas estrictas para código de producción.
- **`tests/.editorconfig`**: Reglas adaptadas para pruebas (ej. `Metodo_Escenario_Resultado`).

### Hooks Automáticos (Husky.NET)
Hemos configurado **Husky** para proteger la calidad del código:
- **Pre-commit**: Ejecuta automáticamente `dotnet format` en los archivos que vas a subir (staged files).
    - Si el error es corregible (espacios, indentación), se arregla y se incluye en el commit.
    - Si el error requiere intervención manual, el commit fallará para que lo revises.

### Ejecutar Pruebas
Para correr toda la suite de pruebas (Unitarias + E2E):
```bash
dotnet test
```

## Arquitectura del proyecto

```
Cli → Engine → Core    (Regla de Dependencia: nunca al revés)
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
4. **Verificación Local:**
   Husky se encargará del formato al hacer commit, pero puedes correr manualmente:
   ```bash
   dotnet format
   dotnet build
   dotnet test
   ```
5. **Commit** usando [Conventional Commits](https://www.conventionalcommits.org/):
   - `feat: agregar soporte para TypeScript`
   - `fix: corregir cálculo de complejidad ciclomática`
   - `refactor: extraer lógica de filtrado a servicio`
   - `docs: actualizar guía de contribución`
6. **Abre un Pull Request** hacia `main`

## Convenciones de código

- **Estilo:** Gestionado por `.editorconfig` y `dotnet format`.
- **Idioma y Localización:**
  - **Documentación y Comentarios**: Español.
  - **Constructores y Propiedades**: Inglés (reglas StyleCop específicas).
  - **Términos Técnicos**: Mantener en Inglés (ej. "Helper", "Task", "Wrapper").
  - **Identificadores de Código**: Inglés (ej. `CSharpFileAnalyzer`).
- **Tests:** Todo feature nuevo debe incluir tests unitarios o E2E.
- **Namespaces:** Deben coincidir con la estructura de carpetas.

## ¿Dónde va mi código?

| Quiero... | Proyecto | Carpeta |
|:---|:---|:---|
| Agregar un nuevo analizador (ej. TypeScript) | `ContextWeaver.Engine` | `Analyzers/` |
| Agregar un nuevo formato de reporte (ej. XML) | `ContextWeaver.Engine` | `Reporters/` |
| Agregar un nuevo modelo de datos | `ContextWeaver.Core` | `Models/` |
| Agregar lógica de prueba unitaria | `ContextWeaver.Engine.Tests` o `Core.Tests` | Según corresponda |
| Agregar fixtures de prueba | `ContextWeaver.Tests.Shared` | `Fixtures/` |

## Reportar bugs y Proponer Features

Usa las plantillas de Issue en `.github/ISSUE_TEMPLATE/` para reportar problemas o sugerir mejoras.
