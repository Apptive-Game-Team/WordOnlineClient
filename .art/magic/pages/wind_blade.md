# 바람 칼날

- 서버 키: `wind_blade`
- 현재 이름: 바람 칼날
- 컨셉 이름: 초승달 바람칼
- 시전 계열: 투사체
- 진영: 세계수 바람 정령
- 상태: game 서버 구현 확인

## 컨셉

세계수 바람 정령의 방향성과 궤적이 핵심인 초승달 바람칼.

## 컨셉 설명

초승달 바람칼은 세계수 바람 정령의 투사체 마법이다. 시전자에서 목표 방향으로 투사체 또는 연속 개체를 보낸다. 전투에서는 직접 공격 역할을 맡으며, 대상 제한을 이 파일에서 직접 확인하지 못함 대상을 방향성 방식으로 다룬다. 추가 특수 이동은 없다. 시각적으로는 넓고 읽기 쉬운 곡선 종이 띠로 흐름과 회전을 표현한다.

## 설명

플레이어가 사용하는 **투사체 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 바람 칼날 투사체의 관통·타격 동작을 수행한다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 투사체·전파형 | 시전자에서 목표 방향으로 투사체 또는 연속 개체를 보낸다. |
| 전투 역할 | 직접 공격 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 방향성 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 대상 제한을 이 파일에서 직접 확인하지 못함 | `TargetMask` 기준 |
| 특수 이동·행동 | 추가 특수 이동 없음 | 부착 컴포넌트 기준 |
| 생명주기 | 효과 완료 후 소멸 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `WindBladeMagic` |
| 상위 클래스 | `AbstractShotMagic` |
| 주 프리팹 | `WindBlade` |
| 부가 프리팹 | 없음 또는 소스에서 직접 확인되지 않음 |
| 부착 컴포넌트 | `CircleCollider`, `WindBladeShot` |
| 파라미터 키 | `RADIUS`, `DAMAGE`, `SPEED` |
| 오브젝트 파라미터 | `WIND_BLADE` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

넓고 읽기 쉬운 곡선 종이 띠로 흐름과 회전을 표현한다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 현재 확인된 세계관 충돌 없음

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/shoot/WindBladeMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/wind/WindBladePrefabInitializer.java`
