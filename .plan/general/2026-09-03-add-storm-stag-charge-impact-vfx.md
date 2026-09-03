# 2026-09-03 — Storm Stag 고가속 돌진 피격 VFX 추가

- Date: 2026-09-03
- GitHub Issue: None
- Status: Complete

## Goal

Storm Stag가 가속 3단계 이상에서 돌진 공격을 적중시켰을 때 약 0.2초 동안 재생되는 전용 전기 충돌 VFX를 추가한다. 정지 스프라이트 한 장을 띄우는 대신 압축 섬광, 뿔 모양 확장, 잔광 소멸의 시간 흐름이 읽히게 한다.

## Non-goals

- Storm Stag 본체 아트 또는 이동 애니메이션 변경
- 서버 전투 판정이나 피해량 변경
- 다른 공격이 사용하는 ElectricShot 및 공용 HitEffect 교체
- 승인되지 않은 스타일 변경

## Context / Constraints

- Unity 2022.3.34f1 WebGL 클라이언트다.
- 기준 아트는 `.art/anchors/master-v2/`의 2.5D cut-paper 스타일이다.
- VFX 프레임은 투명 PNG, 최대 256px, 동일 피벗과 스케일을 사용한다.
- 서버 이벤트가 가속 단계를 제공하지 않으면 클라이언트가 신뢰할 수 있는 기존 상태/투사체 정보만 사용하고, 추측 기반 판정은 추가하지 않는다.
- 기존 사용자 작업과 무관한 변경은 포함하지 않는다.

## Approach (Checklist)
- [x] **Step 0: Recon** (HitEvent, projectile, Storm Stag 리소스 이름과 가속 상태 계약 확인)
- [x] **Step 1: Implementation** (3단계 VFX 이미지, 전용 재생 컴포넌트/프리팹, 조건부 연결, 같은 충돌의 ElectricShot 시각 억제)
- [x] **Step 2: Tests** (Edit Mode 테스트, C# 빌드, Unity import/console 및 시각 재생 검증)
- [x] **Step 3: Rollout / Rollback** (기존 HitEffect fallback 유지, 전용 리소스가 없을 때 안전하게 무시)

## Validation
- **Commands to run:** `dotnet build Assembly-CSharp.csproj -v minimal`, 관련 Edit Mode 테스트, Unity 콘솔 확인 및 프리팹 재생 캡처
- **Expected output:** 컴파일 오류 없음, 고가속 Storm Stag 적중에만 전용 VFX 재생, 기타 공격은 기존 연출 유지
- **Completed:** Unity 2022.3.34f1 import/compile 성공, 최초 전용 규칙·리소스 테스트 7개와 전체 Edit Mode 테스트 130개 통과. 후속 ElectricShot 억제 규칙 테스트 3개를 추가했고 `Assembly-CSharp.csproj` 및 `WordOnline.Tests.EditMode.csproj` 빌드가 오류 0개로 통과했다. 새 게스트 세션에서는 사용자 작성 `New Deck`이 보존되지 않아 후속 실제 충돌 화면 재확인은 제한됐다.

## Risks & Rollback
- **Risks:** 서버 이벤트에 가속 단계가 없어 잘못된 조건으로 재생될 수 있음; VFX가 64px에서 복잡하거나 목표물을 가릴 수 있음; WebGL에서 과도한 오브젝트 생성 가능
- **Rollback steps:** 전용 분기와 프리팹/스프라이트를 제거하면 기존 공용 HitEffect 동작으로 즉시 복귀

## Open Questions
- None. 서버는 `StormStagCharge3`/`StormStagCharge4` 효과와 `StormStag` 프리팹 타입을 이미 제공한다.
