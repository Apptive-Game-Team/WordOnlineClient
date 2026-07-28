---
name: make-game-art
description: Create, edit, review, or standardize WordOnline visual assets while keeping one project-wide art style. Use for character, creature, faction, building, environment, background, World Tree, VFX, icon, sprite, concept-art, style-guide, anchor, or art-direction work, including any request that creates a new style-defining reference or changes the canonical visual style.
---

# Make Game Art

Create game art from one shared rendering system. Separate rendering technique
from faction shape language. Never let each faction drift into a different
drawing style.

## Sources of truth

Read these before generating or editing art:

1. `.art/WORLD.md` — confirmed lore; do not invent missing canon.
2. `.art/STYLE.md` — canonical rendering technique, faction language, palette,
   and current master-style decision.
3. `.art/ANCHORS.md` — frozen production anchors and exclusions.
4. `.art/CONCEPT-BRIEF.md` — active explorations and comparison Site.

Precedence:

1. User's explicit decision in the current task.
2. Selected master-style anchor recorded in `.art/STYLE.md`.
3. Faction-specific shape and palette rules.
4. Legacy art, used for subject identity only.

Once a master style is selected, do not use legacy faction images as rendering
references. They may describe *what* exists, never *how* to draw it.

## Classify the task

### Production art

Use when adding or revising an asset without changing the art direction.

- Load the selected master-style image as the primary image reference.
- Apply the target faction's shape language and palette from `.art/STYLE.md`.
- Keep camera, lighting, proportions, edge treatment, detail density, facial
  grammar, and shadow treatment identical to the master.
- Generate concepts outside `Assets/`.
- Move only approved, game-ready assets into Unity paths.

### Style-defining art

Use when an image could become a new reference for future assets, or when the
user asks to change drawing style, rendering, proportions, lighting, palette
system, camera, facial grammar, or material treatment.

1. Treat the result as a proposal, not a canonical anchor.
2. Keep subjects and composition fixed; generate 2–4 style candidates that vary
   only the questioned rendering axis.
3. Save candidates under `.art/concept/`.
4. Publish them to the comparison Site recorded in `.art/CONCEPT-BRIEF.md`.
5. Stop canonicalization until the user selects or explicitly approves one.
6. After approval:
   - record the winning file and reason in `.art/STYLE.md`;
   - update shared rendering rules to describe the visible result;
   - promote a frozen copy to `.art/anchors/`;
   - update `.art/ANCHORS.md` and `.art/CONCEPT-BRIEF.md`;
   - identify which existing assets now violate the new standard;
   - regenerate downstream faction boards using only the new master reference.

Never silently change style because one generated output looks better.

### Concept exploration

Use for lore, silhouettes, roles, faction lineups, or composition exploration.
Concept boards help decide *what* to draw. They are not production anchors.
Do not feed a multi-subject board or scene directly into sprite generation
unless it is explicitly the selected master-style key.

## Generation workflow

1. Confirm asset role: production, style-defining, or exploration.
2. Confirm subject, faction, game use, tier, and output path.
3. Read the sources of truth.
4. Inspect the selected master reference.
5. Build a prompt that explicitly locks:
   - rendering technique;
   - camera and facing;
   - proportions and silhouette;
   - shared light and shadow;
   - edge and detail density;
   - faction shape language and palette;
   - required exclusions.
6. Use the `imagegen` skill for bitmap generation or editing.
7. Save exploratory output under `.art/concept/`.
8. Compare all candidates at thumbnail size through the project Site and
   `./.art/make-sheets.sh`.
9. Use the `magick` skill for resize, trim, alpha, format, and dimension checks.
10. Move an asset into `Assets/` only after approval.

## Production sprite rules

- Single subject; centered; right-facing unless gameplay needs otherwise.
- Transparent background.
- No text, frame, card mockup, scene, or watermark.
- Preserve aspect ratio and trim transparent padding.
- Maximum size: small `128x128`, middle `192x192`, big `256x256`.
- Silhouette must survive at `64px`.
- Keep the server-derived filename unchanged when `resourceName` depends on it.
- Preserve or create Unity `.meta` through normal import.

Typical finalization:

```bash
magick input.png -resize 256x256 -trim +repage output.png
magick identify output.png
```

## Validation

- Compare the new asset beside the master-style key and at least one asset from
  every affected faction, not only its nearest relative.
- Reject visible changes in renderer, camera, eye style, edge softness,
  highlight shape, shadow shape, or detail density.
- Run `./.art/make-sheets.sh`.
- Verify dimensions, alpha, trim, filename, and Unity path.
- Keep rejected generations out of `.art/anchors/` and `Assets/`.
- Record any approved style change before generating dependent assets.

## Boundaries

- Do not edit `.art/anchors/` in place. Promote a new frozen file deliberately.
- Do not move `.art/` under `Assets/`.
- Do not overwrite shipped art before user approval.
- Do not infer lore absent from `.art/WORLD.md`; mark it exploratory.
- Do not mix unrelated working-tree changes into the art change.
