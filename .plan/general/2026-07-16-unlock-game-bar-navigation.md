# 2026-07-16 — 게임 bar 이동 락 해제

- Date: 2026-07-16
- GitHub Issue: None
- Status: Complete

## Goal

Allow the GameScene bar to open and close while a magic input response is pending.

## Non-goals

- Do not remove the card, field, or magic submission lock.
- Do not change the STOMP input-response protocol or timeout behavior.
- Do not modify unrelated pointer-selection work already present in the worktree.

## Context / Constraints

- `BarController` currently treats `CardInputSender.IsWaitingInputResponse()` as a navigation lock.
- Gameplay input must remain locked until `magicValid` or timeout.
- Field-select mode should still close the bar to expose the targeting field.
- The client worktree already contains unrelated user changes; preserve them.

## Approach (Checklist)
- [x] **Step 0: Recon** (Inspect `BarController`, `CardInputSender`, scene serialization, and history)
- [x] **Step 1: Implementation** (Decouple bar navigation from pending-response state in `Assets/Scripts/GameScene/BarController.cs`)
- [x] **Step 2: Tests** (Run a focused C# compile when available and inspect the final diff)
- [x] **Step 3: Rollout / Rollback** (No migration; revert the controller change if UI behavior regresses)

## Validation
- **Commands to run:** `dotnet build Assembly-CSharp.csproj --no-restore`; `git diff --check`
- **Expected output:** No new compile errors; bar controller diff only removes pending-response navigation guards.

## Risks & Rollback
- **Risks:** Players can move the bar while a magic response is pending, but card and gameplay actions remain guarded by `CardInputSender`.
- **Rollback steps:** Revert the `BarController` change; no data or protocol rollback required.

## Open Questions
- None.
