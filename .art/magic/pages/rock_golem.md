# 바위 골렘

- 서버 키: `rock_golem`
- 현재 이름: 바위 골렘
- 컨셉 이름: 이끼바위 골렘
- 시전 계열: 소환
- 진영: 돌 골렘 부족
- 상태: game 서버 구현 확인

## 컨셉

돌 골렘 부족의 독립 개체로 전장에 합류하는 이끼바위 골렘.

## 컨셉 설명

이끼바위 골렘은 돌 골렘 부족의 소환 마법이다. 이동 AI가 지면 경로와 `RigidBody` 속도를 사용한다. 전투에서는 근접 공격 역할을 맡으며, 지상 대상을 단일 표적 방식으로 다룬다. 추가 특수 이동은 없다. 시각적으로는 따뜻한 석재 조각과 이끼. 인간제 회색 기계와 구분한다.

## 설명

플레이어가 사용하는 **소환 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 근접 거리까지 접근해 공격한다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 지상 이동형 | 이동 AI가 지면 경로와 `RigidBody` 속도를 사용한다. |
| 전투 역할 | 근접 공격 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 단일 표적 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 지상 | `TargetMask` 기준 |
| 특수 이동·행동 | 추가 특수 이동 없음 | 부착 컴포넌트 기준 |
| 생명주기 | HP 소진 시 파괴 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `RockGolemMagic` |
| 상위 클래스 | `AbstractSingleSpawnMagic` |
| 주 프리팹 | `RockGolem` |
| 부가 프리팹 | 없음 또는 소스에서 직접 확인되지 않음 |
| 부착 컴포넌트 | `RigidBody`, `ZPhysics`, `CircleCollider`, `MeleeAttackMob`, `CommonEffectReceiver` |
| 파라미터 키 | `MASS`, `RADIUS`, `HP`, `SPEED`, `DAMAGE`, `ATTACK_INTERVAL` |
| 오브젝트 파라미터 | `ROCK_GOLEM` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

따뜻한 석재 조각과 이끼. 인간제 회색 기계와 구분한다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 현재 확인된 세계관 충돌 없음

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/spawn/RockGolemMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/misc/RockGolemPrefabInitializer.java`
