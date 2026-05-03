# Client Magic Patterns

Use this reference when you need exact client file examples for a new magic.

## Relevant files

- `Assets/Scripts/Data/Magic/LocalCombinedMagicData.cs`
- `Assets/Localization/Magic Shared Data.asset`
- `Assets/Localization/Magic_en.asset`
- `Assets/Localization/Magic_ko-KR.asset`
- `Assets/Resources/Prefabs/*.prefab`
- `Assets/Resources/Game/*/*.png`

## Leafair example

Current client data already shows the expected pattern:

- `LocalCombinedMagicData` contains:
  `id = 24`
  `magicName = "Leafair"`
  `recipe = new () { CardType.Drop, CardType.Nature }`
  `spritePath = "Game/drop/leafair"`
- `Magic Shared Data.asset` contains the key `leafair`
- `Magic_en.asset` contains `leafair`
- `Magic_ko-KR.asset` contains the Korean text `술이파리`
- `Assets/Resources/Prefabs/Leafair.prefab` exists
- `Assets/Resources/Game/drop/leafair.png` exists

## Nearby prefab examples

- `Assets/Resources/Prefabs/FrenzyTotem.prefab`
- `Assets/Resources/Prefabs/RainCloud.prefab`
- `Assets/Resources/Prefabs/LightningDrop.prefab`
- `Assets/Resources/Prefabs/WillOWisp.prefab`

Use the closest family match when duplicating a prefab:

- Drop magic with persistent object: start from `FrenzyTotem.prefab`, `RainCloud.prefab`, or `LightningDrop.prefab`
- Projectile-like magic: start from a nearby shot prefab such as `WillOWisp.prefab`

## Notes

- The gameplay icon path in `LocalCombinedMagicData` comes from `Resources.Load<Sprite>(spritePath)`.
- Runtime world objects load from `Resources/Prefabs` through `ObjectSpawner`.
- The current working tree may include unrelated changes such as `Assets/Scripts/Data/ServerList.cs`; do not fold those into a magic task unless the user asks for it.
