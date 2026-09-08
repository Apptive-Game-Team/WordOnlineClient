# Hand-Edited Assets, Prefabs and Scenes

The Unity Editor cannot run in this environment, so `.asset`, `.prefab`, `.unity`
and `.meta` files are edited as YAML by hand. Read this before changing a
serialized type or deleting a script. Nothing here fails the build; every trap
below shows up only as a wrong value or a missing script at runtime.

## A serialized enum stores its integer, not its name

`Assets/Art/Images/UI/Card/CardImageMapper.asset` stores rows as
`elementType: 1`, not `elementType: Fire`. Change the declaration order of an
enum that any asset serializes and every existing row silently points at a
different member. Renaming the enum member does nothing; renumbering it changes
the data.

When you change such an enum:

1. grep the enum name and the serialized field name across `Assets/` to find
   every asset, prefab and scene holding a value.
2. Write down the old member for each stored integer, then write the new
   integer for that same member.
3. Rewrite the rows in the asset in the same pass as the enum.

This bit `CardType` → `ElementType`: `CardType.Fire` was `6` and
`ElementType.Fire` is `1`, so leaving the asset alone would have given every
element the wrong icon while still loading and rendering.

## Renaming a method is safer than changing its parameter type

`CardImageMapper.GetCardImage(string)` took a card name. After the redesign the
same call had to take an element name. Keeping the name would have left every
call site compiling and returning `null` at runtime. Renaming it to
`GetElementImage` turned each stale call site into a compile error, which is the
only review signal available when you cannot run the Editor. Prefer the rename
whenever the meaning of an argument changes but its C# type does not.

## Deleting a MonoBehaviour takes three edits, not one

Deleting `Assets/Scripts/Data/Magic/CombinedMagicResolver.cs` required:

1. the `.cs` and its `.cs.meta`,
2. the component entry in every prefab that carries it — both the
   `- component: {fileID: N}` line in the GameObject's `m_Component` list and
   the `--- !u!114 &N MonoBehaviour:` block itself
   (`Assets/Prefabs/MagicResolver.prefab`),
3. the `--- !u!114 &M stripped` block in every scene that instances that prefab,
   plus any `someField: {fileID: M}` line pointing at it
   (`Assets/Scenes/GameScene.unity`).

Find them by grepping the script's `.meta` guid across `Assets/`, then grepping
the local `fileID` each hit declares. Stop after step 1 and the prefab opens in
the Editor with a "missing script" component.

## Writing a new `.meta` by hand

Copy the shape from a sibling of the same importer. A script meta is three
lines: `fileFormatVersion: 2`, `guid: <32 lowercase hex>`, `timeCreated: <int>`.
The guid must be new — grep it across `Assets/`, `Packages/` and
`ProjectSettings/` before committing. Never reuse the guid of the file you are
replacing: scenes and prefabs resolve components by guid, so a reused guid makes
every old reference silently bind to the new type.
