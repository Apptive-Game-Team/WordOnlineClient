# 프로덕션 아트 교체 현황

게임에 적용한 새 아트를 한 곳에서 추적하는 정본 문서다. 새 이미지 교체 작업을
시작하기 전에 이 문서를 확인하고, 적용을 마친 같은 변경에서 해당 행을 갱신한다.
이 목록에 `완료`로 기록된 리소스는 다시 교체 후보로 제안하지 않는다. 재작업이
필요하면 먼저 상태를 `재작업 필요`로 바꾸고 구체적인 이유를 적는다.

## 상태 판정 기준

- `완료`: 새 아트가 클라이언트에 적용됐고, 웹사이트에 해당 이미지가 등록됐거나
  클라이언트 PR에서 검수까지 끝났다.
- `재작업 필요`: 새 아트를 적용했지만 프레임 정렬, 투명 배경, 스타일 등 확인된
  문제가 남았다.
- `미확인`: 새 이미지처럼 보여도 교체 완료 근거를 아직 대조하지 못했다.
- 웹사이트 기준은 [`Apptive-Game-Team/theevilent`](https://github.com/Apptive-Game-Team/theevilent)의
  `main`에 있는
  `public/concept-art/`와 `public/game-assets/`다. 웹사이트 파일은 표시용 WebP이고,
  실제 런타임 정본은 `Assets/Resources/Game/sprites/` 아래 PNG다.
- 최초 목록은 웹사이트 커밋 `860526f`을 기준으로 대조했다. 2026-08-31의
  후속 동기화는 웹사이트 PR
  [`#29`](https://github.com/Apptive-Game-Team/theevilent/pull/29)에 기록했다.
  이후에는 최신 `main`을 다시 확인해 새로 추가된 파일을 반영한다.

## 완료

| 런타임 리소스 | 웹사이트/검수 근거 | 비고 |
|---|---|---|
| `AquaArcher.png` | `concept-art/aqua-archer-drawn.webp` | 물의 궁수 기본 프레임 |
| `AquaArcherAttack.png` | `concept-art/aqua-archer-release.webp` | 화살을 놓은 공격 프레임 |
| `ChickenCommando.png` | `concept-art/chicken-commando.webp`, `game-assets/chicken-commando.webp` | 인간 공수 특공대 방향 적용 |
| `DimensionToad.png` | `concept-art/dimension-toad.webp`, `game-assets/dimension-toad.webp` | 차원 유랑종 경계 운반자 방향 적용 |
| `EmberSpiritSwarm.png` | `concept-art/ember-spirit-swarm.webp` | 지옥불 악마병 무리로 교체 |
| `EvilEnt.png` | 웹사이트 PR #29 `game-assets/evil-ent-idle.webp` | 사악한 나무 골렘 기본 자세 |
| `EvilEnt2.png` | 웹사이트 PR #29 `game-assets/evil-ent-attack.webp` | 팔을 뻗은 공격 자세 |
| `FireChildSpirit.png` | `concept-art/fire-child-spirit.webp` | 하급 악마 방향 적용 |
| `FireLordSpirit.png` | `concept-art/fire-lord-spirit.webp` | 지옥불 군단 지휘관으로 교체 완료 |
| `FireSpirit.png` | `concept-art/fire-spirit.webp` | 하급 뿔 악마 방향 적용 |
| `FireTadpole.png` | `concept-art/fire-tadpole.webp` | 차원 유랑종 화산편 방향 적용 |
| `RockGolem.png` | `concept-art/rock-golem.webp` | 이끼바위 골렘 기본 자세 |
| `RockGolem2.png` | `game-assets/rock-golem-attack.webp` | 이끼바위 골렘 공격 자세 |
| `RockRemnant.png` | `game-assets/rock-remnant.webp` | 사망 후 이동 방해 잔해 |
| `LightningTadpole.png` | `concept-art/lightning-tadpole.webp` | 차원 유랑종 폭풍편 방향 적용 |
| `MagmaSpirit.png` | `concept-art/magma-spirit-idle.webp` | 기본 자세 |
| `MagmaSpiritAttacking.png` | `concept-art/magma-spirit-attack.webp` | 공격 자세 |
| `MagmaSpiritSpawn.png` | `concept-art/magma-spirit-spawn.webp` | 소환 자세 |
| `WaterSlimeSwarm.png` | `concept-art/water-slime.webp` | 물방울 생존자 무리 기본 자세 |
| `WaterSlimeAttackSpit.png` | `game-assets/water-slime-attack.webp` | 물 뱉기 공격 프레임 |
| `cloud.png` | `game-assets/cloud-dragon-water-aura.webp` | 운룡 전용 구형 물 아우라 |
| `fire_aura.png` | `game-assets/fire-aura.webp` | 지옥불 공용 오라 |
| `LightningCloud.png` | 웹사이트 PR #29 `game-assets/lightning-cloud-idle.webp` | 번개 구름 대기 프레임 |
| `LightningCloudStrike0.png` | 웹사이트 PR #29 `game-assets/lightning-cloud-strike-0.webp` | 강타 1 프레임 |
| `LightningCloudStrike1.png` | 웹사이트 PR #29 `game-assets/lightning-cloud-strike-1.webp` | 강타 2 프레임 |
| `LightningCloudStrike2.png` | 웹사이트 PR #29 `game-assets/lightning-cloud-strike-2.webp` | 강타 3 프레임 |
| `LightningCloudStrike3.png` | 웹사이트 PR #29 `game-assets/lightning-cloud-strike-3.webp` | 강타 4 프레임 |
| `LightningCloudStrike4.png` | 웹사이트 PR #29 `game-assets/lightning-cloud-strike-4.webp` | 강타 5 프레임 |
| `LightningCloudStrike5.png` | 웹사이트 PR #29 `game-assets/lightning-cloud-strike-5.webp` | 강타 6 프레임 |
| `LightningDrop.png` | 웹사이트 PR #29 `game-assets/lightning-drop.webp` | 번개 투하 인게임 스프라이트 |
| `ChainLightning.png` | 클라이언트 PR #557, 웹사이트 PR #29 | 256x76 RGBA, 64px 실루엣 검수 완료 |
| `MagmaExplosion.png`, `MagmaExplosionStrike1.png`–`MagmaExplosionStrike4.png` | 클라이언트 PR #565 | 217x256 RGBA, 갑각 파편 우선 실루엣과 4프레임(균열→개방→피크→냉각) 검수 완료 |

## 다음 교체 후보

아래는 완료 목록에 없는 항목 중 아트 문서에 명시된 우선 후보들이다. 순서는
실제 작업 시 세계관·프리팹 연결·현재 이미지를 다시 확인한 뒤 정한다.

| 리소스 | 필요한 작업 |
|---|---|
| `MagmaExplosion.png` | 지옥불 갑각 파편과 내부 용암광 중심으로 재설계 |
| `FireShot.png` | 지옥불 군단의 뿔·갑각 모티프로 통일 |
| `Crater.png` | 현재 개념을 유지하고 마스터 렌더링 기법으로 통일 |
| `RallyingTotem.png` | 지옥불 설치물 셰이프 랭귀지 적용 |
| `TreeGolem.png`, `TreeGolem2.png` | 공유 크롭·배율·지면 접점과 PPU를 일치시켜 프레임 재작업 |

## 갱신 규칙

1. 웹사이트에 새 이미지가 추가돼 있으면 대응하는 Unity 리소스명을 찾아 `완료`에
   기록한다.
2. 클라이언트에서 먼저 교체했다면 PR 번호와 검수 결과를 근거로 기록한다.
3. 기본/공격/소환 프레임과 오라는 각각 별도 행으로 기록한다.
4. 단순 업스케일이나 포맷 변환은 아트 교체 완료로 기록하지 않는다.
5. 웹사이트 표시용 이미지와 런타임 PNG가 실제로 대응하는지 파일명과 외형을 함께
   확인한다.
