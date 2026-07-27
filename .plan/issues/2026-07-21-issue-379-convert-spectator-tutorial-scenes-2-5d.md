# 2026-07-21 — 관전 및 튜토리얼 씬 2.5D 전환

- Date: 2026-07-21
- GitHub Issue: #379
- Status: Implemented and verified in Unity MCP

## Goal

현재 `GameScene`에 적용된 2.5D 카메라, XZ 월드 좌표, 지면 선택 방식과 시각 표현을 `SpectatingScene` 및 실제 전투 튜토리얼인 `InteractiveTutorialScene`에 일관되게 적용한다.

## Non-goals

- 온보딩 진입용 `TutorialScene`의 UI 흐름이나 로컬라이제이션 변경
- 게임 서버 좌표 체계 또는 네트워크 프로토콜 변경
- 게임 씬의 기존 2.5D 아트 방향 재설계

## Context / Constraints

- `GlobalTutorialManager`가 전투 튜토리얼로 `InteractiveTutorialScene`을 로드한다.
- `SpectatingScene`은 게임 런타임 스크립트를 공유하지만 씬 카메라/월드 구성이 이전 2D 상태다.
- 튜토리얼 입력은 아직 `ScreenToWorldPoint` 기반 XY 평면을 사용하므로 XZ 지면 레이캐스트 방식으로 바꿔야 한다.
- Unity MCP 연결이 현재 세션에 노출되지 않아 저장 파일 비교와 C# 빌드로 우선 검증하고, 에디터 시각 검증 절차를 명시한다.

## Approach (Checklist)
- [x] **Step 0: Recon** (`GameScene`, `SpectatingScene`, `InteractiveTutorialScene` 카메라/맵/입력 차이 확인)
- [x] **Step 1: Implementation** (`SpectatingScene`과 `InteractiveTutorialScene`의 카메라·지면·월드 UI/입력을 2.5D 기준으로 변경)
- [x] **Step 2: Tests** (`dotnet build`, 씬 YAML 참조 검사, Unity MCP 편집/플레이 모드 검증 완료)
- [x] **Step 3: Rollout / Rollback** (씬별 커밋 범위를 확인하고 문제 시 #379 변경만 되돌릴 수 있게 유지)

## Validation
- **Commands to run:** `dotnet build Assembly-CSharp.csproj -v minimal`; 변경 씬과 스크립트 대상 `git diff --check`
- **Expected output:** 컴파일 오류 및 whitespace 오류 없음; 관전/튜토리얼 카메라와 지면 입력이 `GameScene`의 2.5D 기준과 일치
- **Result:** 임시 MSBuild 타깃으로 이전 브랜치의 stale csproj 참조만 제외한 전체 빌드 성공(오류 0); `git diff --check` 및 씬별 카메라/지면/SceneRoots 검사 통과. Unity MCP에서 두 씬 무결성, 2.5D 카메라/바닥/UI 배치, 튜토리얼 플레이 모드 빌보드 2개 생성과 화면 구성을 확인했다.

## Risks & Rollback
- **Risks:** 월드 스페이스와 UI 캔버스 좌표 혼용, 튜토리얼 목표 지점 변환 누락, 카메라 프레이밍 차이
- **Rollback steps:** `feature/379`의 변경 커밋을 `git revert`하여 관전/튜토리얼 씬과 입력 변경만 되돌린다.

## Open Questions
- Unity 에디터에서 16:9와 WebGL 기준으로 관전/튜토리얼 UI가 카메라 프레이밍을 가리지 않는지 최종 시각 확인이 필요하다. 현재 다른 Unity 인스턴스가 프로젝트를 열고 있어 배치 모드 시각 검증은 수행하지 못했다.
