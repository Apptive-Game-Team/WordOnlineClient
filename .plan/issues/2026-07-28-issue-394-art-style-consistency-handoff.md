# Handoff — 아트 스타일 일관성 파이프라인

작업 대상 저장소: `word-online/client` (Unity 2022.3.22f1, WebGL)
GitHub Issue: [#394](https://github.com/Apptive-Game-Team/WordOnlineClient/issues/394)
브랜치: `feature/394` (`origin/main` 기준)
연관 플랜: [.plan/issues/2026-07-28-issue-394-art-style-consistency-pipeline.md](.plan/issues/2026-07-28-issue-394-art-style-consistency-pipeline.md)

---

## 배경 요약

마법 스프라이트를 생성할 때마다 스타일이 흔들린다. 원인은 두 가지다.

**1. 참조 체인이 드리프트를 누적한다.**
현재 `make-magic` 스킬은 "가까운 스프라이트 3~6장을 골라 스타일 단서를 프롬프트로
*서술*하라"고 지시한다. 최신 생성물을 참조하므로 N번째 생성물이 N-1번째의 오차를
그대로 물려받고, 텍스트 경유라 손실도 크다.

**2. 렌더링 기법과 진영 정체성이 분리되지 않았다.**
"지옥불은 험악해야 한다" → "다른 모델로 뽑는다" 로 이어져, 형태언어 대신 렌더링
기법이 달라졌다. `MagmaSpirit` 계열이 그 결과다.

세계관상 지옥불 차원은 **악마**이지 정령이 아니다. 그런데 현재 `FireSpirit`,
`EmberSpiritSwarm`, `FireLordSpirit`, `FireTadpole`은 둥근 블롭 + 큰 웃는 눈으로
물·풀 정령과 형태언어가 동일하다. 불 세트 전체가 컨셉을 위반한다.

---

## 이미 되어 있는 것

`.art/` 디렉터리 생성 완료. `Assets/` 밖이라 Unity가 임포트하지 않고 WebGL 빌드
용량에 영향이 없다. **이 전제를 깨지 말 것.**

| 경로 | 내용 |
|---|---|
| `.art/STYLE.md` | 공통 렌더링 기법 + 진영 5종 형태언어 + 팔레트 |
| `.art/ANCHORS.md` | 정본 master-v2 앵커 선정 근거와 변경 규칙 |
| `.art/CONCEPT-BRIEF.md` | 컨셉아트 생성 프롬프트 (지옥불 3안 포함) |
| `.art/anchors/master-v2/` | 동결된 유일한 정본 앵커 세트 |
| `.art/make-sheets.sh` | 콘택트 시트 생성 스크립트 |
| `.art/sheets/` | `master-v2-anchors.png`, `sprites.png` |

작업 시작 전 `.art/STYLE.md`와 `.art/ANCHORS.md`를 먼저 읽을 것. 이 핸드오프는
요약일 뿐이고 규칙의 정본은 그 두 파일이다.

---

## 이번 핸드오프의 범위

이미지 생성과 문서/스킬 작업을 함께 진행한다. 생성 결과는 `.art/`에 저장하고
비교 Site에 누적한다:
<https://wordonline-hellfire-art.dev-yunseong.chatgpt.site>

### 작업 A — `make-game-art` 신설 및 `make-magic` 위임 (완료)

파일: `.claude/skills/make-magic/SKILL.md`
참고: `.claude/skills/make-magic/references/client-magic-patterns.md`

`make-game-art`가 전체 아트 스타일, 생성, 승인, 앵커 승격, Sites 비교를 소유한다.
`make-magic`은 로컬라이제이션과 서버 파생 이름만 소유하고 이미지 작업을 위임한다.

1. **참조 대상을 `.art/anchors/master-v2/`로 고정.**
   `Assets/Resources/Game/sprites/*.png`를 참조하라는 현재 지시를 바꾼다. 라이브
   스프라이트는 재작업 대상이 섞여 있어 참조원으로 부적합하다.

2. **앵커 이미지를 생성기에 직접 투입.**
   "스타일 단서를 프롬프트로 서술" 대신, 해당 진영 앵커 이미지를 참조 이미지로
   함께 넘기도록 워크플로를 바꾼다. 텍스트 서술은 보조로만 남긴다.

3. **진영 판정 단계를 신설.**
   새 마법이 어느 진영인지 먼저 결정하고, `.art/STYLE.md`의 해당 진영
   형태언어·팔레트를 적용한다. 진영은 정령(전기/풀/바람), 물 슬라임, 돌 골렘 부족,
   인간, 지옥불 군단 5종.

4. **컨셉아트를 참조 이미지로 쓰지 말 것을 명시.**
   `.art/concept/`는 사람이 읽는 용도다. 배경·프레임·멀티뷰가 있어 참조로 넣으면
   그 구도까지 복제된다.

5. **검증 단계 추가.**
   산출 후 `./.art/make-sheets.sh`를 실행하고 `.art/sheets/sprites.png`에서 신규
   스프라이트가 격자 안에서 튀는지 대조. 튀면 리젝.

기존 규칙 중 유지할 것: 티어별 최대 크기(small 128 / middle 192 / big 256),
비율 보존 리사이즈, 최종 트림(`magick input.png -resize 256x256 -trim +repage
output.png`), 투명배경, 우향, `Assets/Resources/Game/sprites/<PascalCase>.png`
경로 규칙. 로컬라이제이션 관련 섹션은 건드리지 않는다.

### 작업 B — `.art/` 도구 보강 (선택)

- `make-sheets.sh`에 진영별 시트 생성 추가. 현재는 전체 + 앵커 + 컨셉만 만든다.
  진영별로 끊어 보면 팔레트 이탈이 더 잘 보인다.
- 스프라이트 크기·알파·트림 상태를 일괄 점검하는 린트 스크립트
  (`.art/lint-sprites.sh`). 티어 상한 초과, 투명 여백 잔존, 알파 채널 부재를
  검출한다.

---

## 이미지 생성 진행 상태

**스타일 A — 2.5D 컷페이퍼를 2026-07-28 정본으로 확정했다.**
정본은 `.art/anchors/master-v2/` 하나뿐이다. 구 앵커는 생성기가 잘못 참조하지
않도록 제거했다. 과거 피사체 정체성은 Git 이력이나 라이브 에셋에서 확인한다.

완료된 master-v2 앵커:

- `MasterStyleKey.png`
- `ApprenticeMage.png`
- `WorldTreeSpirit.png`
- `WaterSlime.png`
- `RockGolem.png`
- `HumanMagicTower.png`
- `HellfireDemon.png`
- `ArcaneImpact.png`
- `WorldTree.png`

비교 시트는 `.art/sheets/master-v2-anchors.png`에 있다. 다음 작업은 이 앵커를
직접 참조해 실제 게임 스프라이트를 재작업하는 것이다.

- 지옥불 컨셉아트 3안 생성 및 스타일 A 선정 → master-v2 앵커 확보 (완료)
- 인간(차가운 회색+스틸블루+청동) / 골렘(따뜻한 탄+이끼) 색 분리 재작업
- 불 세트 재작업 — 컨셉 위반군과 기법 위반군의 수정 방향이 서로 다르다
- `ChainLightning`, `LightningDrop` 실루엣 확보 / `RockRolling` 채도 보정

---

## 하지 말 것

- `.art/anchors/master-v2/` 안의 파일을 수정·재생성·교체하지 말 것. 동결이 이
  구조의 전제다.
- `.art/`를 `Assets/` 아래로 옮기지 말 것. Unity가 임포트해 빌드 용량이 는다.
- `Assets/Scripts/Data/Magic/LocalCombinedMagicData.cs`를 건드리지 말 것.
  마법 데이터는 서버 파생이다.
- 스프라이트 파일명을 바꾸지 말 것. 서버 `resourceName`과 결합돼 있다.
- 작업 트리의 무관한 변경사항을 함께 커밋하지 말 것.

---

## 검증

```bash
./.art/make-sheets.sh
```

- `.art/sheets/` 3종(또는 컨셉 없을 시 2종)이 정상 생성된다.
- `.claude/skills/make-magic/SKILL.md`가 `.art/STYLE.md`와
  `.art/anchors/master-v2/`를
  명시적으로 참조하고, 라이브 스프라이트를 참조원으로 지시하지 않는다.
- 스킬 문서 안에 `.art/STYLE.md`와 모순되는 스타일 규칙이 중복 서술로 남아 있지
  않다.

---

## 미결 사항 — 결정되면 `.art/STYLE.md`에 기록

- 접지 그림자 규칙: 아키타입별인지 전체 통일인지. 현재 `RockGolem`만 접지 파편이 있다.
- `Towerback`(골렘 눈이 달린 석조 블록)의 진영 귀속.
- 회색 기계류(`Tower`, `RockTurret`)의 무거운 렌더링을 인간 진영의 의도된
  서브스타일로 인정할지.
- 물 슬라임을 정령과 별개 진영으로 유지할지.
