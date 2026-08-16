# 2026-08-16 — 전투 튜토리얼 씬 2.5D 월드를 GameScene과 동일하게 맞추기

- Date: 2026-08-16
- GitHub Issue: #500
- Status: Implemented (Unity 에디터 시각 검증 미수행)

## Goal

`InteractiveTutorialScene`의 2.5D 월드 구성을 `GameScene`과 시각적으로 동일하게 만든다.
#498(관전 씬)과 같은 작업을 전투 튜토리얼 씬에 적용한다.

## Non-goals

- 온보딩용 `TutorialScene`(순수 UI 씬) 변경
- 튜토리얼 진행 로직, 대사, 단계 데이터 변경
- 플레이어 배치 변경 — 월드 좌표는 그대로 유지한다

## Context / Constraints

이슈 #379가 카메라만 2.5D로 전환하고 월드 배경은 옮기지 않아 `Map`에 배경 아트가 없었다.
관전 씬(#498)과 달리 튜토리얼 `Map`은 `(9,0,5)`에 있고 실제 게임플레이 자식인 `LeftPlayer`,
`RightPlayer`를 갖는다. 따라서 `Map`을 원점으로 옮길 때 두 자식의 로컬 좌표에 기존 오프셋을
그대로 접어 넣어야 월드 배치가 보존된다.

`Map` 트랜스폼 값에 의존하는 스크립트는 없다(`Assets/Scripts/TutorialScene` 전체 확인). `Map`은
배경/플레이어 컨테이너 역할만 한다.

## Approach (Checklist)

- [x] **Step 0: Recon** — `GameScene`과 `InteractiveTutorialScene`의 카메라·`Map`·`background`·
  `PopupBookGround`·RenderSettings 비교, `Map` 참조 스크립트 조사
- [x] **Step 1: Implementation**
  - `GameScene`의 `Map` 하위 76개 배경 오브젝트를 이식 (228 블록, fileID 충돌 없음)
  - `Map` Transform `(9,0,5)` → `(0,0,0)`, 오프셋을 자식에 반영:
    `LeftPlayer` `(-8,0,0)` → `(1,0,5)`, `RightPlayer` `(8,0,0)` → `(17,0,5)`
  - 미사용 `Grid` 컴포넌트 제거
  - `PopupBookGround` Transform 정렬 (`(9,-0.02,5)`/`(2.8,1,1.6)` → `(9,0,5)`/`(3,1,3)`)
- [x] **Step 2: Tests** — 씬 YAML 구조 검증 및 플레이어 월드 좌표 불변 확인
- [x] **Step 3: Rollout / Rollback** — 변경이 `InteractiveTutorialScene.unity` 한 파일에 한정됨

## Validation

- **Commands to run:** 씬 YAML 파싱, 내부 fileID 참조 무결성 검사, 배경 서브트리 대조,
  변경 전후 플레이어 월드 좌표 계산 비교, guid 참조 해석 검사
- **Expected output:** YAML 파싱 성공, dangling 참조 0건, 배경 76노드가 `GameScene`과 일치,
  플레이어 월드 좌표 불변
- **Result:** 419 YAML 문서 파싱 성공, dangling 내부 참조 0건, 배경 76노드
  (이름/위치/회전/스케일/스프라이트/sortingLayer/sortingOrder) `GameScene`과 완전 일치,
  `LeftPlayer` `(1,0,5)` / `RightPlayer` `(17,0,5)` 변경 전후 동일. 미해석 guid는
  `GameScene`도 갖는 내장 에셋과 변경 전부터 있던 `694060f9…` 하나뿐
- **Not verified:** Unity 에디터 시각 검증 및 튜토리얼 플레이 모드 통과. 이 세션에서 Unity MCP
  서버가 연결되지 않았고 환경에 Unity 에디터가 없어 배치 모드 검증도 불가능했다.

## Risks & Rollback

- **Risks:** 배경 오브젝트가 튜토리얼 UI/하이라이트 연출과 겹칠 가능성,
  `PopupBookGround` 크기 변경이 팝업북 연출에 주는 영향
- **Rollback steps:** `feature/500` 커밋을 `git revert`한다.

## Open Questions

- 없음. 프레이밍 관련 질문은 #498과 동일하다.
