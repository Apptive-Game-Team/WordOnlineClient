---
name: create-game-audio
description: Generate, review, and integrate game SFX/ambience for this Unity WebGL client (issue #377 style). Use when the user asks to create, regenerate, replace, audition, convert, or approve sound effects, BGM, or ambience — e.g. "버튼 소리 만들어줘", "효과음 후보 생성", "사운드 교체", "fire attack sfx", "ambience loop". Covers ElevenLabs prompt design, 4-candidate batches, golden-asset rules, and Unity integration gates.
---

# 게임 오디오 생성 (Claude Code용)

## 역할

자연어 오디오 요청을 명시적 파라미터를 가진 영문 ElevenLabs Sound Effects
프롬프트로 변환하고, 4개 후보 배치를 생성·검사·기록한다. 후보를 절대 바로
런타임에 적용하지 않는다. 사용자의 청취 승인만이 골든 에셋을 만든다.

사운드 방향: [스타일 가이드](references/style-guide.md).
승인 이력과 재사용: [골든 에셋](references/golden-assets.md).
승인/거절의 공식 기록처는 `.plan/issues/2026-07-21-issue-377-sfx-asset-manifest.md`이다.
스타일 최종 권위는 `.plan/issues/2026-07-21-issue-377-sfx-style-guide.md`이다.

## 사운드 정체성: 팝업북 디오라마 폴리

이 게임은 수공예 팝업북 디오라마다(유닛이 책처럼 펼쳐져 소환되고, UI는
나무·종이). 모든 소리는 **실제 재질이되 미니어처 스케일**이어야 한다.

판정 문장 하나로 모든 후보를 거른다:

> 이 소리를 낼 수 있는 물건이 손바닥에 올라가는가?

- 폭발·시네마틱 저역은 금지가 아니라 불필요하다. 돌 골렘의 죽음은 폭발이
  아니라 조약돌 무더기가 테이블에 쏟아지는 소리다.
- 죽음/소멸 = 소품이 쓰러지고 재질로 흩어지는 소리.
- 소환 = 종이 펼쳐짐(팝업) 한 겹 + 계열 재질 도착음 한 겹.
- 로비 BGM = 나무 말렛(마림바) 3–4음 돌림노래, 긴 쉼표, 아주 작게.
- 계열별 재질 소스는 [스타일 가이드](references/style-guide.md)의
  미니어처 재질 팔레트 표를 따른다.

## 절대 규칙 (위반 금지)

1. **후보 저장 위치**: `.sfx-work/issue-377/<family>/<event>/` (gitignored).
   후보를 Unity `Assets/Resources` 아래에 절대 넣지 않는다. `Assets/Art`에도
   승인 전에는 넣지 않는다.
2. **ELEVENLABS_MCP_BASE_PATH 함정**: `.codex/config.toml`은 MCP 출력 경로를
   `Assets/Resources/Sound/Generated`로 지정한다. ElevenLabs MCP가 그 경로에
   파일을 쓰면 즉시 `.sfx-work/issue-377/...`로 이동하고 Resources에 남기지
   않는다. `Generated` 폴더와 `.meta`가 생겼다면 함께 제거한다.
3. **후보 수**: 이벤트당 정확히 4개. API가 호출당 1개를 반환하면 같은
   파라미터로 4회 호출한다. 한 결과를 복제해 4개처럼 보이게 하지 않는다.
4. **duration 명시**: Auto 금지. 프리셋 범위 안에서 하나를 정해 family 전체에
   재사용한다. 범위를 벗어난 duration은 모델이 남는 시간을 임의의 잔향/링으로
   채우거나 경계에서 잘라 이상한 마무리음을 만든다(2026-07-23 버튼 배치 실패
   사례: 0.48 s 클릭 → 꼬리 링·말단 클릭). 커넥터 최소치가 프리셋을 초과하면
   넉넉히 생성한 뒤 **후처리 체인에서 목표 길이로 트림**한다. 생성 그대로의
   파일을 청취에 제시하지 않는다.
5. **looping**: 앰비언스/지속음만 on. one-shot은 off.
6. **유료 호출**: 사용자가 생성을 명시적으로 요청했을 때만 실행한다.
7. **청취 정직성**: Claude는 오디오를 재생해 들을 수 없다. 파형·스펙트럼·수치
   분석은 반드시 "분석"이라고 말하고 "들어봤다"라고 표현하지 않는다. 청취
   승인의 주체는 항상 사용자다.
8. **MCP 정직성**: 호출 전에 연결된 MCP 도구 스키마를 확인한다(Claude Code에서는
   ToolSearch로 elevenlabs 도구를 조회). `prompt_influence`를 노출하지 않는
   MCP라면 설계값을 기록하되 "실제 호출에는 적용되지 않았음"을 명시한다.
   적용된 척하지 않는다. MCP가 없으면 없다고 보고하고 직접 API 또는 사용자
   실행 경로를 제안한다.
9. **골든 에셋 보존**: 승인된 파일과 SHA-256이 곧 골든 에셋이다. 재생성으로
   재현하려 하지 않는다. 변환(WAV 정규화 포함)은 해시를 바꾸므로 반드시
   새 청취 승인을 받는다. 승인된 해시를 가진 파일을 덮어쓰지 않는다.
10. **최종 포맷**: one-shot은 mono PCM WAV 16-bit 44.1 kHz. 승인된 인게임
    앰비언스(48 kHz)는 문서화된 예외이며 리샘플하지 않는다. 변환 명령:
    `ffmpeg -i <in> -ac 1 -ar 44100 -c:a pcm_s16le <out.wav>` (게인/EQ 없이).
    MP3 후보는 절대 최종 런타임 에셋이 되지 않는다.

## 정규화된 요청 (생성 전 필수 출력)

```yaml
category: <preset category>
prompt: <completed English prompt, <=450 chars>
duration_seconds: <explicit number>
prompt_influence: <0.0-1.0>   # 미지원 MCP면 "design target only" 명시
looping: <true|false>
audio_term: <Impact|Whoosh|Ambience|One-shot|Loop|Drone|Stem>
candidate_count: 4
output_format: <WAV PCM 44.1 kHz preferred|MP3 44.1 kHz candidate>
family_id: <shared identity for related sounds>
```

기본 `prompt_influence`는 0.7. 구체적 폴리 재현은 0.75–0.85, 자유 변주가
명시적으로 필요할 때만 0.55–0.65.

## 카테고리 프리셋

| 카테고리 | Duration | Influence | Loop | 용어 | 기준 |
|---|---:|---:|---|---|---|
| UI 클릭·버튼 | 0.12–0.35s | 0.75 | Off | One-shot | 짧고 건조, 연타 중첩 안전 |
| 카드 터치·배치 | 0.25–0.70s | 0.75 | Off | One-shot | 종이 섬유·손가락·휨 분리 |
| 바닥/타깃 클릭 | 0.15–0.40s | 0.75 | Off | One-shot | 대상 표면 재질의 작은 접촉음 |
| 타격·임팩트 | 0.35–1.20s | 0.70 | Off | Impact | 공격 재질 + 피격 재질 모두 명시 |
| 발사·릴리즈 | 0.45–1.50s | 0.70 | Off | Whoosh/One-shot | release와 hit를 한 파일에 섞지 않음 |
| 이동·발소리 | 0.25–0.80s | 0.75 | Off | One-shot | 표면·무게별 family, 길이 고정 |
| 생성·소환 | 0.70–2.00s | 0.70 | Off | Whoosh/Impact | 물질 등장을 시간 순서로 |
| 피격 반응 | 0.30–1.00s | 0.75 | Off | Impact | 대상 재질·크기 중심 |
| 죽음·소멸 | 1.00–3.00s | 0.70 | Off | One-shot/Drone | 시작-본체-꼬리 |
| 화염·지속음 | 2.00–5.00s | 0.70 | On | Loop/Drone | 금속성 어택 없는 지속 질감 |
| 배경 앰비언스 | 20–30s | 0.70 | On | Ambience/Loop | 반복 경계·전경 사건 제거 |

이 프로젝트 방향(사실적 근접 폴리)에서는 Braam/Glitch/트레일러 히트 계열을
사용하지 않는다.

## 프롬프트 템플릿

여섯 요소를 순서대로 포함한다.

```text
[Material or source] producing [action or event], in [environment or acoustic space],
heard from [distance and perspective], [audio term and production tags].
Temporal sequence: [onset] -> [main body] -> [decay or loop behavior].
[Exclusions].
```

이 프로젝트 기본 배제 문구:

```text
No chime, no bell, no glass, no sparkle, no synth, no cartoon character,
no metallic ring, no plastic tick, no radio noise, no digital tick.
```

release, travel, hit, death는 항상 별도 에셋으로 분리한다.

## 워크플로우

1. 이벤트·재질·원근·기존 family를 확인하고 골든 에셋/manifest를 읽는다.
2. 프리셋과 고정 duration을 정하고 정규화된 요청을 사용자에게 제시한다.
3. 생성 승인 후 4개 후보를 `.sfx-work/issue-377/<family>/<event>/`에 서로
   다른 이름 + 날짜 접미사로 저장한다. 초단타(≤0.4 s) 이벤트는 커넥터
   최소치 때문에 넉넉히 생성될 수 있다 — 원본을 그대로 두고 다음 단계로.
4. **후처리 체인(필수)**: 각 후보를
   `python3 .claude/skills/create-game-audio/scripts/postprocess_candidate.py <in> --peak -3 [--target-duration <s>]`
   로 가공한다 — 선행 침묵 제거, 이벤트 구간 트림, 꼬리 -60 dBFS 페이드,
   peak -3 dBFS 정규화, mono 44.1 kHz 16-bit WAV 출력(`*_proc.wav`).
   원본 생성물은 삭제하지 않는다. 청취에는 항상 가공본을 제시한다.
   numpy/soundfile venv가 필요하다(스크립트가 안내 출력).
5. 검사: `python3 .claude/skills/create-game-audio/scripts/audio_probe.py <files>`
   — 가공본(`*_proc.wav`)을 검사한다. 길이, 채널, 샘플레이트, peak, RMS,
   꼬리 잔류음, 말단 클릭을 본다. 다음은 자동 탈락(제시 없이 재생성):
   - 원본 peak가 -20 dBFS보다 낮은 사실상 무음 파일
   - 가공 후에도 파일 끝에서 감쇠가 끝나지 않는 경우(말단 클릭)
   - 본체 감쇠 후 꼬리에 강한 tonal ring/노이즈 재상승
   - 손바닥 스케일 테스트를 명백히 위반하는 스펙트럼(강한 서브베이스 등)
6. 측정 결과를 "분석"으로 보고하고 사용자 청취를 요청한다. 재생 명령 예시를
   제공한다.
7. 사용자가 고른 후보만 골든 소스로 지정하고 manifest와
   `references/golden-assets.md`에 파일명, SHA-256, 프롬프트, 파라미터,
   MCP 제약, 후처리 체인, 거절 후보명, 승인자·날짜를 기록한다.
8. 골든 소스가 이미 규격 WAV면 그대로, 아니면 최종 변환 후(해시 변경) 다시
   청취 승인을 받고 나서야 Unity에 넣는다. 기존 `.meta`/GUID를 보존하고
   import 설정(Force To Mono, 2D, Decompress On Load ≤1.2s, preload,
   normalization off)과 loop, 이벤트 연결을 Unity Editor에서 검증한다.
9. 거절 후보와 임시 파일은 커밋에 포함하지 않는다(`.sfx-work`는 gitignored).

## 직접 API 경로 (MCP 제약 회피)

MCP가 `prompt_influence`나 짧은 duration을 지원하지 않으면 REST를 직접
호출한다(`ELEVENLABS_API_KEY` 필요):

```bash
curl -s -X POST https://api.elevenlabs.io/v1/sound-generation \
  -H "xi-api-key: $ELEVENLABS_API_KEY" -H "Content-Type: application/json" \
  -d '{"text": "<prompt>", "duration_seconds": 0.5, "prompt_influence": 0.75}' \
  -o .sfx-work/issue-377/<family>/<event>/<name>.mp3
```

- API도 duration 하한이 있으면 하한으로 생성하고 후처리 트림으로 목표
  길이를 만든다.
- 응답이 JSON 에러면 파일에 저장하지 말고 에러를 그대로 보고한다.
- 호출 1회 = 후보 1개. 4회 호출로 배치를 만든다.

## 흔한 실패와 교정

| 실패 | 원인 | 교정 |
|---|---|---|
| 마무리가 이상함(꼬리 링/딸깍) | duration이 실제 사건보다 길어 모델이 잔향을 지어내거나 경계에서 잘림 | 프리셋 하한에 가깝게 duration을 줄이고, 승인 후 변환 단계에서 -60 dBFS 이하로 꼬리를 페이드/트림 |
| 사실상 무음 후보 | 생성 실패 배치 | probe에서 peak < -20 dBFS면 폐기하고 재생성 |
| `챙`/금속성 | 모델이 밝은 마법 어택을 추론 | 배제 문구 유지 + 실제 재질과 시간 전개를 프롬프트 앞에 배치 |
| 카툰/장난감 느낌 | `click`, `pop` 같은 추상 단어 | hardwood, fingertip compression, dry Foley, restrained transient로 구체화 |
| 앰비언스 반복 경계 들림 | looping off 또는 경계 사건 | looping on + seamless boundary 명시, 경계 근처 사건 제거 |
| 네 후보가 같은 파일 | 한 결과 복제 | 실제로 4회 생성 |
| 재생성 시 재현 안 됨 | Sound Effects는 seed 비결정적 | 통과한 파일 자체를 보존, 재생성에 의존 금지 |
| influence 미반영 | MCP가 필드 미노출 | 스키마 확인 후 미적용 사실을 기록 |

## 로컬 도구

```bash
python3 .claude/skills/create-game-audio/scripts/audio_probe.py .sfx-work/issue-377/**/*.wav
```

ElevenLabs 사용 불가 시 숲 환경음 구조 검토용 프로토타입만:

```bash
python3 .claude/skills/create-game-audio/scripts/generate_forest_ambience.py \
  --preset breezy \
  --output .sfx-work/issue-377/ambience-forest/loop/ingame_forest_air_candidate.wav
```

프로토타입을 자동으로 최종 에셋으로 승인하지 않는다.
