# shock_trap 감시 범위 표시 concept

대상: `shock_trap` 이 감시하는 반경을 게임 화면에서 보여 주는 바닥 그림. 서버
(`game` 저장소, branch `feature/521`) 의 `ShockTrapDetector.start()` 가
`drawCircle(Vector3.ZERO, radius, GizmoCategory.DetectionRange)` 로 반경을
보내고 있고, 그 값은 `CreatedObjectDto.gizmos` 에 실려 client 까지 온다.
`shock_trap` 의 `radius` 는 3.0 (`database` 저장소 branch `feature/132`,
`V078_20260908__register_shock_trap.sql`).

## 왜 선을 그리지 않았나

client 에는 이미 `SkillIndicatorShapeRenderer` 가 있고 `LineRenderer` 로 원을
그린다. 이걸 월드에 상시로 띄우는 방법을 쓰지 않은 이유는 두 가지다.

- `.art/STYLE.md` 의 공유 렌더링 규칙이 "No outer contour line. Forms separate
  by value, not by stroke." 다. 얇은 테두리 원은 이 규칙이 금지하는 바로 그
  형태다.
- 같은 모양을 `ServedObjectGizmoRenderer` 가 debug 선으로 이미 쓰고 있어서,
  화면에 남아 있으면 개발용 표시로 읽힌다.

기존 `Assets/Resources/Game/field/electric_field.png` 를 그대로 쓰는 방법도
검토했다가 뺐다. 그 그림은 불투명한 접시라 위에 선 유닛을 가리고, 팔레트가
인간 진영 쪽 청록이라 World Tree 정령 재질과 맞지 않는다.

## 생성

reference image 세 장을 매 호출에 붙였다: `.art/anchors/master-v2/MasterStyleKey.png`
(기법), `.art/anchors/master-v2/WorldTreeSpirit.png` (World Tree 목재 재질),
그리고 `Assets/Resources/Game/sprites/ShockTrap.png` (이 표시가 딸린 덫 본체).
`prompts/shared-prefix.txt` 가 투명 배경 요구와 기법, 카메라, 재질을 고정하고,
`prompts/variant-*.txt` 가 후보별 형태를 한 문단씩 더한다.

`.art/concept/shock-trap/finalize.py` 를 그대로 쓴다 — alpha 가 없는 원본을
거부하고, alpha bounding box 로 자르고, 비율을 지켜 줄인다. 배경을 지우는
기능은 없다.

## 후보

- **`ShockTrapRange-single-segment-source.png`** (선택) — 바닥에 누운 World Tree
  뿌리 한 토막. 양 끝이 뿌리 끝으로 가늘어지고, 가운데에 작은 금색 결정
  조각이 반쯤 묻혀 있고, 한쪽에 잎 두 장이 난다. 이 한 조각을 원 위에 열 개
  안팎으로 배치한다 (`DetectionRangeMarker`). 나무가 넓고 결정이 작아서 덫
  본체의 결정 core 가 화면에서 가장 밝은 자리를 유지한다.
- **`ShockTrapRange-crystal-markers-source.png`** — 결정 여덟 개가 뿌리로 이어진
  타원 한 장. alpha 도 깨끗하고 형태도 읽히지만, 테두리 결정이 덫 본체의
  결정만큼 크고 밝아서 표시가 본체보다 눈에 띈다. `compare-marker-with-trap.png`
  가 그 비교다. 그래서 쓰지 않았다.

조각을 반복하는 쪽은 부수 효과가 하나 더 있다. 반경이 바뀌어도 그림을 늘리지
않고 배치 지름과 개수만 바뀐다. 한 장짜리 타원은 반경마다 늘어난다.

## 배치

땅은 XZ 평면이다 — `GameScene.unity` 의 카메라는 `(9, 21, -16)` 에서 x축으로
45도 내려다보고, `SkillIndicatorShapeRenderer.BuildCircleEdge` 도 원을
`(cos, 0, sin)` 으로 그린다. 그래서 `DetectionRangeMarker` 는 조각을
`(cos θ · radius, 0, sin θ · radius)` 에 놓고, 그림은 `ElectricField` 처럼
회전 없이 세워 둔다. 화면의 타원은 카메라가 만든다.

조각 하나의 가로 폭은 1.3 world 단위, 이웃 조각 중심 사이는 1.9 world 단위로
두어 둘레를 나눈다. `radius` 3.0 에서 조각 열 개가 된다.
`layout-check.png` 가 그 배치를 카메라 기울기까지 반영해 미리 그려 본 것이다.

## 생성 기록

`image_gen` 호출 23번 중 진짜 alpha 가 실린 결과는 3번이다. 실패한 20번은 전부
불투명 RGB 로, 투명해야 할 자리에 회색 격자를 **그려서** 돌려줬다. 배경을
chroma-key 로 지우는 건 `make-game-art/SKILL.md` 가 금지하므로 다시 생성하는
것 말고는 방법이 없었다.

- 1차 (`shared-prefix.txt`, 투명 요구가 첫 문장): 타원 한 장 후보 세 개로 12번,
  alpha 1번. 그 1번(`c-cracked-ground-attempt1`)은 잎사귀만 남은 초록 원이라
  주제에서 벗어나 후보가 되지 못했다.
- 2차 (`shared-prefix-round2.txt`, 요구 문구를 다시 씀): 4번, alpha 0번.
- 3차 (`shared-prefix-round3.txt`, 정사각 canvas 를 명시): 5번, alpha 1번
  (`b-crystal-markers-round3-attempt1`).
- 조각 하나짜리 (`variant-d-single-segment.txt`): 2번, alpha 1번.

가로로 아주 긴 canvas 를 요구한 호출(1774x887, 1983x793)은 17번 모두
불투명하게 돌아왔고, alpha 가 실린 3번 중 2번은 정사각(1254x1254) 이었다.
표본이 작아 규칙이라고 하기는 이르지만, 같은 시각 이 기계의 다른 session 들이
정사각 요청으로 alpha 를 받고 있었다는 점은 기록해 둔다.

## 검증

`Assets/Resources/Game/field/shock_trap_range_marker.png` (254x75 RGBA):

- 네 모서리 alpha 0, alpha extrema (0, 255) — 가장자리에 실제 gradient 가 있다.
- 투명 면적 비율 0.78.
- 밝고 채도 낮은 불투명 pixel (칠한 배경의 흔적) 0.0000%.
- `marker-64.png` 는 64px 축소본이다. 뿌리 덩어리와 금색 점이 남는다.
- `layout-check.png` 는 배치 결과를, `compare-marker-with-trap.png` 는 쓰지 않은
  타원 후보와 덫 본체의 크기 비교를 보여 준다.

## 하지 않은 것

- `./.art/make-sheets.sh` 는 이 기계에 ImageMagick 이 없어 돌리지 못했다. 위
  비교 그림들은 scratchpad 의 Pillow 가상 환경으로 같은 취지로 만들었다.
- Unity Editor 를 열 수 없어 `.png.meta` 는 `electric_field.png.meta` 를 복사하고
  GUID 만 새로 넣어 손으로 썼다. Editor 가 한 번 열어 확인해야 한다.
