# Issue #377 SFX Event Ownership Matrix

## Rules

- One event has one runtime owner.
- A missing or unknown mapping is silent and logs once.
- Existing prefab/projectile owners remain active until a replacement passes listening.
- The lifecycle profile never duplicates an event already owned by a legacy component.
- Snapshot hydration is silent; only subsequent state transitions can emit lifecycle SFX.

## Global Interaction Ownership

| Interaction | Trigger | Owner | Clip/profile slot | Must remain silent when |
|---|---|---|---|---|
| Standard button click | successful interactable click | `ButtonBase` | UI button anchor | disabled or non-interactable |
| Plain Unity button click | successful click without parent `ButtonBase` | `GlobalButtonSoundPlayer` | same UI button anchor | disabled, opted out, or covered by `ButtonBase` |
| Card hover | pointer enters card after cooldown | `CardUIZoom` | Card hover | repeat enter inside cooldown |
| Card select | local card selection succeeds | `CardUI` / tutorial equivalent | Card select | blocked or invalid selection |
| Card deselect | selected card is released/cancelled | `CardUI` / tutorial equivalent | Card deselect | no prior selection |
| Card draw | new stable card identity enters reconciled hand | hand reconciliation owner, to identify | Card draw | initial hand snapshot or unstable identity |
| Field confirmation | `TrySendInput` returns true | `FieldSelector` | Field confirm | UI overlap, invalid ray, blocked state, cancel, failed send |

## Object and Spell Ownership

| Event | Preferred owner | Fallback/profile behavior | Duplicate guard |
|---|---|---|---|
| Spawn | lifecycle controller after registered initialization | play only enabled profile slot and `playSpawnPresentation=true` | reconnect/snapshot flag suppresses it |
| Movement | lifecycle controller receiving a real position transition | enabled for mobile archetypes only; 450 ms cooldown | buildings/static/transient profiles disable it |
| Attack | existing `OnAttackSoundPlayer` or explicit projectile launcher | profile attack only if no legacy owner exists | validator rejects two attack owners |
| Projectile flight | explicit projectile component | silent by default | lifecycle movement disabled for projectile profiles |
| Projectile impact | projectile collision/impact component | explicit impact profile only | target hit remains independently owned by target |
| Hit | target lifecycle HP decrease | profile hit slot | first HP snapshot is baseline and silent |
| Heal | target lifecycle HP increase | profile heal slot | first HP snapshot is baseline and silent |
| Death | target lifecycle terminal transition | profile death slot, once | one-shot state reset only during Initialize |
| Drop | explicit transient spell component/profile | only events necessary to explain drop | generic spawn/movement/death disabled |
| Explode | explicit transient impact component/profile | impact/explode only | generic death must not repeat it |

## Vertical Slice Contract

The first implementation slice uses one moving creature and one static building
as representative fixtures. Approved slots activate for every runtime row that
shares the corresponding profile; effective ownership is inventoried for the
entire profile before activation. Other profiles remain disabled until the slice
passes isolated listening, combat overlap, reconnect, and WebGL smoke tests.

| Runtime type | Concept | Spawn | Move | Attack | Hit | Heal | Death | Expected owner notes |
|---|---|---|---|---|---|---|---|---|
| `FireSlime` | small fire creature | `FireCreature` | `FireCreature`, cooldown | approved `FireCreature` attack; disable verified inherited `HitSoundPlayer` owner before subscription | `FireCreature` | silent unless game state supports it | `FireCreature` | natural flame/body Foley; never universal wind, chime, or explosion |
| `GroundTower` | static rock/earth structure | `StoneBuilding` | intentional silence | verified inherited `HitSoundPlayer` owner until Stone attack approval | `StoneBuilding` | silent unless game state supports it | `StoneBuilding` | placement/collapse language; no creature motion or generic projectile duplication |

Exact runtime names must be verified against the top-level Resources prefab
snapshot before catalog assets are created. If either proposed name does not
exist, select the nearest existing fire slime and ground tower and update this
table in the same commit.

## Validator Failures

The Editor validator must fail on:

- runtime prefab without catalog row
- catalog row without runtime prefab or documented server alias
- duplicate runtime type
- enabled event without a clip
- movement enabled for a static profile
- two effective attack subscribers on the same runtime prefab; when an approved
  profile attack slot is active, the lifecycle controller disables the legacy
  owner before `Start` and becomes the sole subscriber; when the slot is
  disabled, the lifecycle controller suppresses itself and preserves legacy
  ownership
- lifecycle movement enabled for a projectile/transient profile
- two components claiming the same event
