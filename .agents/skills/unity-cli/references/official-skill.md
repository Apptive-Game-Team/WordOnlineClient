# Unity's official `unity-cli` skill

Command syntax, per-command flags, and CI recipes live in Unity's own skill. This file says how to
get it and what it does and does not cover, so the two are not confused.

## Fetching it

The CLI can install it, but **it writes to the host OS's skills directory**. Run from WSL, the
Windows binary installs into `C:\Users\<user>\.claude\skills\unity-cli` — which an agent running
inside WSL never reads.

```bash
"$UNITY_BIN" skill install claude-code --dry-run    # confirm the target path first
```

To read it from WSL or to vendor it, clone the source instead:

```bash
git clone --depth 1 https://github.com/Unity-Technologies/skills
```

The `unity-cli` skill is `skills/unity-cli/`. The repository also carries 21 other Unity skills —
`new-unity-project`, `build-live-game`, `localization`, `optimize-web`, `ui-uitk`,
`unity-package-management`, `sprite-editor`, and others.

A central installer also exists: `npx skills add Unity-Technologies/skills`.

## What it covers

`SKILL.md` plus eight reference files, ~2,400 lines total:

| Topic | File |
|---|---|
| `auth`, `license`, `cloud` | `references/auth-license-cloud.md` |
| `editors`, `install`, `modules`, `install-modules` | `references/editors-install.md` |
| `projects`, `releases`, `templates` | `references/projects-templates.md` |
| `config`, `hub install` | `references/config-hub.md` |
| `run`, `test`, `build` | `references/build-run-test.md` |
| `logs`, `doctor`, `env`, `cache`, `upgrade`, `self-uninstall` | `references/diagnostics-maintenance.md` |
| `mcp`, `skill`, `pipeline` / `command` / `status`, `shell` | `references/integration-advanced.md` |

The parts worth carrying over regardless of platform:

- **Drive a live Editor rather than editing asset files.** Where `com.unity.pipeline` is present
  (Unity 6.0+), `unity command` and `unity command eval` act on the Editor's actual in-memory
  scene. Hand-editing `.unity` / `.prefab` / `.asset` YAML is error-prone (fileIDs and GUIDs
  assigned by hand), invisible to a running Editor until reimport, and easy to aim at the wrong
  file. Fall back to file edits only when no Editor is reachable — and say so.
- **Safe Mode masquerades as "no Editor".** A C# compile error boots the Editor into Safe Mode,
  where the Pipeline package does not load, so `unity status` / `command` / `list` cannot connect
  at all. Confirm with `unity pipeline list` before concluding the Editor is absent.
- **Parse stdout, not stderr.** A failed command still writes a complete JSON envelope to stdout
  with `success: false` and a populated `errors` array; `errors[0].code` is the stable token to
  branch on. Branch on `success` rather than on `data` being empty — some partial failures carry
  a populated `data`.
- **`--non-interactive` plus `--yes`** suppresses every prompt; both are needed in CI.
- Service-account auth for CI via `UNITY_SERVICE_ACCOUNT_ID` / `UNITY_SERVICE_ACCOUNT_SECRET`,
  which keeps the secret out of the argument list and shell history.

## What it does not cover

The official skill assumes the CLI, the Editor, and the project share one operating system.
Across ~2,400 lines there is a single incidental mention of case sensitivity and none of WSL.

Not addressed anywhere in it:

- Calling a Windows `unity.exe` from a WSL shell.
- Unix path arguments being resolved against `\\wsl.localhost\<distro>\...`.
- Unity's refusal to open projects on case-sensitive filesystems, and what that forces about
  where a project can live.
- The project lock that blocks batch mode while a GUI Editor is open, and `unity status` not
  reporting it.
- Cross-filesystem I/O cost when a project is reached over `/mnt/c`.

Those are this skill's job. Resolve the environment here first, then use the official syntax.
