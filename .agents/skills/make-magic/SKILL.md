---
name: make-magic
description: Add or scaffold a new magic in this Unity client when the user asks to create a magic, add a magic, or wire a magic into the client. Use for requests that need Magic localization text and, only when still present, LocalCombinedMagicData fallback entries. Use `make-prefab` separately when the request also includes runtime prefabs or icon sprites.
---

# Make Magic

Use this skill when adding a new magic to `word-online/dev/client`.

## Scope

This skill covers the client-side localization and optional local fallback data work:

- inspect `Assets/Scripts/Data/Magic/LocalCombinedMagicData.cs` to determine whether local fallback entries still exist
- add the localization key to `Assets/Localization/Magic Shared Data.asset`
- add localized text to `Assets/Localization/Magic_en.asset`
- add localized text to `Assets/Localization/Magic_ko-KR.asset`

If the request includes prefab or sprite asset work, use `make-prefab` alongside this skill.
If the request includes server work, only handle the client portion here.

## Workflow

1. Inspect a nearby magic of the same family first.
2. Inspect `LocalCombinedMagicData.cs` before editing:
   - If it builds `CombinedMagicData` from server-provided `IMagicRecipeSource` records, do not add a local recipe entry or invent a server magic id.
   - If it still contains a local fallback list, update that list using the existing fields and nearby entry pattern.
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

- Current pattern: `LocalCombinedMagicData` builds entries from server records. `id`, `serverName`, and recipe cards come from the server.
- Current sprite lookup uses `CombinedMagicData.resourceName`, derived from the server name with PascalCase, under `Assets/Resources/Game/sprites`.
- Current localization lookup uses `CombinedMagicData.localizationKey`, derived from the server name with lower camel case.
- Do not ask for or invent a server magic id when the task only needs client localization/sprite wiring.
- If a future version restores local fallback entries, then any local `id` must match the authoritative server magic id and any local resource field must match the actual `Resources` asset path.

## Validation

- Search for the new key across `Assets/Localization` and confirm it appears in the shared asset plus both locale assets.
- If `LocalCombinedMagicData.cs` was edited, search for the new magic id/name there. If it is server-derived and unedited, confirm the expected server-derived localization key and PascalCase sprite filename instead.
- If the request included asset work, confirm the paired `make-prefab` task completed with matching names and paths.

## Example

For a current server-derived magic such as `Fire Lord Spirit`, the client-side work is:

- do not edit `LocalCombinedMagicData.cs` if it derives `localizationKey = StringUtils.ToCamelCase(serverRecipe.Name)` and `resourceName = StringUtils.ToPascalCase(serverRecipe.Name)`
- add the shared key `fireLordSpirit`
- add localized text for `fireLordSpirit` in English and Korean
- if needed, use `make-prefab` to ensure matching assets such as `Assets/Resources/Prefabs/FireLordSpirit.prefab` and `Assets/Resources/Game/sprites/FireLordSpirit.png` exist

For current asset patterns and a concrete example, read `references/client-magic-patterns.md`.
