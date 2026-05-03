# Quizanchos

A multi-game quiz platform built on ASP.NET Core MVC + REST API + SignalR. Users sign in, play minigames (single- and multi-player), earn coins, buy market items, and climb a monthly leaderboard.

For architecture, the plugin system, SignalR hubs, and how minigames are wired up, see [`CLAUDE.md`](./CLAUDE.md).

## Prerequisites

- **.NET 10 SDK**
- **SQL Server** (LocalDB or SQLEXPRESS). Default connection string in `appsettings.json` points at `localhost\SQLEXPRESS` with `Trusted_Connection=True`.
- **SMTP credentials** (required) — sign-up email confirmation and password reset. e.g. Gmail with an [App Password](https://myaccount.google.com/apppasswords).
- *(Optional, for full functionality)* Cloudinary (avatar uploads), Google OAuth (social login), Binance API (top-ups).

## First-time setup

The `appsettings.json` checked into the repo has empty placeholders for secrets. Fill them in via [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) — never commit real values.

```bash
cd Quizanchos.WebApi

# Owner account (created on first startup; do NOT keep the default "owner123")
dotnet user-secrets set "Owner:Email" "you@example.com"
dotnet user-secrets set "Owner:Password" "<a-real-password>"

# Email (SMTP — required for sign-up confirmation and password reset).
# Host/Port/FromName have defaults in appsettings.json (Gmail on 587).
dotnet user-secrets set "EmailConfirmation:Smtp:User"      "you@gmail.com"
dotnet user-secrets set "EmailConfirmation:Smtp:Password"  "<gmail-app-password>"
dotnet user-secrets set "EmailConfirmation:Smtp:FromEmail" "you@gmail.com"

# Cloudinary (avatar uploads)
dotnet user-secrets set "Cloudinary:CloudName" "..."
dotnet user-secrets set "Cloudinary:ApiKey"    "..."
dotnet user-secrets set "Cloudinary:ApiSecret" "..."

# Google OAuth (optional — leave empty to disable Google sign-in)
dotnet user-secrets set "Auth:Google:ClientId"     "..."
dotnet user-secrets set "Auth:Google:ClientSecret" "..."

# Binance (top-ups)
dotnet user-secrets set "BinanceApi:ApiKey"          "..."
dotnet user-secrets set "BinanceApi:ApiSecret"       "..."
dotnet user-secrets set "BinanceApi:UsdtAddressBep20" "..."
dotnet user-secrets set "BinanceApi:UsdtAddressTrc20" "..."
```

## Run

```bash
dotnet run --project Quizanchos.WebApi
```

The DbContext auto-migrates and seeds the database on startup. Visit `https://localhost:<port>` and sign in with the owner credentials you set above.

Swagger UI is available at `/swagger` in Development. The health endpoint is at `/health`.

## Tests

```bash
dotnet test Quizanchos.slnx
```

Tests live under `Tests/`. The first project, `Quizanchos.Quiz.Tests`, covers `QuizGameLogic` (pure rules — no DB required).

CI runs build + test on every push and PR (`.github/workflows/ci.yml`).

## Adding a new minigame

The platform discovers minigames at runtime via assembly scanning. To scaffold a new one, see the architecture section of [`CLAUDE.md`](./CLAUDE.md#minigame-plugin-system-most-important-pattern). Existing minigames under `Minigames/` and `Quizanchos.Game2048/` are good references.

## Health & operations

- `GET /health` — DB connectivity check (returns `Healthy` / `Unhealthy`).
- Logs roll daily into `Quizanchos.WebApi/logs/` (gitignored). Retention: 14 files, 50 MB each.
- Auth endpoints (`SignIn`, `SignUp`, `RequestPasswordReset`, `ConfirmPasswordReset`) are rate-limited to 5 requests / minute / IP.

## Security notes

- Default seed Owner credentials are placeholders — override via user-secrets before any deploy.
- Password reset uses an emailed confirmation code; SMTP credentials are required at startup.
- Account deletion is blocked when the user has top-up history (`TopUpOrder` rows are preserved for financial records).
