# 2026-07-27 — Vine World 마법 생성

- Date: 2026-07-27
- GitHub Issue: None
- Status: Complete

## Goal

Vine World 마법의 클라이언트 로컬라이징과 아이콘을 추가하고, 서버가 생성하는 GiantVine 오브젝트가 기존 GiantVine 스프라이트를 사용하는 올바른 런타임 프리팹으로 로드되도록 구성한다.

## Non-goals

- 서버 마법 ID나 조합 레시피를 클라이언트에 추가하지 않는다.
- VineWorld 이름의 런타임 프리팹은 만들지 않는다.
- 기존 Vine 프리팹이나 서버 생성 로직은 변경하지 않는다.

## Context / Constraints

- 마법 표시 데이터는 서버에서 오며 클라이언트 키는 `vineWorld`, 스프라이트 이름은 `VineWorld`로 파생된다.
- 제공된 `VineWorld.png`를 새 아이콘 자산으로 사용한다.
- `GiantVine.png`와 준비된 `GiantVine.prefab`은 커밋되지 않은 사용자 자산이므로 보존하며 필요한 최소 수정만 적용한다.
- GitHub 이슈 생성은 현재 외부 쓰기 권한 정책으로 허용되지 않아 로컬 브랜치에서 작업한다.

## Approach (Checklist)

- [x] **Step 0: Recon** (Inspect existing Vine family assets, localization, and prefab patterns)
- [x] **Step 1: Implementation** (Add localization/icon and correct GiantVine prefab overrides)
- [x] **Step 2: Tests** (Validate YAML IDs, PNG alpha/dimensions, GUID references, and compile)
- [x] **Step 3: Rollout / Rollback** (Document remaining issue/PR step and file-level rollback)

## Validation

- **Commands to run:** localization/resource searches, PNG metadata/alpha-bound inspection, prefab GUID/name checks, and `dotnet build Assembly-CSharp.csproj -v minimal`
- **Expected output:** `vineWorld` exists once in each localization table with a shared ID; both sprite assets have alpha and valid dimensions; `GiantVine.prefab` references `GiantVine.png` and has root name `GiantVine`; build has no errors.

## Risks & Rollback

- **Risks:** A server object type or magic name that differs from `GiantVine`/`Vine World` would not match the resource lookup convention.
- **Rollback steps:** Revert the localization rows, remove the new VineWorld sprite/meta, and restore or remove the GiantVine prefab assets before commit.

## Open Questions

- Confirm the server uses the exact names `Vine World` and `GiantVine` when integrating with the server branch.
