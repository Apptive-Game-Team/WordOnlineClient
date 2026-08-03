# 2026-08-03 — Rallying Totem 리메이크

- Date: 2026-08-03
- GitHub Issue: #396
- Status: Complete

## Goal

서버에서 `RallyingTorch`가 `RallyingTotem`으로 변경된 흐름에 맞춰 Unity 프리팹, 스프라이트, 로컬라이징을 `RallyingTotem` / `집결의 토템`으로 일치시키고, 상단에 불꽃이 타오르는 토템 아트를 제작한다.

## Non-goals

- 서버 마법 ID, 레시피, 동작 로직은 변경하지 않는다.
- 프로젝트의 고정된 `master-v2` 렌더링 스타일은 변경하지 않는다.
- 현재 작업 트리의 다른 #396 아트 변경은 수정하지 않는다.

## Context / Constraints

- `CombinedMagicData`는 서버 이름으로부터 `rallyingTotem` 로컬라이징 키와 `RallyingTotem` 리소스 이름을 파생한다.
- 프리팹과 스프라이트는 기존 GUID를 보존해 참조 손실을 막는다.
- 새 이미지는 `master-v2` 2.5D 컷페이퍼 스타일과 지옥불 군단의 어두운 외피/주황 내부 발광을 따른다.
- 프로덕션 아트는 사용자 승인 전까지 `.art/concept/`에 보관한다.

## Approach (Checklist)

- [x] **Step 0: Recon** 기존 프리팹, 스프라이트, 로컬라이징, 런타임 참조를 조사한다.
- [x] **Step 1: Art candidate** 불꽃이 타오르는 단일 토템 후보를 생성하고 투명도·실루엣·스타일을 검증한다.
- [x] **Step 2: Approval** 후보를 사용자에게 제시하고 Unity 프로덕션 반영 승인을 받는다.
- [x] **Step 3: Implementation** 프리팹/스프라이트 이름과 로컬라이징을 `RallyingTotem`으로 변경하고 GUID를 보존한다.
- [x] **Step 4: Validation** 참조 검색, 이미지 메타, 빌드 및 Unity 로딩 경로를 검증한다.

## Validation

- **Commands to run:** `rg -n "RallyingTorch|rallyingTorch" Assets`, `git diff --check`, `dotnet build "Assembly-CSharp.csproj" -nologo`
- **Expected output:** 런타임 리소스와 로컬라이징에 이전 이름이 남지 않고, `RallyingTotem` 프리팹/스프라이트 및 `.meta`가 존재하며 빌드가 성공한다.
- **Result:** 이전 이름 없음, 스프라이트 `82x192` RGBA 및 투명 모서리 확인, 프리팹/스프라이트 GUID 보존, `dotnet build` 오류 0개.

## Risks & Rollback

- **Risks:** 서버 타입 이름과 프리팹 이름이 어긋나면 `Resources.Load`가 실패할 수 있다. 스프라이트 GUID가 바뀌면 프리팹 참조가 끊길 수 있다.
- **Rollback steps:** 이번 변경 파일만 이전 이름과 이미지로 되돌리고 기존 `.meta` GUID를 복구한다.

## Open Questions

- 없음.
