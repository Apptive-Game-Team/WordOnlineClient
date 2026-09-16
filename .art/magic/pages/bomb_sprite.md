# 폭탄 정령

- 서버 키: `bomb_sprite`
- 현재 이름: 폭탄 정령
- 컨셉 이름: 공습 폭탄요정
- 시전 계열: 소환
- 진영: 귀속 미정
- 상태: game 서버 구현 확인

## 컨셉

귀속 미정의 독립 개체로 전장에 합류하는 공습 폭탄요정.

## 컨셉 설명

공습 폭탄요정은 귀속 미정의 소환 마법이다. `ZPhysics(gameObject, hoverY)`로 고도를 유지한다. 전투에서는 전투 개체 역할을 맡으며, 대상 제한을 이 파일에서 직접 확인하지 못함 대상을 직접 공격 없음 또는 별도 효과 방식으로 다룬다. 추가 특수 이동은 없다. 시각적으로는 기존 세계관만으로 귀속을 확정할 수 없다. 시각 제작 전 설정 결정이 필요하다.

## 설명

플레이어가 사용하는 **소환 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 목표 지점에 아군 개체를 소환한다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 공중 부유형 | `ZPhysics(gameObject, hoverY)`로 고도를 유지한다. |
| 전투 역할 | 전투 개체 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 직접 공격 없음 또는 별도 효과 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 대상 제한을 이 파일에서 직접 확인하지 못함 | `TargetMask` 기준 |
| 특수 이동·행동 | 추가 특수 이동 없음 | 부착 컴포넌트 기준 |
| 생명주기 | 소스에서 시간 제한을 직접 확인하지 못함 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `BombSpriteMagic` |
| 상위 클래스 | `AbstractSpawnMagic` |
| 주 프리팹 | `BombSprite` |
| 부가 프리팹 | 없음 또는 소스에서 직접 확인되지 않음 |
| 부착 컴포넌트 | `RigidBody`, `ZPhysics`, `CircleCollider`, `BombSpriteMob`, `CommonEffectReceiver` |
| 파라미터 키 | `MASS`, `RADIUS`, `HP`, `SPEED`, `ATTACK_INTERVAL`, `ATTACK_RANGE` |
| 오브젝트 파라미터 | `BOMB_SPRITE` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

기존 세계관만으로 귀속을 확정할 수 없다. 시각 제작 전 설정 결정이 필요하다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 세계관 진영 귀속 필요

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/spawn/BombSpriteMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/wind/BombSpritePrefabInitializer.java`
