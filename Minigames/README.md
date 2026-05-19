# Quizanchos Minigames

Standalone solution containing third-party Quizanchos minigame plugins, built against the
[`Quizanchos.Core`](https://www.nuget.org/packages/Quizanchos.Core) NuGet package from
nuget.org. No local feed or `../Quizanchos` checkout is required to build the plugins.

## Plugins

| Plugin | Game | Mode | TypeId |
| --- | --- | --- | --- |
| `Quizanchos.Plugin.Caravan` | Caravan (Fallout) | Single-player vs AI | 1100 |
| `Quizanchos.Plugin.CountryGuesser` | Country Guesser (map) | Single-player | 1200 |
| `Quizanchos.Plugin.CountryGuesserMultiplayer` | Country Guesser (map) | Multiplayer | 1201 |
| `Quizanchos.Plugin.Game2048` | 2048 | Single-player | 1300 |
| `Quizanchos.Plugin.TicketToRideEurope` | Ticket to Ride: Europe | Premium, multiplayer | 1400 |

## Build

```bash
dotnet build Minigames.slnx
```

## Including these plugins in the WebApi host

The host (`Quizanchos.WebApi`) discovers plugins at startup by scanning the `plugins/` folder
under its content root. **No code change in the host is required** — drop the published plugin
folder in, restart, and it appears on the home page automatically.

### TL;DR

```bash
# from the Minigames/ folder
dotnet publish Minigames.slnx -c Release

# Copy each plugin's publish output into the host's plugins/ folder
HOST_PLUGINS=../Quizanchos/Quizanchos.WebApi/plugins
mkdir -p "$HOST_PLUGINS"

cp -r Quizanchos.Plugin.Caravan/bin/Release/net10.0/publish                     "$HOST_PLUGINS/Caravan"
cp -r Quizanchos.Plugin.CountryGuesser/bin/Release/net10.0/publish              "$HOST_PLUGINS/CountryGuesser"
cp -r Quizanchos.Plugin.CountryGuesserMultiplayer/bin/Release/net10.0/publish   "$HOST_PLUGINS/CountryGuesserMultiplayer"
cp -r Quizanchos.Plugin.Game2048/bin/Release/net10.0/publish                    "$HOST_PLUGINS/Game2048"
cp -r Quizanchos.Plugin.TicketToRideEurope/bin/Release/net10.0/publish          "$HOST_PLUGINS/TicketToRideEurope"

# Run the host
dotnet run --project ../Quizanchos/Quizanchos.WebApi
```

Open <http://localhost:5000/> — you should see Caravan, Country Guesser, and Country Guesser
Multiplayer cards on the lobby.

### What the host expects

Each plugin folder under `plugins/` must contain:

- The plugin entry DLL (named in `plugin.json` under `entryAssembly`, e.g. `Quizanchos.Plugin.Caravan.dll`)
- `plugin.json` manifest
- `wwwroot/` directory with all referenced CSS and JS
- Any extra runtime files the plugin reads (e.g. `Data/countries.json` for the country guesser)
- Any transitive DLLs **except** `Quizanchos.Core`/`Quizanchos.Common`/`Microsoft.*`/`System.*`,
  which the host provides

Use `dotnet publish` (not `dotnet build`) — only `publish` copies `plugin.json` and `wwwroot/`
into the output, and resolves transitive dependencies. The plugin csproj files set
`<CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>` for these items.

### Configuring the plugin root

By default the host scans `{contentRoot}/plugins/`. Override it via `Plugins:Root` in
`appsettings.json` (absolute or relative path):

```json
{
  "Plugins": {
    "Root": "C:\\Quizanchos\\plugins"
  }
}
```

Or via environment variable: `Plugins__Root=/var/quizanchos/plugins`.

### Verifying the plugin loaded

On startup, the host logs one line per plugin:

```
[INF] Loaded plugin 'Caravan' (1 backend, 1 frontend descriptors) from .../plugins/Caravan
```

If the plugin loaded but doesn't appear on the home page, check that:

- `MinigameTypeId` is `>= 1000` (third-party requirement enforced by `PluginLoader`)
- The descriptor classes are `public`, non-abstract, and have a parameterless constructor
- The frontend descriptor's `LobbyUrl` doesn't collide with another plugin's

### Restarting after a redeploy

Plugins are loaded once at startup into a non-collectible `AssemblyLoadContext`. To pick up
a rebuilt plugin DLL you must restart the host process — there is no hot reload.

### Folder names matter

The folder name under `plugins/` is the plugin ID used in logs. It does **not** have to match
`GameKey`, but using the same name (`Caravan`, `CountryGuesser`, `CountryGuesserMultiplayer`)
keeps things readable.

### URL paths after install

Once loaded, each plugin is available at:

| Plugin | Lobby URL | Static assets |
| --- | --- | --- |
| Caravan | `/Minigame/Caravan` | `/minigames/caravan/...` |
| CountryGuesser | `/Minigame/CountryGuesser` | `/minigames/countryguesser/...` |
| CountryGuesserMultiplayer | `/Minigame/CountryGuesserMultiplayer` | `/minigames/countryguessermultiplayer/...` |
| Game2048 | `/Minigame/Game2048` | `/minigames/game2048/...` |
| TicketToRideEurope | `/Minigame/TicketToRideEurope` | `/minigames/tickettorideeurope/...` |

The static-asset mount point is always `/minigames/{GameKey-lowercased}/` — that's what the
descriptor's `*Styles` / `*Scripts` URLs reference.

## UI design system

The host serves a shared CSS shell at `/css/minigame-shell.css` (auto-loaded before
your plugin's styles). Use `.minigame-card`, `.minigame-btn`, `.minigame-option`,
`.minigame-score`, `.minigame-finished`, etc. for chrome — only ship your own CSS
for game-specific visuals (boards, cards, maps).

See [MINIGAME-SHELL.md](MINIGAME-SHELL.md) for the full class contract and tokens.

## Architecture

Each plugin follows the same pattern as `samples/Quizanchos.Plugin.ClickCounter` in the
host repo:

- A class implementing `IMinigameDescriptor` (backend lifecycle)
- A class implementing `IMinigameFrontendDescriptor` (frontend asset map)
- A `IGameLogic<TState, TMove>` implementation (pure rules)
- A factory wrapping `IGameStatePersistence` for state I/O
- Static assets under `wwwroot/`

The `Quizanchos.Core` SDK reference is marked `PrivateAssets="all" ExcludeAssets="runtime"`
so the host's copy is used at runtime (the host's `PluginLoadContext` redirects
`Quizanchos.Core` and `Quizanchos.Common` to the default load context for type identity).
