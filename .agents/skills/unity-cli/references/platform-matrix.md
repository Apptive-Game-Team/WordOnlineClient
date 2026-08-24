# Platform matrix and known beta bugs

Everything below was verified on Unity CLI `1.0.0-beta.5` against Unity 2022.3.34f1, on a
Windows 11 machine running Ubuntu 26.04 under WSL2. Version-specific claims are marked.

## Binary, path form, and reach

| Shell | Binary | Path form for arguments | Can open projects on |
|---|---|---|---|
| WSL | `/mnt/c/Program Files/Unity Hub/resources/cli/unity.exe` | Windows (`C:\...`) — convert with `wslpath -w` | NTFS only (`/mnt/c/...`) |
| WSL | `~/.local/bin/unity` (Linux-native, installed separately) | Unix | Linux filesystem only (ext4) |
| Windows | `unity` on `PATH`, or the Hub's `resources\cli\unity.exe` | Windows | NTFS |
| macOS | `which unity`, or a CLI inside `/Applications/Unity Hub.app` | Unix | Case-**insensitive** APFS/HFS+ only |
| Linux | `which unity`, or `~/.local/bin/unity` | Unix | Any local filesystem |

## Filesystem constraint

Unity refuses case-sensitive volumes outright — exit code **21**:

```
Fatal Error! The project is on case sensitive file system.
Case sensitive file systems are not supported at the moment.
Please move the project folder to a case insensitive file system.
```

| Editor | Filesystem | Result |
|---|---|---|
| Windows | NTFS | works |
| Windows | WSL ext4 via `\\wsl.localhost\...` | **refused, exit 21** |
| macOS | case-insensitive APFS (default) | works |
| macOS | case-sensitive APFS volume | **refused, exit 21** |
| Linux | ext4 | works — the check does not apply to Linux Editors |

There is no flag, mount option, or `fsutil` invocation that lifts this. `fsutil file
setCaseSensitiveInfo` operates on NTFS directories and has no effect on the ext4 filesystem
inside the WSL VM.

## Filesystem performance across the WSL boundary

Measured on this machine, 2000 small files:

| Operation | ext4 (`~/dev/...`) | `/mnt/c` (NTFS via WSL) |
|---|---|---|
| write | 0.020s | 13.429s |
| read | — | 9.558s |

Roughly 4.8ms per file open on `/mnt/c`. For scale: this repository holds 6,538 files,
2,250 of them under `Assets/`. A recursive grep that takes 0.02s on ext4 becomes tens of seconds
on `/mnt/c`.

Keep the canonical checkout on ext4. Use an NTFS checkout for the Editor only.

## Editor lock

Batch-mode commands (`build`, `test`, `run`) cannot take the project lock while a GUI Editor has
the project open:

```
Aborting batchmode due to fatal error:
It looks like another Unity instance is running with this project open.
```

`unity status` will **not** reveal this. It reports only Editors connected via
`com.unity.pipeline`; a plain GUI Editor does not appear. Do not read an empty `unity status` as
"no Editor running".

## Archived Editor versions

`unity releases` lists only the current live feed — at the time of writing, Unity 6000.x only,
20 entries by default. Older LTS lines such as 2022.3 are archived and will not appear, and
`unity install 2022.3.34f1` alone resolves nothing.

Archive installs need the changeset, which every project records in
`ProjectSettings/ProjectVersion.txt` as `m_EditorVersionWithRevision`:

```bash
grep m_EditorVersionWithRevision ProjectSettings/ProjectVersion.txt
#   m_EditorVersionWithRevision: 2022.3.34f1 (4886f5360533)

unity install 2022.3.34f1 -c 4886f5360533 -m webgl --accept-eula -y
```

## Credential stores are per-installation

The Windows CLI and a Linux-native CLI on the same machine keep separate sign-in state. Being
signed in through one says nothing about the other. Check the one you are about to use:

```bash
"$UNITY_BIN" auth status --no-banner
"$UNITY_BIN" license --no-banner
```

## Where a Linux-native install puts things

Installed by `install.sh`, it inherits Unity Hub's directory conventions even though Hub itself
is not required:

```
~/.local/bin/unity           binary (~21 MB)
~/Unity/Hub/Editor/<ver>     editors (~9.6 GB for 2022.3.34f1 + WebGL)
~/.config/unityhub           config, credentials, download cache
~/Applications/UnityHub.AppImage   only if `unity hub install` was run (~195 MB)
```

Full removal:

```bash
unity self-uninstall -y --purge
rm -rf ~/Unity ~/.config/unityhub ~/Applications/UnityHub.AppImage
```

## Known bugs in 1.0.0-beta.5

- **Empty human-format output.** Several commands print nothing and exit 0 where a human-readable
  result is expected — `install --dry-run` and `hub install` among them. Add `--json` to see the
  actual result. Treat an empty human-format response as "unknown", not "nothing to report".
- **`skill install --list` aborts on an unreadable client config.** One inaccessible file
  (observed: `C:\Users\<user>\.codex\AGENTS.md`) fails the entire listing instead of skipping that
  client. Target a client directly — `skill install claude-code --dry-run` — to work around it.
- **`help <cmd> <subcmd>` does not descend.** `unity help skill install` prints the parent's help.
  Use `unity skill install --help` instead.

## Running a Linux Editor GUI under WSLg — not viable

Recorded so it is not re-attempted. WSLg itself works: the RDP peer connects, windows are created
and mapped, and Windows sees them. Electron and Unity rendering is what fails.

Chromium-based apps (Unity Hub) fail with:

```
ContextResult::kTransientFailure: Failed to send GpuControl.CreateCommandBuffer
WARNING:media/gpu/vaapi/vaapi_wrapper.cc:1655] drmGetDevices2() has not found any devices
```

WSL exposes the GPU as `/dev/dxg`, not as a DRM node — `/dev/dri` does not exist. Mesa handles
this through its `d3d12` Gallium driver, but Chromium runs its own device discovery via
`drmGetDevices2()`, finds nothing, and cannot create a command buffer. There is no Vulkan
fallback either: the installed ICDs cover Intel, AMD, nouveau and Apple hardware, but not WSL's
d3d12 backend, leaving only `lvp` (CPU software rendering).

Unity Hub can be forced through with `--no-sandbox --disable-gpu`. The Unity Editor cannot — it
needs a real GL context. Combined with the case-sensitivity block, a Linux Editor under WSL is
useful only for headless work on an ext4 checkout, and useless for GUI authoring.
