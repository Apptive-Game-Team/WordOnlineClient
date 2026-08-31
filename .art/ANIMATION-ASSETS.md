# 애니메이션 프레임과 오라 에셋

Unity 런타임에서 하나의 소환수 외형을 구성하는 기본 프레임, 공격 프레임, 오라를
구분하는 정본 문서다. 숫자 접미사는 별도 캐릭터가 아니라 상태 프레임일 수 있다.

## 의미 이름

| 실제 파일 | 의미 이름 | 런타임 연결 |
|---|---|---|
| `MagmaSpirit.png` | 용암 갑각 악마 · 선 기본 자세 | `MagmaSpirit.prefab` 기본 SpriteRenderer |
| `MagmaSpiritAttacking.png` | 용암 갑각 악마 · 내려찍기 자세 | `OnAttackSpriteSwapper.onAttackSprite` |
| `MagmaSpiritSpawn.png` | 용암 갑각 악마 · 지면 돌파 소환 자세 | 생성 시 0.28초 표시 |
| `RockGolem.png` | 이끼바위 골렘 · 기본 자세 | `RockGolem.prefab` 기본 SpriteRenderer |
| `RockGolem2.png` | 이끼바위 골렘 · 공격 자세 | `OnAttackSpriteSwapper.onAttackSprite` |
| `TreeGolem.png` | 고목 수호자 · 기본 자세 | `TreeGolem.prefab` 기본 SpriteRenderer |
| `TreeGolem2.png` | 고목 수호자 · 공격 자세 | `OnAttackSpriteSwapper.onAttackSprite` |
| `EvilEnt.png` | 사악한 고목 · 기본 자세 | `EvilEnt.prefab` 기본 SpriteRenderer |
| `EvilEnt2.png` | 사악한 고목 · 팔을 뻗은 공격 자세 | `AttackSpriteSwapController.swapSprite`, 공격 이벤트에서 0.1초 표시 |
| `AquaArcher.png` | 물결 궁수 · 활시위를 당긴 기본 자세 | `AquaArcherAttackPresenter` 기본 Sprite |
| `AquaArcherAttack.png` | 물결 궁수 · 시위를 놓은 공격 자세 | 공격 이벤트에서 0.08초 표시 |
| `RockTurret.png` | 인간제 투석 포탑 · 장전 자세 | `RockTurret.prefab` 기본 SpriteRenderer |
| `RockTurretAttacking.png` | 인간제 투석 포탑 · 발사 직후 자세 | `AttackSpriteSwapController.swapSprite`, 공격 이벤트에서 0.1초 표시 |

실제 파일명과 `.meta` GUID는 런타임 참조 때문에 유지한다. 문서와 홈페이지에서는
숫자 접미사 대신 의미 이름을 표시한다.

## 공격 프레임 규칙

`OnAttackSpriteSwapper`는 공격 이벤트에서 공격 프레임으로 바꾸고 기본값 기준
0.2초 뒤 원래 프레임으로 복귀한다. Transform 보정 없이 Sprite만 바꾸므로 두
이미지가 아래 조건을 만족해야 한다.

`AquaArcherAttackPresenter`는 평상시에 당긴 프레임을 유지하다 공격 이벤트에서
놓은 프레임으로 즉시 바꾸고 0.08초 뒤 복귀한다. 위치 튐을 막기 위해 별도
Transform 스케일 변형은 적용하지 않는다.

`MagmaSpiritSpawnPresenter`는 일반 소환 연출을 재생하는 생성에서만 지면 돌파
프레임을 0.28초 표시한 뒤 선 기본 자세로 복귀한다. 동기화 복구처럼
`playSpawnPresentation`이 꺼진 생성에서는 소환 프레임을 건너뛴다.

`RockTurret`은 공용 `AbstractBuild`의 `AttackSpriteSwapController`를 사용한다.
다른 건물 프리팹은 `swapSprite`가 비어 있어 공격 이벤트를 구독하지 않으며,
`RockTurret`만 발사 직후 프레임을 0.1초 표시한다.

`EvilEnt`는 공용 `AbstractRangeMob`의 `AttackSpriteSwapController`를 사용한다.
`TreeGolem`이 쓰는 레거시 `OnAttackSpriteSwapper`가 아니다. `EvilEnt.prefab`은
`swapSprite`에 `EvilEnt2.png`만 지정하고 `duration`은 상위 프리팹 값 0.1초를
그대로 쓴다. 이 컨트롤러도 Transform은 건드리지 않고 `SpriteRenderer.sprite`만
바꾸므로 아래 조건이 그대로 적용된다.

