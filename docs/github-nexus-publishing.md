# GitHub to Nexus Publishing

This repository publishes a GitHub Release archive to Nexus Mods through GitHub Actions.

## Required Secrets

Add these repository secrets:

- `NEXUS_API_KEY`: Nexus API key used by the upload action.
- `NEXUS_FILE_ID`: v3 `mod_file` ID for the existing Nexus file that receives new versions.
- `NEXUS_MOD_ID`: v3 internal Nexus mod ID used for changelog entries.

`NEXUS_FILE_GROUP_ID` is an older identifier used by the deprecated upload endpoint. It is not interchangeable with `NEXUS_FILE_ID`.

## Find The IDs

The mod URL contains the game-scoped mod ID, for example:

```text
https://www.nexusmods.com/planetcrafter/mods/212
```

Create a temporary `.env` in the repository root:

```env
NEXUS_API_KEY=your_key_here
```

The file is ignored by Git. Run the helper:

```powershell
.\scripts\get-nexus-ids.ps1 -GameDomain planetcrafter -GameScopedModId 212
```

Use the `id` from `data.mod_files` as `NEXUS_FILE_ID`. Use the printed `NEXUS_MOD_ID` as the second secret. Choose the active main file when a mod has multiple files. Different patches may have different file IDs.

Delete `.env` after the lookup and verify it is absent:

```powershell
Remove-Item .\.env
git status --short
```

## Release Flow

1. Update the plugin version, translations, and `scripts/package-release.ps1`.
2. Build and package the release ZIP.
3. Create a GitHub Release with the ZIP attached.
4. The `Publish Nexus From Release` workflow downloads the ZIP.
5. `release/nexus-description.txt` is sent as the Nexus file description.
6. The GitHub Release body is sent as the Nexus changelog.
7. The workflow uploads a new version to the configured Nexus file.

The Nexus upload action must use the current v3 input names: `file_id`, `category`, and `archive_existing_version`. The changelog additionally requires `mod_id` and `changelog`.
