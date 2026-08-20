# 2026-08-20 — 훈수(코치) 힌트 시스템 추가

- Date: 2026-08-20
- GitHub Issue: #511
- Status: In progress

## Goal

플레이 중 유저가 막힌 상황을 감지해서 힌트 메시지를 띄우고 관련 UI를 테두리로 강조하는 훈수 시스템을 추가한다. 로비 Setting Page에서 켜고 끌 수 있어야 한다.

## Non-goals

- 온보딩 튜토리얼(`InteractiveTutorialScene`) 흐름 변경. 훈수는 튜토리얼과 별개이며 튜토리얼 진행 중에는 완전히 억제된다.
- 서버 측 변경. 전부 클라이언트 로컬 판단이다.
- 새 힌트 UI 디자인. 기존 `Assets/Prefabs/UI/Tutorial/TutorialMessagePanel.prefab`을 재사용한다.
- 인게임 일시정지 메뉴의 옵션 항목 추가. 이번에는 로비 Setting Page만 다룬다.

## Context / Constraints

### 재사용할 기존 자산

| 자산 | 위치 | 훈수에서 쓰는 방식 |
|---|---|---|
| `TutorialPanel` | `Assets/Scripts/TutorialScene/TutorialPanel.cs` | 메시지 표시 패널. `Show(key, onNext: null, side)`로 호출하면 다음 버튼이 자동으로 꺼지므로 비차단 힌트에 그대로 맞는다. 마스크와 클릭 차단은 `SceneTutorialController`가 담당하므로, 훈수는 `SceneTutorialController`를 상속하지 않고 `TutorialPanel`만 직접 참조한다. |
| `TutorialMessagePanel.prefab` | `Assets/Prefabs/UI/Tutorial/` | 훈수 패널 인스턴스로 재사용 |
| `MagicHelperUI` | `Assets/Scripts/GameScene/Card/MagicHelperUI.cs` | `RefreshSuggestions()`가 손패 기준 추천 마법 3개를 뽑고, `OnSuggestionClicked`가 필요한 카드를 `CardUI.SetHighlighted(true)`로 강조한다. 마법 실패/미사용 힌트가 이걸 호출한다. |
| `CardUI.SetHighlighted` | `Assets/Scripts/GameScene/Card/CardUI.cs:89` | 내부에서 `Outline.enabled`를 토글한다. 훈수의 범용 강조도 같은 방식(`Outline` + 펄스)으로 만든다. |
| `SoundData` + `SoundDataSetter` | `Assets/Scripts/Data/Sound/`, `Assets/Scripts/LobbyScene/SettingPage/` | `PlayerPrefs` 옵션 저장 + Setting Page 위젯 배선 패턴. `CoachData` / `CoachDataSetter`가 이 구조를 그대로 따른다. |
| `Onboarding` 로컬라이제이션 테이블 | `Assets/Localization/Onboarding/` | `TutorialPanel`이 테이블 이름 `"Onboarding"`을 상수로 갖고 있다. 새 테이블을 만들면 `TutorialPanel`을 고쳐야 하므로, 훈수 문구를 이 테이블에 `coach.*` 키로 추가한다. |

### 상태 신호는 대부분 폴링으로 충분하다

`CardInputSender`가 이미 필요한 상태를 공개하고 있어서 이벤트를 새로 만들 필요가 거의 없다.

- `IsFieldSelectMode()` — 시전 위치 선택 대기 여부
- `CanSelectField` — 카드가 1장 이상 선택된 상태
- `IsWaitingInputResponse()` — 서버 응답 대기 중

예외 두 가지는 훅이 필요하다.

- **마나바 개방 여부**: `GameScene/BarController.cs`의 `isActive`가 private이다. `public bool IsBarOpen => isActive;`를 추가한다.
- **마법 실패**: 두 경로가 있다.
  - `CardInputSender.Confirm()`의 `!combinedMagicResolver.CanResolve(...)` 분기 (조합 불가)
  - `CardInputSender.HandleInputResponse()`의 `else` 분기 (서버가 거절, `SystemMessageUI`로 메시지 표시)

  두 곳에서 `static event Action OnMagicFailed`를 발행한다.

