# 포탑등이

- 서버 키: `towerback`
- 현재 이름: 포탑등이
- 컨셉 이름: 포탑등이
- 시전 계열: 설치
- 진영: 인간 마법 문명
- 상태: game 서버 구현 확인

## 컨셉

인간 마법 문명의 전장에 남아 지속적으로 영향력을 만드는 포탑등이.

## 컨셉 설명

포탑등이는 인간 마법 문명이 납치한 꼬마돌의 등에 지대공 타워를 강제로 결속한 고정형 병기다. 생물 본체는 돌 골렘 부족이지만 전장에서는 인간 진영의 원거리 방어 구조물로 운용되며 지상·공중 대상을 공격한다.

## 설명

플레이어가 사용하는 **설치 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 공격 상태를 수행한다.
2. 움직이지 않고 탐지 범위 안 적을 공격한다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 고정형 | 이동 AI 없이 설치 위치에서 행동한다. |
| 전투 역할 | 원거리 공격 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 범위 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 지상·공중 | `TargetMask` 기준 |
| 특수 이동·행동 | 추가 특수 이동 없음 | 부착 컴포넌트 기준 |
| 생명주기 | HP 소진 시 파괴 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `TowerbackMagic` |
| 상위 클래스 | `AbstractSummonMagic` |
| 주 프리팹 | `Towerback` |
| 부가 프리팹 | 없음 또는 소스에서 직접 확인되지 않음 |
| 부착 컴포넌트 | `RigidBody`, `ZPhysics`, `CircleCollider`, `AttackMob`, `Tower`, `CommonEffectReceiver` |
| 파라미터 키 | `MASS`, `RADIUS`, `HP`, `SUB_SPEED`, `SUB_DAMAGE`, `ATTACK_INTERVAL`, `SUB_ATTACK_RANGE`, `DAMAGE`, `ATTACK_RANGE` |
| 오브젝트 파라미터 | `TOWERBACK` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

따뜻한 꼬마돌 몸체와 차가운 인간제 지대공 타워를 명확히 분리한다. 가죽끈·금속 고정구로 강제 결속된 흔적을 보여준다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 현재 확인된 세계관 충돌 없음

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/build/TowerbackMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/misc/TowerbackPrefabInitializer.java`
