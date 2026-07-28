# LiteOverlay ⚡

> **Ultra Low-End Gaming Performance HUD & Hardware System Monitor for Low-End PCs & Laptops**

LiteOverlay is a high-performance, lightweight gaming HUD dashboard and standalone native Windows overlay application designed specifically for low-end PCs and laptops. It consumes **under 15 MB RAM** and **0% CPU** overhead during gameplay.

---

## 🛠️ Tech Stack

```text
Frontend
• HTML5
• CSS3
• Vanilla JavaScript

Desktop / Wrapper
• Tauri

Native Layer
• C# (.NET WinForms / P/Invoke)

Build System
• PowerShell

Deployment
• Vercel (Static Web Host)

Version Control
• Git + GitHub
```

---

## 🌟 Key Features

- **🎮 Floating Borderless Gaming HUD**: Real-time transparent overlay displaying Live FPS (DWM presentation API), Network Ping, RAM, CPU Usage, GPU Usage, Thermal Temp, Battery %, Network Speed, and Disk Storage.
- **🎨 Custom HUD Styling**: Full control over background opacity sliders, font sizes, corner rounding radius, accent color themes, and border line/glow toggles.
- **⚡ Ultra-Low Resource Usage**: Consistently runs under **15 MB RAM** and near **0% CPU** footprint.
- **💻 Clean Native Executable**: Standalone `LiteOverlay.exe` with zero web browser dependencies or extra profile folders.
- **🌐 Web Dashboard & Vercel Support**: Ready for 1-click zero-config deployment on Vercel or any static web host.

---

## 📁 Repository Structure

```text
LiteOverlay/
├── assets/             # App branding assets (logo, icon)
│   ├── app.ico
│   └── logo.png
├── css/                # Styling design system
│   └── styles.css
├── js/                 # Web dashboard & Tauri bridge logic
│   ├── app.js
│   └── tauri_bridge.js
├── overlay/            # Native C# overlay sources & compiled binaries
│   ├── LiteOverlay.cs
│   ├── SetupLiteOverlay.cs
│   └── bin/
│       ├── LiteOverlay.exe
│       └── SetupLiteOverlay.exe
├── scripts/            # Build & utility scripts
│   └── build.ps1
├── src-tauri/          # Tauri framework configuration
├── index.html          # Web dashboard entry point
├── package.json        # Node.js project manifest
├── README.md           # Documentation
├── vercel.json         # Vercel deployment configuration
└── .gitignore          # Git ignore rules
```

---

## 🚀 Live Web Dashboard Deployment (Vercel)

You can host and deploy LiteOverlay directly on Vercel:

1. Import this repository into **[Vercel](https://vercel.com)**.
2. Vercel automatically detects `index.html` and static assets.
3. Your web dashboard and direct download links for `SetupLiteOverlay.exe` will be live instantly!

---

## 🛠️ Building Executables Locally

To compile the native Windows executables (`LiteOverlay.exe` and `SetupLiteOverlay.exe` installer):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/build.ps1
```

---

## 📜 License

Distributed under the **MIT License**.