### 강조 대상은 타입으로 찾는다

대상마다 컴포넌트 타입이 유일하므로 씬에 참조를 배선하지 않고 `FindObjectOfType`으로 지연 해석한다. 씬 파일 수정을 최소화하는 것이 목적이다.

| 대상 | 해석 방법 |
|---|---|
| 마나 바 버튼 | `BarController`의 `manaBarButton` (새 public 접근자 추가) |
| 주문 버튼 | `FindObjectOfType<MagicCombineButton>()` |
| 시전 필드 | `FindObjectOfType<FieldSelector>()` |
| 추천 마법 목록 | `FindObjectOfType<MagicHelperUI>()`의 `suggestionRoot` (새 public 접근자 추가) |
| 봇 전투 버튼 | `FindObjectOfType<PracticeButton>()` |

해석 결과는 캐시하되 대상이 파괴되면 다시 찾는다.

### 튜닝 수치

`GameScene/GameConfig.cs`는 `const`만 담는 정적 클래스이고, `ParametersDataSource`는 서버가 내려주는 파라미터라 성격이 다르다. 훈수 수치는 `CoachTuning` 정적 상수 클래스에 기본값을 두고, `CoachDirector`의 `[SerializeField]` 필드가 그 값을 기본값으로 초기화하도록 한다. 인스펙터에서 빌드 없이 조정할 수 있다.

## Approach (Checklist)

### Step 0: Recon (완료)

- [x] `TutorialPanel` / `SceneTutorialController` 구조 확인 — 패널만 떼어 쓸 수 있음
- [x] `MagicHelperUI` 추천 + 하이라이트 이미 구현됨 확인
- [x] `CardInputSender` 공개 상태 확인 — 폴링 가능
- [x] `SoundData` / `SoundDataSetter` 옵션 패턴 확인
- [x] `Onboarding` 로컬라이제이션 테이블이 손으로 편집 가능한 YAML임 확인 (`m_Id` 최대값 뒤에 이어서 추가)
- [x] 워크트리 생성: `/home/yunseong/dev/worktrees/client-feature-511`, 브랜치 `feature/511`

### Step 1: 옵션 저장과 Setting Page 토글

- [x] `Assets/Scripts/Data/Coach/CoachData.cs` 신규
  - `SoundData`와 동일 구조. `PlayerPrefs` 키 `coach.enabled` (int, 기본 1)
  - `public static bool Enabled`, `Load()`, `Save()`
  - 숙달 카운터도 여기서 관리: 키 `coach.satisfied.<ruleId>`, `GetSatisfiedCount(id)` / `IncreaseSatisfied(id)` / `IsRetired(id)`
  - `ResetAll()` — 디버그 및 테스트용
- [x] `Assets/Scripts/LobbyScene/SettingPage/CoachDataSetter.cs` 신규
  - `[SerializeField] Toggle coachToggle`
  - `Awake`에서 현재 값으로 초기화, `onValueChanged`에서 `CoachData.Enabled` 갱신 후 즉시 `Save()` (토글은 드래그가 없어 `SoundDataSetter`의 지연 저장 코루틴이 필요 없다)
- [x] `LobbyUI` 테이블에 토글 라벨 키 `CoachHint` 추가 (한국어 "훈수" / 영어 "Hints")
- [ ] Setting Page 패널에 Toggle 배치 — Step 5의 에디터 메뉴로 처리한다

### Step 2: 훈수 코어

판단 로직은 엔진에 기대지 않는 순수 클래스로 떼어냈다. `Assets/Tests/EditMode`의 테스트
어셈블리가 `WordOnline.Contracts`만 참조하므로, 검증하려면 그 어셈블리 안에 있어야 한다.

- [x] `Assets/Scripts/Contracts/Coach/CoachRuleId.cs` — enum. `PlayerPrefs` 키에도 이 이름을 쓴다
- [x] `Assets/Scripts/Contracts/Coach/CoachTuning.cs` — 기본 상수

  | 항목 | 값 |
  |---|---|
  | 전역 쿨다운 | 12초 |
  | 표시 상한 | 8초 |
  | 씬 진입 유예 | 5초 |
  | 숙달 판정 창 | 5초 |
  | 숙달 은퇴 기준 | 3회 |
  | 백오프 단계 | 30 / 60 / 120초 (상한 120) |

