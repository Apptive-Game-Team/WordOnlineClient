# 차원 두꺼비

- 서버 키: `dimension_toad`
- 현재 이름: 차원 두꺼비
- 컨셉 이름: 균열두꺼비
- 시전 계열: 소환
- 진영: 차원 유랑종
- 상태: game 서버 구현 확인

## 컨셉

차원 유랑종의 독립 개체로 전장에 합류하는 균열두꺼비.

## 컨셉 설명

균열두꺼비는 차원 틈새를 떠도는 야생 생물이다. 다른 차원의 마력을 먹고 알에 저장하며, `FireTadpole`과 `LightningTadpole`을 10초마다 번갈아가며 제한 없이 소환한다. 잿불 올챙이와 뇌광 올챙이는 악마나 정령이 아니라 흡수한 마력에 적응한 균열두꺼비의 새끼다.

## 설명

플레이어가 사용하는 **소환 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 직접 공격하지 않고 지상 위협에서 거리를 벌린다.
2. `FireTadpole`과 `LightningTadpole`을 10초마다 번갈아가며 제한 없이 소환한다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 지상 이동형 | 이동 AI가 지면 경로와 `RigidBody` 속도를 사용한다. |
| 전투 역할 | 원소 올챙이 소환 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 10초 간격 교대 소환 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 지상 위협 감지 | `TargetMask` 기준 |
| 특수 이동·행동 | 위협에서 도주·두 종류 하위 개체 무제한 생성 | 부착 컴포넌트 기준 |
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

차원 틈새의 이질적인 생물 조직과 흡수한 원소 마력이 함께 보이도록 표현한다. 특정 원소 진영의 재질을 그대로 사용하지 않는다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 현재 확인된 세계관 충돌 없음

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/spawn/DimensionToadMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/misc/DimensionToadPrefabInitializer.java`
