# 2026-07-28 — Magic Book Prefab Preview

- Date: 2026-07-28
- GitHub Issue: https://github.com/Apptive-Game-Team/WordOnlineClient/issues/402
- Owning repository: Apptive-Game-Team/WordOnlineClient
- Status: In Progress

## Goal

Magic Book에서 소환형 마법을 선택하면 실제 게임 프리팹의 소환·공격 연출과 연결된 효과음을 확인할 수 있게 한다.

## Acceptance Criteria

- `Resources/Prefabs/<resourceName>`이 존재하면 기존 프리팹을 프리뷰에 사용한다.
- `MagmaSpirit` 소환 프레임과 공격 프레임이 기존 유지 시간으로 재생된다.
- `AquaArcher` 공격 프레임과 프리팹 효과음이 기존 컴포넌트를 통해 재생된다.
- 상세 페이지에는 기존 정적 이미지를 유지하고, 이미지를 클릭하면 Canvas 최상단 프리뷰 모달을 연다.
- 모달은 둥근 9-slice 패널, 테두리, 그림자와 둥근 프리뷰 Mask를 사용한다.
- 모달의 프리뷰를 클릭하면 기존 공격 이벤트 경로가 다시 실행된다.
- 프리뷰 UI를 상세 ScrollRect 밖에 두어 Magic Info 스크롤을 방해하지 않는다.
- 프리팹이 없거나 프리뷰에 적합하지 않으면 기존 정적 마법 이미지를 유지한다.
- `ServedObject`가 없는 유성우 같은 주문 이펙트 prefab은 기존 정적 마법 이미지를 유지한다.
- 선택 전환과 씬 종료 시 프리팹, 카메라, RenderTexture를 정리한다.

## Non-goals

- 도감 전용 애니메이션 또는 효과음 데이터 복제
- 서버/리소스 키 변경
- 전투 오브젝트 등록 및 네트워크 상태 재현
- 비소환 주문 이펙트의 전체 시뮬레이션

## Context / Constraints

- Magic Book은 Screen Space Camera Canvas의 `MagicInfo.magicImage`에 정적 스프라이트를 표시한다.
- 게임 프리팹은 `SpriteRenderer` 기반이므로 UI에 표시하려면 격리된 카메라와 RenderTexture가 필요하다.
- `ObjectSpawner` 전체 경로는 `ObjectContainer`와 게임 씬 싱글턴을 요구하므로 도감에서 호출하지 않는다.
- 프리팹 자체와 `ServedObject`의 기존 공격 이벤트 구독자는 재사용한다.
- 작업 브랜치는 PR #398의 신규 프레임을 검증하기 위해 `feature/396` 위에 스택한다.

## Affected Repositories and Contracts

- Client만 변경한다.
- 서버 API, DTO, 리소스 이름 계약은 변경하지 않는다.
- `CombinedMagicData.resourceName`과 동일한 프리팹이 존재할 때만 프리뷰를 활성화한다.

## Approach

- [x] Recon
- [x] Implementation
- [x] Focused validation
- [x] Compatibility and regression validation
- [x] Release order and rollback check

`MagicInfo`에 프리팹 프리뷰 수명주기를 위임하는 컴포넌트를 붙인다. 상세 ScrollRect에는 정적 이미지를 유지하고, 이미지 클릭 시 루트 Canvas에 정사각형 프리뷰 모달을 만든다. 컴포넌트는 선택한 `resourceName` 프리팹을 격리된 레이어에 생성하고 전용 직교 카메라로 모달의 RenderTexture에 렌더한다. `ServedObject`, 기존 공격 프레임/효과음 구독자, 공용 소환 프레젠터를 재사용한다. 공격 재생은 `ServedObject`에 추가하는 작은 공개 메서드가 기존 `OnAttack`과 스윙 코드를 호출하게 하며, 실제 전투 상태 처리도 같은 메서드를 사용한다.

## Validation

- Commands:
  - `dotnet build Assembly-CSharp.csproj --no-restore`
  - `git diff --check`
- Manual checks:
  - Magic Book에서 `MagmaSpirit` 선택 → 소환 프레임 후 기본 자세
  - 프리뷰 클릭 → 내려찍기 프레임과 기존 효과음
  - `AquaArcher` 선택 및 클릭 → 시위 해제 프레임과 기존 효과음
  - 프리팹 없는 마법 선택 → 기존 정적 이미지
  - 이미지 클릭 → 모달 열림, 프리뷰 클릭 → 공격, 바깥 영역 또는 Esc → 닫힘
  - 여러 마법 반복 전환 → 잔상·중복 오디오·RenderTexture 누수 없음
- Expected results:
  - 게임 프리팹과 동일한 연출 경로가 도감에서도 동작하고 기존 도감 표시가 유지된다.

Unity 2022.3.34f1 배치 컴파일이 성공했다. 실제 Magic Book의 클릭·음향·프레임 전환은 Editor Play Mode 수동 확인이 남는다.

## Risks & Rollback

- 프리팹의 전투 전용 `MonoBehaviour.Start`가 게임 씬 싱글턴을 참조할 수 있다. 프리뷰 전용 레이어와 비활성화 가능한 월드 UI로 격리한다.
- 프리팹 크기 편차가 크다. 활성 `SpriteRenderer` bounds로 직교 카메라를 자동 맞춘다.
- 등장 연출이 프리팹을 확대하기 전에 최종 bounds로 카메라를 맞춰 확대 후 잘림을 막는다.
- 롤백은 이 이슈의 단일 Client 커밋을 revert한다.

## Release Order

1. PR #398이 먼저 병합되어 신규 상태 프레임을 제공한다.
2. 이 작업은 `feature/396`을 base로 한 스택 PR로 검증한다.
3. #398 병합 후 base를 `main`으로 변경하거나 rebase해 병합한다.

## Open Questions

- 없음. 최초 범위는 프리팹 이름이 마법 `resourceName`과 일치하는 소환형 마법으로 제한한다.
