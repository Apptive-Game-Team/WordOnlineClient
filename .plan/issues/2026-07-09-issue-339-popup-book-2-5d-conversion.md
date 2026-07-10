# 2026-07-09 — 팝업북 스타일 2.5D 전환

- Date: 2026-07-09
- GitHub Issue: #339
- Status: In Progress

## Goal

- 기존 2D 스프라이트, 프레임 애니메이션, 서버 전투 좌표 및 UI를 최대한 유지한다.
- GameScene을 Perspective 카메라와 3D 지면을 사용하는 팝업북/종이 디오라마 스타일로 전환한다.
- 서버의 `(x, y, z)` 좌표를 Unity의 `(x, height, depth)` 공간에 일관되게 표시한다.
- WebGL에서도 안정적인 프레임과 기존 게임 플레이 판정을 유지한다.

## Non-goals

- 캐릭터와 마법을 3D 모델로 다시 제작하지 않는다.
- 서버의 전투 시뮬레이션 좌표계와 네트워크 DTO 규격을 변경하지 않는다.
- 첫 단계에서 전체 프로젝트를 URP로 일괄 변환하지 않는다.
- 로비, 덱 편집, 결과 화면 등 GameScene 외 UI를 재디자인하지 않는다.
- 첫 프로토타입에서 동적 실시간 그림자나 고비용 후처리를 필수로 삼지 않는다.

## Context / Constraints

- 현재 Main Camera는 Orthographic이며 GameScene의 월드 오브젝트는 SpriteRenderer 중심이다.
- Resources/Prefabs의 게임 오브젝트는 SpriteRenderer와 일부 Collider2D에 의존한다.
- 서버 좌표는 `x`, `y`, `z`를 제공하며 현재 클라이언트는 `z`를 높이처럼 화면 `y`에 더해 표현한다.
- `PositionUpdater`는 이동 방향에 따라 `SpriteRenderer.flipX`를 갱신한다.
- `FieldSelector`는 `ScreenToWorldPoint` 후 `z = 0`으로 고정하여 조준 위치를 계산한다.
- 선택 처리는 `BoxCollider2D`와 `Physics2DRaycaster`를 사용한다.
- UI는 Screen Space-Camera Canvas이므로 월드 전환과 분리해 유지할 수 있다.
- 대상 플랫폼이 WebGL이므로 투명 스프라이트 오버드로우, 그림자, 후처리 비용을 측정해야 한다.

## Approach (Checklist)

- [ ] **Step 0: Recon** (Inspect existing code, locate files)
  - [ ] GitHub 이슈를 생성하고 `<issue-label>/<issue-num>` 형식의 작업 브랜치를 만든다.
  - [ ] GameScene과 대표 전투 프리팹의 백업 기준 커밋을 확정한다.
  - [ ] 플레이어, 지상 몹, 공중 몹, 건물, 투사체, 범위 표시기 각각의 대표 프리팹을 선정한다.
  - [ ] SpriteRenderer, Collider2D, sortingOrder, flipX, ZVisualizer 의존 코드를 목록화한다.
  - [ ] Built-in RP 유지안과 URP 14 프로토타입안을 동일한 대표 장면에서 비교한다.
  - [ ] 목표 카메라 구도, 스프라이트 기울기, 바닥 재질, 그림자 형태의 레퍼런스를 확정한다.

- [ ] **Step 1: Coordinate and scene foundation**
  - [x] 서버 좌표와 Unity 표시 좌표를 변환하는 `GameCoordinateConverter`를 추가한다.
  - [x] 기본 변환을 `server(x, y, z) -> unity(x, z, y)`로 정의하고 역변환도 제공한다.
  - [x] DTO에는 서버 좌표를 그대로 보관하고 스폰/이동/입력 경계에서만 변환한다.
  - [x] GameScene 런타임 프로토타입에서 Perspective 카메라를 구성한다.
  - [ ] 카메라 FOV, 위치, 각도, 클리핑 범위를 16:9 기준으로 설정한다.
  - [x] 기존 배경 SpriteRenderer를 프로토타입 3D 지면 Plane으로 교체한다.
  - [ ] 월드 오브젝트와 UI 레이어/Culling Mask를 분리한다.

