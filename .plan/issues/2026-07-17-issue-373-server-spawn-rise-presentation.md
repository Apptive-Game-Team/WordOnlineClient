# 2026-07-17 — 서버 생성 오브젝트 등장 연출

- Date: 2026-07-17
- GitHub Issue: #373
- Status: Implemented (Play Mode visual QA pending)

## Goal

- 서버의 신규 생성 이벤트로 나타난 전투 오브젝트가 카메라 화면의 가로축(screen X)을 경첩 축으로 삼아, 팝업북 그림처럼 바닥에 납작하게 누운 상태에서 빠르게 일어나는 DOTween 등장 연출을 재생한다.
- 건물류는 일어나는 동작의 착지 시점에 짧은 먼지구름을 생성하고 자연스럽게 확산·페이드아웃한다.
- 기존 이동, 공격, idle DOTween 및 서버 동기화용 루트 Transform과 충돌하지 않도록 시각 계층만 애니메이션한다.

## Non-goals

- 투사체, Drop/Explode/Field/Rune 같은 단발성 마법 오브젝트에 등장 회전을 일괄 적용하지 않는다.
- 서버 판정, 충돌체, 실제 위치 동기화, HP 바와 팀 표시의 좌표를 등장 연출에 종속시키지 않는다.
- 이번 작업에서 전체 이펙트 풀링 시스템을 새로 만들지 않는다. 대량 생성 성능 문제가 측정되면 먼지 프리팹 풀링을 후속 작업으로 분리한다.

## Context / Constraints

- 신규 생성 경로는 `DeltaFrameHandler -> ObjectSpawner.SpawnObject`이며, 스냅샷 복원도 `ObjectSyncer -> ObjectSpawner.SpawnObject`를 공유한다.
- `ObjectSpawner`는 이미 `PopupBookVisualPresenter.Attach(servedObject)`를 호출한다. 이 presenter는 `LateUpdate`에서 visual root를 `Camera.main.transform.rotation`으로 맞추므로, 그 하위 피벗의 `local X`는 항상 카메라 화면의 가로축과 일치한다.
- `GameScene` 카메라는 Perspective이며 월드 X축 기준 약 45도 기울어져 있다. 따라서 월드 X 회전을 직접 적용하기보다 카메라 정렬 presenter 아래에서 local X를 회전해야 카메라 변경에도 팝업북 방향이 유지된다.
- `ObjectSpawner`는 `Resources/Prefabs/{type}`을 로드하며 유닛/건물 이외의 서버 오브젝트도 생성하므로 스포너에서 타입 문자열로 건물을 추측하면 유지보수가 어렵다.
- 유닛과 건물 대부분은 `AbstractMeleeMob`, `AbstractRangeMob`, `AbstractAerialMob`, `AbstractSlime`, `AbstractBuild` 프리팹 Variant 계층을 사용한다. 연출 대상 여부와 세부 설정은 프리팹 컴포넌트가 명시하는 편이 현재 구조와 맞다.
- 기존 `HoppingMotionController`, `CrawlMotionController` 등은 자식 Sprite Transform을 `Awake`부터 계속 움직인다. 등장 연출은 별도의 지면 피벗 wrapper를 회전해 같은 Transform 프로퍼티에 대한 tween 충돌을 피해야 한다.
- 현재 Sprite 피벗은 대표적으로 중앙(`ElectricTower`: 0.5, 0.5)이어서 Sprite 자체를 90도 회전하면 지면에 눕는 대신 중앙을 축으로 돈다.
- `SpawnPopInEffect`는 일부 마법 프리팹에서 `Awake` 재생되는 기존 스케일 연출이다. 이 동작은 유지하고 신규 유닛/건물 등장 연출과 대상을 분리한다.

## Approach (Checklist)

- [x] **Step 0: Recon**
  - [x] 실제 `AbstractBuild` 파생 prefab 목록을 기준으로 먼지 대상 건물 타입을 확정한다.
  - [x] 추상 프리팹의 visual root, shadow, HP bar 계층과 기존 `PopupBookVisualPresenter` 삽입 구조를 확인한다.
  - [x] 델타 신규 생성에서만 연출하고 스냅샷 복원은 생략한다.

- [x] **Step 1: Implementation**
  - [x] `PopupBookVisualPresenter.Attach`가 `SpawnPresentationPivot`을 생성하고 actual visual을 그 아래에 배치하도록 확장했다.
    - [x] `logical root -> PopupBookVisualPresenter(camera aligned) -> SpawnPresentationPivot(local X tween) -> actual visual` 계층을 구성했다.
    - [x] `Attach`가 presenter를 반환하고 `ObjectSpawner` 등록 성공 뒤 `PlaySpawnPresentation`을 호출한다.
    - [x] Sprite local bounds 하단으로 경첩 오프셋을 계산하고 upright 위치가 보존되도록 상쇄한다.
    - [x] 카메라 정렬 root와 local X 등장 pivot을 분리했다.
  - [x] 카메라 기준 local X `-84° -> 0°`, 0.42초 `Ease.OutBack` 등장 연출을 구현했다.
  - [x] Tween 중단/파괴/완료 시 pivot 회전과 World Space UI 활성 상태를 복원한다.
  - [x] `BuildingSpawnDustEffect`가 런타임 소프트 원형 Sprite를 한 번 생성하고 6개 먼지 조각을 좌우 확산·확대·페이드한 뒤 스스로 제거한다.
  - [x] `SpawnPresentationTypeCatalog`에 현재 `AbstractBuild` 파생 타입과 구조물 형태의 `FrenzyTotem`, `RallyingTorch`를 격리했다.
  - [x] `ObjectSpawner.SpawnObject`에 `playSpawnPresentation` 인자를 추가하고 `ObjectSyncer` 복원 경로는 false로 호출한다.

