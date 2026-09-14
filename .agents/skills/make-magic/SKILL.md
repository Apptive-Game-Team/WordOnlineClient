---
name: make-magic
description: Add or scaffold a new magic in this Unity client when the user asks to create a magic, add a magic, or wire a magic into the client. Use for requests that need Magic localization text and generated magic icon/sprite art for the current server-derived magic data flow. Use `make-prefab` separately when the request also includes runtime prefabs or prefab wiring.
---

# Make Magic

Use this skill when adding a new magic to `word-online/dev/client`.

## Scope

This skill covers the client-side localization and sprite art work for server-derived magic data:

- add the name key to `Assets/Localization/Magic Shared Data.asset`
- add the name text to `Assets/Localization/Magic_en.asset`
- add the name text to `Assets/Localization/Magic_ko-KR.asset`
- add the description key to `Assets/Localization/MagicBook Shared Data.asset`
- add the description text to `Assets/Localization/MagicBook_en.asset`
- add the description text to `Assets/Localization/MagicBook_ko-KR.asset`
- when the request needs a magic icon/sprite image, delegate art direction,
  generation, approval, and validation to `make-game-art`

All six tables, not three. A magic with a name and no description shows an empty
magic-book entry, and one with neither shows its raw key.

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
5. Add the magic-book description the same way across the three `MagicBook` tables.
6. If an icon/sprite image is needed, make the image before prefab validation:
   - Read and follow `.agents/skills/make-game-art/SKILL.md`.
   - Let `make-game-art` choose the selected master-style reference, faction
     language, generation workflow, approval gate, and validation.
   - Do not reconstruct or override art rules inside this skill.
   - Save the sprite as `Assets/Resources/Game/sprites/<PascalCaseServerName>.png` so it matches `CombinedMagicData.resourceName`.
   - If the image is created outside Unity, ensure the `.png` exists and add or preserve the `.meta` file through normal Unity import when possible.
7. If a runtime prefab is needed, hand prefab wiring to `make-prefab` and keep the prefab name aligned with the sprite/resource name.
8. Ignore unrelated working-tree changes unless the user explicitly asks to include them.

## File Patterns

- Name keys and text:
  `Assets/Localization/Magic Shared Data.asset`
  `Assets/Localization/Magic_en.asset`
  `Assets/Localization/Magic_ko-KR.asset`
- Magic-book description keys and text:
  `Assets/Localization/MagicBook Shared Data.asset`
  `Assets/Localization/MagicBook_en.asset`
  `Assets/Localization/MagicBook_ko-KR.asset`
- Magic icon sprite:
  `Assets/Resources/Game/sprites/<PascalCaseServerName>.png`

## Localization Rules

- Reuse the same `m_Id` for the shared key row and both locale rows.
- **The two table families use different key spellings.** `Magic Shared Data`
  keys are lower camel, for example `wallGolem`, because `CombinedMagicData`
  derives `localizationKey` from the server name that way. `MagicBook Shared
  Data` keys are the server name unchanged, for example `wall_golem`:
  `MagicInfo.GetMagicBookKeyCandidates` tries `textLocalizationKey`, then
  `serverName`, then the snake-cased key, then the camel key.
- **Nothing fails loudly when a row is missing.** `MagicInfo.SetData` and
  `DeckInfoMagicPopup` fall back to the key itself, so an untranslated magic
  ships the string `wallGolem` to the player. Read the tables to check; do not
  rely on an error.
- Pick the next `m_Id` by continuing the pattern the table already uses. The two
  families number differently — `Magic Shared Data` runs in wide steps such as
  `78335500000000000`, `MagicBook Shared Data` runs `1000001` upward for magic
  descriptions.
- After editing, confirm every `m_Id` in a shared table appears in both of its
  locale tables and that no row is left in one file only.
- Write descriptions from behavior confirmed in the server component, not from
  the magic's name. `repair_totem` freezes allied buildings' decay rather than
  healing them, and a description saying "heals" would be wrong.
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

- Search for the new name key across `Assets/Localization` and confirm it
  appears in `Magic Shared Data` plus both `Magic` locale assets, and the
  description key in `MagicBook Shared Data` plus both `MagicBook` locale
  assets. Six rows for one magic.
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
