---
name: create-game-audio
description: Convert natural-language game audio requests into consistent ElevenLabs Sound Effects prompts and parameter sets, generate review batches, select golden assets, and integrate approved clips into Unity. Use for UI, cards, clicks, impacts, weapons, footsteps, magic and prefab events, ambience, loops, drones, trailer hits, or any sound-effect generation and replacement task.
---

# ElevenLabs 게임 오디오 생성

## 역할

사용자의 자연어 요청을 일관된 영문 사운드 프롬프트와 명시적 파라미터 세트로 변환하라. 동일 계열의 재질, 길이, 공간감, 원근감과 음량 위계를 유지하라. 생성 후보를 바로 적용하지 말고, 4개 후보를 비교하여 승인된 결과만 골든 에셋으로 저장·재사용하라.

사운드 방향을 정할 때 [프로젝트 스타일 가이드](references/style-guide.md)를 함께 따르라.
승인된 음원의 변형 또는 교체 작업을 시작할 때 [골든 에셋 목록](references/golden-assets.md)을 확인하라.

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
- 계열별 재질 소스는 스타일 가이드의 미니어처 재질 팔레트 표를 따르라.
- 프롬프트의 환경·원근 요소는 전장이 아니라 "dry close studio, tabletop,
  extreme close-up" 계열로 작성하라.

## 필수 출력

생성 전에 다음 형식으로 정규화한 요청을 먼저 제시하라.

```yaml
category: <preset category>
prompt: <completed English prompt>
duration_seconds: <explicit number>
prompt_influence: <0.0-1.0>
looping: <true|false>
audio_term: <Impact|Whoosh|Ambience|One-shot|Loop|Braam|Glitch|Drone|Stem>
candidate_count: 4
output_format: <WAV PCM 44.1 kHz preferred|MP3 44.1 kHz candidate>
family_id: <shared identity for related sounds>
```

`duration_seconds`를 Auto로 두지 마라. 같은 `family_id`에 속한 같은 이벤트 계열은 길이를 통일하라. 재현성과 프롬프트 충실도가 중요한 게임 에셋은 `prompt_influence: 0.7`을 기본값으로 사용하라. 자유 변주가 명시적으로 필요할 때만 0.55–0.65로 낮추고, 매우 구체적인 폴리 재현에는 0.75–0.85를 사용하라.

## MCP 호환성 규칙

- ElevenLabs Sound Effects의 목표 범위는 0.1–30초이다.
- 실제 호출 전에 연결된 MCP 도구 스키마를 확인하라.
- 현재 MCP가 더 좁은 범위만 받으면 값을 조용히 보정하지 마라. 제약을 알리고 직접 API 사용, 구간 분할, 또는 지원 범위 내 대안을 선택하라.
- 현재 MCP가 `prompt_influence`를 노출하지 않으면 표준 파라미터에는 0.7을 유지하되, 실제 호출에 적용되지 않았음을 명시하라. 적용된 척하지 마라.
- ElevenLabs 웹 제품은 한 번의 생성에서 4개 변형을 제공하지만 API는 일반적으로 호출당 1개를 반환한다. MCP가 하나만 반환하면 동일 파라미터로 4회 호출하여 하나의 논리적 후보 배치를 구성하라.
- Sound Effects 생성에는 결정론적 seed 재현을 기대하지 마라. 동일 프롬프트와 파라미터도 결과가 달라질 수 있다.
- 유료 호출이므로 사용자가 생성을 명시적으로 요청한 경우에만 실행하라.

## 사운드 카테고리 프리셋

