# 2026-08-16 — 관전 씬 2.5D 월드를 GameScene과 동일하게 맞추기

- Date: 2026-08-16
- GitHub Issue: #498
- Status: Implemented (Unity 에디터 시각 검증 미수행)

## Goal

`SpectatingScene`의 2.5D 월드 구성을 `GameScene`과 시각적으로 동일하게 만든다.

## Non-goals

- 관전 전용 UI(관전 버튼, 타이머) 레이아웃 변경
- 게임 플레이 전용 오브젝트(`FieldSelector`, `CardInputSender`, `AdminPanal`, `MagicResolver` 등) 이식
- 카메라/입력 좌표계 변경 — 이슈 #379에서 이미 2.5D로 전환됨

## Context / Constraints

이슈 #379가 `SpectatingScene` 카메라를 2.5D(perspective, FOV 25, pos `(9,21,-16)`, rot `(45,0,0)`)로
바꿨지만 월드 배경은 옮기지 않았다. 그 결과 `Map` 오브젝트가 비어 있어 관전 화면에는 배경 아트가
전혀 보이지 않고, `Map` Transform과 `Grid` 컴포넌트는 구 2D 타일맵 시절 값이 그대로 남아 있었다.

두 씬의 `Map` GameObject는 fileID `1756437238`로 동일하고, `background`와 RenderSettings는 이미
일치한다.

## Approach (Checklist)

- [x] **Step 0: Recon** — 두 씬 YAML의 카메라·`Map`·`background`·`PopupBookGround`·RenderSettings 비교
- [x] **Step 1: Implementation**
  - `GameScene`의 `Map` 하위 76개 배경 오브젝트(grass/tree/rock, 45° 빌보드 SpriteRenderer)를
    `SpectatingScene`으로 이식 (GameObject/Transform/SpriteRenderer 228 블록, fileID 충돌 없음)
  - `Map` Transform `(9,5,0)` → `(0,0,0)`, 미사용 `Grid` 컴포넌트 제거
  - `PopupBookGround` Transform을 `GameScene`과 동일하게 정렬 (`(9,-0.02,5)`/`(2.8,1,1.6)` →
    `(9,0,5)`/`(3,1,3)`)
- [x] **Step 2: Tests** — 씬 YAML 구조 검증
- [x] **Step 3: Rollout / Rollback** — 변경이 `SpectatingScene.unity` 한 파일에 한정됨

## Validation

- **Commands to run:** 씬 YAML 파싱(`yaml.load_all`), 내부 fileID 참조 무결성 검사,
  두 씬 `Map` 서브트리(이름/위치/회전/스프라이트/sortingOrder) 대조, guid 참조 해석 검사
- **Expected output:** YAML 문서 파싱 성공, dangling 참조 0건, `Map` 서브트리 77노드 완전 일치
- **Result:** SpectatingScene 293 문서 파싱 성공, dangling 내부 참조 0건, `Map` 서브트리
  `GameScene`과 완전 일치, 미해석 guid는 `GameScene`도 동일하게 갖는 Unity 내장/패키지 에셋뿐
- **Not verified:** Unity 에디터 시각 검증. 이 세션에서 Unity MCP 서버가 연결되지 않았고 WSL에
  Unity 에디터가 설치되어 있지 않아 배치 모드 검증도 불가능했다. 머지 전 에디터에서 관전 화면
  프레이밍을 눈으로 확인해야 한다.

## Risks & Rollback

- **Risks:** 관전 UI(Canvas)가 새로 들어온 배경 오브젝트와 겹쳐 보일 가능성,
  `PopupBookGround` 크기 변경이 팝업북 연출 배치에 주는 영향
- **Rollback steps:** `feature/498` 커밋을 `git revert`한다. 변경 범위가 씬 파일 하나라 되돌리기 쉽다.

## Open Questions

- 관전 화면에서는 카메라 프레이밍을 게임 화면과 완전히 동일하게 두는 것이 맞는지, 아니면 전장
  전체가 보이도록 더 넓게 잡아야 하는지 확인이 필요하다. 현재는 "GameScene과 똑같이"를 그대로 따랐다.
