# 바다뱀 — magic book 아이콘 후보

- 대상 파일: `Assets/Resources/Game/sprites/SeaSerpent.png` (없어서 magic book 이 빈 칸)
- 이슈: Apptive-Game-Team/WordOnlineClient#672
- 생성 날짜: 2026-09-14

## 문법 판단

소환수 문법. `SeaSerpentMagic` 이 `AbstractSpawnMagic` 을 상속하고, 초기화기가 `SeaSerpentMob extends BehaviorMob` 을 붙인다. 체력 바가 있는 유닛이라 `AquaArcher`·`WaterSlimeSwarm` 과 같은 언어를 쓴다. 원소는 WATER 다.

## 상태

적용 완료. 최종 파일은 `Assets/Resources/Game/sprites/SeaSerpent.png` 다.

첫 시도는 투명 배경을 요구했고, `image_gen` 이 alpha 대신 회색·흰색 체커보드를
픽셀에 그려서 돌려줬다. 세 대상 합쳐 13장을 뽑는 동안 진짜 alpha 는 0장이었다.
`candidates/rising-coils-checkerboard-reject.png` 가 그때의 결과다.

그래서 배경을 magenta 로 지정해 뽑고 그 색을 빼는 방식으로 바꿨다. 방법은
`.agents/skills/make-game-art/SKILL.md` 의 "Ask for a key colour, not for
transparency" 에 적어 뒀다. `prompts/shared-prefix.txt` 의 첫 문단이 그 요구다.

```bash
.art/tools/key-out-background.py candidates/rising-coils-keyed-1-source.png cut.png --key magenta
.art/tools/finalize-candidate.py cut.png Assets/Resources/Game/sprites/SeaSerpent.png --max-size 256
```

`enclosed_pixels=0` 이라 key 색이 피사체에 묻은 곳은 없다.

## 전투용 몸통 조각 batch — 2026-09-15

전투 화면의 바다뱀은 아이콘 한 장이 아니라 조각 네 개를 런타임에 이어 붙여 그린다.
`SeaSerpent.png` 아이콘은 그대로 두고, 아래 네 파일을 새로 만들었다.

| 파일 | 크기 | alignment | 역할 |
|---|---|---|---|
| `Assets/Resources/Game/sprites/SeaSerpentHead.png` | 134x188 | 7 (bottom centre) | 머리와 세운 목, 목 아래가 평평하게 잘려 있다 |
| `Assets/Resources/Game/sprites/SeaSerpentSegment.png` | 256x166 | 7 (bottom centre) | 등 hump 하나, 좌우로 반복해 몸통을 만든다 |
| `Assets/Resources/Game/sprites/SeaSerpentTail.png` | 205x120 | 7 (bottom centre) | 가늘어지는 꼬리 끝과 꼬리 지느러미 |
| `Assets/Resources/Game/sprites/SeaSerpentHydroPump.png` | 256x79 | 4 (left centre) | hydro pump 물줄기, 9-slice tiled |

prompt 은 `prompts/battle-parts-shared-prefix.txt` 에 공통 부분을 두고
`prompts/part-head.txt`, `prompts/part-segment.txt`, `prompts/part-tail.txt` 를
이어 붙여 썼다. hydro pump 은 뱀의 눈·지느러미 설명이 섞이면 안 되니
`prompts/part-hydro-pump.txt` 한 장에 전부 담았다. 참고 이미지는
`.art/anchors/master-v2/MasterStyleKey.png`,
`Assets/Resources/Game/sprites/SeaSerpent.png`,
`.art/anchors/master-v2/WaterSlime.png` 세 장을 붙였다.

배경은 이번에도 magenta 로 지정해서 뽑고 `--key magenta` 로 뺐다. 네 장 모두 한
번에 통과했다. `enclosed_pixels` 는 segment·tail·hydro pump 가 0 이고 head 만 7 이
나왔는데, 좌표를 찍어 보니 벌어진 입 안쪽 x 793-806, y 300-305 에서 위아래
입술이 거의 맞닿아 갇힌 배경 2 픽셀이라 palette 충돌이 아니다. 이 palette 에
magenta 계열 색은 없다.

## 조각이 이어지는 기준

세 조각의 몸통 굵기를 최종 픽셀에서 74 픽셀로 맞췄다. 이것이 조각들이 한 마리로
읽히게 하는 유일한 불변값이다. 원본에서 잰 값은 head 의 목 절단면 424 픽셀,
segment 의 좌우 절단면 각 329 픽셀, tail 의 왼쪽 절단면 390 픽셀이고, 각각
0.1745 · 0.2250 · 0.1897 배로 줄여 74 픽셀에 모았다.

segment 의 좌우 절단면은 원본에서 둘 다 y 652-980 로 높이와 위치가 정확히
같아서, 복사본을 이어 붙여도 단차가 없다. `candidates/battle-assembly-mock.png`
이 head 한 장에 segment 여섯 장과 tail 한 장을 곡선으로 늘어놓고 겹쳐 본
결과다.

주의할 점 하나. 지시받은 비율은 head 높이가 segment 높이의 1.6 배였는데 실제로는
188 대 166, 1.13 배다. 생성된 hump 가 요청한 1.8 units 보다 낮고 넓게 나왔기
때문이다 (몸통 굵기 1 units 기준으로 높이 2.25 units, 너비 3.46 units). 굵기를
맞추는 쪽과 높이 비율을 맞추는 쪽이 동시에 성립하지 않아서 굵기를 택했다. 높이
비율을 1.6 으로 맞추면 몸통이 53 픽셀이 되어 목 74 픽셀과 눈에 띄게 어긋난다.

## hydro pump 의 9-slice

`BeamProjectile` 이 Unity 의 Tiled draw mode 로 그리므로 가운데 구간이 자기
자신과 이어 붙어야 한다. 최종 256x79 에서 잰 값은 이렇다.

- 두께가 완전히 일정한 구간: x 65-219, 위 25 아래 55, 두께 31 픽셀
- `spriteBorder: {x: 68, y: 0, z: 39, w: 0}` — 양쪽에서 3 픽셀씩 안으로 들여
  잡았다
- 가운데 tile 너비 149 픽셀. 216 번째 열과 68 번째 열의 색 차이는 최대 8 이라
  이어 붙은 자리가 보이지 않는다
- `spriteMeshType: 0` (FullRect). Tiled 와 Sliced 는 FullRect 를 요구하고,
  기본값 1 이면 조용히 깨진다

`candidates/battle-hydropump-tiled-check.png` 가 가운데 구간을 다섯 번 반복해
붙여 본 결과다.

## meta

Unity Editor 를 이 shell 에서 열 수 없어 `SeaSerpent.png.meta` 를 그대로 복사하고
`guid`, `alignment`, hydro pump 의 `spriteBorder` 와 `spriteMeshType` 만 고쳤다.
`spriteID` 는 이 repository 의 다른 meta 67 개와 같은 값을 유지한다. Editor 가 한
번 import 해야 실제로 반영된다.

이 batch 는 `-source.png` 만 남긴다. `-cut.png` 는 아래 한 줄로 언제든 다시
만들어지는 중간 파일이라 저장소에 넣지 않았다.

```bash
.art/tools/key-out-background.py candidates/battle-<part>-1-source.png cut.png --key magenta
.art/tools/finalize-candidate.py cut.png Assets/Resources/Game/sprites/SeaSerpent<Part>.png --max-size 256
```
