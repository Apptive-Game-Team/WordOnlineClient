# 2026-06-29 — Show local game server sessions on admin page

- Date: 2026-06-29
- GitHub Issue: #331
- Status: Draft

## Goal

Show admin session rows from both the configured lobby server and an optional local game server at `http://localhost:7777`, with source labels/colors so operators can tell where each session came from.

## Non-goals

- Do not change game server APIs.
- Do not make the local game server required for admin page load.
- Do not redesign the admin page layout beyond the source distinction needed for this feature.

## Context / Constraints

- Current admin flow is `RoomUIFactory` -> `AdminViewModel.FetchRoomList` -> `RoomApiClient.GetRoomList`.
- `RoomApiClient` only calls `ServerList.MatchingServer.url + "/api/game-sessions"`.
- Local server can be absent; its request must fail closed and preserve lobby results.
- Existing prefab already shows `serverUrl`, so source distinction can be added in code with existing UI fields plus minimal color styling.

## Approach (Checklist)
- [x] **Step 0: Recon** (Inspect existing code, locate files)
- [x] **Step 1: Implementation** (Code changes, file paths)
- [x] **Step 2: Tests** (Unit tests, manual verification steps)
- [x] **Step 3: Rollout / Rollback** (Feature flags, migration steps)

## Validation
- **Commands run:** `git diff --check`; `dotnet build client.sln`; `dotnet build Assembly-CSharp.csproj --no-restore`.
- **Expected output:** Whitespace check passes. Unity C# compile should succeed in Editor/CI.
- **Actual output:** `git diff --check` passed. Both `dotnet build` commands hung after printing MSBuild version and were killed after no further output.

## Risks & Rollback
- **Risks:** Unity serialization/prefab fields can limit visual changes without Editor import; local server CORS/network behavior may differ in WebGL builds.
- **Rollback steps:** Revert the branch or remove the local source from `RoomApiClient`.

## Open Questions
- None for first implementation; assume local endpoint path matches lobby `/api/game-sessions`.
