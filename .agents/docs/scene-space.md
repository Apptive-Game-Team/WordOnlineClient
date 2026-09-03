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

### Replacing a hit visual may also require suppressing a projectile

Do not assume that changing `HitEffectController` replaces every visual attached
to an attack. The server can emit a `HitEvent` and a projectile DTO for the same
impact. Storm Stag tier 3/4 charges do exactly this: the hit event selects the
dedicated impact prefab, while the accompanying `ElectricShot` must be suppressed
for that charged source in `ProjectileSpawner`. If only the hit effect is changed,
the old projectile remains visible on top of the new impact.

When replacing an attack visual, inspect the full frame payload path and verify
both `events` and `objects.projectile`. Preserve projectiles with the same type
from unrelated sources; identify the source through a
`ReferenceProjectileTarget` and its active effects before suppressing anything.

`Player.prefab` has no `HitEffectController`. Any hit visual that must also work
against a player must be selected before the controller lookup and spawned from
the target `ServedObject` position (prefer
`GetEdgeWorldPositionTowards`). If the handler returns when the controller is
missing, the effect will work against creatures but silently disappear against
players; this is how the Storm Stag impact regression presented.

### A tween value is not always inside 0 to 1

`Ease.OutBack` peaks at **1.10** and `Ease.InBack` dips to **-0.10**; the Elastic
family is worse. That is the point of those curves, and it is harmless while the
value drives a scale or a colour. It is not harmless when the value is
multiplied by a distance: a 10% overshoot threw the stretching arm's hand a tenth
of its reach past its target, and because the arm runs downhill from the shoulder
to the enemy's body, past the target also meant into the floor.

Clamp with `Mathf.Clamp01` before using a tween value as a fraction of a
distance, and express any deliberate reach past the target in world units so it
does not scale with how far away the target happens to be.

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
