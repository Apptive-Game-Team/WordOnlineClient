# 2026-07-13 — 기존 server-state client renderer와 fallback 제거

- Date: 2026-07-13
- GitHub Issue: #361
- Status: Complete

## Goal

게임 runtime을 lockstep session/confirmed-frame 기반 simulation과 `SimulationRendererBridge` 단일 경로로 연결한다. server frame DTO handler, snapshot/delta object renderer, authoritative input endpoint를 제거한다.

## Non-goals

- authoritative fallback/feature flag 유지
- UI/scene 재설계
- main 병합
- game server authoritative 코드 삭제

## Context / Constraints

- 기존 simulation systems는 구현됐지만 runtime coordinator가 아직 없다.
- `StompConnector`의 unknown message fallback은 `GeneralHandler`로 server state를 적용한다.
- `CardInputSender`는 old `/app/game/input` endpoint와 magic-valid response를 기다린다.
- scene/prefab visual scripts는 simulation renderer prefab에서 계속 재사용될 수 있다.

## Approach (Checklist)
- [x] **Step 0: Recon** fallback, DTO handlers, old object renderer, card input endpoint, renderer bridge 참조 확인
- [x] **Step 1: Implementation** lockstep runtime coordinator와 local input queue 연결; unknown protocol reject; old handler/DTO/object sync 삭제
- [x] **Step 2: Tests** no-fallback static contract, local input drain/order test, 기존 Edit/Play 전체 검증
- [x] **Step 3: Rollout / Rollback** Development WebGL build 후 `feature/lockstep` PR

## Validation
- **Commands to run:** Unity EditMode, PlayMode, Development WebGL, `rg` old paths, `graphify update .`
- **Expected output:** old endpoint/handlers/DTO refs 0, 전체 test/build 통과

## Risks & Rollback
- **Risks:** scene serialization에 남은 deleted MonoBehaviour, card recipe config bootstrap, result/UI event 연결
- **Rollback steps:** #361 squash commit revert

## Open Questions
- production card cost/recipe export는 versioned config 생성기로 대체해야 한다. coordinator는 injected rules를 요구하고 silent fallback을 만들지 않는다.
