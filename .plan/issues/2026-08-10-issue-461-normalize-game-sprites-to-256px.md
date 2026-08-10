# 2026-08-10 — 게임 스프라이트를 256px 기준과 PPU 크기로 표준화

- Date: 2026-08-10
- GitHub Issue: #461
- Status: Complete

## 2026-08-10 Post-merge follow-up

- [x] `feature/460` 병합 후 장변이 다시 256px가 아닌 스프라이트를 조사했다.
- [x] `CloudDragon.png`, `CloudDragonAttacking.png`, `LightningCloud.png`를 장변 256px로 정규화했다.
- [x] Cloud Dragon 두 프레임을 460 작업물의 월드 장변 2.2 기준인 PPU 116.363636으로 일치시켰다.
- [x] Unity 임포트, GUID, 피벗, 프리팹 참조와 비교 시트를 재검증했다.

## Goal

`Assets/Resources/Game/sprites`의 PNG 스프라이트를 종횡비와 알파를 유지한 채 장변 256px 기준으로 정규화하고, 변환 전 월드 크기는 Sprite Importer의 Pixels Per Unit 값으로 보존한다.

## Non-goals

- 화풍, 색상, 실루엣, 이펙트 구성 변경
- 서버 밸런스나 런타임 오브젝트 스케일 변경
- UI, 배경, 오라 등 대상 폴더 밖 이미지 변경

## Context / Constraints

- 현재 대상은 71개이며 장변 128, 192, 256 등과 일부 예외 크기가 섞여 있다.
- 기존 PPU는 대부분 100이고 일부는 80이다.
- 파일명과 `.meta` GUID를 유지해야 `Resources.Load` 및 프리팹 참조가 보존된다.
- 기본/공격/소환 프레임은 PPU, 피벗, 본체 배율, 지면 접점이 일치해야 한다.
- ImageMagick 실행 파일이 없으므로 동등한 고품질 알파 지원 리샘플러를 사용하고 별도 검증한다.

## Approach (Checklist)

- [x] **Step 0: Recon** 대상 PNG의 픽셀 크기, 알파 경계, PPU, 피벗, 애니메이션 프레임 관계를 기록한다.
- [x] **Step 1: Implementation** 종횡비를 유지해 캔버스 장변을 256px로 리샘플링하고 `newPPU = oldPPU × 256 / oldLongSide`로 `.meta`를 갱신했다. 의도적인 투명 패딩과 기존 피벗은 보존했다.
- [x] **Step 2: Tests** 모든 대상의 장변, 알파, GUID, 월드 크기 오차를 자동 검증하고 Unity 임포트 및 빌드를 확인했다.
- [x] **Step 3: Rollout / Rollback** 변경 파일 목록과 69개 스프라이트 비교 시트를 검토했다.

## Validation

- **Commands to run:** 이미지/메타 검증 스크립트, `dotnet build Assembly-CSharp.csproj -v minimal`, Unity 콘솔 및 임포트 API 확인, 아트 비교 시트 생성
- **Expected output:** 69개 대상 장변 256px, RGBA 및 GUID 유지, 월드 축 오차 최대 0.00508 유닛, Unity 임포트 실패 0건, 빌드 오류 0건

## Risks & Rollback

- **Risks:** 리샘플링으로 인한 미세한 가장자리 변화, 소수 PPU 반올림 오차, 특수 피벗 프레임의 시각적 위치 변화, 이펙트까지 포함한 범위가 과도할 가능성
- **Rollback steps:** `feature/461`의 대상 PNG와 `.meta` 변경을 파일 단위로 되돌리고 기존 리소스를 복원한다.

## Open Questions

- 없음. 256px 기준은 정사각 캔버스 강제가 아니라 기존 종횡비와 의도적인 투명 여백을 유지한 캔버스의 장변 256px로 적용했다.
