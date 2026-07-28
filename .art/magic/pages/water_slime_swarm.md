# 물 슬라임 무리

- 서버 키: `water_slime_swarm`
- 현재 이름: 물 슬라임 무리
- 컨셉 이름: 물방울 생존자 무리
- 시전 계열: 소환
- 진영: 물 슬라임
- 상태: game 서버 구현 확인

## 컨셉

물 슬라임의 독립 개체로 전장에 합류하는 물방울 생존자 무리.

## 컨셉 설명

물방울 생존자 무리은 물 슬라임의 소환 마법이다. 소스에서 지속 이동 AI를 확인하지 못했다. 전투에서는 전투 개체 역할을 맡으며, 지상 대상을 직접 공격 없음 또는 별도 효과 방식으로 다룬다. 특수 행동으로 이동 경로에 원소 장판 생성을 수행한다. 시각적으로는 물질적인 반투명 종이층과 둥근 체적. 세계수 정령과 구분한다.

## 설명

플레이어가 사용하는 **소환 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 이동 경로를 따라 원소 지대를 남긴다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 이동 없음 | 소스에서 지속 이동 AI를 확인하지 못했다. |
| 전투 역할 | 전투 개체 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 직접 공격 없음 또는 별도 효과 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 지상 | `TargetMask` 기준 |
| 특수 이동·행동 | 이동 경로에 원소 장판 생성 | 부착 컴포넌트 기준 |
| 생명주기 | 소스에서 시간 제한을 직접 확인하지 못함 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `WaterSlimeSwarmMagic` |
| 상위 클래스 | `AbstractSwarmSpawnMagic` |
| 주 프리팹 | `WaterSlime` |
| 부가 프리팹 | `WaterField` |
| 부착 컴포넌트 | `RigidBody`, `ZPhysics`, `CircleCollider`, `Slime`, `PathSpawner`, `CommonEffectReceiver` |
| 파라미터 키 | `MASS`, `RADIUS`, `HP`, `SPEED`, `DAMAGE`, `ATTACK_INTERVAL` |
| 오브젝트 파라미터 | `WATER_SLIME` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

물질적인 반투명 종이층과 둥근 체적. 세계수 정령과 구분한다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 현재 확인된 세계관 충돌 없음

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/spawn/WaterSlimeSwarmMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/water/WaterSlimePrefabInitializer.java`
