# 정령 폭탄 — magic book 아이콘 후보

- 대상 파일: `Assets/Resources/Game/sprites/SpiritBomb.png` (없어서 magic book 이 빈 칸)
- 이슈: Apptive-Game-Team/WordOnlineClient#672
- 생성 날짜: 2026-09-14

## 문법 판단

효과 문법. 필드에 오브젝트가 아예 없다. `SpiritBombMagic` 이 플레이어에게 `SpiritBombChannel` 만 붙이고 1초 간격 4회로 때린다. 이 sprite 는 순수하게 책 아이콘 용도다. 원소는 LIGHTNING 과 NATURE 둘이라 금색과 풀색을 같이 쓴다.

## 상태

적용 완료. 최종 파일은 `Assets/Resources/Game/sprites/SpiritBomb.png` 다.

첫 시도는 투명 배경을 요구했고, `image_gen` 이 alpha 대신 회색·흰색 체커보드를
픽셀에 그려서 돌려줬다. 세 대상 합쳐 13장을 뽑는 동안 진짜 alpha 는 0장이었다.
`candidates/orb-and-beam-checkerboard-reject.png` 가 그때의 결과다.

그래서 배경을 magenta 로 지정해 뽑고 그 색을 빼는 방식으로 바꿨다. 방법은
`.agents/skills/make-game-art/SKILL.md` 의 "Ask for a key colour, not for
transparency" 에 적어 뒀다. `prompts/shared-prefix.txt` 의 첫 문단이 그 요구다.

```bash
.art/tools/key-out-background.py candidates/orb-and-beam-keyed-1-source.png cut.png --key magenta
.art/tools/finalize-candidate.py cut.png Assets/Resources/Game/sprites/SpiritBomb.png --max-size 256
```

`enclosed_pixels=0` 이라 key 색이 피사체에 묻은 곳은 없다.

## 빔 조각 두 장 — 2026-09-15

- 대상 파일: `Assets/Resources/Game/shoot/spirit_bomb_beam_segment.png`,
  `Assets/Resources/Game/shoot/spirit_bomb_beam_cap.png`
- 위 아이콘과 달리 이쪽은 필드에 그려지는 그림이다. `SpiritBombBeamProjectile` 이
  런타임에 `Texture2D` 로 칠하던 띠와 둥근 빛을 이 두 장으로 바꿨다.
- 프롬프트: `prompts/beam-shared-prefix.txt` 에 `prompts/beam-segment.txt`
  또는 `prompts/beam-cap.txt` 를 이어 붙인다. 참조 이미지는
  `.art/anchors/master-v2/MasterStyleKey.png` 와 `ArcaneImpact.png` 두 장이다.

### 띠

`beam-segment-source.png` 가 생성 원본(2172x724)이다. 셰브런 주기는 543px 로
일정하고, 가로 547px 창을 잘라 내면 좌우 끝의 단면이 같아 이어 붙여도 이음매가
없다. `beam-segment-tiled-four.png` 가 네 장을 붙여 본 결과다.

띠는 좌우로 흘러 나가야 하므로 alpha 로 잘라내는 대상이 아니다. magenta 를 뺀
뒤 세로로만 잘랐고, 결과는 256x139 의 완전히 불투명한 직사각형이다. 자른 창의
x 범위는 `[850, 1397)` 이다.

```bash
.art/tools/key-out-background.py beam-segment-source.png keyed.png --key magenta
# 이어서 x 850 부터 547px 을 잘라 세로로만 트림하고 가로 256 으로 줄인다
```

### 별

`beam-cap-source.png` 가 생성 원본(1254x1254)이다. `ArcaneImpact.png` 의 여덟
갈래 결정 배치를 그대로 따르되 색만 spirit bomb 팔레트로 바꿨다 — 가운데 마름모는
연한 금색, 대각선 네 갈래는 금색, 상하좌우 네 갈래는 풀색이다. 방향이 없어서
밑동과 끝이 같은 장을 배수만 달리해 쓴다.

```bash
.art/tools/key-out-background.py beam-cap-source.png cut.png --key magenta
.art/tools/finalize-candidate.py cut.png Assets/Resources/Game/shoot/spirit_bomb_beam_cap.png --max-size 256
```

두 장 모두 `enclosed_pixels=0`, 불투명 픽셀에 남은 magenta 0 이다. 별은 네 모서리
alpha 0 이고 71.3% 가 투명하다. `beam-assembled-on-grass.png` 가 세 조각을 실제
비율(굵기 0.3, 밑동 1.8배, 끝 2.4배)로 조립해 본 그림이다.
