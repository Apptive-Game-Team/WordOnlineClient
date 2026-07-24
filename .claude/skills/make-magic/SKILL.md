---
name: make-magic
description: Add or scaffold a new magic in this Unity client when the user asks to create a magic, add a magic, or wire a magic into the client. Use for requests that need Magic localization text and generated magic icon/sprite art for the current server-derived magic data flow. Use `make-prefab` separately when the request also includes runtime prefabs or prefab wiring.
---

# Make Magic

Use this skill when adding a new magic to this Unity client.

## Scope

This skill covers the client-side localization and sprite art work for server-derived magic data:

- add the localization key to `Assets/Localization/Magic Shared Data.asset`
- add localized text to `Assets/Localization/Magic_en.asset`
- add localized text to `Assets/Localization/Magic_ko-KR.asset`
- when the request needs a magic icon/sprite image, inspect existing magic images and create a style-matched sprite under `Assets/Resources/Game/sprites`

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
   - Inspect existing magic sprites in `Assets/Resources/Game/sprites` first.
   - Use the closest element/family references, for example fire, water, lightning, wind, rock, leaf/nature, summon, totem, projectile, field, or explosion.
   - Match the existing sprite style: readable small icon silhouette, game-friendly fantasy object/creature/effect, transparent background when appropriate, centered composition, and similar color saturation/edge treatment.
   - Generate or edit the image with the image-generation workflow when a new bitmap is needed; use existing art directly only when the user asks for reuse.
   - Save the sprite as `Assets/Resources/Game/sprites/<PascalCaseServerName>.png` so it matches `CombinedMagicData.resourceName`.
   - Resize by unit tier, then trim transparent padding as the final image step. Trimming is more important than forcing a square canvas.
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

- Always inspect current images before writing a generation prompt. Useful references live in:
  `Assets/Resources/Game/sprites/*.png`
  `Assets/Resources/Game/rune/*.png`
  `Assets/Resources/Game/field/*.png`
  `Assets/Art/Images/Effect/*.png`
- Choose 3-6 nearby reference images by concept and element, then describe those style cues in the image prompt.
- Keep generated art as a sprite/icon asset, not a UI mockup, screenshot, framed card, logo, or text-bearing image.
- Prefer transparent PNG output for object, creature, projectile, and effect sprites. Backgrounds should be transparent unless the nearest existing family uses a filled field/effect texture.
- Keep the subject centered, right-facing, and legible at small sizes. Characters should face toward the right side of the image unless the user explicitly asks for another direction.
- Use a simple flat cartoon style. Avoid flashy rendering, outer/dark contour lines, fine details, text, photorealism, complex scenes, and any description of effects or environment around the character.
- Use unit tier sizes as maximum dimensions, not required square canvases: small max `128x128`, middle max `192x192`, big max `256x256`.
- Preserve aspect ratio when resizing; do not stretch the image.
- Trim transparent padding after resizing, for example `magick input.png -resize 256x256 -trim +repage output.png`.
- Final sprite files should be tightly trimmed around the subject. Do not add transparent padding just to make the file square.
- Typical finalization command:
  `magick input.png -resize 256x256 -trim +repage output.png`
- Use `magick identify` or equivalent inspection to verify dimensions and alpha after generation.

## Validation

- Search for the new key across `Assets/Localization` and confirm it appears in the shared asset plus both locale assets.
- Confirm the expected server-derived localization key and PascalCase sprite filename. `LocalCombinedMagicData.cs` should normally remain unedited for a new magic.
- If an image was created, confirm the sprite exists at `Assets/Resources/Game/sprites/<PascalCaseServerName>.png`, inspect its dimensions/format/alpha, verify it is trimmed rather than padded to a square canvas, and compare it visually against the chosen reference images.
- If the request included prefab work, confirm the paired `make-prefab` task completed with matching names and paths.

## Example

For a current server-derived magic such as `Fire Lord Spirit`, the client-side work is:

- do not edit `LocalCombinedMagicData.cs`; recipe data comes from server-provided magic records and there is no local fallback list
- add the shared key `fireLordSpirit`
- add localized text for `fireLordSpirit` in English and Korean
- if an icon is needed, inspect nearby fire/summon sprites, generate a style-matched transparent PNG, and save it as `Assets/Resources/Game/sprites/FireLordSpirit.png`
- if needed, use `make-prefab` to ensure matching runtime assets such as `Assets/Resources/Prefabs/FireLordSpirit.prefab` exist

For current asset patterns and a concrete example, read `references/client-magic-patterns.md`.
