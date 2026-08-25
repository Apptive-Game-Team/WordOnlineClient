# Scene Space and Animation

How the game scene is arranged, and the two mistakes that arrangement invites.
Read this before writing gameplay code that positions anything in the world or
animates a `ServedObject`.

## The camera is tilted, and sprites are billboarded to it

`Assets/Scenes/GameScene.unity` holds a **perspective** camera at `(9, 21, -16)`,
rotated 45° about X, field of view 25. The ground is the world XZ plane and world
`+Y` is height, matching the server's `Vector3`. `PositionUpdater` assigns
`UpdatedObjectDto.position` straight to `transform.position` — there is no axis
swap anywhere in the client.

Sprites are then **billboarded to that tilted camera**. A sprite's own up axis is
the camera's up axis, not world up.

### Offsets measured off a sprite use screen-up, never `Vector3.up`

A height read off a sprite — a shoulder, a muzzle, an anchor point — is a
distance along the camera's up vector. Applying it along `Vector3.up` raises the
point by only its cosine on screen, about 71% at this tilt, and spends the rest
moving the point toward the camera in depth. The error also changes with the
direction the object faces, so it reads as an offset that drifts.

Use the existing helpers rather than reaching for `Vector3.up`:

- `ProjectileUtil.GetScreenUp()` — screen-up in world space.
- `ServedObject.GetAnchorUpDirection()` — the same thing, for anchors.
- `ServedObject.GetEdgeWorldPositionTowards(from, edgeBias)` — a point on a
  sprite facing something else. Pass `0f` for the sprite centre. Prefer this over
  a hand-tuned height whenever the target's size varies, so the same code aims at
  the body of a slime and of a golem.

`Vector3.up` is still correct for anything measured in world terms, such as a
server-sent height or a physics jump. The rule is about distances read off a
billboarded sprite.

### Distance between two points uses the camera plane, not the world

Anything drawn as a sprite spanning two world points — a beam, a stretching arm,
a tether — lies in the camera plane. `Vector3.Distance` is the wrong length for
it: a delta along world `+Z` is foreshortened while a delta along world `+X` is
not, so reach appears to change with facing, and the perspective camera adds a
depth error on top.

Use `ProjectileUtil.GetCameraPlaneLength(start, end)`. It round-trips the end
point through screen space at the start point's depth, which is exact for both
projection and perspective, and is consistent with `ProjectileUtil.GetRotation`
by construction because both consume the same two `WorldToScreenPoint` results.

## There is no Animator in this project

There are no `.controller` or `.anim` assets and no `Animator` reference in
`Assets/Scripts`. Do not plan animation states, parameters or clips. Animation is
three mechanisms:

- **Sprite swap.** `AttackSpriteSwapController` swaps `SpriteRenderer.sprite` for
  a duration on the `OnAttack` event. `OnAttackSpriteSwapper` is the older
  equivalent still used by `TreeGolem` and `RockGolem`; prefer the former.
  `SpriteFrameAnimator` loops a `Sprite[]` on a plain timer.
- **DOTween.** `DOTweenAction` holds the shared motions; the
  `ServedObjectComponent/Motion` controllers start an idle tween in `Awake`.
- **Spawned effect prefabs**, for hits, deaths and spawns.

Server `Status` arrives as a raw string and `ServedObject.HandleStatus` routes
only `Destroyed`, `Attack` and `Damaged`. Everything else, including `Idle` and
`Move`, falls through to `OnOtherStatus`, which only `PlayerActionController`
subscribes to. A creature cannot react to `Idle` or `Move` through status.

## Verifying without the Editor

The Editor cannot open this checkout from WSL, so gameplay geometry usually
cannot be run before review. When that is the case, verify the maths against the
real camera values above — read them out of `GameScene.unity` rather than
assuming — and say in the pull request which cases were computed and which still
need an Editor pass. For anything aimed across the field, check all four
headings: `+X`, `-X`, `+Z`, `-Z`. The `+Z` and `-Z` cases are where a world-space
length or offset fails, and where a bug hides if only `+X` was tried.
