# 치킨 특공대

- 서버 키: `chicken_commando`
- 현재 이름: 치킨 특공대
- 컨셉 이름: 비전 강하대
- 시전 계열: 투하
- 진영: 인간 마법 문명
- 상태: game 서버 구현 확인

## 컨셉

인간 마법 문명의 상공에서 목표 지점에 개입하는 비전 강하대.

## 컨셉 설명

비전 강하대는 인간 마법 문명의 공수 전투원이다. 지정 지점 상공에서 낙하산을 펼친 상태로 투입되며, 높이 0.5 이하로 내려오면 낙하산 없는 지상 전투 프레임으로 전환한다. 착지 후에는 지상 대상을 추적해 근접 공격한다.

## 설명

플레이어가 사용하는 **투하 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 지정 지점 상공에서 낙하산을 펼친 인간 공수대원으로 생성된다.
2. 높이 0.5 초과에서는 낙하산 프레임, 지상에서는 전투 프레임을 표시한다.
3. 착지 후 지상 대상을 추적해 근접 공격한다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 상공 낙하 후 지상 이동 | 지정 높이에서 중력 낙하한 뒤 지상 이동 AI로 전환한다. |
| 전투 역할 | 공수 투입·근접 공격 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 단일 표적 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 지상 | `TargetMask` 기준 |
| 특수 이동·행동 | 높이에 따른 낙하산·지상 2프레임 전환 | 부착 컴포넌트 기준 |
| 생명주기 | HP 소진 시 파괴 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `ChickenCommandoMagic` |
| 상위 클래스 | `Magic` |
| 주 프리팹 | `ChickenCommando` |
| 부가 프리팹 | 없음 또는 소스에서 직접 확인되지 않음 |
| 부착 컴포넌트 | `RigidBody`, `ZPhysics`, `CircleCollider`, `ChickenCommandoMob`, `CommonEffectReceiver` |
| 파라미터 키 | `SPAWN_HEIGHT`, `MASS`, `FALL_GRAVITY`, `RADIUS`, `HP`, `SPEED`, `DAMAGE`, `ATTACK_INTERVAL` |
| 오브젝트 파라미터 | `CHICKEN_COMMANDO` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

같은 인간 공수대원을 낙하산 전개 상태와 지상 전투 상태로 나눈다. 두 프레임의 얼굴·복장·장비·오른쪽 방향과 45도 카메라를 동일하게 유지한다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 현재 확인된 세계관 충돌 없음

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/drop/ChickenCommandoMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/misc/ChickenCommandoPrefabInitializer.java`
