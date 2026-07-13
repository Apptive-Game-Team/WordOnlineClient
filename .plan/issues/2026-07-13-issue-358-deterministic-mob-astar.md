# 2026-07-13 — mob state machine과 deterministic A* 포팅

- Date: 2026-07-13
- GitHub Issue: #358
- Status: Complete

## Goal

서버의 mob 탐색·이동·공격 상태 흐름을 `GameScene.Simulation`으로 포팅한다. 모든 시간은 simulation frame으로 표현하고, target 선택과 A* 경로가 실행 순서나 런타임에 영향받지 않게 만든다.

## Non-goals

- 각 mob prefab의 최종 밸런스 수치 이관
- Unity renderer/animation 연결
- 기존 authoritative 서버와 병행 운용
- 동적 장애물 회피 및 navmesh 생성

## Context / Constraints

- 대상 브랜치는 장기 통합 브랜치 `feature/lockstep`의 자식이다.
- 물리와 수치는 BEPUphysics1int/Fix64 기반이어야 한다.
- 서버 `BehaviorMob`의 float timer, 순회 순서 target 선택, 직선 path를 그대로 복제하지 않는다.
- AI 결과는 기존 `SimulationWorld` 입력과 합성 가능해야 한다.
- 부분 배포/authoritative fallback은 만들지 않는다.

## Approach (Checklist)
- [x] **Step 0: Recon** 서버 `BehaviorMob`, detector, pathfinder와 client simulation 경계 확인
- [x] **Step 1: Implementation** 정수 grid, 고정 neighbor/tie 순서 A*, frame 기반 mob 상태와 canonical hash 추가
- [x] **Step 2: Tests** 장애물/tie/target ID/frame transition/replay hash EditMode fixture 추가
- [x] **Step 3: Rollout / Rollback** `feature/lockstep`에 PR; 문제 시 squash commit revert

## Acceptance Criteria

- 상태 전이는 simulation frame만 사용한다.
- A* open-set tie와 neighbor 순서는 완전 순서(total order)를 가진다.
- 같은 거리 target은 작은 entity ID가 선택된다.
- 입력 순서를 바꾼 fixture와 반복 replay가 같은 path/hash를 만든다.

## Validation
- **Commands to run:** Unity EditMode tests, `git diff --check`, `graphify update .`
- **Expected output:** 전체 EditMode 통과, 동일 replay frame hash, graph 갱신 성공

## Risks & Rollback
- **Risks:** grid 좌표 변환 경계, AI input sequence 충돌, 공격/이동 전이 frame off-by-one
- **Rollback steps:** `feature/lockstep`에서 #358 squash commit revert

## Open Questions
- production map obstacle source 연결은 후속 end-to-end integration에서 확정한다. 이번 범위는 immutable grid contract와 fixture를 제공한다.
