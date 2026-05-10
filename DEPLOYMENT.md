# Deployment & CI/CD

How the host is packaged, how plugins ship, and how releases are cut.

## Repo layout

```
.
├── .dockerignore                  # build context = repo root
├── .github/workflows/
│   ├── ci.yml                     # build & test host + every plugin
│   ├── docker-publish.yml         # push image to GHCR
│   ├── release-plugin.yml         # tag-triggered plugin ZIP release
│   └── release-sdk.yml            # publish Core/Common to NuGet.org
├── Quizanchos/                    # host (.NET 10 ASP.NET Core)
│   ├── Dockerfile
│   ├── docker-compose.yml         # base: db + app + (optional) cloudflared
│   ├── docker-compose.plugins.yml # override: mount host plugin folder
│   └── docker-compose.prod.yml    # override: pull image instead of build
└── Minigames/                     # third-party plugin solution
    └── Minigames.slnx
```

## Image build

Build context is the repo root (parent of `Quizanchos/`) so the Dockerfile can
see both the host and the plugin solution.

The Dockerfile has four stages:

| Stage | What it does |
|---|---|
| `sdk-pack` | `dotnet pack` of `Quizanchos.Core` + `Quizanchos.Common` into `/feed` |
| `host-publish` | `dotnet publish Quizanchos.WebApi` → `/app/publish` |
| `plugins` | restore `Minigames.slnx` against `/feed`, publish each `Quizanchos.Plugin.*` into `/app/plugins/<Name>` |
| `runtime` | `aspnet:10.0` + host + plugins, runs as `app` user, healthcheck on `/health` |

`sdk-pack`, `host-publish`, and `plugins` are independently cacheable — bumping
a single plugin's source only invalidates the `plugins` stage.

Each plugin folder in `/app/plugins/<Name>` is stripped of `Quizanchos.Core.dll`,
`Quizanchos.Common.dll`, and `Microsoft.Extensions.DependencyInjection.Abstractions.dll`
so the host's copies are used (the plugin load context redirects those types
to the default load context — see `Services/PluginSystem/PluginLoadContext.cs`).

## Local development

```bash
cd Quizanchos
cp .env.example .env
# edit POSTGRES_PASSWORD + secrets

docker compose up -d --build
docker compose logs -f app
```

Healthcheck: `curl http://localhost:8080/health` should return `Healthy`.

### Mounting a host plugin folder (hot-swap)

```bash
PLUGINS_DIR=/srv/quizanchos/plugins \
  docker compose -f docker-compose.yml -f docker-compose.plugins.yml up -d
```

Drop a published plugin folder into `$PLUGINS_DIR` (must contain
`<Name>.dll`, `plugin.json`, `wwwroot/`) and `docker compose restart app` —
plugins load once at startup, there's no hot reload.

### Behind Cloudflare Tunnel

```bash
docker compose --profile tunnel up -d
```

Requires `CF_TUNNEL_TOKEN` in `.env`.

## Production deployment

Pull the image instead of building locally:

```bash
IMAGE=ghcr.io/navarti/quizanchos:1.2.3 \
PLUGINS_DIR=/srv/quizanchos/plugins \
  docker compose \
    -f docker-compose.yml \
    -f docker-compose.prod.yml \
    -f docker-compose.plugins.yml \
    pull

docker compose \
    -f docker-compose.yml \
    -f docker-compose.prod.yml \
    -f docker-compose.plugins.yml \
    up -d
```

## CI workflows

### `ci.yml` — every push & PR

| Job | Output |
|---|---|
| `host` | `dotnet build` + `dotnet test` `Quizanchos.slnx`, packs SDK, uploads `sdk-nupkgs` artifact |
| `plugins` (matrix) | `dotnet publish` each plugin (Caravan, CaravanMultiplayer, CountryGuesser, CountryGuesserMultiplayer, Game2048, TicketToRideEurope) against the just-packed SDK, uploads `plugin-<Name>` ZIP |

Reviewers get drop-in plugin ZIPs from any PR run for manual smoke testing.

### `docker-publish.yml` — master pushes & `v*.*.*` tags

Builds + pushes the image to `ghcr.io/<org>/quizanchos`. Tags applied:

- branch tag (`master`)
- semver fan-out for `v1.2.3` → `1.2.3`, `1.2`, `1`
- `latest` on the default branch
- `sha-<short>` on every push
- `:buildcache` for cross-runner layer reuse

PRs build but don't push.

### `release-plugin.yml` — plugin tag

Tag a single plugin to ship it as a downloadable installable ZIP without
rebuilding the host:

```bash
git tag plugin/Caravan/v0.2.0
git push origin plugin/Caravan/v0.2.0
```

The workflow:

1. Packs the SDK locally (so plugin restore works against `Quizanchos/nupkgs`).
2. Stamps the version into `plugin.json`.
3. Publishes the plugin and zips it.
4. Creates a GitHub Release with the ZIP + `.sha256`.

Operators install with:

```bash
unzip Quizanchos.Plugin.Caravan-0.2.0.zip -d /srv/quizanchos/plugins/
docker compose restart app
```

You can also fire it manually via **Actions → Release plugin → Run workflow**
to publish a pre-release ZIP without tagging.

### `release-sdk.yml` — SDK tag

Tag with `sdk/v<Version>` (matching the `<Version>` in the two SDK csproj
files) to push `Quizanchos.Core` + `Quizanchos.Common` to NuGet.org:

```bash
git tag sdk/v0.1.2
git push origin sdk/v0.1.2
```

Requires the `NUGET_API_KEY` secret. The workflow refuses to publish if the
tag doesn't match the csproj version, preventing accidental version drift.

## Release flow at a glance

```
                       master push
                        │
                        ▼
                ┌───────────────┐
                │ ci.yml        │ ── builds & tests host + plugins
                └───────┬───────┘
                        │
                        ▼
                ┌───────────────────┐
                │ docker-publish.yml│ ── ghcr.io/<org>/quizanchos:latest + sha
                └───────────────────┘

   tag plugin/Foo/v1.0.0           tag sdk/v0.1.2          tag v1.2.3
        │                                │                     │
        ▼                                ▼                     ▼
┌─────────────────────┐   ┌──────────────────────┐   ┌────────────────────┐
│ release-plugin.yml  │   │ release-sdk.yml      │   │ docker-publish.yml │
│ → GitHub Release    │   │ → NuGet.org          │   │ → :1.2.3 :1.2 :1   │
│   with ZIP + sha256 │   │ + GitHub Release     │   │   :latest tags     │
└─────────────────────┘   └──────────────────────┘   └────────────────────┘
```

## Required secrets

| Secret | Used by | Purpose |
|---|---|---|
| `GITHUB_TOKEN` | `docker-publish.yml`, `release-plugin.yml` | provided automatically; used for GHCR push and Release creation |
| `NUGET_API_KEY` | `release-sdk.yml` | NuGet.org push key (create at nuget.org → API Keys, scope: push to `Quizanchos.Core` + `Quizanchos.Common`) |
