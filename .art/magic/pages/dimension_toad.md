# 경계 운반자

- 서버 키: `dimension_toad`
- 공개 이름: 경계 운반자
- 주 소환수 컨셉 이름: 경계 운반자
- 시전 계열: 소환
- 진영: 차원 유랑종
- 상태: game 서버 구현 확인

## 컨셉

차원 사이를 이동하며 무너진 세계를 품는 경계 운반자와 두 독립 유랑종을 부른다.

## 컨셉 설명

경계 운반자는 직접 공격하지 않고 위협에서 물러난다. 몸 안에 품은 세계의
경계를 열어 10초마다 `FireTadpole` 화산편과 `LightningTadpole` 폭풍편을
번갈아 부른다. 세 개체는 부모·자식이 아닌 서로 다른 독립 종이다.

## 설명

플레이어가 사용하는 **소환 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 직접 공격하지 않고 지상 위협에서 거리를 벌린다.
2. 화산편과 폭풍편으로 이어지는 통로를 10초마다 번갈아 열어 제한 없이 부른다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 지상 이동형 | 이동 AI가 지면 경로와 `RigidBody` 속도를 사용한다. |
| 전투 역할 | 차원 유랑종 호출 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 10초 간격 교대 소환 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 지상 위협 감지 | `TargetMask` 기준 |
| 특수 이동·행동 | 위협에서 도주·두 독립 종 무제한 호출 | 부착 컴포넌트 기준 |
| 생명주기 | HP 소진 시 파괴 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `DimensionToadMagic` |
| 상위 클래스 | `AbstractSingleSpawnMagic` |
| 주 프리팹 | `DimensionToad` |
| 부가 프리팹 | `FireTadpole`, `LightningTadpole` |
| 부착 컴포넌트 | `RigidBody`, `ZPhysics`, `CircleCollider`, `NonAttackingCowardMob`, `LimitedSequenceSpawner`, `CommonEffectReceiver` |
| 파라미터 키 | `MASS`, `RADIUS`, `HP`, `SPEED`, `ATTACK_INTERVAL`, `DETECTION_RANGE`, `PANIC_DURATION` |
| 오브젝트 파라미터 | `DIMENSION_TOAD` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

승인 방향은 B — 움직이는 세계 조각이다. 어두운 보라색 각면 갑각과 옅은 석재
띠를 공통 문법으로 쓰고, 각 개체가 품은 세계와 실루엣은 분리한다. 두꺼비나
올챙이 해부 구조를 사용하지 않는다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 컨셉 참조: `.art/concept/dimensional-wanderers/`
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 현재 확인된 세계관 충돌 없음

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/spawn/DimensionToadMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/misc/DimensionToadPrefabInitializer.java`