| 카테고리 | Duration | Prompt influence | Looping | 기본 용어 | 설계 기준 |
|---|---:|---:|---|---|---|
| UI 클릭·버튼 | 0.12–0.35s | 0.75 | Off | One-shot | 짧고 건조하며 연타 시 겹침이 적어야 한다 |
| 카드 터치·배치 | 0.25–0.70s | 0.75 | Off | One-shot | 종이 섬유, 손가락 마찰, 카드 휨을 분리한다 |
| 마우스·바닥 클릭 | 0.15–0.40s | 0.75 | Off | One-shot | 대상 표면에 맞는 작은 접촉음으로 만든다 |
| 타격·임팩트 | 0.35–1.20s | 0.70 | Off | Impact | 공격 재질과 피격 재질을 모두 명시한다 |
| 무기 휘두름·발사 | 0.45–1.50s | 0.70 | Off | Whoosh / One-shot | release와 hit를 한 파일에 섞지 않는다 |
| 발소리·이동 | 0.25–0.80s | 0.75 | Off | One-shot | 표면·신발·무게별 family를 만들고 길이를 고정한다 |
| 생성·소환 | 0.70–2.00s | 0.70 | Off | Whoosh / Impact | 물질의 등장과 마법 에너지를 시간 순서로 구성한다 |
| 피격 반응 | 0.30–1.00s | 0.75 | Off | Impact | 대상의 재질과 크기가 중심이어야 한다 |
| 죽음·소멸 | 1.00–3.00s | 0.70 | Off | One-shot / Drone | 붕괴 또는 소멸 과정을 시작-본체-꼬리로 만든다 |
| 화염·마법 지속음 | 2.00–5.00s | 0.70 | On | Loop / Drone | 금속성 어택 없이 안정적인 지속 질감을 만든다 |
| 배경 앰비언스 | 20–30s | 0.70 | On | Ambience / Loop | 뚜렷한 시작·끝과 짧은 반복 표식을 없앤다 |
| UI·환경 Stem | 5–30s | 0.70 | On 또는 Off | Stem | 다른 레이어와 섞을 수 있도록 단일 요소만 생성한다 |
| Glitch 전환 | 0.20–1.20s | 0.65 | Off | Glitch / One-shot | 정보 전달을 방해하지 않는 짧은 디지털 변형으로 만든다 |
| 트레일러 히트 | 1.50–4.00s | 0.70 | Off | Braam / Impact | 저역 어택, 본체, 감쇠 꼬리를 순서대로 만든다 |

범위 안에서 임의의 길이를 매번 고르지 마라. 한 family를 시작할 때 기준 길이 하나를 정하고 이후 후보와 변형에 재사용하라.

이 프로젝트의 미니어처 디오라마 방향에서는 `트레일러 히트(Braam)`와
`Glitch 전환` 프리셋을 사용하지 않는다. 생성 duration 하한이 프리셋보다
길면 하한으로 생성한 뒤 후처리 단계에서 목표 길이로 트림하라.

## 프롬프트 표준 템플릿

모든 프롬프트에 다음 여섯 요소를 순서대로 포함하라.

```text
[Material or source] producing [action or event], in [environment or acoustic space],
heard from [distance and perspective], [standard audio term and production tags].
Temporal sequence: [onset] -> [main body] -> [decay or loop behavior].
[Necessary exclusions].
```

- **재질/음원:** wood, paper fiber, leather, stone, flame, foliage, steel, magical air 등
- **동작/이벤트:** pressed, dragged, flexed, ignited, swung, impacted, shattered, dissipated 등
- **환경/공간:** dry close studio, small wooden room, open forest clearing, stone hall 등
- **거리/원근:** extreme close-up, close perspective, 2 meters away, distant wide perspective 등
- **오디오 용어 태그:** `Impact`, `Whoosh`, `Ambience`, `One-shot`, `Loop`, `Braam`, `Glitch`, `Drone`, `Stem`
- **시간적 전개:** onset → body → decay, 또는 시간 순서가 있는 사건 시퀀스

필요한 배제 조건만 짧게 추가하라. 이 프로젝트의 사실적 폴리에는 다음 문구를 기본으로 고려하라.

```text
No chime, no bell, no glass, no sparkle, no synth, no cartoon character, no metallic ring.
```

