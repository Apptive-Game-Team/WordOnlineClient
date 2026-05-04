---
name: make-prefab
description: Add or validate a runtime magic prefab and icon sprite in this Unity client when the user asks to create a prefab, add a magic object asset, or wire up `Assets/Resources/Prefabs` and `Assets/Resources/Game` assets for a magic.
---

# Make Prefab

Use this skill when a request needs client-side prefab or sprite asset work for `word-online/dev/client`.

## Scope

This skill covers the client-side asset minimum:

- add or validate the runtime prefab under `Assets/Resources/Prefabs`
- add or validate the magic icon sprite under `Assets/Resources/Game/...`
- keep prefab naming aligned with the object type the server will spawn
- keep sprite asset paths aligned with the `spritePath` used by client display metadata

If the request also needs `LocalCombinedMagicData` or localization updates, use `make-magic` alongside this skill.

## Workflow

1. Inspect a nearby magic of the same family first.
2. If the magic family has a matching abstract base under `Assets/Resources/Prefabs/Abstract`, start from that pattern.
3. Prefer copying a nearby prefab of the same family and editing instance overrides instead of building YAML from scratch.
4. Add or validate the prefab in `Assets/Resources/Prefabs/<Name>.prefab`.
5. Confirm the prefab filename and root object name match what the server-created object type will load through `Resources.Load<GameObject>($"Prefabs/{createdObjectDto.type}")`.
6. Add or validate the icon sprite at the path expected by client metadata, for example `Assets/Resources/Game/<family>/<icon_name>.png`.
7. When a prefab sprite or other asset reference is swapped, verify the referenced asset and `.meta` file both exist.
8. Ignore unrelated working-tree changes unless the user explicitly asks to include them.

## File Patterns

- Runtime prefab:
  `Assets/Resources/Prefabs/<MagicPrefabName>.prefab`
- Optional abstract base:
  `Assets/Resources/Prefabs/Abstract/*.prefab`
- Magic-book / HUD icon:
  `Assets/Resources/Game/<family>/<icon_name>.png`

## Prefab Rules

- Keep the prefab filename in PascalCase, for example `Leafair.prefab` or `FrenzyTotem.prefab`.
- Keep the in-prefab root name aligned with the magic object name expected by the server.
- If a prefab already exists, validate it instead of recreating it.
- For explode-family magic, follow the `Assets/Resources/Prefabs/Abstract/AbstractExplode.prefab` pattern by duplicating an existing concrete variant such as `LeafExplode.prefab`.
- Prefer the smallest possible override set when duplicating an existing prefab.

## Asset Rules

- The gameplay icon path used by client metadata is a `Resources` path without extension, for example `Game/drop/leafair`.
- The icon asset should live at the exact file path implied by that resource path, for example `Assets/Resources/Game/drop/leafair.png`.
- Keep icon naming and prefab naming aligned with the same magic concept to avoid mismatched client data.

## Validation

- Check that the prefab file and `.meta` file both exist.
- Check that the sprite file and `.meta` file both exist.
- Confirm the prefab and sprite names match the paired `make-magic` metadata when both skills are used together.

## Example

For `Leafair`, the asset-side work is:

- ensure `Assets/Resources/Prefabs/Leafair.prefab` exists and uses the expected object name
- ensure `Assets/Resources/Game/drop/leafair.png` exists
- ensure both assets have matching `.meta` files

For current asset patterns and a concrete example, read `../make-magic/references/client-magic-patterns.md`.
