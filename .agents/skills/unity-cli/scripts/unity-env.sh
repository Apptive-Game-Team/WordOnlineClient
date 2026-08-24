#!/usr/bin/env bash
# Resolve the Unity CLI binary and platform for the current shell.
#
# Usage:
#   source .claude/skills/unity-cli/scripts/unity-env.sh
#   "$UNITY_BIN" --version --no-banner
#   "$UNITY_BIN" test "$(unity_path /mnt/c/Users/me/Projects/Game)"
#
# Exports UNITY_BIN and UNITY_PLATFORM (wsl | windows | mac | linux),
# and defines unity_path() for translating path arguments.
#
# Sourced, not executed: it must set variables in the caller's shell.

unity_detect_platform() {
  case "$(uname -s 2>/dev/null)" in
    Darwin) echo mac ;;
    Linux)
      # WSL identifies itself in the kernel release string.
      if grep -qi microsoft /proc/version 2>/dev/null; then echo wsl; else echo linux; fi
      ;;
    MINGW*|MSYS*|CYGWIN*) echo windows ;;
    *) echo linux ;;
  esac
}

unity_find_bin() {
  local platform="$1"

  case "$platform" in
    wsl)
      # The Windows Unity Hub bundles the CLI. WSL runs .exe through binfmt_misc.
      # Prefer it: it drives Windows Editors, which is what a Windows-plus-WSL
      # machine actually has installed.
      local win_cli="/mnt/c/Program Files/Unity Hub/resources/cli/unity.exe"
      if [ -x "$win_cli" ]; then echo "$win_cli"; return 0; fi

      # A Linux-native CLI drives Linux Editors only, against Linux-filesystem
      # projects only. Different tool, not a fallback for the above.
      if [ -x "$HOME/.local/bin/unity" ]; then echo "$HOME/.local/bin/unity"; return 0; fi
      ;;

    windows)
      command -v unity >/dev/null 2>&1 && { command -v unity; return 0; }
      local hub_cli="/c/Program Files/Unity Hub/resources/cli/unity.exe"
      [ -x "$hub_cli" ] && { echo "$hub_cli"; return 0; }
      ;;

    mac)
      command -v unity >/dev/null 2>&1 && { command -v unity; return 0; }
      # Hub bundles a CLI, but the path inside the app bundle is not stable
      # enough to hardcode — search for it.
      local found
      found="$(find "/Applications/Unity Hub.app" -name unity -type f 2>/dev/null | head -1)"
      [ -n "$found" ] && { echo "$found"; return 0; }
      ;;

    linux)
      command -v unity >/dev/null 2>&1 && { command -v unity; return 0; }
      [ -x "$HOME/.local/bin/unity" ] && { echo "$HOME/.local/bin/unity"; return 0; }
      ;;
  esac

  return 1
}

# Translate one path argument into the form the resolved binary expects.
#
# On WSL the binary is a Windows program: it resolves a Unix-style argument
# against the WSL filesystem's own UNC root, so /mnt/c/... silently becomes
# \\wsl.localhost\<distro>\mnt\c\... and the project is not found. Every path
# argument must be converted — the project path and each path-valued option
# (--output, --junit-output, --logfile, --build-path).
unity_path() {
  local p="$1"
  if [ "$UNITY_PLATFORM" = "wsl" ] && [ "${UNITY_BIN##*.}" = "exe" ]; then
    wslpath -w "$p"
  else
    echo "$p"
  fi
}

UNITY_PLATFORM="$(unity_detect_platform)"
export UNITY_PLATFORM

if UNITY_BIN="$(unity_find_bin "$UNITY_PLATFORM")"; then
  export UNITY_BIN
  echo "UNITY_PLATFORM=$UNITY_PLATFORM"
  echo "UNITY_BIN=$UNITY_BIN"
  if [ "$UNITY_PLATFORM" = "wsl" ] && [ "${UNITY_BIN##*.}" = "exe" ]; then
    echo "note: Windows binary — pass paths through unity_path(); it cannot open projects on WSL ext4."
  fi
else
  echo "unity: no CLI found for platform '$UNITY_PLATFORM'." >&2
  case "$UNITY_PLATFORM" in
    wsl|windows)
      echo "Install Unity Hub on Windows — it bundles the CLI at" >&2
      echo "  C:\\Program Files\\Unity Hub\\resources\\cli\\unity.exe" >&2
      ;;
    mac|linux)
      echo "Install with:" >&2
      echo "  curl -fsSL https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.sh | UNITY_CLI_CHANNEL=beta bash" >&2
      ;;
  esac
  return 1 2>/dev/null || exit 1
fi
