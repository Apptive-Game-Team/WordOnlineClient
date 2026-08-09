# 2026-08-09 — LightningCloud 마법과 먹구름 프리팹 추가

- Date: 2026-08-09
- GitHub Issue: #459
- Status: Complete

## Goal

낙뢰 마법 사용 시 서버가 생성하는 `LightningCloud` 타입을 클라이언트가 올바른 이름, 로컬라이제이션, 먹구름 스프라이트, 런타임 프리팹으로 표시하도록 한다.

## Non-goals

- 서버 레시피, 마법 ID, 생성 로직을 추가하거나 변경하지 않는다.
- 프로젝트의 확정된 2.5D cut-paper 아트 스타일을 변경하지 않는다.

## Context / Constraints

- 서버 이름에서 `lightningCloud` 로컬라이제이션 키와 `LightningCloud` 리소스 이름이 파생된다.
- `RainCloud`와 `LightningDrop`이 같은 Drop 계열 추상 프리팹을 사용하는 가장 가까운 구현이다.
- 승인된 v2 이미지만 Unity 최종 경로로 승격한다.

## Approach (Checklist)
- [x] **Step 0: Recon** (`RainCloud`, `LightningDrop`, 로컬라이제이션 및 아트 기준 확인)
- [x] **Step 1: Implementation** (`lightningCloud` 로컬라이제이션, `LightningCloud.png`, `LightningCloud.prefab` 추가)
- [x] **Step 2: Tests** (GUID 참조, PNG alpha·크기·trim, 리소스 이름 검증; C# 프로젝트 파일은 worktree에 없어 빌드 제외)
- [x] **Step 3: Rollout / Rollback** (서버 타입 배포와 함께 사용; 문제 시 신규 로컬라이제이션 및 자산 파일만 되돌림)

## Validation
- **Commands to run:** `rg` 로 키/이름 확인, Pillow 이미지 검사, Unity 형식/GUID 정적 검증, `dotnet build Assembly-CSharp.csproj -v minimal`
- **Expected output:** 동일 ID의 3개 로컬라이제이션 항목, alpha가 있는 tier 제한 내 PNG, `.meta`가 있는 프리팹/스프라이트, Unity YAML/GUID 참조 오류 없음

## Risks & Rollback
- **Risks:** 서버 타입 이름 불일치, 프리팹의 잘못된 스프라이트 GUID, 작은 화면에서 뭉개지는 실루엣
- **Rollback steps:** 이슈 #459 변경 파일만 `git revert`하여 기존 리소스 로딩 경로를 복구한다.

## Open Questions
- 서버가 생성하는 타입이 정확히 `LightningCloud`라는 요청의 이름을 계약으로 사용한다.
