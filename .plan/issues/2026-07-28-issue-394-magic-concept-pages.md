# 2026-07-28 — Magic concept pages from server behavior

- Date: 2026-07-28
- GitHub Issue: #394
- Owning repository: `word-online/client`
- Status: Complete

## Goal

Create one structured concept page per concrete game-server magic. Each page
combines server-proven runtime behavior with client naming, world lore, and art
direction so gameplay and visual design use the same source material.

## Acceptance Criteria

- Every concrete `*Magic.java` under the game server has one generated page.
- Each page records name, concept, description, cast family, primary prefab,
  components, parameters, secondary spawns, runtime behavior, art requirements,
  source paths, and unresolved conflicts.
- Generated facts stay distinguishable from art-direction interpretation.
- A browsable index groups pages by cast family and faction.
- Re-running the generator produces deterministic output.

## Non-goals

- Do not change game-server behavior, database rows, recipes, or parameters.
- Do not rename existing server beans, prefabs, client resources, or localization
  keys.
- Do not generate or replace production art in this step.
- Do not claim numeric balance values when they only exist in runtime DB data.

## Context / Constraints

- The server magic registry is authoritative and database-backed.
- Client localization contains stale, duplicated, and mismatched keys.
- Concrete magic behavior is split between `Magic` implementations,
  `PrefabInitializer`s, mob/magic components, and runtime parameters.
- Worldbuilding can reinterpret visuals and names, but must not silently rewrite
  proven runtime behavior.

## Affected Repositories and Contracts

- `game/`: read-only source of magic execution and object behavior.
- `client/`: owns `.art/magic/` generated concept pages and generator.
- No runtime or protocol contract changes.

## Approach

- [x] Recon
- [x] Implementation
- [x] Focused validation
- [x] Compatibility and regression validation
- [x] Release order and rollback check

1. Discover all concrete server magic classes and their Spring bean names.
2. Resolve primary and secondary `PrefabType` references.
3. Resolve matching prefab initializers and attached component types.
4. Extract referenced parameter and game-object keys.
5. Add curated Korean names, faction concepts, and art notes.
6. Generate deterministic per-magic Markdown pages and an index.
7. Validate page count, links, source paths, and clean regeneration.

## Validation

- Commands:
  - `python3 .art/tools/generate-magic-pages.py`
  - `find .art/magic/pages -name '*.md' | wc -l`
  - `git diff --check -- .art/magic .art/tools/generate-magic-pages.py`
- Manual checks:
  - Review representative build, drop, explode, shoot, single-spawn, and
    swarm-spawn pages.
  - Verify special multi-stage spells such as `Overgrowth`, `Crater`,
    `DimensionToad`, `FireLordSpirit`, and `VineToss`.
- Expected results:
  - One page per concrete server magic.
  - No broken relative page links or nonexistent source references.
  - Second generation produces no content change.

## Risks & Rollback

- Heuristic Java parsing can miss behavior hidden behind inheritance. Pages must
  retain source links and mark inferred prose.
- Runtime DB values are unavailable from source alone. Record parameter keys,
  not invented numbers.
- Rollback is deletion of `.art/magic/` and its generator; runtime is unaffected.

## Release Order

1. Generate and review client art documentation.
2. Resolve naming/worldbuilding conflicts separately.
3. Generate art only after affected concept pages are approved.

## Open Questions

- Should concept pages cover PVE-only magic in the same index or a separate PVE
  section?
- Which server names should be renamed later to remove `Spirit` from hellfire
  demons without breaking resource contracts?
- Should runtime DB recipes and balance values be snapshotted into documentation
  by a future authenticated export?