복합음은 요소를 나열하지 말고 사건 순서로 작성하라. 편집 가능한 독립 이벤트가 필요하면 복합 생성 대신 release, travel, hit, death를 별도 에셋으로 분리하라.

## 변환 예시

### 나무 버튼 클릭

사용자 요청: `나무 버튼을 누르는 소리`

```yaml
category: UI 클릭·버튼
prompt: "A compact hardwood button being firmly pressed by a fingertip, recorded in a dry close studio, heard from an extreme close-up perspective, One-shot, clean Foley, restrained transient. Temporal sequence: soft fingertip contact -> short wooden compression click -> immediate dry decay. No plastic tick, no chime, no bell, no cartoon pop, no metallic ring."
duration_seconds: 0.25
prompt_influence: 0.75
looping: false
audio_term: One-shot
candidate_count: 4
output_format: WAV PCM 44.1 kHz preferred
family_id: ui-wood
```

### 카드 선택과 배치

사용자 요청: `카드를 만져서 테이블에 놓는 소리`

```yaml
category: 카드 터치·배치
prompt: "Thick matte paper card stock handled by a fingertip and placed on a wooden tabletop, recorded in a quiet dry room, heard from a close player perspective, One-shot, detailed Foley. Temporal sequence: subtle paper-fiber rub -> gentle card flex -> muted surface contact -> very short natural decay. No coin sound, no glass, no sparkle, no metallic edge, no casino flourish."
duration_seconds: 0.60
prompt_influence: 0.75
looping: false
audio_term: One-shot
candidate_count: 4
output_format: WAV PCM 44.1 kHz preferred
family_id: card-matte-paper
```

### 사실적인 화염 공격 발사

사용자 요청: `불 마법을 발사하는 소리, 챙 소리 없이`

```yaml
category: 무기 휘두름·발사
prompt: "A small hand-held flame source igniting and releasing a brief focused flame, like a match catching dry twigs, on a tabletop, recorded in a dry close studio, heard from an extreme close-up perspective, Whoosh, One-shot, realistic miniature fire Foley. Temporal sequence: soft air intake -> fast dry ignition -> small focused flame rush -> short smoky decay. No impact layer, no sword clang, no metal, no bell, no chime, no glass, no sparkle, no large-scale roar."
duration_seconds: 1.10
prompt_influence: 0.80
looping: false
audio_term: Whoosh
candidate_count: 4
output_format: WAV PCM 44.1 kHz preferred
family_id: magic-fire
```

### 간헐적인 숲 바람

사용자 요청: `조용하다가 가끔 바람이 부는 숲 배경음`

```yaml
category: 배경 앰비언스
prompt: "Natural forest air, soft wind through distant tree canopies, and sparse dry foliage, in an open temperate forest clearing, heard from a stationary wide listener perspective, Ambience, Loop, realistic environmental field recording. Temporal sequence: quiet forest air -> gradual medium breeze -> sparse leaf movement -> calm interval -> a second softer gust -> return to quiet air, with a seamless loop boundary. No music, no birdsong foreground, no storm, no constant strong wind, no dramatic swell."
duration_seconds: 30.0
prompt_influence: 0.70
looping: true
audio_term: Ambience / Loop
candidate_count: 4
output_format: MP3 44.1 kHz or supported loop format
family_id: ambience-forest
```

### 석재 골렘 피격

사용자 요청: `큰 돌 골렘이 둔한 공격에 맞는 소리`

```yaml
category: 피격 반응
prompt: "A hand-sized weathered stone figurine struck by a small blunt wooden stick, on a felt-covered tabletop, recorded in a dry close studio, heard from an extreme close-up perspective, Impact, One-shot, realistic miniature Foley. Temporal sequence: muted wood contact -> dense small stone knock -> a few pebble fragments settling on the table -> short natural decay. No metal clang, no explosion, no cinematic braam, no cartoon crack, no sub-bass."
duration_seconds: 0.90
prompt_influence: 0.75
looping: false
audio_term: Impact
candidate_count: 4
output_format: WAV PCM 44.1 kHz preferred
family_id: creature-stone-golem
```

