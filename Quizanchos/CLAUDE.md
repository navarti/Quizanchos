# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Build (no .sln file — use the slnx)
dotnet build Quizanchos.slnx

# Run the web app
dotnet run --project Quizanchos.WebApi

# Run migrations standalone
dotnet run --project Quizanchos.Migrations
```

The WebApi project auto-applies EF Core migrations and seeds data on startup — no separate migration step needed for development.

**Database:** PostgreSQL via Npgsql.EntityFrameworkCore.PostgreSQL. Local dev expects Postgres on `localhost:5432` (e.g. `docker run -p 5432:5432 -e POSTGRES_PASSWORD=devpassword postgres:17-alpine`). Connection string in `appsettings.json` / user secrets.

**Npgsql legacy timestamp behavior** is enabled at startup (`AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)`) so `DateTime` properties don't have to be `Kind=Utc`.

**EF migrations** live in `Quizanchos.Domain/Migrations/`. To add a new migration:
```bash
dotnet ef migrations add <Name> --project Quizanchos.Domain --startup-project Quizanchos.WebApi
```

## Architecture

**Target:** .NET 10 / C# 14. ASP.NET Core MVC + REST API + SignalR.

### Project Dependency Graph

```
WebApi → Core, Domain, Common, Quiz.DbUpdater
Core → Common
Domain → Common (+ EF Core Identity)
Common → (none)
Minigame plugins → Core, Common (no reference to WebApi or Domain)
```

### Minigame Plugin System (most important pattern)

Minigames are **plugins discovered at runtime** via assembly scanning. No hardcoded references in WebApi to any minigame project — they are built separately and their DLLs are copied to the output directory (see `MinigamePluginProject` items in WebApi.csproj).

**To add a new minigame:**

1. Create a project implementing `IMinigameDescriptor` and `IMinigameFrontendDescriptor` (in a `/Descriptors` folder)
2. Implement `IGameLogic<TState, TMove>` (combines `IGameStateFactory`, `IGameValidator`, `IGameRules`)
3. Add the project as a `<MinigamePluginProject>` in `Quizanchos.WebApi.csproj`
4. Add static assets to `wwwroot/minigames/{gameKey}/`

Each descriptor self-registers its DI services via `RegisterServices(IServiceCollection)`.

**Key plugin interfaces (`Quizanchos.Core`):**
- `IMinigameDescriptor` — backend: create/load/save game engines, register services, define `MinigameTypeId`/`GameKey`/`MoveType`
- `IMinigameFrontendDescriptor` — frontend: CSS/JS URLs, display metadata, lobby/game view configuration
- `IGameEngine` — runtime game instance: `MakeMove()`, `GetState()`, `NeedToFinish()`
- `IGameLogic<TState, TMove>` — pure game rules (state factory + validator + rules)

**Runtime flow:** `GameService` → `GameLogicFactory` → `IMinigameRegistry` (lookup by gameKey) → `IMinigameDescriptor.CreateGameEngineAsync()` → `GameEngineWrapper<TState, TMove>` wrapping `GameEngine<TState, TMove>`.

Game state is persisted as JSON in `GameSessionState.StateJson` (one blob per game session).

### Polymorphic Move Deserialization

`GameMove` subtypes are registered at startup using `System.Text.Json` type discriminator (`"gameType"` field), populated dynamically from all descriptors' `MoveDiscriminator` values. When adding a new minigame, the move type is auto-registered.

### SignalR Hubs

- `GameHub` at `/hubs/game` — game state push, move submission, chat, emoji (900ms rate limit)
- `GameRoomHub` at `/hubs/room` — multiplayer lobby events

Groups are named `game-{guid}` and `room-{guid}`. Server pushes via `SignalRGameNotifier`/`SignalRRoomNotifier`.

### Authentication & Authorization

- ASP.NET Identity with cookie auth (`QAuth` cookie) + optional Google OAuth
- Three roles: `User`, `Admin`, `Owner` (hierarchical policies in `Startup.AddAuthorizaiton()`)
- Optional email confirmation flow toggled by `EmailConfirmation:ShouldUse` config
- Premium access: `PremiumUntilUtc` on `ApplicationUser`, checked by `PremiumAccessService` for minigames with `IsPremium = true`

### Frontend

Server-rendered Razor views (`/Views`) with client-side JS. Per-minigame assets in `wwwroot/minigames/{gameKey}/`. `MinigameViewController` uses `IMinigameFrontendRegistry` to inject the right CSS/JS into lobby/game views.

### Registered Minigames

| TypeId | GameKey | Project | Notes |
|--------|---------|---------|-------|
| 1 | Quiz | Quizanchos.Quiz | Single-player |
| 2 | Game2048 | Quizanchos.Game2048 | Single-player |
| 3 | QuizMultiplayer | Quizanchos.QuizMultiplayer | Multiplayer with teams |
| 4 | TicketToRideEurope | Quizanchos.TicketToRideEurope | Premium, multiplayer |

## Guidelines

- Do not implement or preserve legacy compatibility paths; use the current minigame plugin architecture only.
- Minigame plugins must not reference `Quizanchos.WebApi` or `Quizanchos.Domain` — only `Core` and `Common`.
- Game room state is in-memory (`ConcurrentDictionary` in `InMemoryGameRoomManager`) — not persisted across restarts.
- The Quartz job `LeadersUpdaterJob` runs monthly (1st of each month) to update the leaderboard.
