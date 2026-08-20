# 🤝 Contribuir a Zytech Boost

¡Gracias por tu interés en contribuir a Zytech Boost! Este documento explica cómo puedes ayudar.

---

## 📋 Tipos de Contribuciones

### 🐛 Reportar Bugs

1. Verificar que el bug no haya sido reportado en [Issues](https://github.com/Elvis-Optimizer0/optimized/issues)
2. Abrir un nuevo issue usando la plantilla **Bug Report**
3. Incluir:
   - Versión de Windows
   - Versión de .NET
   - Pasos para reproducir
   - Comportamiento esperado vs actual
   - Logs si es posible (el archivo de log se guarda en el Escritorio)

### 💡 Sugerir Funcionalidades

1. Abrir un issue usando la plantilla **Feature Request**
2. Describir la funcionalidad, caso de uso y beneficios
3. Indicar si estás dispuesto a implementarla

### 🔧 Enviar Código

1. Fork el repositorio
2. Crear una rama para tu cambio
3. Hacer tus modificaciones
4. Asegurar que compila sin errores
5. Abrir un Pull Request

---

## 🛠 Ambiente de Desarrollo

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (recomendado) o VS Code con extensión C#
- Windows 10/11 x64
- Git

### Configuración

```bash
# Clonar tu fork
git clone https://github.com/TU_USUARIO/optimized.git
cd optimized

# Agregar upstream
git remote add upstream https://github.com/Elvis-Optimizer0/optimized.git

# Compilar
dotnet build ZytechBoost/ZytechBoost.csproj -c Debug
```

### Ejecutar

```bash
dotnet run --project ZytechBoost/ZytechBoost.csproj
```

> ⚠️ **Importante:** La aplicación requiere permisos de administrador para ejecutar los scripts de optimización. Ejecuta tu IDE como administrador.

---

## 📐 Convenciones de Código

### C#

- Usar **PascalCase** para clases, métodos y propiedades públicas
- Usar **camelCase** para campos privados y variables locales
- Prefijar campos privados con `_` (ej: `_fieldName`)
- Usar `namespace ZytechBoost` (file-scoped namespaces)
- Habilitado `Nullable` — manejar `null` adecuadamente
- Agregar XML documentation a clases y métodos públicos

### XAML

- Nombres de elementos en **PascalCase** (ej: `MainFrame`, `StatusBarText`)
- Estilos definidos en `Styles/` separados por funcionalidad
- Colores en `Colors.xaml` usando Brushes

### Scripts PowerShell

- Nomenclatura: `Invoke-Opti{Categoría}.ps1`
- Cada función debe tener `#Requires -RunAsAdministrator`
- Usar `Write-Output` para mensajes al UI
- Usar `Write-Warning` para advertencias
- Manejar errores con `try/catch`

### Commits

- Usar **Conventional Commits**:
  - `feat:` nueva funcionalidad
  - `fix:` corrección de bug
  - `docs:` documentación
  - `style:` formato de código
  - `refactor:` refactorización
  - `test:` tests
  - `chore:` tareas de mantenimiento
- Ejemplo: `feat: Add CPU core parking toggle`

---

## 📁 Estructura de Categorías

Cada categoría de optimización sigue esta estructura:

1. **Definición** en `Models/CategoryRegistry.cs`
2. **Script(s) PowerShell** en `Scripts/Invoke-Opti{Nombre}.ps1`
3. **Función mapeada** en el diccionario de `PowerShellEngine.cs`

### Agregar una nueva categoría

1. Agregar la categoría en `CategoryRegistry.cs`:
```csharp
new OptiCategory
{
    Id = "nueva_categoria",
    Name = "Nombre de Categoría",
    Description = "Descripción breve",
    Icon = "🎮",
    TileColor = "AccentBrush",
    ScriptFunctions = new() { "Invoke-OptiNuevaCategoria" },
    Tweaks = new()
    {
        new Tweak { Id = "tweak_1", Name = "Nombre Tweak", 
            Description = "Descripción", Icon = "⚡" },
    }
}
```

2. Crear el script en `Scripts/Invoke-OptiNuevaCategoria.ps1`
3. Registrar la función en `PowerShellEngine.cs`

---

## 🔍 Revisión de Pull Requests

Los PRs serán revisados verificando:

- ✅ Compila sin errores
- ✅ No rompe funcionalidad existente
- ✅ Sigue las convenciones de código
- ✅ Incluye documentación si es necesario
- ✅ Scripts PowerShell son seguros y manuejan errores

---

## ❓ Preguntas

Si tienes preguntas, abre un issue con la etiqueta **question**.
