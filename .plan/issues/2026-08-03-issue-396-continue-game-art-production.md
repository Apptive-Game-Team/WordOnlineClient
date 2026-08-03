# 2026-08-03 — 남은 게임 아트 제작 계속

- Date: 2026-08-03
- GitHub Issue: #396
- Status: In Progress

## Goal

`master-v2` 화풍을 유지하면서 인간 건축물과 돌 골렘 진영의 재질·팔레트를 분리하고 승인된 후보를 Unity에 반영한다.

## Non-goals

- 정본 앵커, 서버 키, `resourceName`, 프리팹 이름과 전투 동작은 변경하지 않는다.

## Context / Constraints

- 인간은 차가운 회색·스틸 블루·청동/목재, 골렘은 따뜻한 탄색 석재·이끼로 구분한다.
- `Cannon`과 `RockMage` 후보는 2026-08-03 사용자 승인을 받았다.

## Approach (Checklist)

- [x] **Step 0: Recon** — 정본 문서와 런타임 연결을 확인한다.
- [x] **Step 1: Implementation** — 승인 후보를 생성하고 Unity PNG를 교체한다.
- [x] **Step 2: Tests** — 알파·크기·GUID, 빌드, Unity 로그와 축소 표시를 확인한다.
- [x] **Step 3: Rollout / Rollback** — 승인된 `ElectricTower`와 팔·다리를 추가한 `MiniRockSwarm` v3를 Unity에 반영하고 기존 `.meta`를 유지한다.
- [x] **Step 4: Next batch** — 승인된 `Towerback` v3 compact를 실제 리소스에 반영하고 기존 `.meta`를 유지한다.
- [x] **Step 5: Shared tower** — 승인된 `Tower.png`를 반영하고 `GroundTower`와 `RockTurret`의 직렬화 참조를 확인한다.
- [x] **Step 6: Rock turret** — 인간제 투석 포탑 `RockTurret.png` 후보를 제작하고 단일 발사체 및 64px 실루엣을 검증한다.
- [x] **Step 7: Rock turret rollout** — 승인된 `RockTurret.png`를 반영하고 기존 `.meta`를 유지한다.
- [x] **Step 8: Rock turret attack frame** — 승인된 발사 직후 Sprite를 추가하고 `AttackSpriteSwapController`를 빌드 프리팹 경로에 연결한다.

## Validation

- **Commands to run:** 후보 알파/크기 검사, `git diff --check`, Unity 콘솔 확인
- **Expected output:** 64px 진영 구분, 투명 배경, 티어 상한, 기존 `.meta` GUID 유지

## Risks & Rollback

- **Risks:** 새 비율이 프리팹 표시 크기나 발사점과 어긋날 수 있다.
- **Rollback steps:** PNG 교체 커밋을 파일 단위로 `git revert`한다.

## Open Questions

- `Towerback` v3 compact는 반영됐지만 Unity MCP가 연결되지 않아 실제 전투 장면의 발사점은 수동 확인이 필요하다.
- `Tower.png`는 `GroundTower`가 직접 사용하며 `RockTurret`에도 별도 직렬화 참조가 남아 있어 두 프리팹을 함께 확인해야 한다.
- 공용 `AbstractBuild`의 공격 프레임은 기본값이 비어 있어 다른 건물의 동작에는 영향을 주지 않는다.
- Unity MCP가 연결되지 않아 `Cannon`과 `RockMage`의 실제 전투 장면 크기·발사점은 수동 확인이 필요하다.
