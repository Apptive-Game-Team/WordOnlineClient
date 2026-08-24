# 사악한 나무 골렘

- 서버 키: `evil_ent`
- 현재 이름: 사악한 나무 골렘
- 컨셉 이름: 사악한 고목
- 시전 계열: 소환
- 진영: 타락한 정령
- 상태: game 서버 구현 확인

## 컨셉

타락한 정령의 독립 개체로 전장에 합류하는 사악한 고목.

## 컨셉 설명

사악한 고목은 타락한 정령의 소환 마법이다. 이동 AI가 지면 경로와 `RigidBody` 속도를 사용한다. 전투에서는 원거리 공격 역할을 맡으며, 지상 대상을 단일 표적 방식으로 다룬다. 특수 행동으로 두 번째 팔로 대상을 끌어당긴 뒤 세 번째 팔인 불타는 주먹으로 마무리하는 연계 공격을 수행한다. 시각적으로는 세계수 풀 정령의 컷페이퍼 골격은 유지하되 수분이 빠진 숯빛 목질로 바꾼다. 갈라진 틈으로만 지옥불이 비치게 하고, 불꽃 자체를 몸 밖에 흩뿌리지 않는다.

## 설명

플레이어가 사용하는 **소환 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 사거리 안의 지상 대상에게 나무 팔을 뻗어 기본 공격한다.
2. 별도 재사용 대기시간마다 두 번째 팔을 던져 대상을 걸고 자기 쪽으로 끌어당긴다.
3. 끌려온 대상에게 세 번째 팔인 불타는 주먹을 내질러 추가 피해를 준다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 지상 이동형 | 이동 AI가 지면 경로와 `RigidBody` 속도를 사용한다. |
| 전투 역할 | 원거리 공격 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 단일 표적 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 지상 | `TargetMask` 기준 |
| 특수 이동·행동 | 두 번째 팔로 대상을 끌어당긴 뒤 세 번째 팔인 불타는 주먹으로 마무리하는 연계 공격 | 부착 컴포넌트 기준 |
| 생명주기 | HP 소진 시 파괴 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `EvilEntMagic` |
| 상위 클래스 | `AbstractSpawnMagic` |
| 주 프리팹 | `EvilEnt` |
| 부가 프리팹 | 없음 또는 소스에서 직접 확인되지 않음 |
| 부착 컴포넌트 | `RigidBody`, `ZPhysics`, `CircleCollider`, `EvilEntMob`, `CommonEffectReceiver` |
| 파라미터 키 | `MASS`, `RADIUS`, `HP`, `SPEED`, `DAMAGE`, `ATTACK_INTERVAL`, `ATTACK_RANGE`, `PROJECTILE_SPEED`, `SUB_DAMAGE`, `SUB_ATTACK_RANGE`, `SUB_SPEED`, `SUB_ATTACK_INTERVAL`, `PULL_MASS_LIMIT` |
| 오브젝트 파라미터 | `EVIL_ENT` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

세계수 풀 정령의 컷페이퍼 골격은 유지하되 수분이 빠진 숯빛 목질로 바꾼다. 갈라진 틈으로만 지옥불이 비치게 하고, 불꽃 자체를 몸 밖에 흩뿌리지 않는다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 현재 확인된 세계관 충돌 없음

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/spawn/EvilEntMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/misc/third/EvilEntPrefabInitializer.java`
