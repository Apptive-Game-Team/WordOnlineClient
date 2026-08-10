# Handoff — 공유 공격 슬롯 적용과 금속성 자동 탈락

작업 대상 저장소: `word-online/client` (Unity 2022.3.22f1, WebGL)
GitHub Issue: [#444](https://github.com/Apptive-Game-Team/WordOnlineClient/issues/444)
PR: [#445](https://github.com/Apptive-Game-Team/WordOnlineClient/pull/445) (열림)
브랜치: `feature/444` (`origin/main` 기준)
부모 이슈: #377 (닫힘)

관련 문서:

| 문서 | 역할 |
|---|---|
| [2026-07-27 크리처 사운드 컨셉](2026-07-27-issue-377-creature-sound-concepts.md) | 무엇을 만들지의 최종 권위 |
| [2026-07-23 워크리스트](2026-07-23-issue-377-sound-replacement-worklist.md) | 10단계 순서와 사용자 결정 |
| [2026-07-21 스타일 가이드](2026-07-21-issue-377-sfx-style-guide.md) | 소리 방향의 최종 권위 |
| [2026-07-21 매니페스트](2026-07-21-issue-377-sfx-asset-manifest.md) | 승인·해시의 공식 기록처 |
| `.claude/skills/create-game-audio/references/golden-assets.md` | 승인분 재사용 참조 |

---

## 배경 요약

크리처·건물 사운드는 커밋 `84919f2e`에서 **의도적으로 비워둔 상태**였다. 프로필
11개의 clip 슬롯 66개가 전부 비어 있어 게임 오브젝트 소리는 전부 무음이었다.

2026-07-27 컨셉 문서가 방향을 두 번 바꿨다.

1. 원소별 공격음 7종 폐기 → **근거리 / 원거리 / 폭발** 3분류 공유
2. 원소는 **소환음에만** 남긴다 (공격·피격은 초당 여러 번 울리므로 개성이 곧 소음)

이번 작업은 그 3분류 중 두 자리를 채우고, 반복되던 판정 실패의 원인을 자동화로
막은 것이다.

### 왜 필터를 만들었나

사용자 판정이 매 배치마다 같았다 — **"챙 소리가 너무 난다."** 그런데
`audio_probe.py`는 피크·RMS·꼬리만 재고 있어서 이걸 잡을 지표 자체가 없었다.
즉 금속성 후보가 걸러지지 않고 매번 사람 귀까지 올라왔다. Claude는 오디오를
들을 수 없으므로, 이 실패는 사람이 반복해서 대신 잡아주는 구조였다.

---

## 이미 되어 있는 것

### 1. 금속성 자동 탈락 (`audio_probe.py`)

```
metallic_reject = ring_prominence_db >= 12 && hf_decay_to_-40dB_s >= 0.15
```

| 지표 | 의미 |
|---|---|
| `ring_peak_hz` | 고역 최강 공진 주파수 |
| `ring_prominence_db` | 주변 스펙트럼 대비 돌출량 (종처럼 좁으면 큼, 바람소리면 작음) |
| `hf_decay_to_-40dB_s` | 3 kHz 위 성분의 잔향 길이 |

**공진 탐색 하한은 2.5 kHz다.** 처음엔 1.5 kHz였는데 나무 몸통 울림(1~2 kHz)이
챙으로 잡혀서 올렸다. 이건 후보를 통과시키려고 낮춘 게 아니라는 근거가 있다 —
기승인 `wood_button_click_v4` / `card_hover_v1`을 재검사해서 여전히 통과하는 것을
확인했고, 합성 클랭은 탈락 / 나무 둔탁음은 통과하는 자체 검사를 남겼다.

```bash
python3 .claude/skills/create-game-audio/scripts/audio_probe.py --selftest
```

**한계:** 짧고 밝은 틱은 통과시킨다. 안 울면 챙이 아니라고 보기 때문이고, 거기까지
막으면 종이 소리도 같이 죽는다. 최종 판정은 여전히 사람 귀다.

### 2. 승인분 (2026-08-03, 사용자 청취)

| 슬롯 | 승인 후보 | 길이 / 피크 | 상태 |
|---|---|---|---|
| 폭발 | `r2_twolayer_b` | 0.45 s / −6 dBFS | **적용됨** |
| 카드 호버 | `candidate_02` | 0.12 s / −16 dBFS | **적용됨** |
| 근접 공격 | `candidate_07` | 0.25 s / −10 dBFS | 기록만, 미배선 |
| 원거리 공격 | `transient_release_02` | 0.80 s / −6 dBFS | **적용됨** (필터 예외) |

레벨은 컨셉 문서 위계대로 파일에 구웠다(폭발 −6 > 원거리 −9 > 근거리 −10 > 호버
−16). 프로필 볼륨을 건드리기 전에도 상대 크기가 맞다는 뜻이므로, **재정규화하지
말 것.**

### 3. 런타임 적용

| 경로 | 내용 |
|---|---|
| `Assets/Resources/Sound/Game/Shared/explosion_v1.wav` | 폭발 (신규) |
| `Assets/Resources/Sound/Game/Shared/ranged_attack_v1.wav` | 원거리 발사 (신규, 필터 예외) |
| `Assets/Resources/Sound/Game/Card/card_hover_v2.wav` | 카드 호버 (신규) |
| `Assets/Resources/Sound/Config/Profiles/TransientExplode.asset` | 신규 프로필, spawn 슬롯만 활성 |
| `Assets/Resources/Sound/Config/Profiles/TransientShot.asset` | 신규 프로필, spawn 슬롯만 활성 |
| `Assets/Resources/Sound/Config/ObjectSfxCatalog.asset` | 폭발 9종 + 발사 8종 재배정 |
| `Assets/Scripts/Sound/SoundAssets.cs` | `CardHover` 경로 v1 → v2 |

`TransientExplode`로 옮긴 9종:
`ElectricExplode` `FireExplode` `LeafExplode` `MagmaExplosion` `RockExplode`
`ShockOverload` `WaterExplode` `WaterExplosion` `WindExplode`

`TransientShot`으로 옮긴 8종:
`ChainLightning` `ElectricShot` `FireShot` `LeafShot` `MagmaFist` `TideCall`
`WaterShot` `WindBlade`

남은 `TransientLegacy` 21종(장판·낙하 계열)은 컨셉 문서대로 **무음**이다.

프리팹마다 클립을 꽂지 않고 프로필을 하나 더 만든 이유는, 폭발이 **런타임 타입
단위로 공유되는 슬롯**이기 때문이다. 카탈로그 한 곳만 보면 어느 타입이 무슨 소리를
내는지 알 수 있어야 한다.

---

## 미결 사항

### ~~A. 원거리 승인분이 새 필터에 걸린다~~ → 결정됨 (2026-08-03)

`transient_release_02`는 3.5 kHz / 12.2 dB / 0.216 s로 `metallic_reject = true`이고,
길이도 컨셉 문서 목표 0.30 s의 두 배가 넘는 0.80 s다. 승인 시점(2026-07-27)이
"챙 소리 난다"는 판정보다 앞선다는 점도 함께 보고했다.

**사용자 판단: 그대로 넣는다.** `Sound/Game/Shared/ranged_attack_v1.wav`로 설치하고
`TransientShot` 프로필을 통해 발사 계열 8종에 배선했다.

이건 **필터 예외 1건**이다 — 자동 탈락 판정을 사람이 뒤집은 유일한 사례이므로,
Play Mode에서 실제로 거슬리는지 확인할 때 이 클립을 먼저 볼 것. 거슬리면 목표
0.30 s로 재생성하면 되고, 파이프라인은 그대로다.

### B. 근접 공격을 어디에 켤 것인가

어느 유닛이 근접인지는 **서버 `attack_range`** 가 정한다. 클라이언트에서는 알 수
없다 — 투사체는 프리팹 참조가 아니라 서버가 보내는 런타임 타입으로 생성되므로
프리팹을 뒤져도 안 나온다. 이 리포의 마이그레이션에서 확인되는 건 6종뿐이고,
그중 5종이 원거리다.

| 오브젝트 | attack_range | 버킷 |
|---|---:|---|
| `crater_ember` | 1.5 | 근거리 |
| `bubble_generator` | 2.5 | 원거리 |
| `fire_spirit` | 4.0 | 원거리 |
| `magma_spirit` | 4.0 | 원거리 |
| `bubble_spirit` | 4.5 | 원거리 |
| `electric_tower` | 5.0 | 원거리 |

전 프로필에 근접음을 켜면 원거리 유닛이 붙어서 때리는 소리를 낸다. **#377이 고치려던
바로 그 정체성 오류다.** 서버 `attack_range` 표가 확보되면 프로필별로 슬롯만 켜면
된다 — 파일은 이미 승인·검사 완료 상태로 `.sfx-work`에 있다.

---

## 하지 말 것

- **승인된 파일을 재생성으로 재현하려 하지 말 것.** ElevenLabs Sound Effects는
  seed가 비결정적이다. 통과한 파일 자체가 골든 에셋이고, 해시가 곧 승인이다.
- **승인분을 재정규화하지 말 것.** 레벨 위계가 이미 파일에 구워져 있다.
- **필터 임계값을 후보 통과 목적으로 낮추지 말 것.** 조정이 필요하면 기승인 에셋
  재검사와 `--selftest`를 함께 돌려서 근거를 남길 것.
- **후보를 `Assets/` 아래에 두지 말 것.** `.sfx-work/`(gitignored)가 후보 보관처다.
- `.mcp.json`에 API 키를 하드코딩하지 말 것 — git 추적 파일이다.

---

## 환경 메모

**ElevenLabs 키는 스코프 제한 키다.** `check_subscription`은 `user_read` 권한이
없어 항상 401이지만 **생성은 정상 동작한다.** 이걸 "키가 죽었다"로 오독하지 말 것.

MCP 서버가 `.zshenv`의 환경변수를 못 받는 문제가 있어(GUI로 뜬 앱은 zsh 프로파일을
읽지 않는다) `.claude/settings.local.json`(gitignored)에 키를 넣어뒀다. 앱 재시작
후 적용된다. MCP가 여전히 안 되면 REST 직접 호출 경로가 스킬 문서에 있다.

후처리 스크립트는 `soundfile`이 필요하다:

```bash
python3 -m venv /tmp/sfx-venv && /tmp/sfx-venv/bin/pip install numpy soundfile
```

`audio_probe.py`는 numpy만 있으면 되고 시스템 python3로 돈다.

---

## 검증 — Unity에서 해야 한다

`.meta` 파일을 **손으로 작성해서 넣었다.** GUID는 새로 생성했고 import 설정은
`wood_button_click_v4.wav.meta`를 템플릿으로 복사했다(Force To Mono, normalize
off, preload, 2D). 에디터가 한 번 확인해주는 것이 안전하다.

1. Unity Editor로 프로젝트 열기 → 신규 에셋 import 확인
2. `Tools > Sound > Validate Object SFX Catalog` → 에러 0
3. Play Mode: 폭발 계열 스폰 시 소리 나는지, 카드 호버가 짧아졌는지
4. 볼륨 0 / 50 / 100 확인
5. WebGL 스모크 빌드

폭발은 레거시 클립이 `LegacySfxMuter`로 이미 죽어 있으므로 새 소리와 겹치지 않아야
한다. 겹치면 뮤터가 그 프리팹을 못 잡은 것이니 거기부터 볼 것.

---

## 다음 작업 순서

1. 미결 B 결정 (서버 `attack_range` 확보)
2. 소환음 7종 (원소별 — 유일하게 원소를 남기는 자리)
3. 피격 / 크리처 죽음 / 건물 죽음 3종 마무리 (후보 판정 중 상태)
4. 프로필 배선 → 카탈로그 검증 → Play Mode → WebGL

한 번에 한 이벤트씩 끝내고 다음으로 간다. 30개 일괄 생성은 하지 않는다(부모 계획
non-goal). 청취 승인 없이는 어떤 후보도 배선하지 않는다.
