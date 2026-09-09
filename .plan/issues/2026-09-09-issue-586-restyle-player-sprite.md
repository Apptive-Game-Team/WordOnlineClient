# 2026-09-09 — 플레이어 캐릭터를 master-v2 스타일로 다시 그린다

- Date: 2026-09-09
- GitHub Issue: #586
- Status: 시안 생성 완료, 방향 선택 대기

## Goal

인게임 플레이어 `Assets/Art/Images/Customize/PlayerCharacterBase.png` 를 `.art/STYLE.md`
의 master-v2 규칙을 지키는 스프라이트로 교체한다. 이 문서가 다루는 범위는 방향을
고르기 위한 시안 생성까지다.

## Non-goals

- `.art/anchors/master-v2/` 의 파일은 건드리지 않는다.
- 마스터 스타일 자체를 바꾸지 않는다. rendering technique 은 앵커에 고정한다.
- 방향이 정해지기 전에는 `Assets/` 아래 파일을 바꾸지 않는다.
- 생성된 배경을 스크립트로 지워서 투명하게 만들지 않는다.

## Context / Constraints

- 현재 스프라이트는 굵은 검은 외곽선과 흰 스티커 테두리, 정면 둥근 덩어리다.
  `STYLE.md` 의 외곽선 없음, faceted low-poly papercraft, 3/4 각도 오른쪽, 3등신,
  인간 진영 팔레트를 모두 어긴다.
- 같은 인물의 앵커가 이미 있다. `.art/anchors/master-v2/ApprenticeMage.png`.
- 교체는 파일 하나를 바꾸는 것이라 화면 크기와 접지점이 유지되어야 한다.
  캔버스 2048x2048, PPU 100, pivot Center, 몸통 bbox `(404, 539, 1831, 1679)`.
- 상대편 플레이어는 `ServedObject` 가 같은 스프라이트를 `flipX` 로 뒤집어 쓴다.
- 공격은 `DOTweenAction.SwingMobAttack`, 사망은 `DOTweenAction.FallForward` 로
  스프라이트 전체를 돌린다. 별도 공격 frame 은 필요 없다.
- 머리 위에 `MagicFailEffect` 가 뜬다.
- `Assets/Resources/Game/player.png` 과 `Assets/Art/Images/Customize/` 의 모자·망토는
  참조가 없는 미사용 파일이다. `Assets/Scripts/CustomizeScene` 도 없다.

## Approach (Checklist)

- [x] **Step 0: Recon** — 런타임 참조, 커스터마이즈 사용 여부, 캔버스 기준값 확인
- [x] **Step 1: 기획** — 시안 다섯 갈래와 프롬프트를 `.art/CONCEPT-BRIEF.md` 에 기록
- [x] **Step 2: 생성** — codex `image_gen` 으로 갈래별 생성, 34장을 `.art/concept/player-restyle/` 에 보관
- [ ] **Step 3: 선택** — 방향 하나를 고르고 이유를 `.art/STYLE.md` 에 기록
- [ ] **Step 4: 투명 배경 확보** — 고른 갈래를 alpha 가 실제로 들어온 파일로 다시 생성
- [ ] **Step 5: 마감** — 2048x2048 캔버스에 몸통 높이 1140px, 발끝 y=1679, 가로 중심 x=1117 로 합성해 교체
- [ ] **Step 6: 상태 기록** — `.art/PRODUCTION-STATUS.md` 갱신

## Validation

- **Commands to run:** Pillow 로 mode, 네 모서리 alpha, 투명 픽셀 비율 확인. 이어서
  자홍색 위에 합성해 눈으로 확인. `./.art/make-sheets.sh` 로 대비 시트 생성.
- **Expected output:** mode `RGBA`, 네 모서리 alpha 0, 캔버스의 5분의 1 이상이 투명.

## Risks & Rollback

- **Risks:** codex `image_gen` 이 투명 배경을 거의 만들어 주지 않는다. 34번 중 7번만
  alpha 가 들어왔고, alpha 가 들어온 컷은 3등신이 4.5등신으로 늘어나 앵커에서
  멀어진다. 이 상태로는 마감할 수 없다.
- **대안:** `OPENAI_API_KEY` 를 넣고 imagegen 의 CLI 경로(`gpt-image-1.5`,
  `--background transparent`)를 쓴다.
- **Rollback steps:** `Assets/` 는 아직 손대지 않았다. 되돌릴 것은 `.art/` 아래 파일뿐이다.

## Open Questions

- 어느 갈래로 갈 것인가. 시안 비교판에서 고른다.
- 캐릭터 정체성을 지금의 갈색 긴 머리로 이을 것인가(D), 앵커의 소년으로 갈 것인가(A·B·C·E).
