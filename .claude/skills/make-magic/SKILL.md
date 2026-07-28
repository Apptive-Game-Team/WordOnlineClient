---
name: make-magic
description: Add or scaffold a new magic in this Unity client when the user asks to create a magic, add a magic, or wire a magic into the client. Use for requests that need Magic localization text and generated magic icon/sprite art for the current server-derived magic data flow. Use `make-prefab` separately when the request also includes runtime prefabs or prefab wiring.
---

# Make Magic

Use this skill when adding a new magic to `word-online/dev/client`.

## Scope

This skill covers the client-side localization and sprite art work for server-derived magic data:

- add the localization key to `Assets/Localization/Magic Shared Data.asset`
- add localized text to `Assets/Localization/Magic_en.asset`
- add localized text to `Assets/Localization/Magic_ko-KR.asset`
- when the request needs a magic icon/sprite image, delegate art direction,
  generation, approval, and validation to `make-game-art`

If the request includes prefab wiring, use `make-prefab` alongside this skill.
If the request includes server work, only handle the client portion here.

## Workflow

1. Inspect a nearby magic of the same family first.
2. Treat magic recipe/display data as server-derived:
   - Do not add local recipe entries.
   - Do not invent a server magic id.
   - Do not edit `LocalCombinedMagicData.cs` for ordinary new-magic client work; it currently returns an empty list when no server payload is cached.
3. Add a new localization key tuple to `Magic Shared Data.asset`.
4. Add matching English and Korean localized values using the same `m_Id` in both locale assets.
5. If an icon/sprite image is needed, make the image before prefab validation:
   - Read and follow `.agents/skills/make-game-art/SKILL.md`.
   - Let `make-game-art` choose the selected master-style reference, faction
     language, generation workflow, approval gate, and validation.
   - Do not reconstruct or override art rules inside this skill.
   - Save the sprite as `Assets/Resources/Game/sprites/<PascalCaseServerName>.png` so it matches `CombinedMagicData.resourceName`.
   - If the image is created outside Unity, ensure the `.png` exists and add or preserve the `.meta` file through normal Unity import when possible.
6. If a runtime prefab is needed, hand prefab wiring to `make-prefab` and keep the prefab name aligned with the sprite/resource name.
7. Ignore unrelated working-tree changes unless the user explicitly asks to include them.

## File Patterns

- Localization shared keys:
  `Assets/Localization/Magic Shared Data.asset`
- Localization text:
  `Assets/Localization/Magic_en.asset`
  `Assets/Localization/Magic_ko-KR.asset`
- Magic icon sprite:
  `Assets/Resources/Game/sprites/<PascalCaseServerName>.png`

## Localization Rules

- Reuse the same `m_Id` for the shared key row and both locale rows.
- The shared asset stores the key in lower camel or server bean style, for example `leafair` or `frenzyTotem`.
- `Magic_en.asset` may temporarily use the raw key as fallback text if no final English copy is provided, but prefer the real display name.
- Keep `Magic_ko-KR.asset` in escaped YAML string form when Unity serializes it that way.

## Data Rules

- Current pattern: `LocalCombinedMagicData` builds entries from server records. `id`, `serverName`, and recipe cards come from the server.
- There is no local fallback magic recipe list. If the server payload is unavailable, `LocalCombinedMagicData.GetEffectiveDataList()` returns an empty list.
- Current sprite lookup uses `CombinedMagicData.resourceName`, derived from the server name with PascalCase, under `Assets/Resources/Game/sprites`.
- Current localization lookup uses `CombinedMagicData.localizationKey`, derived from the server name with lower camel case.
- Do not ask for or invent a server magic id when the task only needs client localization/sprite wiring.
- Do not edit `Assets/Scripts/Data/Magic/LocalCombinedMagicData.cs` unless the user explicitly asks to change the data-loading behavior itself.

## Image Rules

`make-game-art` is the only source for image rules. It owns:

- master-style and faction reference selection;
- concept versus production classification;
- style-change approval and documentation;
- generation and post-processing;
- comparison Site and contact-sheet validation;
- promotion into `.art/anchors/` and `Assets/`.

`make-magic` owns localization and server-derived naming only.

## Validation

- Search for the new key across `Assets/Localization` and confirm it appears in the shared asset plus both locale assets.
- Confirm the expected server-derived localization key and PascalCase sprite filename. `LocalCombinedMagicData.cs` should normally remain unedited for a new magic.
- If an image was created, confirm `make-game-art` validation passed and the
  approved sprite exists at `Assets/Resources/Game/sprites/<PascalCaseServerName>.png`.
- If the request included prefab work, confirm the paired `make-prefab` task completed with matching names and paths.

## Example

For a current server-derived magic such as `Fire Lord Spirit`, the client-side work is:

- do not edit `LocalCombinedMagicData.cs`; recipe data comes from server-provided magic records and there is no local fallback list
- add the shared key `fireLordSpirit`
- add localized text for `fireLordSpirit` in English and Korean
- if an icon is needed, use `make-game-art` to generate and approve a
  style-locked transparent PNG, then save it as
  `Assets/Resources/Game/sprites/FireLordSpirit.png`
- if needed, use `make-prefab` to ensure matching runtime assets such as `Assets/Resources/Prefabs/FireLordSpirit.prefab` exist

For current asset patterns and a concrete example, read `references/client-magic-patterns.md`.