## 표준 오디오 용어

| 용어 | 의미 |
|---|---|
| Impact | 물체 간 충돌과 접촉의 어택 및 본체 |
| Whoosh | 공기를 가르거나 빠르게 이동하는 동작음 |
| Ambience | 장소와 환경을 형성하는 배경 사운드 |
| One-shot | 한 번 재생되고 끝나는 비반복 효과음 |
| Loop | 끝과 시작이 자연스럽게 이어지는 반복 구간 |
| Braam | 트레일러에서 쓰는 크고 낮은 금관성·저역 히트 |
| Glitch | 디지털 오류, 끊김, 지터와 불규칙 변형 |
| Drone | 오래 지속되는 음정감 또는 질감 중심의 사운드 |
| Stem | 믹싱을 위해 분리된 단일 요소 또는 레이어 |

## 생성 워크플로우

1. 게임 이벤트, 대상 재질, 청취 시점, 기존 family와 믹서 경로를 확인하라.
2. 프리셋을 선택하고 명시적 duration 하나를 확정하라.
3. 여섯 요소 템플릿으로 450자 이내의 간결한 영문 프롬프트를 작성하라.
4. 정규화된 파라미터 세트를 사용자에게 제시하라.
5. 유료 생성 승인이 있으면 4개 후보 배치를 생성하라.
6. 후보를 gitignored `.sfx-work/issue-377/<family>/<event>/`에 서로 다른
   이름으로 저장하라. Unity 런타임 `Assets/Resources`에 후보를 넣지 마라.
7. **후처리 체인(필수)**: `scripts/postprocess_candidate.py <in> --peak -3
   [--target-duration <s>]`로 각 후보를 가공하라 — 선행 침묵 제거, 이벤트
   구간 트림, 꼬리 -60 dBFS 페이드, peak -3 dBFS 정규화, mono 44.1 kHz
   16-bit WAV(`*_proc.wav`) 출력. 원본은 보존하고, 청취에는 항상 가공본을
   제시하라. 생성 duration이 프리셋보다 길어졌다면 이 단계에서 목표 길이로
   트림하라. 생성 그대로의 파일을 청취에 제시하지 마라.
8. `scripts/audio_probe.py`로 가공본의 길이, 채널, 샘플레이트, peak, RMS,
   꼬리 잔류음, 말단 클릭을 검사하라. 원본 peak가 -20 dBFS 미만이거나
   가공 후에도 꼬리가 감쇠를 끝내지 못하면 제시 없이 재생성하라.
9. 청취 가능 여부를 명시하라. 파형과 수치 검사를 청취로 표현하지 마라.
10. 사용자가 통과시킨 후보만 골든 에셋으로 지정하라.
11. 골든 에셋의 프롬프트, 파라미터, 후처리 체인, family, 용도와 파일명을 기록하고 변형 생성에 재사용하라.
12. Unity 적용 시 기존 `.meta`와 GUID를 보존하고 import, loop, mixer, prefab/scene 이벤트를 검증하라.
13. 거절된 후보와 임시 파일은 최종 커밋에서 제외하라.

## 생성 전 품질·일관성 체크리스트

- [ ] 이벤트가 release, travel, hit, death 중 무엇인지 분명한가?
- [ ] 재질 또는 음원이 구체적으로 적혀 있는가?
- [ ] 동작 또는 사건이 동사로 적혀 있는가?
- [ ] 환경과 음향 공간이 적혀 있는가?
- [ ] 거리와 플레이어 원근이 적혀 있는가?
- [ ] 표준 오디오 용어가 하나 이상 포함되었는가?
- [ ] onset → body → decay 또는 반복 전개가 적혀 있는가?
- [ ] duration이 Auto가 아닌 명시적 초 단위인가?
- [ ] 같은 family의 길이, 재질, 공간감과 일치하는가?
- [ ] 기본 prompt influence가 약 0.7인가?
- [ ] 지속음과 앰비언스에 looping이 켜져 있는가?
- [ ] 불필요한 금속음, 차임, 음악, 음성 등 배제 조건이 있는가?
- [ ] 연타, 중첩 재생, 반복 청취 상황을 고려했는가?
- [ ] 후보 4개의 저장 이름과 검토 절차가 준비되었는가?
- [ ] MCP가 모든 요청 파라미터를 실제로 지원하는지 확인했는가?
- [ ] 유료 API 호출에 대한 사용자 요청이 있는가?