- [ ] **Step 2: Popup-book sprite presentation**
  - [ ] 각 ServedObject 루트 아래에 `VisualRoot` 규약을 정의한다.
  - [ ] 서버 동기화는 루트 Transform에 적용하고 SpriteRenderer는 VisualRoot 아래에 둔다.
  - [ ] VisualRoot를 바닥에 세워 카메라 방향에 맞는 고정 기울기를 적용한다.
  - [ ] 완전 Billboard 대신 Y축 방향 또는 카메라 투영 방향만 제한적으로 맞추는 방식을 비교한다.
  - [ ] 기존 `flipX`, SpriteFrameAnimator, 스프라이트 교체 애니메이션을 유지한다.
  - [ ] 캐릭터 피벗을 발 위치로 통일하여 지면 접촉이 흔들리지 않게 한다.
  - [ ] 기존 Shadow 프리팹을 바닥에 눕힌 투명 Quad 또는 Blob Shadow로 전환한다.
  - [ ] 공중 오브젝트는 실제 Unity Y 높이를 사용하고 그림자 크기/알파로 높이를 보조 표현한다.
  - [ ] `SimpleZVisualizer`와 `ZVisualizer`의 가상 높이 투영을 제거하거나 2.5D 표시 계층에 맞게 대체한다.

- [ ] **Step 3: Input, selection, and indicators**
  - [x] 마우스 입력을 `Camera.ScreenPointToRay`와 지면 Plane/Raycast 교차로 변환한다.
  - [x] 지면에서 얻은 Unity 좌표를 서버 좌표로 역변환하여 기존 입력 DTO에 전달한다.
  - [x] 선택용 Collider2D를 런타임 3D BoxCollider로 교체한다.
  - [x] Physics2DRaycaster 의존을 PhysicsRaycaster로 교체한다.
  - [x] 원형 범위 표시기를 지면에 눕힌다.
  - [x] 직선 범위 표시기의 회전 계산을 XY 평면 회전에서 XZ 지면 회전으로 변경한다.
  - [ ] UI 위 클릭이 월드 입력으로 전달되지 않는 기존 차단 동작을 유지한다.

- [ ] **Step 4: Projectiles, effects, and world UI**
  - [ ] 투사체 위치와 방향 계산을 3D 표시 좌표에 맞게 변환한다.
  - [ ] 2D 투사체 스프라이트가 카메라를 향하도록 하되 이동 궤적은 실제 3D 공간에서 처리한다.
  - [ ] 공격 오라, 피격, 회복, 잔상 이펙트의 VisualRoot 기준을 통일한다.
  - [ ] Z축 회전만 가정하는 DOTween 애니메이션을 시각 축 기준 회전으로 변경한다.
  - [ ] HP 바, 이름표, 게이지는 World Space Canvas 또는 화면 추적 UI 중 한 방식으로 통일한다.
  - [ ] sortingOrder 기반 겹침을 깊이 테스트와 명시적 렌더 큐 규칙으로 대체한다.
  - [ ] 투명 스프라이트의 ZWrite, 알파 클리핑, 앞뒤 면 표시 정책을 확정한다.

- [ ] **Step 5: Rendering pipeline decision**
  - [ ] Built-in RP에서 대표 전투 장면의 비주얼과 성능을 측정한다.
  - [ ] 별도 실험 브랜치에서 Unity 2022.3 대응 URP 14를 설치한다.
  - [ ] URP Asset에 WebGL용 그림자 거리, 추가 광원, MSAA, Render Scale 프리셋을 구성한다.
  - [ ] 대표 Sprite 머티리얼과 바닥 머티리얼만 먼저 URP로 변환한다.
  - [ ] Built-in/URP의 로딩 크기, 평균 FPS, 드로우콜, 투명 오버드로우를 비교한다.
  - [ ] 팝업북 스타일에 필요한 조명 이득이 명확할 때만 전체 URP 전환을 확정한다.

- [ ] **Step 6: Prefab migration**
  - [ ] Abstract 계열 프리팹부터 VisualRoot/Shadow/Collider 구조를 표준화한다.
  - [ ] 지상 몹, 공중 몹, 건물, 드롭, 룬, 투사체 순서로 프리팹을 이관한다.
  - [ ] 개별 캐릭터의 전용 SpriteRenderer 참조와 효과 스크립트를 새 구조에 연결한다.
  - [ ] 변환 누락을 탐지하는 Editor 검증 도구 또는 Prefab 검사 테스트를 추가한다.
  - [ ] 기존 Resources 경로와 서버 prefab 문자열을 유지하여 로딩 규약 변경을 피한다.

