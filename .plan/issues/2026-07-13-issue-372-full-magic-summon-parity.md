# 2026-07-13 — 전체 마법·소환수 결정론적 parity 및 runtime 통합

- Date: 2026-07-13
- GitHub Issue: #372
- Owning repository: Apptive-Game-Team/WordOnlineClient
- Status: Completed

## Goal

현재 Game server source의 concrete magic 67개, production prefab 96개 및 필요한 component 동작을 Client의 `GameScene.Simulation`으로 결정론적으로 port하고, PVP/Practice/PVE production runtime에서 하나의 frame pipeline으로 실행한다.

## Acceptance Criteria

- server source 기반 support matrix에서 모든 magic/prefab/component가 implemented, runtime-wired, parity-tested 상태다.
- spawn ID/order, transform, HP, state, effect, resource, match result가 동일 seed/config/input에서 일치한다.
- resource, magic, mob, PVE simulation이 `LockstepGameController`의 매 frame에 실제 연결된다.
- unknown magic/prefab/component는 generic fallback 없이 실패한다.
- EditMode, PlayMode, full-catalog replay, Development WebGL이 통과한다.

## Non-goals

- Client authoritative fallback
- 부분 배포 feature flag
- `main` 병합
- 치팅 완전 방지

## Context / Constraints

- 기존 `MagicSimulation`은 소수 generic effect만 지원한다.
- `ResourceSimulation`과 `MobSimulation`은 구현됐지만 production runtime에서 생성되지 않는다.
- PVE runtime은 `PveSessionSimulation.World`만 보관하여 scenario step/result가 실행되지 않는다.
- 기존 단일 `.worktrees/client-lockstep` worktree와 `feature/372` 브랜치를 사용한다.
- server의 float/time/random/iteration semantics를 fixed-point/frame/seed/stable order 계약으로 명시해야 한다.

## Affected Repositories and Contracts

- WordOnlineClient: simulation, runtime coordinator, renderer bindings, parity fixtures/tests
- WordOnlineServer: read-only reference 및 deterministic fixture export 계약. 서버 변경이 필요하면 별도 issue/branch/PR로 분리한다.
- Contract: magic/prefab/component ID, config version, frame rate, seeded RNG, canonical snapshot/hash schema

## Approach

- [x] Recon: server 67 magic / 96 prefab / 120 component inventory와 dependency graph 생성
- [x] Implementation: versioned production catalog 및 component behavior registry
- [x] Implementation: projectile/field/explosion/drop/build/spawn magic family parity
- [x] Implementation: summon detector/state/movement/attack/heal/spawner/effect parity
- [x] Implementation: resource + magic + mob + PVE 단일 deterministic frame pipeline
- [x] Focused validation: family별 fixture/replay parity
- [x] Compatibility and regression validation: PVP/Practice/PVE full catalog, EditMode/PlayMode/WebGL
- [x] Release order and rollback check

## Validation

- Commands: Unity EditMode/PlayMode, `BuildScript.BuildDevWebGL`, server `./gradlew test`, catalog inventory diff, replay checkpoint hash diff, `graphify update .`
- Manual checks: representative projectile, field, building, single summon, swarm summon, special chained behavior를 GameScene에서 확인
- Expected results: inventory 누락 0, generic fallback 0, deterministic replay mismatch 0, test/build failure 0

### Current evidence

- Unity EditMode: 165/165 pass
- Unity PlayMode: 7/7 pass
- Development WebGL: success; loader, framework, data, and wasm artifacts generated
- `GameScene.Simulation` and `GameScene.Simulation.Rendering`: compile success
- Game server `./gradlew test` and `./gradlew build`: success
- server/client inventory diff: magic 67/67, prefab 96/96, component 120/120; renderer resource missing 0
- production prefab lifecycle and all-magic dual replay: every frame hash match
- `graphify update .`: success
- tracked generated `.meta`, temporary WebGLInput workaround, DEV_BUILD setting, and build-generated Addressables/TMP files cleaned

## Risks & Rollback

- 범위가 크므로 family별 coherent commit으로 나누되 하나의 PR에서 `feature/lockstep`에 병합한다.
- server float semantics를 그대로 복사하면 플랫폼 차이가 생길 수 있어 각 변환을 fixture로 고정한다.
- rollback은 #372 squash commit revert이며 기존 authoritative 경로를 복구하지 않는다.

## Release Order

1. Client #372를 `feature/lockstep`에 병합
2. server fixture/contract 변경이 필요하면 Server 별도 PR 병합
3. 전체 client/server cutover 검증
4. 마지막에만 lockstep branches를 main에 병합

## Open Questions

- Game server source의 bean name, behavior class, root prefab, component graph를 source of truth로 사용한다.
- DB magic catalog와 server bean의 불일치는 #372 범위에서 무시하며 Client behavior 구현을 막지 않는다.
