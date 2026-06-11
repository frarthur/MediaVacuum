## v0.1.1

### Nouveautés
- Configuration persistante : le dossier de sortie, la langue et les options sont sauvegardés entre les sessions (`%LOCALAPPDATA%\MediaVacuum\config.json`)
- Support logo : placez `logo_app.png` dans `Assets/` pour remplacer le placeholder "MV"
- Architecture plus propre : séparation ViewModel/View via `IDialogService`, plus de `System.Windows` dans le ViewModel
- Les traductions sont maintenant incluses dans l'archive zip et copiées dans le dossier de données au premier lancement

### Corrections
- Traductions qui ne fonctionnaient pas dans la version publiée (manquantes dans le zip)
- Crash au démarrage avec `ArgumentNullException` dans `LocalizationService`
- `Assembly.Location` vide en mode single-file → remplacé par `Environment.ProcessPath`

### Technique
- Version bumpée à `0.1.1`
- Nouveaux fichiers : `AppPaths.cs`, `AppConfig.cs`, `IDialogService.cs`, `DialogService.cs`
- Publish inclut désormais `Translations/` et `Assets/`
