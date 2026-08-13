# 2026-08-13 — WaterShot 물 투사체 표시 문제 수정

- Date: 2026-08-13
- GitHub Issue: #491
- Status: Completed

## Goal

소환수의 WaterShot 공격 시 런타임 물 투사체가 화면에 정상적으로 생성되고 이동 및 타격 시점까지 표시되도록 수정한다.

## Non-goals

- WaterShot의 서버 전투 판정이나 밸런스 변경
- 다른 마법 또는 소환수의 아트 스타일 변경
- 물 공격 전체의 연출 재설계

## Context / Constraints

- 소환수 자체와 공격 판정은 동작하지만 물 투사체만 화면에 보이지 않는다.
- 서버가 전달하는 타입과 `Assets/Resources/Prefabs`의 리소스 이름 및 루트 오브젝트 이름이 일치해야 한다.
- 기존 정상 투사체 패턴을 우선 재사용하고 관련 없는 작업 트리 변경은 건드리지 않는다.

## Approach (Checklist)
- [x] **Step 0: Recon** WaterShot DTO 처리, 생성 코드, 프리팹과 스프라이트 참조를 정상 투사체와 비교한다.
- [x] **Step 1: Implementation** WaterShot 렌더 정렬값을 소환수보다 높이고 프리팹 이름과 초기 위치를 정상화한다.
- [x] **Step 2: Tests** WaterShot 프리팹 회귀 테스트를 추가하고 런타임 및 EditMode 테스트 어셈블리 컴파일을 검증한다.
- [x] **Step 3: Rollout / Rollback** Unity 임포트 로그와 diff를 확인하고 단일 이슈 범위로 정리한다.

## Validation
- **Commands to run:** `dotnet build Assembly-CSharp.csproj -v minimal`, `dotnet build WordOnline.Tests.EditMode.csproj -v minimal`, `git diff --check`
- **Expected output:** 두 어셈블리가 오류 없이 컴파일되고 diff 형식 오류가 없으며 WaterShot 생성 경로가 전면 정렬된 프리팹을 로드한다.

## Risks & Rollback
- **Risks:** 잘못된 리소스 이름 또는 SpriteRenderer 설정 변경이 다른 투사체 렌더링에 영향을 줄 수 있다.
- **Rollback steps:** 이 브랜치의 이슈 #491 관련 커밋을 `git revert`한다.

## Open Questions
- None
