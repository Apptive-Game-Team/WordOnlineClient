# 2026-08-31 — Storm Stag 단계별 지속 VFX 정리

- Date: 2026-08-31
- GitHub Issue: #554
- Status: Complete

## Goal

Storm Stag의 가속 1~2단계에는 전용 VFX를 표시하지 않고, 3단계에는 지속 잔상,
4단계에는 지속 번개 오라와 지속 잔상을 표시한다.

## Non-goals

- Storm Stag 본체 스프라이트나 공용 번개 오라 이미지는 변경하지 않는다.
- 게임플레이 가속 수치는 변경하지 않는다.
- 공용 `AfterImageSpawner`의 Chain Lightning 동작은 변경하지 않는다.

## Context / Constraints

- `feature/554`에는 Storm Stag 및 `StormStagCharge2/3/4` 프리팹이 이미 추가되어 있다.
- Chain Lightning이 사용하는 공용 `AfterImageSpawner`를 Storm Stag 3·4단계에 연결한다.
- 서버가 전달하는 `StormStagCharge3` 및 `StormStagCharge4` 효과 이름을 그대로 사용한다.
- 잔상은 본체 이미지에 합성하지 않고 런타임 SpriteRenderer로 분리한다.

## Approach (Checklist)
- [x] **Step 0: Recon** (`ChainLightning`, `StormStagCharge3/4`, 공용 잔상 컴포넌트 확인)
- [x] **Step 1: Implementation** (2단계 VFX 제거, 3단계 지속 잔상, 4단계 지속 오라+잔상 구성)
- [x] **Step 2: Tests** (프리팹 직렬화 확인 및 C# 컴파일/관련 테스트 실행)
- [x] **Step 3: Rollout / Rollback** (별도 플래그·마이그레이션 없음; 프리팹 변경만 되돌릴 수 있음)

## Validation
- **Commands to run:** `dotnet build Assembly-CSharp.csproj -nologo`; 프리팹 YAML에서 3·4단계의 `AfterImageSpawner` 연결 확인
- **Expected output:** 컴파일 오류 0개, 2단계는 시각 컴포넌트 없음, 3단계는 잔상만, 4단계는 반복 오라와 잔상 존재

## Risks & Rollback
- **Risks:** 원본 SpriteRenderer 자동 탐색이 실패하면 잔상이 보이지 않을 수 있다. 짧은 생성 간격은 동시 오브젝트 수를 늘릴 수 있으므로 기존 풀링 설정을 재사용한다.
- **Rollback steps:** 세 단계 프리팹 변경을 이전 구성으로 되돌린다.

## Open Questions
- 없음. “3단계 이상”은 현재 존재하는 가속 3단계와 4단계로 해석한다.