- 동일 PPU
- 동일 `Bottom Center` 피벗
- 동일 캐릭터 몸 배율
- 발 또는 지면 접점의 동일한 픽셀 기준점
- 동일 카메라 각도와 우측 방향
- 공격 동작에 필요한 팔·머리·효과만 변화
- 캔버스가 달라도 기준점에서 몸통 핵심점까지의 거리는 동일

검증할 때 두 PNG를 같은 PPU로 겹쳐 발점, 골반, 머리 중심을 대조한다.
`RockGolem`과 `RockGolem2`는 256x244, PPU 100, Bottom Center로 교정했다. 현재
`TreeGolem`과 `TreeGolem2`는 PPU가 서로 달라 재작업 전에 교정이 필요하다.
`TreeGolem`은 240x256에 PPU 100, `TreeGolem2`는 256x215에 PPU 80이다.

`EvilEnt`와 `EvilEnt2`는 224x256, PPU 100, Bottom Center로 일치한다. 두 프레임을
프레임마다 따로 자르지 않고 하나의 공유 크롭 박스와 하나의 공유 배율로 함께
내보냈기 때문에, 지면 접점 픽셀이 사후 대조가 아니라 제작 방식으로 이미 동일하다.
`TreeGolem` 쌍이 어긋난 원인이 프레임별 개별 크롭·개별 배율이므로, 새 공격
프레임은 이 방식으로 만든다. 캔버스 크기가 같아지는 것은 결과이지 목표가 아니다.

## 오라 의미

| 실제 파일 | 의미 이름 | 사용 방식 |
|---|---|---|
| `fire_aura.png` | 지옥불 · 대기 오라 / 공격 파동 | `FireIdleAura`, `FireAttackAura` |
| `wind_aura.png` | 바람 · 대기 오라 / 공격 파동 | `WindIdleAura`, `WindAttackAura` |
| `ligtning_aura.png` | 전기 · 대기 오라 / 공격 파동 | `LightningIdleAura`, `LightningAttackAura` |
| `nature_aura.png` | 자연 · 대기 오라 / 공격 파동 | `NatureIdleAura`, `NatureAttackAura` |
| `rock_aura.png` | 바위 · 대기 오라 / 공격 파동 | `RockIdleAura`, `RockAttackAura` |
| `water_aura.png` | 물 · 대기 오라 / 공격 파동 | `WaterIdleAura`, `WaterAttackAura` |
| `cloud.png` | 운룡 · 구형 물 아우라 | `CloudDragon.prefab` 전용 자식 SpriteRenderer |

오라는 본체에 합성하지 않는다. 공용 투명 Sprite로 별도 생성한다.

- 대기 오라: 반복 맥동, 알파 70%까지 변화, 약한 ±3도 회전
- 공격 파동: 0.3초 동안 0.86배 수축 후 1.35배 확장하며 페이드아웃
- 중심 정렬, 방사형 또는 타원형 실루엣
- 캐릭터 얼굴·팔다리·무기와 경쟁하는 고밀도 디테일 금지
- 불·운룡처럼 오라가 정체성에 필요한 개체도 본체와 오라를 독립 검증
- 운룡의 `cloud.png`는 바람 오라가 아니라 몸 전체를 감싸는 구형 물 아우라다.
  중앙은 본체가 읽히도록 저밀도로 유지하고, 공용 `wind_aura.png`와 혼용하지 않는다.

## 홈페이지 표시

- 기본·공격 프레임이 실제로 다른 포즈면 둘 다 표시한다.
- 컨셉과 축소 인게임 이미지가 같은 포즈면 컨셉만 표시한다.
- 오라는 소환수 상세의 `부속 에셋`으로 표시한다.
- 마법 상세와 소환수 상세은 양방향 링크를 제공한다.

## Cloud Dragon 프레임

| 실제 파일 | 의미 이름 | 프리팹 연결 |
|---|---|---|
| `CloudDragon.png` | 운룡 · 기본 자세 · 입에 물 없음 | `CloudDragon.prefab` 기본 SpriteRenderer |
| `CloudDragonAttacking.png` | 운룡 · 물 분사 공격 자세 | `AttackSpriteSwapController.swapSprite` |

