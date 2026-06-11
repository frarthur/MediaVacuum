<div align="center">

# MediaVacuum

**A modern Windows GUI for [yt-dlp](https://github.com/yt-dlp/yt-dlp)**

[![Windows x64](https://img.shields.io/badge/Download-Windows_x64-blue?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/frarthur/MediaVacuum/releases/latest)
[![Version](https://img.shields.io/github/v/release/frarthur/MediaVacuum?color=brightgreen&style=for-the-badge&label=Latest)](https://github.com/frarthur/MediaVacuum/releases)
[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)

</div>

---

**Personal message :**

I built this project because I was tired of opening a terminal every time I wanted to use yt-dlp. I wanted something lightweight, native to Windows, and easy enough that anyone could use it without learning command-line arguments.

---

MediaVacuum is a **Windows desktop application** that wraps [yt-dlp](https://github.com/yt-dlp/yt-dlp) — the powerful command-line media downloader supporting thousands of sites — into a clean, intuitive graphical interface. No terminal knowledge required.

It automatically downloads and manages `yt-dlp.exe` on first launch, integrates with Windows Explorer via a right-click context menu, and supports 5 languages.

---

## Features

- **🎯 Paste & Download** — Drop a URL and download in one click
- **📁 Right-click context menu** — Right-click any folder → *Download media*
- **🎬 Format selection** — Best, bestvideo+bestaudio, bestvideo, bestaudio, worst
- **🎵 Audio extraction** — Convert to MP3, M4A, OPUS, FLAC, WAV
- **🏷️ Metadata embedding** — Embed thumbnails, metadata, and subtitles
- **🔄 Auto-update** — yt-dlp updates itself in the background
- **🌍 Multi-language** — English, Spanish, French, German, Russian
- **⚡ Single-file .exe** — Self-contained, no runtime installation required

---

## Quick Start

1. Download the latest `MediaVacuum.exe` from [Releases](https://github.com/frarthur/MediaVacuum/releases/latest)
2. Run it — yt-dlp is downloaded automatically on first launch
3. Paste a URL and click **Download**

> Optionally, install the context menu via **Tools → Install menu** (requires admin elevation once).

---

## Screenshots

*Coming soon.*

---

## Installation

### From GitHub Releases (recommended)

```powershell
# Download and run
curl -L https://github.com/frarthur/MediaVacuum/releases/latest/download/MediaVacuum.exe -o MediaVacuum.exe
.\MediaVacuum.exe
```

### From source

```powershell
git clone https://github.com/frarthur/MediaVacuum.git
cd MediaVacuum
dotnet run --project src/MediaVacuum
```

### Context menu

Run with admin privileges:

```powershell
.\MediaVacuum.exe --install
```

---

## How to Build

```powershell
# Single-file self-contained .exe
.\publish.ps1
```

The script produces `publish/MediaVacuum.exe` (win-x64, self-contained).

---

## Languages

| Flag | Code | Language | File |
|------|------|----------|------|
| 🇬🇧 | `en` | English | [en.json](src/MediaVacuum.Core/Translations/en.json) |
| 🇪🇸 | `es` | Spanish | [es.json](src/MediaVacuum.Core/Translations/es.json) |
| 🇫🇷 | `fr` | French | [fr.json](src/MediaVacuum.Core/Translations/fr.json) |
| 🇩🇪 | `de` | German | [de.json](src/MediaVacuum.Core/Translations/de.json) |
| 🇷🇺 | `ru` | Russian | [ru.json](src/MediaVacuum.Core/Translations/ru.json) |

---

## Architecture

```
MediaVacuum/
├── src/
│   ├── MediaVacuum/           # WPF UI (App, MainWindow, ViewModels, Converters)
│   ├── MediaVacuum.Core/      # Business logic (YtDlpService, UpdateService, Models)
│   └── MediaVacuum.Installer/ # Registry context menu management
├── tests/
│   └── MediaVacuum.Tests/     # xUnit unit tests
├── publish.ps1                # Build script (single-file .exe)
├── Directory.Build.props      # Shared version (0.1.0)
└── ROADMAP.md                 # Project roadmap and decisions
```

Built with **.NET 9 / WPF / MVVM** — clean separation of concerns, testable core, professional architecture.

---

## Credits

- **[yt-dlp](https://github.com/yt-dlp/yt-dlp)** — This application is a graphical wrapper around yt-dlp. All downloading capabilities are powered by yt-dlp, which is licensed under the [Unlicense](https://github.com/yt-dlp/yt-dlp/blob/master/LICENSE). yt-dlp is a fork of [youtube-dl](https://github.com/ytdl-org/youtube-dl).
- MediaVacuum is **not** affiliated with yt-dlp or YouTube. It is simply a user-friendly interface for the underlying tool.

---

> MediaVacuum is provided as-is, without any warranty. Respect copyright laws and the terms of service of the websites you download from.
