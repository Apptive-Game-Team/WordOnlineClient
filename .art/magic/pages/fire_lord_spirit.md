# 불 대왕 정령

- 서버 키: `fire_lord_spirit`
- 현재 이름: 불 대왕 정령
- 컨셉 이름: 지옥불 군단장
- 시전 계열: 소환
- 진영: 지옥불 군단
- 상태: game 서버 구현 확인

## 컨셉

지옥불 군단의 독립 개체로 전장에 합류하는 지옥불 군단장.

## 컨셉 설명

지옥불 군단장은 전장 상공에 머무는 거대한 악마 생체 모함이다. 직접 공격하지 않고 안전거리를 유지하며 `FireChildSpirit`를 5초마다 1마리씩, 최대 5마리 방출한다. 소환된 비행 악마는 지상·공중 대상을 향해 `FireShot`으로 원거리 공격한다.

## 설명

플레이어가 사용하는 **소환 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. `FireChildSpirit`를 5초마다 1마리씩, 최대 5마리 소환한다.
2. 소환된 비행 악마는 지상·공중 대상을 향해 `FireShot`으로 원거리 공격한다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 공중 부유형 | `ZPhysics(gameObject, hoverY)`로 고도를 유지한다. |
| 전투 역할 | 하위 비행 악마 소환 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 5초 간격 순차 소환 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 소환 지점 | `TargetMask` 기준 |
| 특수 이동·행동 | 안전거리 유지·동일 하위 개체 최대 5마리 생성 | 부착 컴포넌트 기준 |
| 생명주기 | HP 소진 시 파괴 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `FireLordSpiritMagic` |
| 상위 클래스 | `AbstractSingleSpawnMagic` |
| 주 프리팹 | `FireLordSpirit` |
| 부가 프리팹 | `FireChildSpirit` |
| 부착 컴포넌트 | `RigidBody`, `ZPhysics`, `CircleCollider`, `KeepDistanceMob`, `LimitedSequenceSpawner`, `CommonEffectReceiver` |
| 파라미터 키 | `MASS`, `RADIUS`, `HP`, `SPEED` |
| 오브젝트 파라미터 | `FIRE_LORD_SPIRIT` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

검붉은 종이 갑각과 내부 용암광. 불꽃 정령이 아니라 악마 군단의 마법으로 표현한다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 서버 계약은 유지하더라도 표시명과 외형에서 `정령` 표현 제거 필요

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/spawn/FireLordSpiritMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/misc/FireLordSpiritPrefabInitializer.java`
