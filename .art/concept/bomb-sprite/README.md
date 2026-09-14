# 폭탄 정령 — master-v2 재작업

- 대상 파일: `Assets/Resources/Game/sprites/BombSprite.png`, `BombSpriteBomb.png`
- 이슈: Apptive-Game-Team/WordOnlineClient#679
- 날짜: 2026-09-14

## 왜 다시 그렸나

2026-09-14 에 들어온 sprite 17개 중 이 둘만 부드러운 그러데이션과 광택이 있는
채색 그림체였다. 나머지 15개는 faceted 저폴리로 일관돼 있었다.

## 참조

`ANCHORS.md` 는 master-v2 만 생성 참조로 쓰라고 못박는다. 처음엔 live
`WindSpirit.png` 을 붙였는데 그것부터가 legacy 채색 그림체라 규칙 위반이었다.
`master-v2/WorldTreeSpirit.png` 로 바꿔 다시 뽑았다.

두 오브젝트 다 서버에서 `ElementType.WIND` 이고, 팔레트는 `STYLE.md` 의 Wind 행이다.
`BombSprite` 는 정령 문법을 따른다 — 둥글고 뭉툭한 몸, 팔다리 대신 말린 wisp,
크고 순한 눈.

## 불꽃 색을 맞췄다

정령이 안은 폭탄과 떨어뜨리는 폭탄이 같은 물건으로 읽혀야 해서, 양쪽 도화선 불꽃을
따뜻한 금색(`#FFD34A`, `#E8912A`)으로 통일했다. 첫 시도에서 `BombSpriteBomb` 이
밝은 회색 몸통에 창백한 불꽃으로 나와 90x90 에서 새싹 난 바위처럼 보였다. 몸통을
`#1E2224`~`#3A4246` 로 못박고 불꽃을 금색으로 지정해 다시 뽑았다.

## 배경

`.agents/skills/make-game-art/SKILL.md` 의 "Ask for a key colour, not for
transparency" 를 따랐다.

```bash
.art/tools/key-out-background.py candidates/sprite-clutching-keyed-source.png cut.png --key magenta
.art/tools/finalize-candidate.py cut.png Assets/Resources/Game/sprites/BombSprite.png --max-size 256
```

`enclosed_pixels` 는 채택본 둘 다 0 이다.

## 남은 것

정령 계열 live sprite 10개(`WindSpirit`, `ThunderSpirit`, `ZapMouse`,
`SeedSpiritSwarm`, `VineSpirit`, `CloudDragon`, `WillOWisp`, `Leafair`,
`StormRider`, `BubbleSpirit`)가 아직 legacy 채색 그림체다. 이 변경으로
`BombSprite` 가 정령 중 처음으로 master-v2 가 된다.
