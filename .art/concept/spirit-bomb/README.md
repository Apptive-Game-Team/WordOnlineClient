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
