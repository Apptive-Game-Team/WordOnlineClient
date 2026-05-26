# Client Magic Patterns

Use this reference when you need exact client file examples for a new magic.

## Relevant files

- `Assets/Scripts/Data/Magic/LocalCombinedMagicData.cs`
- `Assets/Localization/Magic Shared Data.asset`
- `Assets/Localization/Magic_en.asset`
- `Assets/Localization/Magic_ko-KR.asset`
- `Assets/Resources/Prefabs/*.prefab`
- `Assets/Resources/Game/*/*.png`

## Current server-derived data pattern

Current client data does not maintain a local magic recipe list. `LocalCombinedMagicData` builds display data from server-provided magic records and returns an empty list when no cached server payload is available:

- `id`, server name, and recipe cards come from the server response.
- `localizationKey` is derived with `StringUtils.ToCamelCase(serverRecipe.Name)`.
- `resourceName` is derived with `StringUtils.ToPascalCase(serverRecipe.Name)`.
- `CombinedMagicData.GetSprite()` loads `Assets/Resources/Game/sprites/{resourceName}.png`.

For a new server magic named `Fire Lord Spirit`, the client-side localization/icon pattern is:

- `Magic Shared Data.asset` contains the key `fireLordSpirit`.
- `Magic_en.asset` contains `Fire Lord Spirit`.
- `Magic_ko-KR.asset` contains the Korean display text.
- `Assets/Resources/Game/sprites/FireLordSpirit.png` exists.

Do not add a fake local `CombinedMagicData` entry, ask for a server id, or edit `LocalCombinedMagicData.cs` for ordinary new-magic client work.

Sprite canvas sizes are tiered by unit size:

- small unit: `128x128`
- middle unit: `192x192`
- big unit: `256x256`

Trim transparent padding before final sizing. Then keep the canvas square and transparent. Preserve the subject's aspect ratio and add transparent padding instead of stretching the art.

Default character sprite style: right-facing, simple flat cartoon, not flashy, no outer/dark contour line, and no extra effects, particles, aura, environment, ground, shadow, or other surrounding description.

## Nearby prefab examples

- `Assets/Resources/Prefabs/FrenzyTotem.prefab`
- `Assets/Resources/Prefabs/RainCloud.prefab`
- `Assets/Resources/Prefabs/LightningDrop.prefab`
- `Assets/Resources/Prefabs/WillOWisp.prefab`

Use the closest family match when duplicating a prefab:

- Drop magic with persistent object: start from `FrenzyTotem.prefab`, `RainCloud.prefab`, or `LightningDrop.prefab`
- Projectile-like magic: start from a nearby shot prefab such as `WillOWisp.prefab`

## Notes

- The gameplay icon path comes from `Resources.Load<Sprite>($"Game/sprites/{resourceName}")`.
- Runtime world objects load from `Resources/Prefabs` through `ObjectSpawner`.
- The current working tree may include unrelated changes such as `Assets/Scripts/Data/ServerList.cs`; do not fold those into a magic task unless the user asks for it.
