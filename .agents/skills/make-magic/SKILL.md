---
name: make-magic
description: Add or scaffold a new magic in this Unity client when the user asks to create a magic, add a magic, or wire a magic into the client. Use for requests that need LocalCombinedMagicData, Magic localization text, and a Resources prefab or sprite path for the new magic.
---

# Make Magic

Use this skill when adding a new magic to `word-online/dev/client`.

## Scope

This skill covers the client-side minimum:

- add the magic entry to `Assets/Scripts/Data/Magic/LocalCombinedMagicData.cs`
- add the localization key to `Assets/Localization/Magic Shared Data.asset`
- add localized text to `Assets/Localization/Magic_en.asset`
- add localized text to `Assets/Localization/Magic_ko-KR.asset`
- add or validate the runtime prefab under `Assets/Resources/Prefabs`
- add or validate the magic icon sprite under `Assets/Resources/Game/...`

If the request includes server work, only handle the client portion here.

## Workflow

1. Inspect a nearby magic of the same family first.
2. Update `LocalCombinedMagicData.dataList` with the new `CombinedMagicData` entry:
   use the server magic id, the display name, the exact recipe card order used by nearby entries, and the `Resources` sprite path without file extension.
3. Add a new localization key tuple to `Magic Shared Data.asset`.
4. Add matching English and Korean localized values using the same `m_Id` in both locale assets.
5. Add or validate the prefab in `Assets/Resources/Prefabs/<Name>.prefab`.
6. Confirm the prefab name matches what the server-created object type will load through `Resources.Load<GameObject>($"Prefabs/{createdObjectDto.type}")`.
7. Confirm the icon sprite exists at the path referenced by `spritePath`.
8. Ignore unrelated working-tree changes unless the user explicitly asks to include them.

## File Patterns

- Display metadata:
  `Assets/Scripts/Data/Magic/LocalCombinedMagicData.cs`
- Localization shared keys:
  `Assets/Localization/Magic Shared Data.asset`
- Localization text:
  `Assets/Localization/Magic_en.asset`
  `Assets/Localization/Magic_ko-KR.asset`
- Runtime prefab:
  `Assets/Resources/Prefabs/<MagicPrefabName>.prefab`
- Magic-book / HUD icon:
  `Assets/Resources/Game/<family>/<icon_name>.png`

## Prefab Rules

- Prefer copying a nearby prefab of the same family and editing the instance overrides instead of building a YAML prefab from scratch.
- If the magic family has a matching abstract base under `Assets/Resources/Prefabs/Abstract`, use that base pattern first. Prefer duplicating a nearby concrete variant that already inherits from the abstract prefab, then only override the root name and the sprite, audio, or other references you actually need.
- For example, explode-family magic should follow the `Assets/Resources/Prefabs/Abstract/AbstractExplode.prefab` pattern by duplicating an existing variant such as `LeafExplode.prefab`.
- Keep the prefab filename in PascalCase, for example `Leafair.prefab` or `FrenzyTotem.prefab`.
- Keep the in-prefab root name aligned with the magic object name expected by the server.
- If a prefab already exists, validate it instead of recreating it.
- When a sprite is swapped in the prefab, also verify the referenced sprite asset exists and has a `.meta` file.

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

## Validation

- Search for the new key across `Assets/Localization` and confirm it appears in the shared asset plus both locale assets.
- Search for the new magic id and name in `LocalCombinedMagicData.cs`.
- Check that the prefab file and `.meta` file both exist.
- Check that the sprite file and `.meta` file both exist.

## Example

For `Leafair`, the client-side work is:

- add `new CombinedMagicData(){id = 24, magicName = "Leafair", recipe = new () { CardType.Drop, CardType.Nature}, spritePath = "Game/drop/leafair"},`
- add the shared key `leafair`
- add localized text for `leafair` in English and Korean
- ensure `Assets/Resources/Prefabs/Leafair.prefab` exists
- ensure `Assets/Resources/Game/drop/leafair.png` exists

For current asset patterns and a concrete example, read `references/client-magic-patterns.md`.