- [x] `Assets/Scripts/Contracts/Coach/CoachScheduler.cs` — 언제 무엇을 띄우고 내릴지 결정하는 순수 상태 기계.
  시간을 인자로만 받고 `CoachAction`(`None` / `Show` / `Hide`)을 돌려준다.
  따른 것으로 판정된 규칙은 `TryDequeueSatisfied`로 꺼낸다
- [x] `Assets/Scripts/Global/Coach/ICoachRule.cs` — 규칙은 "지금 문제가 있는가"만 답한다

  ```csharp
  public interface ICoachRule
  {
      CoachRuleId Id { get; }
      string MessageKey { get; }
      int Priority { get; }            // 낮을수록 우선
      float DwellSeconds { get; }
      int MaxShowsPerSession { get; }
      bool ShowPanelOnRight { get; }
      bool IsActive();                 // 문제가 지금 존재하는가
      Transform[] ResolveTargets();
      void OnShown();
      void OnHidden();
  }
  ```

  별도의 `IsSatisfied`는 두지 않았다. 힌트가 떠 있는 동안 `IsActive`가 거짓으로 바뀌는 것이
  곧 유저가 힌트를 따랐다는 뜻이라, 판정을 한 곳에 모으는 편이 규칙마다 중복을 만들지 않는다.

- [x] `Assets/Scripts/Global/Coach/ICoachRuleProvider.cs` — 씬별 규칙 공급자.
  `CoachDirector`가 같은 오브젝트의 공급자에서 규칙을 모으므로 씬마다 규칙 목록을 하드코딩하지 않는다
- [x] `Assets/Scripts/Global/Coach/ICoachRuleLifecycle.cs` — 정적 이벤트를 구독하는 규칙의 구독과 해제
- [x] `Assets/Scripts/Global/Coach/CoachHighlighter.cs`
  - 대상에 `Outline`을 붙이거나 기존 것을 켜고 DOTween으로 알파 펄스
  - `CardUI`처럼 자기 `Outline`을 가진 대상은 켜져 있었는지와 색을 기억해 원상 복구한다
- [x] `Assets/Scripts/Global/Coach/CoachDirector.cs` — `LocalSingletonObject<CoachDirector>`.
  옵션이 꺼져 있거나 온보딩 진행 중이면 통째로 억제하고, 매 프레임 규칙 상태를 스케줄러에 넣은 뒤
  결과대로 패널과 강조를 켜고 끈다. 숙달 누적은 `CoachData`에 쌓는다
- [x] `Assets/Tests/EditMode/CoachSchedulerTests.cs` — dwell, 쿨다운, 백오프, 숙달 판정, 세션 상한,
  은퇴, 우선순위를 덮는 14개 테스트

### Step 3: 규칙 6종

`Assets/Scripts/GameScene/Coach/` 아래 게임 씬 규칙 5종, `Assets/Scripts/LobbyScene/Coach/` 아래 로비 규칙 1종.

| 클래스 | 우선순위 | 조건 (`IsActive`) | 충족 (`IsSatisfied`) | dwell | 세션 상한 |
|---|---|---|---|---|---|
| `FieldSelectIdleRule` | 1 | `IsFieldSelectMode()` | 필드 선택 모드 해제 | 6초 | 3 |
| `CombineButtonIdleRule` | 2 | `CanSelectField && !IsFieldSelectMode()` | 필드 선택 모드 진입 | 8초 | 3 |
| `MagicFailingRule` | 3 | 연속 실패 3회 누적 | 시전 성공 | 즉시 | 2 |
| `MagicUnusedRule` | 4 | 마지막 카드 사용 후 경과 시간 | 카드 사용 | 25초 | 2 |
| `ManaBarUnopenedRule` | 5 | 매치 시작 후 `BarController.IsBarOpen`이 한 번도 참이 아님 | 마나바 개방 | 20초 | 2 |
| `LobbyIdleRule` | 6 | 무입력 + 매칭 큐 비어 있음 | 봇 전투 시작 또는 매칭 진입 | 45초 | 2 |

