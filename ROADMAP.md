# MediaVacuum — Roadmap Projet

## Vision
Application Windows professionnelle qui intègre **yt-dlp** dans l'explorateur Windows via le menu contextuel, avec une interface graphique WPF complète pour télécharger médias et vidéos depuis des centaines de sites.

---

## Choix Techniques

| Aspect | Choix | Justification |
|--------|-------|---------------|
| **Langage** | C# 12 / .NET 8 | Windows natif, WPF pour GUI riche, .exe standalone, écosystème mature |
| **Framework GUI** | WPF (MVVM) | Standard professionnel Windows, data binding, maintenable |
| **Architecture** | Clean Architecture / MVVM | 3 projets : Core (logique), WPF (UI), Installer (déploiement) |
| **Build** | dotnet publish / MSBuild | Single-file .exe ou MSI |
| **Docker** | Non applicable | Application GUI Windows native, pas de conteneurisation pertinente |
| **Backend** | yt-dlp.exe | Moteur de téléchargement éprouvé (crédité) |

---

## Structure du Projet

```
MediaVacuum/
├── src/
│   ├── MediaVacuum/               # Application WPF (UI + ViewModels)
│   │   ├── App.xaml / App.xaml.cs
│   │   ├── MainWindow.xaml / .cs
│   │   ├── ViewModels/
│   │   │   └── MainViewModel.cs
│   │   ├── Views/
│   │   │   ├── DownloadView.xaml / .cs
│   │   │   └── SettingsView.xaml / .cs
│   │   └── Converters/
│   ├── MediaVacuum.Core/          # Logique métier (sans UI)
│   │   ├── Models/
│   │   │   └── DownloadOptions.cs
│   │   ├── Services/
│   │   │   ├── YtDlpService.cs        # Wrapper yt-dlp
│   │   │   ├── UpdateService.cs       # Auto-update yt-dlp
│   │   │   └── ContextMenuService.cs  # Gestion menu contextuel
│   │   └── Interfaces/
│   │       └── IYtDlpService.cs
│   └── MediaVacuum.Installer/     # Install/Désinstall (CLI ou wrapper)
│       └── Installer.cs
├── tests/
│   └── MediaVacuum.Tests/
├── docs/
├── inspi/                         # Fichiers d'inspiration originaux
├── yt-dlp/                        # Binaires yt-dlp (téléchargés)
└── .gitmessage                    # Template de commit
```

---

## Phases

### Phase 1 — Fondations
- [x] Initialiser le dépôt git
- [x] Créer la solution .NET avec les 3 projets
- [x] Configurer .gitignore, .gitmessage
- [x] Écrire le ROADMAP.md finalisé

### Phase 2 — Core (logique métier)
- [ ] `YtDlpService` — exécution de yt-dlp avec arguments, parsing JSON, streaming stdout
- [ ] `UpdateService` — téléchargement de la dernière version de yt-dlp.exe
- [ ] `ContextMenuService` — ajout/suppression du menu contextuel (registry)
- [ ] `DownloadOptions` — modèle des options de téléchargement

### Phase 3 — Interface WPF (MVVM)
- [ ] `MainWindow` — layout principal (URL, options, logs, progression)
- [ ] `DownloadView` — contrôle de téléchargement simple (URL + preset)
- [ ] `SettingsView` — configuration (dossier sortie, format, ffmpeg, etc.)
- [ ] `MainViewModel` — binding des données, commandes asynchrones
- [ ] Thème visuel propre, icônes, branding

### Phase 4 — Installateur & Déploiement
- [ ] Installation : copie des fichiers + inscription menu contextuel
- [ ] Désinstallation : nettoyage complet (fichiers + registry)
- [ ] Auto-update : mise à jour de yt-dlp.exe via `UpdateService`
- [ ] Single-file publish (.exe autonome)

### Phase 5 — Finitions
- [ ] Tests unitaires (Core)
- [ ] Documentation utilisateur
- [ ] Licence (MIT)
- [ ] Publication GitHub

---

## Règles de Commit

Suivre le format défini dans `.gitmessage` :

```
<type>(<scope>): <sujet>
```

Types : `feat` | `fix` | `refactor` | `style` | `test` | `docs` | `chore` | `perf`

Exemples :
- `feat(core): add yt-dlp process wrapper`
- `feat(ui): implement download view with presets`
- `fix(installer): handle spaces in install path`
- `docs(roadmap): update project phases`

---

## Release Workflow

Chaque version stable est taguée (`v0.1.0`, `v0.2.0`, etc.) et distribuée via **GitHub Releases**.

```powershell
# 1. Build single-file .exe
.\publish.ps1

# 2. Créer la release GitHub (nécessite gh CLI)
gh release create v0.1.0 --title "v0.1.0" --notes "Release notes..." ./publish/MediaVacuum.exe
```

`publish.ps1` produit un `.exe` self-contained dans `./publish/` et une archive `.zip` à la racine.

L'application peut vérifier sa version via l'API GitHub Releases (`/repos/frarthur/MediaVacuum/releases/latest`) pour implémenter l'auto-update.

---

## Crédits

- [yt-dlp](https://github.com/yt-dlp/yt-dlp) — le moteur de téléchargement sous-jacent
