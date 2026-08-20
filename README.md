<div align="center">

# ⚡ Zytech Boost

### Optimización de sistema para Windows

[![Build](https://github.com/Elvis-Optimizer0/optimized/actions/workflows/build.yml/badge.svg)](https://github.com/Elvis-Optimizer0/optimized/actions/workflows/build.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple?logo=dotnet)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078d4?logo=windows)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![WPF](https://img.shields.io/badge/UI-WPF-blue.svg)](https://learn.microsoft.com/dotnet/desktop/wpf/)

**Zytech Boost** es una aplicación de escritorio built con **WPF (.NET 8)** que permite optimizar el rendimiento de Windows de forma sencilla y visual. Con 10 categorías de tweaks y más de 50 optimizaciones, tu sistema alcanza su máximo potencial con un solo clic.

</div>

---

## 📸 Capturas

| Dashboard | Categoría | Registro |
|:---------:|:---------:|:--------:|
| ![Dashboard](preview.html) | ![Categoría](bg-dark.jpg) | ![Log](bg-light.jpg) |

---

## 🚀 Características

### 10 Categorías de Optimización

| # | Categoría | Descripción | Tweaks |
|---|-----------|-------------|:------:|
| 🧹 | **Limpieza y Mantenimiento** | Archivos temporales, caché DNS, SoftwareDistribution, thumbcache, papelera | 5 |
| 🖱 | **Periféricos e Input Lag** | Curva de mouse, teclado, aceleración de puntero | 4 |
| 🧠 | **Núcleo, Memoria y Kernel** | Paginación, MMCSS, VBS, dynamic tick, Fullscreen Optimizations | 6 |
| ⚙ | **CPU: Rendimiento y Core Parking** | Core parking, P-States, C-States | 4 |
| 🌐 | **Red y Conectividad** | TCP, Nagle, heurísticas, RSS, adaptador | 5 |
| 🎮 | **GPU y Gráficos** | Modo MSI, HAGS, TDR, PowerMizer | 5 |
| 💿 | **Almacenamiento (SSD/NVMe)** | TRIM, Last Access Timestamp | 3 |
| 🔋 | **Energía y Dispositivos** | HPET, Ultimate Performance, USB | 4 |
| 👁 | **Visuales, Debloat y Privacidad** | Efectos visuales, bloatware, Copilot, telemetría | 7 |
| ☠ | **Zona Extrema** | Servicios extremos, Defender tiempo real, Timer Resolution | 3 |

### Funcionalidades Principales

- 🎨 **Interfaz moderna** con diseño dark/light y animaciones fluidas
- 📊 **Dashboard** con resumen de categorías y estado del sistema
- 📝 **Registro de sesión** con log completo de todas las optimizaciones aplicadas
- ⚠️ **Sistema de advertencias** para tweaks peligrosos (Zona Extrema)
- 🔄 **Punto de restore** antes de aplicar cambios
- 🛡 **Scripts PowerShell embebidos** ejecutados de forma segura
- 📦 **Ejecutable autocontenido** — sin dependencias de instalación

---

## 📋 Requisitos

- **Sistema operativo:** Windows 10 / 11 (x64)
- **Permisos:** Ejecutar como administrador (requerido para tweaks del sistema)
- **.NET Runtime:** No necesario — el ejecutable es autocontenido

---

## 🛠 Compilación desde código fuente

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 con workloads **.NET Desktop Development** (opcional)
- Windows 10/11 x64

### Usando CLI

```bash
# Clonar el repositorio
git clone https://github.com/Elvis-Optimizer0/optimized.git
cd optimized

# Restaurar dependencias
dotnet restore ZytechBoost/ZytechBoost.csproj

# Compilar en modo Release
dotnet build ZytechBoost/ZytechBoost.csproj -c Release

# Publicar ejecutable autocontenido
dotnet publish ZytechBoost/ZytechBoost.csproj `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -c Release `
  -o publish
```

### Usando Visual Studio

1. Abrir `ZytechBoost.sln`
2. Seleccionar configuración **Release**
3. `Build → Publish Solution`
4. Seleccionar **Folder Profile** con runtime `win-x64`

### Usando GitHub Actions

El ejecutable se genera automáticamente en cada push a `main`. Descárgalo desde la pestaña **Actions → Artifacts** del repositorio.

---

## 📁 Estructura del proyecto

```
ZytechBoost/
├── .github/
│   └── workflows/
│       └── build.yml              # CI/CD pipeline
├── ZytechBoost/
│   ├── App.xaml                   # Entry point de la aplicación
│   ├── App.xaml.cs
│   ├── MainWindow.xaml            # Ventana principal con navegación
│   ├── MainWindow.xaml.cs
│   ├── ZytechBoost.csproj         # Configuración del proyecto
│   ├── app.manifest               # Manifest de administrador
│   ├── Converters/
│   │   └── BoolToOpacityConverter.cs
│   ├── Models/
│   │   ├── CategoryRegistry.cs    # Registro central de categorías
│   │   └── TweakModels.cs         # Modelos de datos
│   ├── Modules/
│   │   └── PowerShellEngine.cs    # Motor de ejecución PS1
│   ├── Scripts/                   # 16 scripts PowerShell embebidos
│   │   ├── Invoke-OptiCleaning.ps1
│   │   ├── Invoke-OptiCPU.ps1
│   │   ├── Invoke-OptiGPU*.ps1
│   │   ├── Invoke-OptiNetwork*.ps1
│   │   └── ...
│   ├── Styles/
│   │   ├── Animations.xaml        # Animaciones y transiciones
│   │   ├── Colors.xaml            # Paleta de colores
│   │   └── Controls.xaml          # Estilos de controles
│   └── Views/
│       ├── DashboardView.xaml     # Panel principal
│       ├── CategoryView.xaml      # Vista de categoría
│       ├── ConfirmationModal.xaml # Modal de confirmación
│       └── LogView.xaml           # Registro de sesión
├── ZytechBoost.sln
├── .gitignore
├── LICENSE
└── README.md
```

---

## ⚠️ Advertencias

> **Zona Extrema:** Los tweaks en la categoría "Zona Extrema" desactivan servicios críticos como Windows Defender en tiempo real. Solo úsalos si sabes lo que haces.

> **Punto de restore:** La aplicación crea automáticamente un punto de restauración antes de aplicar cambios. Se recomienda crear uno manualmente antes de usar tweaks avanzados.

> **Responsabilidad:** El usuario es responsable de los cambios realizados en su sistema. Se recomienda entender cada tweak antes de aplicarlo.

---

## 🤝 Contribuir

Las contribuciones son bienvenidas. Por favor, lee [CONTRIBUTING.md](CONTRIBUTING.md) antes de abrir un Pull Request.

### Quick Start

1. Fork el repositorio
2. Crear una rama (`git checkout -b feature/nueva-funcionalidad`)
3. Hacer commit (`git commit -m 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abrir un Pull Request

---

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Ver [LICENSE](LICENSE) para más detalles.

---

<div align="center">

**Hecho con ❤️ para la comunidad de optimización de Windows**

</div>