`MagicFailingRule`과 `MagicUnusedRule`은 `OnShown()`에서 `MagicHelperUI.RefreshSuggestions()`를 호출하고 최상위 추천의 카드들을 강조한다. 이 강조는 `MagicHelperUI.OnSuggestionClicked`가 이미 하는 일이므로 그 경로를 재사용할 수 있게 필요한 부분을 public으로 노출한다.

### Step 4: 기존 스크립트 훅

- [x] `Assets/Scripts/GameScene/BarController.cs` — `IsBarOpen`, `ManaBarButtonTransform` 접근자 추가
- [x] `Assets/Scripts/GameScene/Card/CardInputSender.cs`
  - `public static event Action OnMagicFailed` — `Confirm()`의 조합 불가 분기와 `HandleInputResponse()`의 거절 분기에서 발행
  - `public static event Action OnMagicSucceeded` — `HandleInputResponse()`의 소비 분기에서 발행
  - `public static event Action OnCardUsed` — `TryUseCard()`에서 발행
  - 정적 이벤트이므로 씬 전환 시 구독 해제를 반드시 확인한다 (`CoachDirector.OnDisable`)
- [x] `Assets/Scripts/GameScene/Card/MagicHelperUI.cs` — `suggestionRoot` 접근자, 추천 강조를 외부에서 호출할 진입점 추가

### Step 5: 씬과 프리팹

씬과 프리팹 YAML을 손으로 고치면 깨지기 쉬워서, 배선을 에디터 메뉴로 만들었다.

- [x] `Assets/Scripts/Editor/CoachSceneSetup.cs` 신규
  - `Tools/Coach/Setup Coach In Active Scene` — 현재 씬에 `CoachSystem` 오브젝트를 만들고
    `CoachDirector`, `CoachHighlighter`, 씬에 맞는 규칙 공급자를 붙인 뒤
    `TutorialMessagePanel.prefab`을 Canvas 아래에 인스턴스로 넣고 참조까지 연결한다
  - `Tools/Coach/Add Hint Toggle To Settings Panel` — `Assets/Prefabs/UI/Lobby/Panal.prefab`의
    `SoundSliders` 아래에 Toggle을 만들고 `CoachDataSetter`에 연결한다
- [ ] `GameScene.unity`에서 메뉴 실행 후 저장 (에디터 필요)
- [ ] `LobbyScene.unity`에서 메뉴 실행 후 저장 (에디터 필요)
- [ ] Toggle 메뉴 실행 후 위치와 라벨 정리 (에디터 필요)

### Step 6: 로컬라이제이션

- [x] `Assets/Localization/Onboarding/Onboarding Shared Data.asset`에 `coach.*` 키 6개 추가 (기존 최대 `m_Id` 다음 번호부터)
- [x] `Onboarding_ko-KR.asset` / `Onboarding_en.asset`에 같은 `m_Id`로 문구 추가

  | 키 | 한국어 |
  |---|---|
  | `coach.manaBar` | 마나 바를 클릭해서 띄워 보세요. |
  | `coach.magicUnused` | 테두리가 강조된 카드를 골라 마법을 써 보세요. |
  | `coach.magicFailing` | 조합이 맞지 않아요. 강조된 카드로 다시 시도해 보세요. |
  | `coach.combineButton` | 주문 버튼을 눌러 시전해 보세요. |
  | `coach.fieldSelect` | 마법을 시전할 위치를 클릭해 보세요. |
  | `coach.lobbyIdle` | 봇 전투로 연습해 보세요. |

### Step 7: 버전과 마무리