## 흔한 실패와 교정

| 실패 | 원인 | 교정 |
|---|---|---|
| 같은 요청인데 결과 성격이 계속 바뀜 | prompt influence가 낮거나 재질·공간이 모호함 | influence를 0.7–0.8로 올리고 family의 재질·공간 문구를 고정한다 |
| 길이가 후보마다 달라 타이밍이 맞지 않음 | Auto duration 사용 | 이벤트별 고정 초 값을 선택하고 모든 후보에 재사용한다 |
| 카드나 불에서 `챙` 소리가 남 | 모델이 밝은 마법·금속 어택을 추론 | `no metal, no bell, no chime, no glass`를 넣고 실제 물리 재질과 시간 전개를 앞에 둔다 |
| 버튼이 장난감·카툰처럼 들림 | `click`, `pop` 같은 추상 단어만 사용 | hardwood, fingertip compression, dry Foley, restrained transient로 구체화한다 |
| 복합음의 사건 순서가 뒤섞임 | 요소를 쉼표로만 나열 | `Temporal sequence: A -> B -> C`로 순서를 고정하거나 에셋을 분리한다 |
| 앰비언스 반복 경계가 들림 | looping off 또는 경계에 강한 사건 존재 | looping on, steady bed, seamless boundary를 명시하고 경계 근처의 큰 사건을 제거한다 |
| 숲 바람이 계속 세게 불어 피곤함 | 지속 세기를 프롬프트에 고정 | quiet interval과 intermittent gust를 시간 순서에 넣고 전경 사건을 줄인다 |
| 타격음이 모든 대상에서 동일함 | 공격 소스만 쓰고 피격 재질을 생략 | 공격 재질과 대상 재질을 함께 명시하고 대상 family별 골든 에셋을 만든다 |
| 네 후보가 사실상 같은 파일임 | 한 API 결과를 복제함 | API가 1개씩 반환하면 실제 생성을 4회 실행한다 |
| 같은 프롬프트를 다시 썼는데 완전히 재현되지 않음 | Sound Effects가 결정론적 seed 재현을 보장하지 않음 | 통과 파일 자체를 골든 에셋으로 보관하고 재생성에 의존하지 않는다 |
| 문서의 influence가 실제 결과에 반영되지 않음 | MCP가 해당 필드를 노출하지 않음 | 호출 전 스키마를 확인하고 미지원 상태를 보고하며 직접 API 경로를 사용한다 |

## 로컬 도구

후보를 청취용으로 가공하라(원본 보존, `*_proc.wav` 생성).

```bash
python3 .agents/skills/create-game-audio/scripts/postprocess_candidate.py \
  .sfx-work/issue-377/<family>/<event>/<name>.mp3 --peak -3 --target-duration 0.25
```

오디오 후보를 검사하라.

```bash
python3 .agents/skills/create-game-audio/scripts/audio_probe.py \
  .sfx-work/issue-377/**/*.wav
```

ElevenLabs를 사용할 수 없을 때 숲 환경음의 구조 검토용 프로토타입만 생성하라. 이를 자동으로 최종 에셋으로 승인하지 마라.

```bash
python3 .agents/skills/create-game-audio/scripts/generate_forest_ambience.py \
  --preset breezy \
  --output .sfx-work/issue-377/ambience-forest/loop/ingame_forest_air_candidate.wav
```
