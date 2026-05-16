---
name: unity-ui-prefabs
description: Use when creating or modifying Unity UI scenes, pages, panels, buttons, list items, or HUD/overlay UI in this client. Enforces inspecting existing scenes first and using Assets/Prefabs/UI prefabs or creating reusable UI prefabs instead of hand-building one-off UI.
---

# Unity UI Prefabs

Use this skill for UI work in `word-online/dev/client`.

## Rule

When making UI, start from existing UI prefabs and scene patterns. Do not hand-build buttons, panels, cards, or common UI containers in code or raw scene YAML unless the user explicitly asks for that approach.

Use the default project background for new UI scenes/pages. In this client, that means the scene-authored Canvas child named `Background` used by Login, Register, and Lobby scenes: an `Image` using `Assets/Art/Images/Background/background.png` with preserve aspect enabled. Do not replace it with custom camera colors, ad hoc full-screen images, or new background panels unless the user explicitly asks for a distinct background.

## Workflow

1. Inspect nearby scenes that already solve a similar UI problem.
   - Search `Assets/Scenes/*.unity` for existing `m_SourcePrefab` usage.
   - Check how the scene overrides prefab positions, text, scripts, and serialized references.
2. Inspect reusable UI prefabs under `Assets/Prefabs/UI`.
3. Choose the closest existing prefab or variant:
   - Generic panel/container: `Assets/Prefabs/UI/UI-Base.prefab`
   - Brown panel/container: `Assets/Prefabs/UI/Brown-UI-Base.prefab`
   - Standard button: `Assets/Prefabs/UI/Button Variant.prefab`
   - Debug item button: `Assets/Prefabs/UI/Debug/DebugItemButton Variant.prefab`
   - Adventure button: `Assets/Prefabs/UI/Adventures/AdventureButton Variant.prefab`
   - Stage/scenario tiles: `Assets/Prefabs/UI/Adventures/StagePanel/*.prefab`
   - Reward popup: `Assets/Prefabs/UI/RewardUI.prefab`
4. Use the same default background approach as nearby scenes.
   - For the current login/register/lobby pattern, add a `Background` Image as the first Canvas child.
   - Use sprite guid `4fe2d02ca2c9b49fc938a577493218e1` (`Assets/Art/Images/Background/background.png`), size `{x: 800.01, y: 605.9091}`, center anchors, and `m_PreserveAspect: 1`.
5. Build scene UI as prefab instances with overrides, not independent recreated objects.
6. If no prefab matches and the element is reusable, create a new prefab under `Assets/Prefabs/UI/<Domain>/` or `Assets/Prefabs/UI/`.
7. For repeated list rows/cards/items, create a prefab and populate it from code with a factory/controller. Do not duplicate one-off rows in the scene.
8. Keep behavior in scripts and layout/visual structure in scene-authored prefabs.
9. Ignore unrelated dirty files unless the user explicitly asks to include them.

## Scene Wiring

- Scene buttons should be scene-authored prefab instances.
- Wire button `OnClick` to an existing `ButtonBase` subclass or scene controller method.
- Do not create UI buttons from `Awake`, `Start`, or other runtime code unless the UI is truly dynamic and repeated.
- If a page deserves its own scene, add the scene to `ProjectSettings/EditorBuildSettings.asset` and navigate with `SceneManager.LoadScene`.

## Prefab Creation

Create a new prefab when:

- the same UI structure appears in more than one place
- a list/table/grid needs repeated item views
- the UI component has its own controller script
- a future scene is likely to reuse the same panel/button/card layout

Prefer duplicating the nearest existing prefab and making minimal overrides. Keep root names, prefab filenames, and controller names aligned.

## Validation

- Confirm each new scene UI element that should be reusable is backed by `m_SourcePrefab`.
- Confirm new prefabs and `.meta` files exist.
- Run a scoped `git diff --check` on changed UI scene, prefab, script, and build settings files.
- If Unity is available, run an Editor import/compile check or open the scene in Unity.

## Current Patterns

Existing scenes heavily instantiate:

- `Button Variant.prefab` for normal buttons
- `UI-Base.prefab` / `Brown-UI-Base.prefab` for framed UI surfaces
- specialized variants under `Assets/Prefabs/UI/Adventures` and `Assets/Prefabs/UI/Debug`

Follow those patterns before inventing new UI structure.
