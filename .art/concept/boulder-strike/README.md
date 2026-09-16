# 바위 강타 — magic book 아이콘 후보

- 대상 파일: `Assets/Resources/Game/sprites/BoulderStrike.png` (없어서 magic book 이 빈 칸)
- 이슈: Apptive-Game-Team/WordOnlineClient#672
- 생성 날짜: 2026-09-14

## 문법 판단

효과 문법. `AbstractShotMagic` 에 `CircleCollider` 와 `BoulderStrikeShot` 만 붙고, 맞으면 스스로 파괴된다. `sub_damage` 200 이 직격 140 보다 크므로 밀쳐내는 쪽이 이 카드의 정체다. 그래서 떨어지는 바위가 아니라 밀려 나가는 바위로 그린다.

## 상태

적용 완료. 최종 파일은 `Assets/Resources/Game/sprites/BoulderStrike.png` 다.

첫 시도는 투명 배경을 요구했고, `image_gen` 이 alpha 대신 회색·흰색 체커보드를
픽셀에 그려서 돌려줬다. 세 대상 합쳐 13장을 뽑는 동안 진짜 alpha 는 0장이었다.
`candidates/trailing-crescents-checkerboard-reject.png` 가 그때의 결과다.

그래서 배경을 magenta 로 지정해 뽑고 그 색을 빼는 방식으로 바꿨다. 방법은
`.agents/skills/make-game-art/SKILL.md` 의 "Ask for a key colour, not for
transparency" 에 적어 뒀다. `prompts/shared-prefix.txt` 의 첫 문단이 그 요구다.

```bash
.art/tools/key-out-background.py candidates/trailing-crescents-keyed-1-source.png cut.png --key magenta
.art/tools/finalize-candidate.py cut.png Assets/Resources/Game/sprites/BoulderStrike.png --max-size 256
```

`enclosed_pixels=0` 이라 key 색이 피사체에 묻은 곳은 없다.
