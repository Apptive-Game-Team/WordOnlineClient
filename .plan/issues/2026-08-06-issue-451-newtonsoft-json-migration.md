# 2026-08-06 — Newtonsoft.Json 전면 마이그레이션 + 다형성 DTO + 테스트

- Date: 2026-08-06
- GitHub Issue: #451
- Status: Draft

## Goal

클라이언트 JSON 처리를 `JsonUtility` 에서 `Newtonsoft.Json` 으로 전면 교체하고, 그 과정에서
`JsonUtility` 한계 때문에 우회 구현되어 있던 지점을 다형성 역직렬화로 정리한다. 후속 작업인
서버 프레임 `events` 리스트(WordOnlineServer#356)와 타격 이펙트 방향(#449)이 이 위에 쌓인다.

## Non-goals

- 서버 DTO 변경. 이 작업은 클라이언트 파싱 계층만 건드린다.
- `events` 이벤트 타입 자체 구현. 판별자 인프라만 만들어 두고 이벤트는 후속 이슈에서 추가한다.
- 게임플레이/연출 동작 변경. 순수 리팩터링이며 관찰 가능한 동작은 동일해야 한다.
- 전체 코드베이스 어셈블리 분리. 테스트에 필요한 최소 범위만 어셈블리로 뗀다.

## Context / Constraints

- Unity `2022.3.34f1`, WebGL 타깃, `ProjectSettings/ProjectSettings.asset` 에 `stripEngineCode: 1`.
- 현재 프로젝트에 `.asmdef` 가 **하나도 없다**. 전부 predefined `Assembly-CSharp`.
  Unity 테스트 어셈블리는 `Assembly-CSharp` 를 참조할 수 없으므로, 테스트 대상 코드는
  어셈블리로 분리해야 한다. predefined 어셈블리는 asmdef 를 자동 참조하므로 역방향은 문제없다.
- `JsonUtility` 사용처: `Assets/Scripts` 전역 53곳 / 20+ 파일 (GameScene, LobbyScene, DeckScene,
  LoginScene, RegisterScene, ProfileScene, Adventures, Admin, Data, Global).
- 다형성 우회가 실재하는 지점:
  - `GameScene/Handler/GeneralHandler.cs` — `TypeChecker` 로 `type` 만 먼저 읽고 동일 문자열 재파싱.
  - `GameScene/Dto/Projectile/ProjectileTarget.cs` — 서버의
    `PositionProjectileTarget` / `ReferenceProjectileTarget` 상속 구조를 한 클래스로 평탄화.
  - `Global/Util/JsonHelper.cs` — 최상위 배열 파싱용 `{"Items":...}` 래핑 꼼수.
- 동작 차이 주의: `JsonUtility` 는 누락된 컬렉션 필드에 빈 컬렉션을 주는 경우가 있고
  Newtonsoft 는 `null` 을 준다. `GameScene/Handler/DeltaFrameHandler.cs` 가 null 가드 없이 순회 중.

## Approach (Checklist)

- [ ] **Step 0: Recon** — 완료. 위 Context 에 정리.
- [ ] **Step 1: 의존성과 스트리핑**
  - `Packages/manifest.json` 에 `com.unity.nuget.newtonsoft-json` 추가
  - `Assets/link.xml` 에 DTO 어셈블리 preserve 규칙 추가
- [ ] **Step 2: 직렬화 계층 신설** (`Assets/Scripts/Global/Serialization`, 신규 asmdef)
  - 공용 진입점 하나로 감싸 `Newtonsoft.Json` 직접 호출을 코드 전역에 흩뿌리지 않는다
  - Unity `Vector3` 컨버터 (기본 동작은 `normalized`, `magnitude` 프로퍼티까지 건드림)
  - 판별자 기반 다형 컨버터 (재사용 가능, 후속 `events` 에 그대로 적용)
- [ ] **Step 3: 게임 메시지 계약을 asmdef 로 분리**
  - `GameScene/Dto` 및 그 의존 타입(`Gauge`, `Gizmo`)을 계약 어셈블리로 이동 (네임스페이스 유지)
  - 서버 메시지 다형 계층 도입, `TypeChecker` 이중 파싱 제거
  - `ProjectileTarget` 을 서버와 동일한 상속 구조로 분리
- [ ] **Step 4: 전면 교체**
  - `JsonUtility` 사용처 53곳 교체, `JsonHelper` 제거
  - 교체한 경로 전부 null 컬렉션 가드 추가
- [ ] **Step 5: 테스트** (`Assets/Tests/EditMode`)
  - 서버 실제 응답 형태 픽스처로 프레임/싱크/투사체/스냅샷 역직렬화 검증
  - 판별자 분기, 알 수 없는 `type`, 누락 필드, `null` 컬렉션 케이스
  - `Vector3` 컨버터 왕복
- [ ] **Step 6: PR** — `feature/451` → `main`, 이슈 #451 참조

## Validation

- **Commands to run:**
  - Edit Mode 테스트: `Unity -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -`
  - WebGL 빌드: `Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildDevWebGL -logFile -`
- **Expected output:** 테스트 전부 통과, WebGL 빌드 성공.
- **Manual:** 로그인 → 로비 → 매칭 → 인게임 프레임/싱크 수신 → 결과 화면까지 1회 통과.
  덱 화면, 어드민 디버그 세션 진입도 확인.

## Risks & Rollback

- **Risks:**
  - IL2CPP 스트리핑으로 에디터는 통과하고 WebGL 빌드에서만 파싱이 깨지는 유형. `link.xml` 로 방어하되 빌드 검증 필수.
  - null 컬렉션 동작 차이로 인한 `NullReferenceException` 회귀.
  - 인증/로비/덱 등 게임 외 경로까지 범위가 넓어 수동 검증 누락 가능.
- **Rollback steps:** 브랜치 단위 `git revert`. 서버 변경이 없어 클라이언트만 되돌리면 된다.

## Open Questions

- 없음. 범위(전면 교체)와 다형성 대상은 확정됨.
