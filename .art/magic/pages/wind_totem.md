# 바람 토템

- 서버 키: `wind_totem`
- 현재 이름: 바람 토템
- 컨셉 이름: 순풍 토템
- 시전 계열: 설치
- 진영: 세계수 바람 정령
- 상태: game 서버 구현 확인

## 컨셉

세계수 바람 정령의 전장에 남아 지속적으로 영향력을 만드는 순풍 토템.

## 컨셉 설명

순풍 토템은 세계수 바람 정령의 설치 마법이다. 이동 AI 없이 설치 위치에서 행동한다. 전투에서는 설치물 역할을 맡으며, 대상 제한을 이 파일에서 직접 확인하지 못함 대상을 직접 공격 없음 또는 별도 효과 방식으로 다룬다. 추가 특수 이동은 없다. 시각적으로는 넓고 읽기 쉬운 곡선 종이 띠로 흐름과 회전을 표현한다.

## 설명

플레이어가 사용하는 **설치 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 목표 지점에 지속 개체를 설치한다. 세부 행동은 프리팹 상속 구조와 런타임 파라미터에 의존한다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 고정형 | 이동 AI 없이 설치 위치에서 행동한다. |
| 전투 역할 | 설치물 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 직접 공격 없음 또는 별도 효과 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 대상 제한을 이 파일에서 직접 확인하지 못함 | `TargetMask` 기준 |
| 특수 이동·행동 | 추가 특수 이동 없음 | 부착 컴포넌트 기준 |
| 생명주기 | 제한시간 후 소멸 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `WindTotemMagic` |
| 상위 클래스 | `AbstractSummonMagic` |
| 주 프리팹 | `WindTotem` |
| 부가 프리팹 | 없음 또는 소스에서 직접 확인되지 않음 |
| 부착 컴포넌트 | `RigidBody`, `CircleCollider`, `DummyMob`, `WindPushComponent`, `Vector3`, `TimedSelfDestroyer`, `CommonEffectReceiver` |
| 파라미터 키 | `MASS`, `RADIUS`, `HP`, `PUSH_FORCE`, `PUSH_RANGE_X`, `PUSH_RANGE_Y`, `ATTACK_INTERVAL`, `DAMAGE` |
| 오브젝트 파라미터 | `WIND_TOTEM` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

넓고 읽기 쉬운 곡선 종이 띠로 흐름과 회전을 표현한다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 현재 확인된 세계관 충돌 없음

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/build/twocard/WindTotemMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/build/WindTotemPrefabInitializer.java`
