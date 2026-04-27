# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity WebGL client for a competitive online word/card game. Target platform is WebGL; CI/CD builds and deploys automatically on push to `main`.

- **Unity version**: 2022.3.22f1
- **Build target**: WebGL only
- **Live URL**: https://word-online.vercel.app/

## Build & CI/CD

There is no local build command — builds run exclusively through Unity Editor or GitHub Actions.

- Push to `main` → GitHub Actions builds WebGL → pushes artifacts to `WordOnline_Play` repo → deploys to itch.io
- Non-`main`/`deploy` branches get `-customDefines DEV_BUILD` injected, which points `MatchingServer` to `dev.lobby.ac.yunseong.dev` instead of `lobby.ac.yunseong.dev`
- Library folder is cached in CI keyed on `Assets/**`, `Packages/**`, `ProjectSettings/**`

## Scripting Defines

| Define | Effect |
|---|---|
| `DEV_BUILD` | Uses `dev.lobby.ac.yunseong.dev` for the matching/lobby server |
| `UNITY_WEBGL && !UNITY_EDITOR` | Selects `WebGLStompTransport` over `NativeStompTransport` |

## Architecture

### Singleton Pattern

Two base classes in `Assets/Scripts/Global/`:

- `LocalSingletonObject<T>` — scene-scoped singleton; destroyed on scene unload
- `SingletonObject<T>` — cross-scene singleton (`DontDestroyOnLoad`); use for global state

### Global State (`SceneContext`)

`SceneContext` (`Global.SceneContext`) is a `SingletonObject` that carries all cross-scene state: `JwtToken`, `User`, `MatchInfo`, `MatchResult`, `SelectedDeck`, `OwnedCards`. Call `SceneContext.ClearContext()` on logout.

### Server Endpoints (`Data.ServerList`)

- `ServerList.MatchingServer` — lobby/matchmaking (`lobby.ac.yunseong.dev:443`, HTTPS/WSS)
- `ServerList.AccountServer` — auth/account (`account.ac.yunseong.dev:443`, HTTPS/WSS)

HTTP helpers are on `Server`: `SetAuthorization(req)` adds Bearer token; `SetAcceptLanguage(req)` adds locale header.

### Real-Time Communication (STOMP)

`GameScene.StompConnector` (scene-local singleton) owns the connection lifecycle:

```
StompConnector
  ├─ IStompTransport          ← WebGLStompTransport | NativeStompTransport (platform-swapped in Awake)
  ├─ StompSubscriptionRegistry  ← subscription storage + resubscribe-on-reconnect
  └─ StompReconnectController   ← exponential-backoff reconnect
```

All inbound frame messages route through `GeneralHandler` (implements `IFrameInfoHandler<string>`), which dispatches by the `type` field:

| `type` value | Handler |
|---|---|
| `"frame"` | `DeltaFrameHandler` — applies delta updates (mana, cards, object create/update, timer) |
| `"sync"` | `SyncFrameHandler` — full snapshot reconciliation via `ObjectSyncer` |
| `"magicValid"` | `MagicValidHandler` |
| `"result"` | `ResultHandler` — transitions to ResultScene |
| `"pveScript"` / `"pveScriptEvent"` | `PveDialoguePresenter` |

Frame rate is fixed at 20 FPS (`GameConfig.FRAME_DURATION = 0.05f`).

### Game Object System

Server-authoritative object management in `GameScene.Object`:

- `ObjectContainer` — `Dictionary<int, ServedObject>` registry (scene-local singleton)
- `ObjectSpawner` — instantiates prefabs from `CreatedObjectDto`
- `ObjectUpdater` — applies `UpdatedObjectDto` to existing objects
- `ObjectSyncer` — reconciles a full `SnapshotObjectDto[]` against `ObjectContainer` (create missing, update existing, destroy removed)

`ServedObject` is the MonoBehaviour component attached to every server-managed prefab. Visual and gameplay behaviors attach as sibling components under `Assets/Scripts/GameScene/ServedObjectComponent/`.

### State/Event Binding

`StateEvent<T>` (`Global.StateEvent`) is a simple typed observable: holds `Data`, fires `OnStateChange` on every `UpdateData()` call. Used in ViewModels (e.g., `LobbySceneViewModel`) to bind UI to state without UnityEvents.

### Scene Structure

```
LoginScene → LobbyScene → GameScene → ResultScene
                       ↘ DeckScene / MagicBookScene / AdventuresScene
```

Each scene has a corresponding namespace under `Assets/Scripts/` (e.g., `LobbyScene`, `GameScene`). Scene-level controllers and ViewModels are `LocalSingletonObject`s.

### Localization

Uses `com.unity.localization` 1.3.1. `LocalizedString` fields on MonoBehaviours are set via Unity Inspector. `Server.SetAcceptLanguage` syncs the active locale to HTTP requests.
