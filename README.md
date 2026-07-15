# Planet Crafter Translation Mod

Translation mod for `The Planet Crafter`, built as a BepInEx plugin.

The current package includes a Bulgarian `translations/labels.txt`, but the loader is intentionally generic so other translators can adapt it for their own language files and target language slots.

## For Players

1. Install BepInEx for `The Planet Crafter`.
2. Start the game once, then close it.
3. Extract the mod archive into the game folder.
4. Start the game and select the language configured in the mod config.

This mod does not require changing `HideManagerGameObject` in `BepInEx.cfg` on the tested setup. If your existing BepInEx installation already works, you can usually leave that file as-is.

Expected paths after installation:

- `BepInEx/plugins/AcTePuKc UI Translation/PlanetCrafterTranslationMod.dll`
- `BepInEx/plugins/AcTePuKc UI Translation/translations/labels.txt`
- `BepInEx/config/actepukc.theplanetcrafter.uitranslationbulgarian.cfg`

## Mod Config

This is the plugin's own config file:

`BepInEx/config/actepukc.theplanetcrafter.uitranslationbulgarian.cfg`

Key settings:

```ini
[General]
EnableTranslationOverrides = true
TargetLanguageName = bulgarian
LabelsFileName = labels.txt
AssumeTargetLanguageOnStartup = false

[Diagnostics]
DumpTranslations = false
CheckMissingKeys = false
```

`translations/labels.txt` is the active translation file.

`DumpTranslations = false` is the normal release setting. Enable it only while investigating missing or hardcoded text, because it writes a local dump file under `dumps/translation-dump.txt`.

If you already ran an older version of the mod, BepInEx may keep old config values. In that case, delete the old config file and let the plugin recreate it.

## Translation File Format

Each line uses:

```text
Key=Translated text
```

Escaped line breaks such as `\n` and `\n\n` are supported directly in values.

## Project Layout

- `src/PlanetCrafterTranslationMod`: plugin source.
- `src/PlanetCrafterTranslationMod/translations/labels.txt`: current translation file included with this repo.
- `dist/`: staged build output.
- `scripts/`: shared build entrypoints.
- `User.targets.example`: local build configuration template.

## Build From Source

Create a local `User.targets` file from `User.targets.example`, then edit `GameDir` to match your installation path.

`User.targets` is ignored by git and should stay local to each contributor.

You can also set `PLANET_CRAFTER_DIR`, or pass `-p:GameDir=...` directly to `dotnet build`.

Build the plugin:

```powershell
.\scripts\build-plugin.ps1
```

The staged output is written under:

```text
dist/
```

This project expects local game/BepInEx assemblies from an installed copy of the game. Those files should not be committed or redistributed.

To rebuild a clean staged `dist/` folder:

```powershell
.\scripts\package-release.ps1 -Version 0.2.0
```

The script creates `dist/PlanetCrafterTranslationMod-<version>.zip`, which is the archive uploaded to Nexus Mods.

## GitHub to Nexus Publishing

The repository includes a GitHub Actions workflow that publishes the matching release archive to Nexus after a GitHub Release is published. Add these repository secrets before using it:

- `NEXUS_API_KEY`
- `NEXUS_FILE_GROUP_ID`

The workflow can also be started manually with `workflow_dispatch` and a release tag such as `v0.2.0`.

## Sharing Rules

Safe to share:

- Plugin source code.
- Documentation.
- Build templates.
- Your own translated `labels.txt`.

Do not share:

- Original game assets.
- Dumps of original game localization text.
- Redistributed game assemblies.

## Disclaimer

This is an unofficial fan-made translation mod. `The Planet Crafter` is owned by its respective developers and publishers. This project does not include original game assets.
