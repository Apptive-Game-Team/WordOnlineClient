# CloudDragon frame pair (issue #694)

Redraws `Assets/Resources/Game/sprites/CloudDragon.png` (base pose, no water)
and `Assets/Resources/Game/sprites/CloudDragonAttacking.png` (water-spray
attack pose) to the master cut-paper style, replacing the legacy painted-cloud
art. References used: `.art/anchors/master-v2/MasterStyleKey.png` (rendering
technique) and the existing `ThunderSpirit.png` / `WindSpirit.png` sprites
(how the Spirits faction's faceted wisp/tuft language reads in production).
Per the make-game-art skill and this issue's own instructions, the twin-panel
technique (base and attack drawn side by side in one canvas) was avoided —
earlier sessions on this asset failed with it more than ten times, the
generator repeatedly shrinking the body to make room for both poses.

## Base pose — `CloudDragon-base-v1-*`

Generated in a parallel worktree (`client-art-frames`, working the `RockTurret`
and `TreeGolem` pairs) as a single square magenta-key generation, one subject,
no water. Read-only reused here rather than spending a fresh `image_gen` call:
the cutout was clean (all four corners alpha 0, zero magenta residue, sane
transparent share) and the style matched `ThunderSpirit`/`WindSpirit` well —
faceted cloud-tuft clusters, three value bands, upper-left light, simple oval
eye. `-raw.png` is the untouched magenta generation; `-cut.png` is after
`key-out-background.py --key magenta`.

Finalized to production geometry with:

```
git show origin/main:Assets/Resources/Game/sprites/CloudDragon.png > old-base.png
python3 embed.py CloudDragon-base-v1-cut.png base-embedded.png 256 182   # ad hoc helper, not committed
python3 .art/tools/fit-to-original.py old-base.png base-embedded.png Assets/Resources/Game/sprites/CloudDragon.png
```

`fit-to-original.py` requires both PNGs at the same canvas size, so
`embed.py` (a small scratchpad helper, not part of the repo's tool set) first
trims the candidate to its alpha bbox and thumbnails it into an empty 256x182
canvas without distorting its aspect ratio; `fit-to-original.py` then does the
real scale/position fit against the old file's content bbox and bottom gap.
Landed at scale 1.00 — the candidate's own proportions already matched the old
file's fill almost exactly. `check-replacement.py origin/main` reports 0
problems.

## Attack pose — `CloudDragon-attack-v1-*` (rejected) and `-v2-*` (shipped)

`CloudDragon-attack-v1-*` is the parallel worktree's matching attack
generation, also read-only reused at first. Its water-spray moment itself was
correct (water actively leaving the mouth with separated droplets, not a
gathering/preparing pose — the failure mode called out for `TreeGolem`) but
its raw dragon body was drawn smaller relative to its own canvas than the base
generation's body (dragon-only content height 816px of a 1254 canvas, vs the
base candidate's 1034px of the same size canvas — about 27% smaller). Fitting
each frame independently to its own old file (as `fit-to-original.py` is
built to do) passed `check-replacement.py` for both files individually, since
each only compares against its own old version, but `check-frame-pair.py`
caught the real problem: fitting the attack frame to fill its own old canvas
the same way rescaled the body inconsistently with the already-finalized base
frame, producing a body that would visibly jump size and position on the
attack/idle swap. Not used.

Regenerated instead as `CloudDragon-attack-v2-raw.png`: `codex exec` with the
*finalized* base sprite (`Assets/Resources/Game/sprites/CloudDragon.png`,
already fit to old geometry) attached as the primary reference and an explicit
instruction to keep body shape, size, and position identical and only open the
mouth and add a short water burst — see
`.art/concept/frame-pair-prompts/cloud-dragon-attack-v1.txt`. The returned
body matches the base reference closely (confirmed by overlaying the two
finalized frames: wing, horn, tail, and spine spikes overlap almost exactly).

The one problem: the generated water jet plus its two droplets extended about
317px in the raw canvas measured from the mouth, while the fixed 256px-wide
production canvas only has ~24px of margin to the right of the body once the
body is placed at the correct scale and at the same horizontal position as the
base frame (the body itself uses ~208 of the 256px). Placing the raw water at
full body-consistent scale would have clipped both droplets and part of the
main stream — tried and rejected, see `CloudDragon-attack-v2-cut.png` for the
unscaled cutout this was measured from. Rather than spend a third `image_gen`
call, `CloudDragon-attack-v2-composited-final.png` was built by compositing at
a fixed body scale: the dragon body is cropped from `-v2-cut.png` and scaled
so its content height fills the 182px canvas height exactly (matching the base
frame's own fill), and the water region (jet + both droplets, cropped as one
piece to keep their relative spacing) is scaled down by an extra factor
(~0.40) so the whole effect fits in the remaining ~24px next to the mouth,
then pasted flush against it. This keeps the body pixel-for-pixel at the
correct scale and position and keeps both droplets visible, at the cost of a
smaller-looking water burst than the raw generation intended.

Finalized file is `Assets/Resources/Game/sprites/CloudDragonAttacking.png`
directly (the composite already targets the 256x182 production canvas; no
`fit-to-original.py` pass needed on top of it).

## Validation

- `python3 .art/tools/check-replacement.py origin/main` — 2 checked, 0
  problems (canvas size, size vs old, bottom gap, facing all hold for both
  files).
- `python3 .art/tools/check-frame-pair.py Assets/Resources/Game/sprites/CloudDragon.png Assets/Resources/Game/sprites/CloudDragonAttacking.png`
  — vertical difference `+0.000` unit, horizontal difference `+0.013` unit,
  both inside the 0.03 / 0.05 unit limits. Pass.
- Corner alpha, transparent share, and magenta-residue checks (per the
  make-game-art skill's cutout section) pass on both finalized files: all four
  corners alpha 0, 0 magenta-toned opaque pixels on either file.
- A 50%-alpha overlay of the two finalized frames (kept only in the session
  scratchpad, not committed) confirmed the body — wing, horn, tail, spine
  spikes — sits in the same place in both frames; only the mouth/water region
  differs.

## Not used

- `.art/concept/frame-pair-prompts/cloud-dragon-idle-v1.txt` — this session's
  own first base-pose attempt, generated before the parallel worktree's
  already-finished candidate was found. Also a reasonable match to the master
  style, but not needed once the existing candidate was confirmed usable; kept
  for the record, not finalized.
- `.art/concept/frame-pairs/CloudDragon-attack-v1-raw.png` /
  `-cut.png` — see rejection above (body-scale mismatch against the base
  frame).

`cloud.png` (the dedicated spherical water aura) is unrelated to this pair and
was not touched.