- [ ] **Step 7: Tests** (Unit tests, manual verification steps)
  - [ ] 좌표 변환과 역변환의 왕복 테스트를 작성한다.
  - [ ] 지면 클릭 위치가 서버 목표 좌표와 허용 오차 내에서 일치하는지 테스트한다.
  - [ ] 스폰, 이동 보간, 방향 전환, 공중 높이, 투사체 도착 위치를 검증한다.
  - [ ] 원형/직선 마법의 사거리와 실제 서버 판정 위치가 일치하는지 확인한다.
  - [ ] 16:9, 16:10, 울트라와이드 해상도에서 카메라와 UI 가림을 확인한다.
  - [ ] PvP, PvE, 관전, 튜토리얼의 GameScene 파생 흐름을 수동 검증한다.
  - [ ] Development WebGL 빌드에서 메모리, 초기 로딩, FPS, 입력 지연을 측정한다.
  - [ ] 투명 스프라이트 겹침, 그림자, 파티클이 많은 전투를 최악 조건으로 측정한다.

- [ ] **Step 8: Rollout / Rollback** (Feature flags, migration steps)
  - [ ] `POPUP_BOOK_3D` 또는 런타임 설정으로 기존 2D 표현과 새 표현을 전환할 수 있게 한다.
  - [ ] 먼저 내부 개발 빌드와 테스트 서버에만 활성화한다.
  - [ ] 대표 마법과 대표 오브젝트가 모두 통과한 뒤 기본값을 2.5D로 변경한다.
  - [ ] 안정화 기간 동안 기존 2D GameScene/프리팹을 유지한다.
  - [ ] 성능 또는 판정 문제가 발생하면 기능 플래그로 즉시 기존 표현으로 복귀한다.

## Validation

- **Commands to run:**
  - `dotnet build WordOnlineClient.sln -v minimal`
  - Unity Edit Mode 테스트: 좌표 변환, 프리팹 구조 검증
  - Unity Play Mode 테스트: 스폰, 이동, 선택, 조준, 투사체
  - `Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildDevWebGL -logFile -`
- **Expected output:**
  - C# 컴파일 오류와 Unity Console 오류가 없다.
  - 서버 좌표와 Unity 표시 좌표의 왕복 오차가 허용 범위 이내다.
  - 기존 2D와 2.5D 모드에서 동일한 입력에 동일한 서버 목표 좌표가 전송된다.
  - 대표 전투 프리팹에 VisualRoot, 지면 피벗, 선택 Collider가 올바르게 구성된다.
  - 목표 WebGL 환경에서 합의된 FPS와 메모리 예산을 충족한다.

## Risks & Rollback

- **Risks:**
  - 좌표 변환을 여러 위치에서 중복 적용하면 위치와 판정이 어긋날 수 있다.
  - 투명 Sprite가 깊이 테스트 및 렌더 순서 때문에 잘못 가려질 수 있다.
  - Perspective 카메라에서는 화면 가장자리의 크기와 조준 체감이 기존과 달라질 수 있다.
  - 모든 Sprite가 같은 방향이면 측면에서 종이 두께가 사라지거나 서로 겹칠 수 있다.
  - 실시간 조명과 그림자는 WebGL 성능 및 배터리 사용량을 크게 높일 수 있다.
  - URP 전환 시 기존 머티리얼, 커스텀 셰이더, 후처리 호환 문제가 발생할 수 있다.
- **Rollback steps:**
  - 기능 플래그를 비활성화하여 기존 Orthographic 2D 표현으로 복귀한다.
  - GameScene과 프리팹 변경은 단계별 커밋으로 분리해 문제 단계만 `git revert`한다.
  - URP는 별도 커밋/브랜치에서 검증하며 채택 전에는 Graphics 설정을 변경하지 않는다.
  - DTO와 서버 프로토콜은 변경하지 않아 클라이언트 표현 계층만 되돌릴 수 있게 한다.

## Open Questions

- 팝업북 카메라는 고정 시점인가, 플레이어를 따라 이동하거나 줌하는가?
- 캐릭터 종이는 카메라를 항상 바라볼 것인가, 책의 한 방향으로 고정할 것인가?
- 바닥과 배경도 기존 2D 아트를 사용할 것인가, 간단한 3D 디오라마 세트를 제작할 것인가?
- 목표 WebGL 기기와 최소 FPS/메모리 예산은 얼마인가?
- URP 도입을 이번 전환 범위에 포함할 것인가, 비주얼 프로토타입 뒤에 결정할 것인가?
