# 용암 정령

- 서버 키: `magma_spirit`
- 현재 이름: 용암 정령
- 컨셉 이름: 용암 갑각 악마
- 시전 계열: 소환
- 진영: 지옥불 군단
- 상태: game 서버 구현 확인

## 컨셉

지옥불 군단의 독립 개체로 전장에 합류하는 용암 갑각 악마.

## 컨셉 설명

용암 갑각 악마은 지옥불 군단의 소환 마법이다. 이동 AI가 지면 경로와 `RigidBody` 속도를 사용한다. 전투에서는 소환 역할을 맡으며, 지상 대상을 직접 공격 없음 또는 별도 효과 방식으로 다룬다. 특수 행동으로 하위 개체 생성을 수행한다. 시각적으로는 검붉은 종이 갑각과 내부 용암광. 불꽃 정령이 아니라 악마 군단의 마법으로 표현한다.

## 설명

플레이어가 사용하는 **소환 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 전투 상태 머신 안에서 하위 개체를 소환한다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 지상 이동형 | 이동 AI가 지면 경로와 `RigidBody` 속도를 사용한다. |
| 전투 역할 | 소환 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 직접 공격 없음 또는 별도 효과 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 지상 | `TargetMask` 기준 |
| 특수 이동·행동 | 하위 개체 생성 | 부착 컴포넌트 기준 |
| 생명주기 | HP 소진 시 파괴 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `MagmaSpiritMagic` |
| 상위 클래스 | `AbstractSingleSpawnMagic` |
| 주 프리팹 | `MagmaSpirit` |
| 부가 프리팹 | `MagmaFist` |
| 부착 컴포넌트 | `RigidBody`, `ZPhysics`, `CircleCollider`, `SummonerMob`, `CommonEffectReceiver`, `AreaEffectProvider` |
| 파라미터 키 | `MASS`, `RADIUS`, `HP`, `SPEED`, `ATTACK_INTERVAL`, `ATTACK_RANGE`, `SUB_ATTACK_RANGE` |
| 오브젝트 파라미터 | `MAGMA_SPIRIT` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

검붉은 종이 갑각과 내부 용암광. 불꽃 정령이 아니라 악마 군단의 마법으로 표현한다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 서버 계약은 유지하더라도 표시명과 외형에서 `정령` 표현 제거 필요

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/spawn/MagmaSpiritMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/misc/MagmaSpiritPrefabInitializer.java`
