# Repository Guidelines

## Project Structure & Module Organization
This repository is a Unity WebGL client. Runtime code lives under `Assets/Scripts`, organized mostly by scene and domain: `LoginScene`, `LobbyScene`, `GameScene`, `ResultScene`, plus shared code in `Global` and `Data`. Scene assets are in `Assets/Scenes`, reusable prefabs in `Assets/Prefabs`, art/audio in `Assets/Art` and `Assets/Resources`, and localized content in `Assets/Localization`. Unity and package configuration live in `ProjectSettings/` and `Packages/`.

## Build, Test, and Development Commands
Open the project with Unity `2022.3.34f1`. Typical local work is done through the Unity Editor.

- `open -a Unity .` opens the project in Unity Hub/Editor on macOS.
- Build from the Editor using `BuildScript.BuildWebGL` or `BuildScript.BuildDevWebGL` in `Assets/Scripts/Editor/BuildScript.cs`.
- Example CLI build: `Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildDevWebGL -logFile -`

CI uses GitHub Actions:
- Push to `main` triggers `.github/workflows/deploy-test.yml`
- Push to `deploy` triggers `.github/workflows/deploy-itch.yml`

## Coding Style & Naming Conventions
Use C# with 4-space indentation and K&R braces, matching the existing codebase. Keep classes and public members in `PascalCase`, private fields in `camelCase`, and scene/domain folders aligned with namespaces where possible. Prefer descriptive MonoBehaviour names such as `GameSceneUIController` and keep shared infrastructure in `Assets/Scripts/Global`.

For visual styling, layout, colors, shapes, and typography guidelines, follow the [DESIGN.md](file:///Users/jeong-yunseong/development/word-online/dev/word-online/client/.agents/docs/DESIGN.md) design system spec.

## Testing Guidelines
`com.unity.test-framework` is installed, but there are no dedicated `Tests` assemblies in the current tree. For new automated tests, create Edit Mode or Play Mode test assemblies under `Assets/Tests` and name files `*Tests.cs`. At minimum, validate scene flow and WebGL-specific behavior manually in the Editor before opening a PR.

## Commit & Pull Request Guidelines
Recent history favors short Conventional Commit subjects such as `feat: ...`, `refactor: ...`, and `chore: ...`. Keep the subject imperative and specific. Pull requests should include a short behavior summary, linked issue or ticket, test notes, and screenshots or short clips for UI or scene changes.
Branch names should follow `<issue-label>/<issue-num>`, for example `feature/299`.

## Agent Workflow
When work starts from a new request, follow this order unless the user explicitly says otherwise:
1. Create or identify the GitHub issue for the task.
2. Create or switch to the branch that matches `<issue-label>/<issue-num>`.
3. Implement and verify the change.
4. Open a pull request that references the issue and includes test notes.

Every issue and pull request must set an assignee and a label. Do not leave either blank.

- Assignee: `--assignee @me`.
- Label: use the same value as the branch prefix. Check available labels with `gh label list`; do not invent a new label when none fit.
- Do not attach a project.
- When GitHub CLI authentication appears invalid inside a sandbox but the user says their session is valid, request escalated execution and retry `gh` with the user's session credentials before asking them to re-authenticate.

```bash
gh issue create --title "..." --body "..." --assignee @me --label documentation
gh pr create --base <base> --title "..." --body "..." --assignee @me --label documentation
```

Confirm the metadata after creation:

```bash
gh issue view <issue-number> --json assignees,labels
gh pr view <pr-number> --json assignees,labels
```

For Unity Editor automation through MCP, use the project skill at
`.agents/skills/unity-mcp-orchestrator/SKILL.md`.

## Configuration Notes
`DEV_BUILD` controls development server routing for WebGL builds. Avoid editing generated Unity metadata by hand unless the change is intentional, and keep `ProjectSettings/ProjectVersion.txt` in sync with the Unity version used for the change.

## Versioning

`PlayerSettings.bundleVersion`, serialized as `bundleVersion` in
`ProjectSettings/ProjectSettings.asset`, is the client's single version source.
Do not bump it in a pull request. The monorepo `deploy` skill bumps it once per
promotion: it commits `chore(release): WordOnlineClient vX.Y.Z` to `main`, merges
`main` into `deploy`, then tags and releases `vX.Y.Z` on the merge commit. The
level comes from the Conventional Commit messages promoted in that release:
MAJOR for a `!` marker or a `BREAKING CHANGE` trailer, MINOR for `feat:`, PATCH
otherwise, so write accurate commit types.

Do not maintain a second client version value.
