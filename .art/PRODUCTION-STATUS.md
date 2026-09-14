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
| `TitanRemnant.png` | 클라이언트 이슈 #593 재작업 검수 | 거신의 잔해 본체 |
| `TitanFist.png` | 클라이언트 이슈 #593 재작업 검수 | 주변 적 위치에서 솟구치는 거대한 돌주먹 |
| `LightningTadpole.png` | `concept-art/lightning-tadpole.webp` | 차원 유랑종 폭풍편 방향 적용 |
| `MagmaSpirit.png` | `concept-art/magma-spirit-idle.webp` | 기본 자세 |
| `MagmaSpiritAttacking.png` | `concept-art/magma-spirit-attack.webp` | 공격 자세 |
| `MagmaSpiritSpawn.png` | `concept-art/magma-spirit-spawn.webp` | 소환 자세 |
| `WaterSlimeSwarm.png` | `concept-art/water-slime.webp` | 물방울 생존자 무리 기본 자세 |
| `WaterSlimeAttackSpit.png` | `game-assets/water-slime-attack.webp` | 물 뱉기 공격 프레임 |
| `TidalWarhead.png` | 클라이언트 이슈 #597 재작업 검수 | 공중 표적용, 어두운 눈, 256x256 RGBA |
| `GroundTidalWarhead.png` | 클라이언트 이슈 #597 재작업 검수 | 지상 표적용, 밝은 눈, 본체와 동일 실루엣 |
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
| `PlayerCharacterBase.png`, `PlayerCharacterAttack.png` | 클라이언트 PR (이슈 #586) | 플레이어 수습 마법생 D안. 2048x2048 RGBA, 캐릭터 키 1140px, 발끝 y=1679 로 두 프레임 정렬. Unity Editor 검수는 남아 있다 |
| `SeaSerpent.png` | 클라이언트 PR (이슈 #672) | 201x256 RGBA. magic book 이 빈 칸이던 신규 아이콘. 물 소환수 문법, 곧추선 몸통에 두 겹 똬리. Unity Editor 검수는 남아 있다 |
| `BoulderStrike.png` | 클라이언트 PR (이슈 #672) | 256x146 RGBA. 밀려 나가는 바위와 뒤따르는 초승달. 전투 화면 투사체는 여전히 `RockRolling.png` 를 빌려 쓴다 |
| `SpiritBomb.png` | 클라이언트 PR (이슈 #672) | 256x183 RGBA. 필드 오브젝트가 없는 마법이라 책 아이콘 전용. LIGHTNING 과 NATURE 두 원소를 금색과 풀색으로 같이 쓴다 |
| `ligtning_aura.png` | 클라이언트 PR (이슈 #680) | 512x469 RGBA. `fire_aura.png` 기법으로 재작업한 전기 공용 오라. 64px 실루엣 검수 완료 |
| `nature_aura.png` | 클라이언트 PR (이슈 #680) | 512x484 RGBA. `fire_aura.png` 기법으로 재작업한 자연 공용 오라. 64px 실루엣 검수 완료 |
| `rock_aura.png` | 클라이언트 PR (이슈 #680) | 506x512 RGBA. `fire_aura.png` 기법으로 재작업한 바위 공용 오라. 64px 실루엣 검수 완료 |
| `water_aura.png` | 클라이언트 PR (이슈 #680) | 512x463 RGBA. `fire_aura.png` 기법으로 재작업한 물 공용 오라. `cloud.png`(운룡 전용 구형 오라)와는 별개. 64px 실루엣 검수 완료 |
| `wind_aura.png` | 클라이언트 PR (이슈 #680) | 512x269 RGBA. `fire_aura.png` 기법으로 재작업한 바람 공용 오라. 64px 실루엣 검수 완료 |
| `BombSprite.png`, `BombSpriteBomb.png` | 클라이언트 PR (이슈 #679) | 222x256 / 231x256 RGBA. 채색 그림체에서 master-v2 로 재작업. 도화선 불꽃을 양쪽 다 금색으로 맞췄다 |
| `explode.png` | 클라이언트 PR (이슈 #682) | 841x769 RGBA. 마법 폭발/실패 표시. 남색-보라 결정 파편 다발, 가운데 비움, 원본의 분홍·보라 정체성은 파편 두 개에 악센트로 남김. green 키 제거, 네 모서리 alpha 0 |
| `hit_effect.png` | 클라이언트 PR (이슈 #682) | 771x700 RGBA. 피격 표시, 8방향 쐐기 방사형 파편, 가운데 비움. magenta 키 제거, 네 모서리 alpha 0 |
| `fire_field.png` | 클라이언트 이슈 #681 재작업 검수 | 192x68 RGBA. 지옥불 팔레트, 테두리는 재 낀 암석 조각 사이로 주황 균열만 노출하고 중앙은 평평한 그을린 단색 지면 |
| `leaf_field.png` | 클라이언트 이슈 #681 재작업 검수 | 192x79 RGBA. 자연 정령 팔레트, 잎 모양 조각으로 테두리를 두르고 중앙은 평평한 단색 지면 |
| `water_field.png` | 클라이언트 이슈 #681 재작업 검수 | 192x69 RGBA. 물 슬라임 팔레트, 물결 조각으로 테두리를 두르고 중앙은 평평한 단색 수면 |

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
| 정령 계열 live sprite 10개 | `WindSpirit`, `ThunderSpirit`, `ZapMouse`, `SeedSpiritSwarm`, `VineSpirit`, `CloudDragon`, `WillOWisp`, `Leafair`, `StormRider`, `BubbleSpirit` 이 아직 legacy 채색 그림체다. #679 에서 `BombSprite` 를 고치다 확인했다 |

## 갱신 규칙

1. 웹사이트에 새 이미지가 추가돼 있으면 대응하는 Unity 리소스명을 찾아 `완료`에
   기록한다.
2. 클라이언트에서 먼저 교체했다면 PR 번호와 검수 결과를 근거로 기록한다.
3. 기본/공격/소환 프레임과 오라는 각각 별도 행으로 기록한다.
4. 단순 업스케일이나 포맷 변환은 아트 교체 완료로 기록하지 않는다.
5. 웹사이트 표시용 이미지와 런타임 PNG가 실제로 대응하는지 파일명과 외형을 함께
   확인한다.
