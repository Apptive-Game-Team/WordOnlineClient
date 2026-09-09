# 2026-09-09 — issue #584 청취 승인 사운드 48건 반입

- Date: 2026-09-09
- GitHub Issue: #584
- Status: Draft — Validation 검토 대기
- Parent plan: `.plan/issues/2026-08-10-issue-475-faction-repartition-and-signature-tier.md`
- Stacked on: PR #477 (issue #475) — 진영 재분할이 먼저 merge되어야 함

## 1. 무엇을 하는가

Gate S2와 S3에서 생성 및 청취로 승인된 사운드 48건을 저장소로 반입한다. profile slot 8개와 signature 39개를 연결하고, 건물 죽음 공유음 1건을 archetype이 Building인 profile 전부의 death slot에 연결한다. 각 파일의 해시는 스타일 가이드의 golden asset 규칙에 따라 변경 없음을 유지한다.

## 2. 승인 파일을 재인코딩하지 않는 이유

청취 승인된 모든 마스터는 peak -3 dBFS로 정규화되어 있다. 스타일 가이드의 golden asset 규칙에서 해시가 바뀌면 승인이 무효가 되므로 파일을 재정규화하지 않는다. 대신 목표 level을 `ObjectSfxEventSlot.volume` 계수로 맞춘다.

예시: 목표 level이 -10 dBFS이면 volume은 0.447이다. 이렇게 하면 승인된 해시가 그대로 유지되고, level 조정은 데이터로 남아 나중에 게임 밸런스를 고려해 조정할 수 있다.

## 3. signature 계층

이슈 #475에서 정의한 signature 계층의 대체 방식을 따른다. signature는 base slot에 겹쳐 재생하지 않고 대체한다. 소유권 매트릭스의 "한 이벤트에 한 owner" 규칙 때문이다.

catalog entry에 `signature` 필드를 추가해 slot 단위로 profile을 대체한다. 필드가 비어 있으면 지금과 동일하게 동작한다.

## 4. 배선 대상

JSON 파일 `Assets/Resources/Sound/Config/approved-sfx.json`에서 센 결과:

| target type | 개수 |
|---|--:|
| profile | 8 |
| signature | 39 |
| buildingDeath | 1 |
| **합계** | **48** |

profile은 진영·원소별 공유 베이스 spawn slot. signature는 유닛 개별 사운드. buildingDeath는 건물 죽음 공유음.

## 5. 제외한 것

승인된 56건 중 8건은 배선하지 않는다.

같은 event에 후보가 둘 이상 선택되어 결정이 필요한 3건:

- `spawn_burning_legion` ← `sp_hell` / `v2_hell_spawn` / `v3_hell_spawn`
- `signature_life_tree_spawn` ← `sig_lifetree_spawn` / `v2_lifetree_spawn`
- `signature_fire_lord_spawn` ← `sig_firelord_spawn` / `v2_firelord_spawn`

배선 경로가 없는 2건:

- `sp_nature` — Nature faction creature의 소속 유닛 전원이 signature를 받으므로 profile이 재생될 경로가 없다.
- `sp_water` — Water Slime faction creature의 소속 유닛 전원이 signature를 받으므로 profile이 재생될 경로가 없다.

미결 사항에 막힌 1건:

- `u2_towerback_spawn` — catalog의 `Towerback` row가 `intentionalSilent`다. 무음 해제 여부는
  이슈 #475 plan의 미결 C이고 아직 승인되지 않았다. 소속 profile이 없어 signature의 faction과
  archetype을 복사할 원본도 없다. 추측해서 배선하지 않고 `approved-sfx.json`의
  `excludedPendingDecision`에 사유와 함께 남겼다.

## 6. 범위가 아닌 것

- 소리가 없는 event의 재생성 (ElevenLabs 크레딧 소진)
- 선택 페이지 4~7의 판정
- 중복 채택 3건의 결정

## 7. Validation

Unity Editor에서 **순서대로** 실행해야 한다. 현재 `Assets/Resources/Sound/Config/Profiles/`에는
`TransientExplode`, `TransientLegacy`, `TransientShot` 세 개만 있고 진영 profile asset은 아직
생성되지 않았다. PR #477이 구 profile asset을 지우고 생성을 builder에 맡겼기 때문이다.
builder를 먼저 돌리지 않으면 `Wire Approved SFX`가 profile을 찾지 못해 대부분 실패한다.

- `Tools > Sound > Create or Update Baseline Object SFX Catalog` 실행 (먼저)
- `Tools > Sound > Wire Approved SFX` 실행
- `Tools > Sound > Validate Object SFX Catalog` 에러 0 확인
- Play Mode에서 기존 slot 재생 확인
- WebGL smoke build

**이 머신에서는 Unity Editor를 사용할 수 없으므로 compile 검증과 Play Mode 테스트는 수행하지 못했다.** 머지 후 CI 와 개발 환경에서의 검증이 필요하다.

## 8. Risks & Rollback

**`.meta` 파일을 손으로 작성했다.** Editor에서 import한 결과와 일치하는지 확인이 필요하다.

롤백: 이 commit을 되돌리고 builder를 다시 실행한다.

`signature` 필드가 비어 있으면 catalog는 지금과 동일하게 동작하므로, 이 변경은 기존 배선을
건드리지 않는다. PR #445에서 배선한 `TransientShot`과 `TransientExplode`도 영향받지 않는다.

`Tools > Sound > Wire Approved SFX`는 멱등이다. 두 번 실행해도 결과가 같다.
