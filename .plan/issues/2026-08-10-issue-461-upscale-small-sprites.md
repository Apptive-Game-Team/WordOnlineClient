# 2026-08-10 — 작은 스프라이트 고화질 업스케일

- Date: 2026-08-10
- GitHub Issue: #461
- Status: Awaiting Art Approval

## Goal

256px 정규화 과정에서 확대되어 선명도가 떨어진 기존 소형 스프라이트를 식별하고, 원래 디자인·실루엣·투명 배경을 유지하면서 고화질 후보로 복원한다.

## Non-goals

- 캐릭터나 마법의 디자인, 색상, 구도 변경
- `.art/anchors/master-v2/` 기준 이미지 수정
- 사용자 승인 전 배포 중인 `Assets/Resources/Game/sprites/` 원본 덮어쓰기

## Context / Constraints

- 현재 브랜치 `feature/461`에서 긴 축 256px 및 보정 PPU가 이미 적용되었다.
- 원본보다 작은 이미지를 단순 보간 확대해 흐릿하거나 계단진 디테일이 보일 수 있다.
- 파일명, 종횡비, 알파, Unity `.meta` GUID, PPU, 피벗 및 프레임 간 정렬은 유지해야 한다.
- 프로젝트의 2.5D cut-paper 렌더링 스타일과 64px 실루엣 가독성을 유지한다.

## Approach (Checklist)
- [x] **Step 0: Recon** 정규화 직전 Git 원본의 크기와 현재 파일을 비교해 업스케일 대상만 목록화한다. 확대 대상 54개(128px 이하 18개)를 확인했고, 축소된 2개는 제외했다.
- [x] **Step 1: Implementation** 128px 이하 원본 18개를 개별 복원하고, 원래 캔버스 비율·알파·접지점을 유지한 긴 축 256px 후보를 생성했다. 원형이 바뀐 3개는 입력 원본만 사용해 재생성했다.
- [x] **Step 2: Tests** 18개 후보의 크기, RGBA, 투명 모서리, 알파 실루엣 겹침률과 64px/256px 비교 시트를 검증했다.
- [ ] **Step 3: Rollout / Rollback** 사용자 검토용 시트를 제공하고 승인된 후보만 런타임 경로에 반영한다.

## Validation
- **Commands to run:** 이미지 크기/알파 검사, 64px 및 256px 비교 시트 생성, Unity importer와 `.meta` 차이 검사
- **Expected output:** 18개 후보 모두 긴 축 256px, RGBA, 모서리 알파 0, 원본 디자인 보존, 흐림·계단 현상 감소. 런타임 `Assets`는 승인 전 변경하지 않는다.

## Risks & Rollback
- **Risks:** 생성형 업스케일이 작은 형태나 표정을 재해석할 수 있고, 알파 가장자리 또는 애니메이션 프레임 정렬이 달라질 수 있다.
- **Rollback steps:** 후보는 별도 경로에 보관하고, 승인 전 런타임 파일을 수정하지 않는다. 반영 후 문제 시 해당 PNG만 직전 Git 버전으로 되돌린다.

## Open Questions
- `.art/concept/upscale-issue-461/comparison-18-256.png`와 `comparison-18-64.png` 검토 후 18개 후보를 런타임 `Assets`에 반영할지 사용자 승인이 필요하다.
- 작업 도중 활성 브랜치가 외부에서 `feature/461`에서 `main`으로 변경되어, `main`의 128px 원본을 깨끗한 베이스라인으로 사용했다. 브랜치 전환이나 런타임 파일 변경은 수행하지 않았다.
