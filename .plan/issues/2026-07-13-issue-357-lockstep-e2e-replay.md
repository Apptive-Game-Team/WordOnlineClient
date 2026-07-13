# 2026-07-13 — PVP Practice PVE lockstep E2E replay 검증

- Date: 2026-07-13
- GitHub Issue: #357
- Status: Complete

## Goal

PVP 두 world와 solo Practice/PVE를 start-to-result로 재생하고, timeout·hash mismatch·disconnect·version mismatch 진단을 deterministic artifact로 남긴다. Development WebGL build까지 검증한다.

## Non-goals

- main 병합
- production 서버 실제 부하/네트워크 장애 시험
- renderer 시각 회귀 시험
- 기존 authoritative 경로 유지

## Context / Constraints

- base는 장기 통합 브랜치 `feature/lockstep`이다.
- replay checkpoint는 frame과 canonical hash를 보존한다.
- 실패는 enum reason, frame, message로 보존해 재현 가능해야 한다.
- WebGLInput 테스트용 주석은 commit하지 않는다.

## Approach (Checklist)
- [x] **Step 0: Recon** protocol queue, connector rejection/timeout, simulation tests, WebGL 설정 확인
- [x] **Step 1: Implementation** version validator, replay verifier/artifact, failure state machine 추가
- [x] **Step 2: Tests** PVP twin replay, mismatch/timeout/disconnect/version, Practice/PVE result fixture 추가
- [x] **Step 3: Rollout / Rollback** EditMode + Development WebGL build 후 `feature/lockstep` PR

## Validation
- **Commands to run:** Unity EditMode, Unity WebGL Development build, `git diff --check`, `graphify update .`
- **Expected output:** replay/failure fixture 포함 전체 통과, WebGL build exit 0

## Risks & Rollback
- **Risks:** replay input adapter와 production input 계약 차이, WebGL platform compile 차이, 진단 hash formatting
- **Rollback steps:** #357 squash commit revert

## Open Questions
- 서버와 실제 socket E2E는 game server lockstep relay 완성 후 별도 environment test로 연결한다.