- [ ] **Step 2: Tests**
  - [x] `dotnet build Assembly-CSharp.csproj -v minimal` 컴파일 성공 및 Unity Asset Pipeline 스크립트 컴파일 오류 없음 확인.
  - [ ] Unity Edit Mode 테스트를 추가할 경우 `Assets/Tests/EditMode`에서 생성 사유에 따른 재생 여부와 컴포넌트 미존재 fallback을 검증한다.
  - [ ] Play Mode/Editor에서 근접, 원거리, 공중, slime, 건물, PvE 예외 프리팹을 좌/우 master 각각 생성한다.
  - [ ] 생성 직후 같은 프레임에 위치/HP/status 업데이트가 와도 루트 위치, collider, HP bar가 흔들리지 않는지 확인한다.
  - [ ] 공격/idle tween이 등장 연출 도중 시작돼도 자식 visual의 Z 회전·이동과 부모 pivot의 X 회전이 자연스럽게 합성되는지 확인한다.
  - [ ] 카메라가 45도인 현재 GameScene에서 화면 가로축을 따라 접히며, 월드축 기준 옆으로 쓰러지는 모션처럼 보이지 않는지 확인한다.
  - [ ] reconnect/스냅샷 복원 시 기존 전장 전체가 동시에 일어나는 연출을 하지 않는지 확인한다.
  - [ ] 건물을 연속 생성하고 파괴해 먼지 오브젝트와 DOTween sequence가 Hierarchy에 남지 않는지, WebGL 프레임 드롭이 없는지 확인한다.

- [ ] **Step 3: Rollout / Rollback**
  - [ ] 연출 파라미터는 프리팹 직렬화 값으로 조절 가능하게 해 코드 변경 없이 튜닝한다.
  - [ ] 문제 발생 시 각 프리팹의 `SpawnRiseEffect`를 disable하면 생성/동기화 로직은 그대로 동작하게 유지한다.
  - [ ] 최종 구현 전 GitHub issue를 생성하고 저장소 규칙에 맞는 `<issue-label>/<issue-num>` 브랜치로 작업한다.

## Validation

- **Commands to run:**
  - `dotnet build Assembly-CSharp.csproj -v minimal`
  - Unity Editor Play Mode 수동 생성 테스트(Delta create, Snapshot restore, 좌/우 master, 연속 건물 생성)
  - 가능하면 WebGL Development Build에서 먼지 sorting/성능 확인
- **Expected output:**
  - C# 컴파일 오류 없음.
  - 선택된 유닛/건물만 1회 일어나며, 건물만 착지 먼지가 재생됨.
  - 서버 좌표, collider, shadow, HP bar 및 지속 idle tween에 시각적/기능적 회귀가 없음.

## Risks & Rollback

- **Risks:**
  - 중앙 피벗 Sprite를 직접 회전하면 바닥에서 미끄러지거나 공중에서 회전해 보일 수 있다.
  - 기존 무한 tween과 동일 Transform을 제어하면 DOTween 값 경쟁으로 점프/회전이 튈 수 있다.
  - Snapshot restore까지 자동 재생하면 입장/재접속 때 모든 오브젝트가 동시에 애니메이션된다.
  - Variant가 아닌 예외 prefab을 누락하면 일부 서버 오브젝트만 연출이 없다.
  - 먼지 sorting layer 또는 scale이 건물 크기별로 맞지 않을 수 있다.
- **Rollback steps:** `SpawnRiseEffect` 컴포넌트를 프리팹에서 비활성화하거나 관련 커밋을 revert한다. 스포너는 컴포넌트 미존재를 정상 경로로 처리한다.

## Open Questions

- 스냅샷에서 처음 발견된 오브젝트도 등장 연출을 보여줄지, 오직 delta `objects.create`만 보여줄지 결정이 필요하다. 권장안은 delta만 재생이다.
- 카메라 쪽으로 넘어져 있다가 뒤로 일어날지(`+X`), 카메라 반대쪽에서 앞으로 일어날지(`-X`) 결정이 필요하다. 팝업북 느낌에는 카메라 쪽으로 누웠다가 일어나는 한 방향 고정안을 권장한다.
- 먼지 색상/크기를 모든 건물 공통으로 할지, 건물별 override가 필요한지 아트 방향 확인이 필요하다.
- 최종 대상에 건물/소환수 외 `Rune`, `Field`, `Drop` 계열도 포함되는지 범위를 확정해야 한다.
