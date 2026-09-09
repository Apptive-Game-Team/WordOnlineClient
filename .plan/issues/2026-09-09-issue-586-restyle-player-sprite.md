# 2026-09-09 — 플레이어 캐릭터를 master-v2 스타일로 다시 그린다

- Date: 2026-09-09
- GitHub Issue: #586
- Status: D안 채택, 두 프레임과 오라 적용 완료, Unity Editor 검수 대기

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
- [x] **Step 3: 선택** — D 긴 머리 수습생 채택, 이유를 `.art/STYLE.md` 에 기록
- [x] **Step 4: 투명 배경 확보** — 기본 프레임, 공격 프레임, 오라 모두 alpha 가 들어온 원본으로 확보
- [x] **Step 5: 마감** — `.art/tools/finalize-player-frames.py` 로 두 프레임을 한 배율·한 기준점에 합성
- [x] **Step 6: 애니메이션** — 공격 프레임 교체와 지팡이 끝 오라 이동을 프리팹에 배선
- [x] **Step 7: 상태 기록** — `.art/PRODUCTION-STATUS.md` 갱신
- [ ] **Step 8: Editor 검수** — Unity 로 한 번 열어 프리팹 배선과 스프라이트 import 확인

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

## 애니메이션

사용자 요청: 몸을 뒤로 젖히는 대신 지팡이를 위로 세우고 그 끝에 오라가 모여 있다가,
공격할 때 지팡이를 앞으로 뻗는다.

- 기본 프레임은 지팡이를 세운 자세, 공격 프레임은 앞으로 뻗은 자세다. 결정 주변은
  비워 두고 오라는 별도 에셋으로 그 위에 얹는다.
- `AttackSpriteSwapController` 가 공격 이벤트에서 0.18초 동안 프레임을 바꾼다.
- `PlayerStaffAuraController` 가 같은 0.18초 동안 `StaffAura` 앵커를 세운 지팡이
  끝 `(-1.58, 7)` 에서 뻗은 지팡이 끝 `(6.14, 0.6)` 으로 옮긴다. 두 좌표는 마감한
  스프라이트에서 파란 결정 덩어리를 찾아 잰 값이다.
- 맥동은 앵커가 아니라 자식 `StaffAuraSprite` 의 `IdleAuraEffect` 가 맡는다.
- `PlayerActionController` 의 `SwingMobAttack` 호출을 지우는 것만으로는 부족했다.
  `ServedObject.PlayAttackPresentation()` 이 `_swingOnAttack` 기본값 `true` 로 같은
  스윙을 한 번 더 돌리고 있었고, Player 프리팹에 이 필드가 직렬화되어 있지 않았다.
  프리팹에 `_swingOnAttack: 0` 을 넣어 껐다.
- 마법 실패 연기 효과는 옛 스프라이트의 얼굴 위치 `(1.15, 0.75)` 에 맞춰져 있었다.
  새 캐릭터의 얼굴 위치 `(0.7, 3)` 으로 옮겼다.

## Open Questions

- Unity Editor 로 한 번 열어야 한다. 이 기계에서는 열 수 없어 `.meta` 와 프리팹
  배선을 전부 손으로 썼다.
- codex `image_gen` 의 투명 배경 성공률이 낮아 공격 프레임 하나에 25번을 썼다.
  `OPENAI_API_KEY` 를 넣으면 imagegen 의 CLI 경로(`gpt-image-1.5`,
  `--background transparent`)로 이 낭비를 없앨 수 있다.
