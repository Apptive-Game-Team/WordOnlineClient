# 2026-07-28 — 아트 스타일 일관성 파이프라인 구축

- Date: 2026-07-28
- GitHub Issue: [#394](https://github.com/Apptive-Game-Team/WordOnlineClient/issues/394)
- Status: Draft

## Goal

마법 스프라이트 아트가 생성될 때마다 스타일이 흔들리는 문제를 구조적으로 막는다.

1. 진영별 형태언어와 공통 렌더링 기법을 분리해 문서로 고정한다.
2. 드리프트가 복리로 쌓이지 않도록 동결된 앵커 세트를 참조 기준으로 삼는다.
3. `make-magic` 스킬이 그 문서와 앵커를 실제로 읽고 따르게 한다.
4. 컨셉과 기법이 어긋난 기존 스프라이트를 재작업한다.

## Non-goals

- 게임플레이 / 서버 데이터 / 프리팹 로직 변경 없음. 순수 아트 자산과 문서 작업.
- 전체 아트 리마스터 아님. 진영 정체성을 깨는 스프라이트만 재작업한다.
- 이미지 생성 결과는 `.art/concept/`와 `.art/anchors/master-v2/`에 저장하고, 비교 결과는
  WordOnline art-direction Site에 누적 게시한다.

## Context / Constraints

### 현재 상태 진단

`Assets/Resources/Game/sprites` 61장을 콘택트 시트로 비교한 결과:

- **페인터리 이탈군** — `FireChildSpirit`, `MagmaSpirit`, `MagmaSpirit2`,
  `MagmaExplosion`, `Crater`. 잔디테일과 불티 산란이 과다해 다른 생성 계보로 보인다.
- **실루엣 부재** — `ChainLightning`, `LightningDrop`. 선만 얇아 축소 시 사라진다.
- **채도 이탈** — `RockRolling`.
- **라인 이탈** — `ChickenCommando`.
- **접지 처리 혼재** — `RockGolem`은 접지 그림자와 파편이 있고 대부분은 없다.

### 세계관에서 파생된 더 큰 문제

- 마법의 신 워드가 죽은 자리에 세계수가 자라고 그 주변에 정령이 발생.
- 지옥불 차원에서 **악마** 군단이 넘어옴. 정령이 아니다.
- 물 슬라임은 온건파라 생존. 나머지 원래 부족은 전멸.
- 전기 / 풀 / 바람 = 정령. 돌 골렘은 별개 부족. 인간은 플레이어 진영(기계류).
- 플레이어와 상대 모두 모든 진영을 소환할 수 있다.

이에 비추면 현재 불 스프라이트(`FireSpirit`, `EmberSpiritSwarm`,
`FireLordSpirit`, `FireTadpole`)는 **둥근 블롭 + 큰 웃는 눈**으로, 물·풀 정령과
형태언어가 동일하다. 악마 진영이어야 하는데 정령으로 읽힌다. 불 세트 전체가
재작업 대상이며, 따라서 지옥불 진영에는 채택 가능한 앵커가 하나도 없다.

역으로 `MagmaSpirit` / `Crater`는 **컨셉은 맞고 기법만 틀렸다**. 반대로
`FireSpirit` 계열은 **기법은 맞고 컨셉이 틀렸다**. 수정 방향이 서로 다르다.

### 팔레트 충돌

진영별 팔레트를 실제 스프라이트에서 양자화로 추출한 결과, 인간 진영(`#A09C92`)과
돌 골렘 부족(`#A3A29D`)이 사실상 같은 회색이다. 색만으로 두 진영이 구분되지 않는다.

### 이미 반영된 작업

`.art/` 디렉터리를 만들었다. `Assets/` 밖이라 Unity가 임포트하지 않고 빌드 용량에
영향이 없다.

| 경로 | 내용 |
|---|---|
| `.art/STYLE.md` | 공통 렌더링 기법 + 진영별 형태언어 + 팔레트 |
| `.art/ANCHORS.md` | 앵커 선정 근거, 제외 목록, 미결 항목 |
| `.art/CONCEPT-BRIEF.md` | 컨셉아트 생성 프롬프트 세트 |
| `.art/anchors/master-v2/` | 동결된 유일한 정본 앵커 세트 |
| `.art/make-sheets.sh` | 콘택트 시트 생성 |
| `.art/sheets/` | 생성된 시트 |

## Approach (Checklist)

- [x] **Step 0: Recon** — 스프라이트 61장 콘택트 시트 비교, 이탈군 식별,
      진영별 팔레트 추출
- [x] **Step 1: 문서화** — `.art/STYLE.md`, `.art/ANCHORS.md`,
      `.art/CONCEPT-BRIEF.md` 작성
- [x] **Step 2: 구 앵커 제거** — 레거시 18장 제거, master-v2만 유지
- [x] **Step 3: 지옥불 컨셉아트 생성 및 master style 선정**
  - [x] `.art/CONCEPT-BRIEF.md` Priority 1의 3안(Molten primitive / Horned demon /
    Ash wraith) 생성
  - [x] 초기 3안은 탈락 처리 후 삭제하고 A 기준 `hellfire-lineup-cut-paper-v2`만 유지
  - `.art/concept/hellfire-<variant>.png`로 저장
  - `./.art/make-sheets.sh` 실행 후 `.art/sheets/concept.png`로 비교
  - 선정 결과와 근거를 `.art/STYLE.md`에 기록
  - 비교 Site: <https://wordonline-hellfire-art.dev-yunseong.chatgpt.site>
- [x] **Step 4: master-v2 앵커 확보**
  - A 2.5D cut-paper 기준 단일 피사체 8종 생성
  - 투명 배경 처리와 알파 검증 후 `.art/anchors/master-v2/`에 동결
  - 캐릭터, 정령, 슬라임, 골렘, 건물, 악마, 이펙트, 세계수 재질 범위 확보
- [x] **Step 5: `make-game-art` 신설 및 `make-magic` 위임**
  - `.agents/skills/make-game-art/`와 `.claude/skills/make-game-art/` 신설
  - 일반 제작과 스타일 기준 변경을 별도 승인 흐름으로 분리
  - `.claude/skills/make-magic/SKILL.md`의 Image Rules를 `.art/STYLE.md` 참조로 교체
  - `make-magic` 이미지 작업을 `make-game-art`로 위임
  - 선택된 master style을 모든 진영의 공통 렌더링 기준으로 고정
  - 진영 판정 단계 추가: 새 마법이 어느 진영인지 먼저 정하고 해당 팔레트/형태언어 적용
  - 산출 후 `./.art/make-sheets.sh` 실행 및 시트 대조를 검증 단계로 추가
- [ ] **Step 6: 인간 / 골렘 색 분리**
  - 골렘: 따뜻한 탄 스톤 + 이끼. 인간: 차가운 회색 + 스틸블루 + 청동/목재
  - 대상: `Cannon`, `Tower`, `Towerback`, `RockTurret`, `ElectricTower`,
    `BubbleGenerator`, `RockGolem`, `RockGolem2`, `RockMage`, `RockDrop`,
    `MiniRockSwarm`
  - `Towerback`(골렘 눈이 달린 석조 블록) 진영 귀속 결정
- [ ] **Step 7: 불 세트 재작업**
  - 컨셉 위반: `FireSpirit`, `EmberSpiritSwarm`, `FireLordSpirit`,
    `FireTadpole`, `FireShot`, `RallyingTorch` → 악마 형태언어로 재디자인
  - 기법 위반: `FireChildSpirit`, `MagmaSpirit`, `MagmaSpirit2`,
    `MagmaExplosion`, `Crater` → 플랫 카툰으로 재렌더, 컨셉 유지
- [ ] **Step 8: 잔여 이탈군 정리**
  - `ChainLightning`, `LightningDrop` 실루엣 질량 확보
  - `RockRolling` 채도 보정
  - `ChickenCommando` 재작업 또는 제거 판단
  - 접지 그림자 규칙 확정 후 일괄 적용
- [ ] **Step 9: 미결 항목 확정 후 `.art/STYLE.md` 갱신**

## Validation

- **Commands to run:**
  ```bash
  ./.art/make-sheets.sh
  ```
  ```bash
  magick identify Assets/Resources/Game/sprites/<New>.png
  ```
- **Expected output:**
  - `.art/sheets/sprites.png`에서 신규/수정 스프라이트가 격자 안에서 튀지 않는다.
  - 썸네일 크기에서 진영이 색만으로 구분된다. 특히 인간 vs 골렘.
  - 지옥불 스프라이트가 귀엽게 읽히지 않는다. 큰 둥근 눈 없음, 불꽃 형태가 아니라
    덩어리 형태.
  - 모든 스프라이트가 투명 여백 없이 타이트하게 트림돼 있고 티어 상한
    (small 128 / middle 192 / big 256)을 넘지 않는다.
  - 64px 축소 시 실루엣이 유지된다.

## Risks & Rollback

- **Risks:**
  - 앵커를 나중에 수정하면 스타일 기준선 전체가 이동한다.
    `.art/anchors/master-v2/`는 동결
    유지가 전제다.
  - 불 세트 재작업은 플레이어가 익숙한 유닛의 외형을 바꾼다. 가독성 회귀 가능.
  - 컨셉아트를 이미지 참조로 그대로 투입하면 배경·프레임·멀티뷰 구도까지 복제된다.
    앵커(투명배경 단일피사체)만 참조로 쓴다.
  - `.art/`가 `Assets/` 밖이라는 전제가 깨지면 WebGL 빌드 용량이 늘어난다.
- **Rollback steps:**
  - 문서/스크립트: `git revert`.
  - 스프라이트: 교체 전 원본이 git 히스토리에 있으므로 파일 단위 `git checkout`.
  - 스킬 개정 되돌리기: `.claude/skills/make-magic/SKILL.md` 단독 revert.

## Open Questions

- 접지 그림자 규칙: 아키타입별로 정할지, 전체 통일할지.
- `Towerback`은 인간 석조인지 골렘인지.
- 회색 기계류(`Tower`, `RockTurret`)의 무거운 렌더링을 인간 진영의 의도된
  서브스타일로 인정할지, 생물 세트 쪽으로 끌어올지.
- 물 슬라임을 정령과 별개 진영으로 유지할지, 형태언어를 정령 쪽에 흡수시킬지.
- 재작업 스프라이트를 같은 파일명으로 덮어쓸지, 새 이름으로 만들고 서버 측
  `resourceName`과 함께 옮길지. 후자는 서버 작업이 동반된다.
