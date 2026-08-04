# 2026-07-27 — Seed Nest 마법 생성

- Date: 2026-07-27
- GitHub Issue: None
- Status: Complete

## Goal

Seed Nest 마법의 클라이언트 로컬라이징과 아이콘을 추가하고, 서버가 생성하는 `SeedNest` 건물이 자연 속성 건물 패턴에 맞는 런타임 프리팹으로 로드되도록 구성한다.

## Non-goals

- 서버 마법 ID나 조합 레시피를 클라이언트에 추가하지 않는다.
- 건물 동작이나 서버 생성 로직을 새로 구현하지 않는다.
- 기존 Vine World 작업 및 다른 사용자 변경은 수정하지 않는다.

## Context / Constraints

- 서버 이름 `Seed Nest`에서 로컬라이징 키 `seedNest`, 리소스 이름 `SeedNest`가 파생된다고 가정한다.
- 제공된 `SeedNest.png`를 원본으로 사용하고 원본 파일은 덮어쓰지 않는다.
- 동일 자연 속성 건물인 `VineColony.prefab`과 `AbstractBuild.prefab`을 기준으로 최소 오버라이드만 변경한다.
- 현재 작업은 앞선 마법 자산과 같은 로컬 브랜치에 이어서 추가된다.

## Approach (Checklist)

- [x] **Step 0: Recon** (Inspect existing code, locate files)
- [x] **Step 1: Implementation** (Add localization, processed sprite, SeedNest prefab, and building presentation registration)
- [x] **Step 2: Tests** (Validate IDs, image alpha/dimensions, prefab GUIDs, and compile)
- [x] **Step 3: Rollout / Rollback** (Document server-name assumption and file rollback)

## Validation

- **Commands to run:** localization count checks, PNG alpha/dimension inspection, prefab name/GUID checks, `git diff --check`, and `dotnet build Assembly-CSharp.csproj -v minimal`
- **Expected output:** `seedNest` exists once in each localization table with a shared ID; the final sprite is no larger than 256×256 and retains alpha; `SeedNest.prefab` references the new sprite and inherits `AbstractBuild`; build completes with no errors.

## Risks & Rollback

- **Risks:** A server-side object type different from `SeedNest` will not match the prefab resource name. Sprite proportions may require later visual tuning in a gameplay scene.
- **Rollback steps:** Remove the three localization rows and delete the SeedNest sprite/meta and prefab/meta files before commit.

## Open Questions

- Confirm the server uses the exact magic name `Seed Nest` and created-object type `SeedNest`.
