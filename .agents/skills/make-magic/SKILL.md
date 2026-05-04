---
name: make-magic
description: Add or scaffold a new magic in this Unity client when the user asks to create a magic, add a magic, or wire a magic into the client. Use for requests that need LocalCombinedMagicData and Magic localization text. Use `make-prefab` separately when the request also includes runtime prefabs or icon sprites.
---

# Make Magic

Use this skill when adding a new magic to `word-online/dev/client`.

## Scope

This skill covers the client-side data and localization work:

- add the magic entry to `Assets/Scripts/Data/Magic/LocalCombinedMagicData.cs`
- add the localization key to `Assets/Localization/Magic Shared Data.asset`
- add localized text to `Assets/Localization/Magic_en.asset`
- add localized text to `Assets/Localization/Magic_ko-KR.asset`

If the request includes prefab or sprite asset work, use `make-prefab` alongside this skill.
If the request includes server work, only handle the client portion here.

## Workflow

1. Inspect a nearby magic of the same family first.
2. Update `LocalCombinedMagicData.dataList` with the new `CombinedMagicData` entry:
   use the server magic id, the display name, the exact recipe card order used by nearby entries, and the `Resources` sprite path without file extension.
3. Add a new localization key tuple to `Magic Shared Data.asset`.
4. Add matching English and Korean localized values using the same `m_Id` in both locale assets.
5. If a prefab or icon is needed, hand that off to `make-prefab` and keep the naming aligned.
6. Ignore unrelated working-tree changes unless the user explicitly asks to include them.

## File Patterns

- Display metadata:
  `Assets/Scripts/Data/Magic/LocalCombinedMagicData.cs`
- Localization shared keys:
  `Assets/Localization/Magic Shared Data.asset`
- Localization text:
  `Assets/Localization/Magic_en.asset`
  `Assets/Localization/Magic_ko-KR.asset`

## Localization Rules

- Reuse the same `m_Id` for the shared key row and both locale rows.
- The shared asset stores the key in lower camel or server bean style, for example `leafair` or `frenzyTotem`.
- `Magic_en.asset` may temporarily use the raw key as fallback text if no final English copy is provided, but prefer the real display name.
- Keep `Magic_ko-KR.asset` in escaped YAML string form when Unity serializes it that way.

## Data Rules

- `id` must match the authoritative server magic id.
- `magicName` is the client display name, usually the English display name.
- `spritePath` must be a `Resources` path without extension, for example `Game/drop/leafair`.
- This file is a display metadata fallback. If `GameDataManager.Config.magicRecipes` is loaded, recipe cards come from the server and are merged with local metadata by id.
- Keep the `magicName` and `spritePath` aligned with any prefab or sprite asset work done via `make-prefab`.

## Validation

- Search for the new key across `Assets/Localization` and confirm it appears in the shared asset plus both locale assets.
- Search for the new magic id and name in `LocalCombinedMagicData.cs`.
- If the request included asset work, confirm the paired `make-prefab` task completed with matching names and paths.

## Example

For `Leafair`, the client-side work is:

- add `new CombinedMagicData(){id = 24, magicName = "Leafair", recipe = new () { CardType.Drop, CardType.Nature}, spritePath = "Game/drop/leafair"},`
- add the shared key `leafair`
- add localized text for `leafair` in English and Korean
- if needed, use `make-prefab` to ensure `Assets/Resources/Prefabs/Leafair.prefab` and `Assets/Resources/Game/drop/leafair.png` exist

For current asset patterns and a concrete example, read `references/client-magic-patterns.md`.
