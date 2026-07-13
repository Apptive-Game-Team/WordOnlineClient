# 2026-07-13 — PVE scenario와 Practice bot simulation 구현

- Date: 2026-07-13
- GitHub Issue: #354
- Status: Complete

## Goal

PVE scenario bootstrap과 Practice bot 결정을 client simulation 안에서 결정적으로 생성한다. solo session은 원격 peer hash/frame quorum 없이 진행한다.

## Non-goals

- authoritative fallback
- bot ML/비결정적 wall-clock 사고 시간
- production scenario DB를 runtime 조회
- renderer/UI 연결

## Context / Constraints

- `START_PVE_SCENARIO`는 versioned checked-in config만 참조한다.
- 음수 participant는 local simulation actor이며 network quorum participant가 아니다.
- bot 결과는 별도 mutation이 아니라 일반 frame input이어야 한다.
- #355 magic, #358 mob simulation 계약을 재사용한다.

## Approach (Checklist)
- [x] **Step 0: Recon** protocol queue, server PVE registry/script, BotBrain 확인
- [x] **Step 1: Implementation** participant/quorum policy, scenario catalog/bootstrap, deterministic bot frame input 추가
- [x] **Step 2: Tests** PVE start/play/result golden replay와 negative bot/quorum fixture 추가
- [x] **Step 3: Rollout / Rollback** `feature/lockstep` 대상 PR; squash revert 가능

## Validation
- **Commands to run:** Unity EditMode, `git diff --check`, `graphify update .`
- **Expected output:** 전체 통과, 반복 golden replay hash/input 동일

## Risks & Rollback
- **Risks:** bootstrap sequence 충돌, bot input sequence 재현성, solo/network session 구분
- **Rollback steps:** #354 squash commit revert

## Open Questions
- production scenario export pipeline은 후속 통합에서 catalog 생성 입력으로 연결한다.
