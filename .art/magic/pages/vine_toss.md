# 덩굴 투척

- 서버 키: `vine_toss`
- 현재 이름: 덩굴 투척
- 컨셉 이름: 솟구치는 덩굴
- 시전 계열: 투사체
- 진영: 세계수 풀 정령
- 상태: game 서버 구현 확인

## 컨셉

세계수 풀 정령의 방향성과 궤적이 핵심인 솟구치는 덩굴.

## 컨셉 설명

솟구치는 덩굴은 세계수 풀 정령의 투사체 마법이다. 시전자에서 목표 방향으로 투사체 또는 연속 개체를 보낸다. 전투에서는 직접 공격 역할을 맡으며, 대상 제한을 이 파일에서 직접 확인하지 못함 대상을 직선 연속 방식으로 다룬다. 추가 특수 이동은 없다. 시각적으로는 잎·씨앗·덩굴의 겹친 컷페이퍼 형태로 생장 방향을 보여준다.

## 설명

플레이어가 사용하는 **투사체 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 시전자에서 목표 방향을 구한다.
2. 첫 덩굴을 전방 1칸에 생성한다.
3. 같은 방향으로 총 6개 덩굴을 0.12초 간격으로 연속 생성한다.
4. 한 연쇄 안에서 같은 대상을 중복 타격하지 않도록 명중 대상을 공유 추적한다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 투사체·전파형 | 시전자에서 목표 방향으로 투사체 또는 연속 개체를 보낸다. |
| 전투 역할 | 직접 공격 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 직선 연속 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 대상 제한을 이 파일에서 직접 확인하지 못함 | `TargetMask` 기준 |
| 특수 이동·행동 | 추가 특수 이동 없음 | 부착 컴포넌트 기준 |
| 생명주기 | 제한시간 후 소멸 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `VineTossMagic` |
| 상위 클래스 | `Magic` |
| 주 프리팹 | `Vine` |
| 부가 프리팹 | 없음 또는 소스에서 직접 확인되지 않음 |
| 부착 컴포넌트 | `CircleCollider`, `EffectProvider`, `TimedSelfDestroyer`, `OnStartAttacker`, `VineHitTracker`, `SequentialLineSpawner` |
| 파라미터 키 | `RADIUS`, `DURATION`, `DAMAGE` |
| 오브젝트 파라미터 | `VINE` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

잎·씨앗·덩굴의 겹친 컷페이퍼 형태로 생장 방향을 보여준다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 현재 확인된 세계관 충돌 없음

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/shoot/VineTossMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/subprefab/VinePrefabInitializer.java`
