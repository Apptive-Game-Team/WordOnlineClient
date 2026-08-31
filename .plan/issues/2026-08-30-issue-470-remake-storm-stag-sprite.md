# 2026-08-30 — Storm Stag 스프라이트 정본 스타일로 재제작

- Date: 2026-08-30
- GitHub Issue: #470
- Status: Complete

## Goal

현재 `StormStag.png`를 프로젝트의 canonical master-v2 2.5D cut-paper 스타일에 맞는 번개 정령 사슴 production sprite 후보로 재제작한다.

## Non-goals

- master-v2 앵커 또는 프로젝트 전체 아트 방향을 변경하지 않는다.
- Storm Stag 프리팹의 전투 동작, 수치, 돌진 이펙트 로직을 변경하지 않는다.
- 사용자 승인 전에 기존 Unity production sprite를 덮어쓰지 않는다.

## Context / Constraints

- 현재 브랜치는 `feature/470`이다.
- `MasterStyleKey.png`와 `WorldTreeSpirit.png`, `ArcaneImpact.png`를 렌더링 기준으로 사용한다.
- 기존 `StormStag.png`는 사슴 형태와 번개 속성이라는 subject identity만 참고하며 렌더링 스타일은 계승하지 않는다.
- 세계수 정령의 둥글고 친근한 형태, 번개 팔레트, 우측을 보는 3/4 카메라, 투명 배경, 단일 피사체를 유지한다.
- 본체에 별도의 고밀도 번개 오라를 굽지 않는다.
- big tier 최대 `256x256`, 64px 축소 시 실루엣이 읽혀야 한다.

## Approach (Checklist)

- [x] **Step 0: Recon** — 정본 문서, 앵커, 기존 스프라이트와 프리팹 참조 확인
- [x] **Step 1: Implementation** — ImageGen 후보를 `.art/concept/storm-stag/`에 비파괴적으로 생성
- [x] **Step 2: Tests** — 실제 알파·우측 방향·64px 실루엣·마스터 스타일 일치 여부를 검증
- [x] **Step 3: Rollout / Rollback** — 사용자 승인 후 `Assets/Resources/Game/sprites/StormStag.png`를 256px production export로 교체하고 기존 `.meta` GUID 유지

## Validation

- **Commands to run:** PNG 픽셀 포맷/모서리 알파 검사, 64px 비파괴 축소 검사; 승인 후 production export와 Unity 프리팹 표시 확인
- **Expected output:** transparent single-subject sprite, right-facing, readable at 64px, master-v2와 동일한 2.5D cut-paper 렌더링

## Risks & Rollback

- **Risks:** ImageGen이 기존의 네온·페인터리 스타일을 따라가거나 번개를 본체 외부 오라로 과도하게 생성할 수 있다. 사슴의 긴 다리와 뿔이 64px에서 뭉개질 수 있다.
- **Rollback steps:** 승인 전 후보는 `.art/concept/`에만 두며, 승인 후 문제가 생기면 이전 `StormStag.png`를 Git에서 복원한다.

## Open Questions

- None. `.art/concept/storm-stag/StormStag-approved-256.png`가 승인되어 Unity production sprite에 반영됐다.