- [x] `ProjectSettings/ProjectSettings.asset`의 `bundleVersion`을 `0.1.1` → `0.2.0`으로 올린다 (하위 호환 신기능이므로 MINOR)
- [ ] PR 생성 (`--base main --assignee @me --label feature`, 이슈 #511 참조)

## Validation

- **Commands to run:**
  - Unity 에디터에서 컴파일 오류 없음 확인 (`Assets/Scripts/Editor/BuildScript.cs`의 `BuildDevWebGL`은 CI에서 돌므로 로컬은 에디터 컴파일로 갈음)
  - `Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildDevWebGL -logFile -` (선택, 시간이 오래 걸림)
- **Expected output:** 컴파일 오류 0건

- **수동 검증 시나리오**
  1. 로비 Setting Page에서 훈수 토글이 보이고, 껐다 켜고 씬을 재시작해도 값이 유지된다
  2. 훈수를 끈 상태로 게임에 들어가면 어떤 힌트도 뜨지 않는다
  3. 게임 시작 후 아무것도 하지 않으면 20초쯤에 마나 바 힌트가 뜨고 마나 바 버튼이 강조된다
  4. 마나 바를 열면 그 프레임에 힌트가 사라진다
  5. 카드를 고르고 8초 대기하면 주문 버튼 힌트가 뜬다. 주문 버튼을 누르면 사라진다
  6. 주문 버튼을 눌러 필드 선택 모드로 들어간 뒤 6초 대기하면 시전 위치 힌트가 뜬다
  7. 조합 불가 카드로 3회 연속 실패하면 추천 마법 힌트가 뜨고 손패의 해당 카드가 강조된다
  8. 힌트가 연달아 뜨지 않는다 (하나가 사라진 뒤 최소 12초 간격)
  9. 힌트를 8초간 무시하면 자동으로 사라지고, 다음 노출은 30초 뒤부터 가능하다
  10. 같은 힌트를 3번 따르면 그 뒤로는 다시 뜨지 않는다. `PlayerPrefs` 초기화 후에는 다시 뜬다
  11. 온보딩 튜토리얼 진행 중에는 훈수가 뜨지 않는다
  12. 로비에서 45초 무입력이면 봇 전투 버튼이 강조된다

## Risks & Rollback

- **Risks:**
  - **힌트 남발.** 전역 쿨다운과 백오프가 제대로 걸리지 않으면 화면을 계속 가린다. 수동 검증 8번과 9번이 이 위험을 직접 겨눈다.
  - **씬 YAML 손상.** `GameScene.unity`와 `LobbyScene.unity`를 손으로 편집하면 깨질 수 있다. Unity MCP를 쓰고, 안 되면 에디터 작업을 요청한다.
  - **정적 이벤트 누수.** `CardInputSender`에 정적 이벤트를 추가하므로 씬 전환 시 구독이 남으면 파괴된 오브젝트를 참조한다. `CoachDirector.OnDisable`에서 반드시 해제한다.
  - **`Outline` 원상 복구 실패.** `CoachHighlighter`가 강조 해제 시 원래 상태로 되돌리지 않으면 카드 테두리가 남는다. `CardUI`가 자체 `Outline`을 쓰므로 특히 주의한다.
  - **로컬라이제이션 `m_Id` 충돌.** 세 파일(`Shared Data`, `ko-KR`, `en`)의 `m_Id`가 어긋나면 문구가 엉킨다. 추가 후 세 파일의 신규 `m_Id` 집합이 동일한지 확인한다.
  - **WebGL `PlayerPrefs` 지연.** IndexedDB 플러시 때문에 저장이 늦을 수 있다. 토글은 변경 즉시 `Save()`를 호출한다.

- **Rollback steps:**
  - `CoachData.Enabled` 기본값을 `false`로 바꾸면 코드 되돌리기 없이 기능 전체가 꺼진다 (사실상의 기능 플래그)
  - 그래도 문제가 남으면 `git revert`로 PR 머지 커밋을 되돌린다

## Open Questions

- 훈수 패널의 화면 위치를 좌우 중 어디로 고정할지. `TutorialPanel.SetSide`가 좌우만 지원하므로 일단 게임 씬은 왼쪽, 로비는 오른쪽으로 두고 플레이해 보며 조정한다.
- 인게임 일시정지 메뉴에도 토글을 노출할지. 이번 범위에서는 제외했다.
