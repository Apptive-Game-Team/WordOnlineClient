# 덩굴 세계

- 서버 키: `vine_world`
- 현재 이름: 덩굴 세계
- 컨셉 이름: 덩굴 세계
- 시전 계열: 범위 폭발
- 진영: 세계수 풀 정령
- 상태: game 서버 구현 확인

## 컨셉

세계수 풀 정령의 한순간에 범위를 장악하는 덩굴 세계.

## 컨셉 설명

덩굴 세계은 세계수 풀 정령의 범위 폭발 마법이다. 목표 지점을 중심으로 범위 효과를 생성한다. 전투에서는 범위 공격 역할을 맡으며, 대상 제한을 이 파일에서 직접 확인하지 못함 대상을 범위 방식으로 다룬다. 추가 특수 이동은 없다. 시각적으로는 잎·씨앗·덩굴의 겹친 컷페이퍼 형태로 생장 방향을 보여준다.

## 설명

플레이어가 사용하는 **범위 폭발 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

1. 한 번의 덩굴 연쇄에서 같은 대상을 중복 타격하지 않도록 추적한다.

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | 목표 지점 고정형 | 목표 지점을 중심으로 범위 효과를 생성한다. |
| 전투 역할 | 범위 공격 | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | 범위 | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | 대상 제한을 이 파일에서 직접 확인하지 못함 | `TargetMask` 기준 |
| 특수 이동·행동 | 추가 특수 이동 없음 | 부착 컴포넌트 기준 |
| 생명주기 | 제한시간 후 소멸 | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `VineWorldMagic` |
| 상위 클래스 | `Magic` |
| 주 프리팹 | `GiantVine` |
| 부가 프리팹 | `VineSpirit` |
| 부착 컴포넌트 | `CircleCollider`, `EffectProvider`, `TimedSelfDestroyer`, `OnStartSeedSpiritEvolver`, `OnStartAttacker`, `VineHitTracker` |
| 파라미터 키 | `RADIUS`, `DURATION`, `DAMAGE` |
| 오브젝트 파라미터 | `GIANT_VINE` |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

잎·씨앗·덩굴의 겹친 컷페이퍼 형태로 생장 방향을 보여준다.

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

- 현재 확인된 세계관 충돌 없음

## 근거

- Magic: `game/src/main/java/com/wordonline/server/game/domain/magic/implement/explode/VineWorldMagic.java`
- Prefab initializer: `game/src/main/java/com/wordonline/server/game/domain/object/prefab/implement/nature/GiantVinePrefabInitializer.java`