두 프레임은 `220x156`, PPU 100, Bottom Center 피벗과 본체 배치를 공유한다.
공격 이벤트가 발생하면 `CloudDragonAttacking.png`를 0.1초간 표시한 뒤 기본 자세로 복원한다.
구형 물 아우라 `cloud.png`는 두 본체 프레임과 계속 분리해서 렌더링한다.

## 번개 강타 프레임

`LightningDropMagic`의 먹구름은 공격할 때마다 스프라이트가 바뀐다. 번개는 별도
오브젝트가 아니라 먹구름 스프라이트 안에서 구름 밑으로 자라는 프레임이다.

| 실제 파일 | 의미 이름 | 런타임 연결 |
|---|---|---|
| `sprites/LightningCloud.png` | 먹구름 · 대기 | `LightningCloud.prefab` 기본 SpriteRenderer |
| `sprites/LightningCloudStrike0.png` | 강타 · 구름 밑에 번개가 돋음 | `AttackFrameSequenceController.frames[0]` |
| `sprites/LightningCloudStrike1.png` | 강타 · 절반까지 자란 줄기 | `frames[1]` |
| `sprites/LightningCloudStrike2.png` | 강타 · 지면 직전까지 자란 줄기 | `frames[2]` |
| `sprites/LightningCloudStrike3.png` | 강타 · 지면 도달, 최대 밝기 | `frames[3]` |
| `sprites/LightningCloudStrike4.png` | 강타 · 잔광 | `frames[4]` |
| `sprites/LightningCloudStrike5.png` | 강타 · 소멸 직전 | `frames[5]` |

`AttackFrameSequenceController`는 공격 이벤트에서 여섯 프레임을 0.05초 간격으로
한 번 재생하고 대기 프레임으로 돌아온다. 한 자세를 잠깐 들고 있는
`AttackSpriteSwapController`와 달리, 자라나는 동작이 필요한 연출에 쓴다.

번개 오브젝트는 없다. 서버는 먹구름이 자기 아래 기둥을 범위 판정으로 때리고
`LightningDrop` 프리팹은 서버·클라이언트 양쪽에서 사라졌다. 지면 연출은
`ElectricField`가 담당한다.

### 캔버스 불변식

- 일곱 파일 모두 `320x640`, PPU 160, 중앙 피벗이다. 월드로는 `2.0 x 4.0` 유닛.
- 서버가 먹구름을 높이 `y=2`(`AERIAL_STANDARD_HEIGHT`)에 소환한다. 중앙
  피벗이므로 스프라이트는 지면 `y=0`부터 `y=4`까지를 정확히 덮는다. 구름은
  캔버스 최상단(월드 `y 2.8~4.0`), 번개는 구름 밑면(`y≈2.95`)에서 지면까지.
- 먹구름 높이는 서버 소환 좌표가 아니라 캔버스 안에서의 구름 위치로 조절한다.
  강타 판정 박스는 지면부터 `y=10`까지라 그림만 움직여도 판정은 그대로다.
- 여섯 프레임 x 0.05초 = 0.3초는 서버
  `LightningCloud.STRIKE_VISUAL_DURATION`과 같은 값이어야 한다. 서버는 마지막
  강타 뒤 그만큼 구름을 더 살려 두고 파괴한다. 한쪽만 바꾸면 마지막 번개가
  중간에 잘린다.
- `LightningCloud` 변형은 `Selectable`을 제거하고 `ServedObject._swingOnAttack`을
  끈다. 스프라이트가 지면까지 닿아서 선택 콜라이더가 그 아래 필드 클릭을 먹고,
  공격 연출의 30도 스윙은 캔버스 중심을 축으로 돌아 번개를 옆으로 던진다.

### 합성

프레임은 생성물이 아니라 결정적 합성 결과다. 원본 두 장
(`.art/concept/lightning-strike/`)에서 다시 만들 수 있다. 검수 시트는
`.art/sheets/lightning-strike-review.png`.

```bash
.art/tools/compose-lightning-strike.py \
  --cloud .art/concept/lightning-strike/storm_cloud_source.png \
  --bolt .art/concept/lightning-strike/lightning_bolt_source.png \
  --idle-out Assets/Resources/Game/sprites/LightningCloud.png \
  --frame-out 'Assets/Resources/Game/sprites/LightningCloudStrike{index}.png'
```
