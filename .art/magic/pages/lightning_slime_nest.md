# 번개 슬라임 둥지

- 서버 키: `lightning_slime_nest`
- 현재 이름: 번개 슬라임 둥지
- 컨셉 이름: 뇌광 정령제단
- 시전 계열: 설치
- 진영: 세계수 전기 정령
- 상태: game 서버 구현 확인

## 컨셉

세계수 전기 정령의 전장에 남아 지속적으로 영향력을 만드는 뇌광 정령제단.

## 컨셉 설명

뇌광 정령제단은 세계수 전기 정령의 설치 마법이다. 이동 AI 없이 설치 위치에서 행동한다. 전투에서는 소환 역할을 맡으며, 대상 제한을 이 파일에서 직접 확인하지 못함 대상을 직접 공격 없음 또는 별도 효과 방식으로 다룬다. 특수 행동으로 하위 개체 생성을 수행한다. 시각적으로는 세계수에서 갈라진 전기 생명과 각진 전격 파편을 사용한다.

## 설명

플레이어가 사용하는 **설치 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 정해진 종류의 개체를 반복 생성한다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 고정형 | 이동 AI 없이 설치 위치에서 행동한다. |
| 전투 역할 | 소환 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 직접 공격 없음 또는 별도 효과 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 대상 제한을 이 파일에서 직접 확인하지 못함 | `TargetMask` 기준 |
| 특수 이동·행동 | 하위 개체 생성 | 부착 컴포넌트 기준 |
| 생명주기 | 소스에서 시간 제한을 직접 확인하지 못함 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `LightningSlimeNestMagic` |
| 상위 클래스 | `AbstractSummonMagic` |
| 주 프리팹 | `ElectricSummon` |
| 부가 프리팹 | `ElectricSlime` |
| 부착 컴포넌트 | `CircleCollider`, `Spawner` |
| 파라미터 키 | `RADIUS`, `HP` |
| 오브젝트 파라미터 | `ELECTRIC_SUMMON` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

세계수에서 갈라진 전기 생명과 각진 전격 파편을 사용한다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 물 외 슬라임이 멸종했다는 세계관과 충돌. 둥지의 정체 재정의 필요

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/build/LightningSlimeNestMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/lightning/ElectricSummonPrefabInitializer.java`
