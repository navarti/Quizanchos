# syntax=docker/dockerfile:1.7
#
# Build context = repo root (parent of this Dockerfile).
# docker-compose sets `context: ..` so this image can see both Quizanchos/ (host)
# and Minigames/ (third-party plugin solution).
#
# Stages:
#   sdk-pack     -> packs Quizanchos.Core/Common nupkgs into /feed (consumed by plugins)
#   host-publish -> publishes Quizanchos.WebApi (host) to /app/publish
#   plugins      -> publishes every plugin in Minigames.slnx to /app/plugins/<Name>/
#   runtime      -> aspnet base image + host + plugins
#
# Each stage is independently cacheable: changing a plugin's source does not
# bust the host-publish layer, and vice versa.

ARG DOTNET_SDK_TAG=10.0
ARG DOTNET_RUNTIME_TAG=10.0

# ---------- sdk-pack: produce the local NuGet feed for plugins ----------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_TAG} AS sdk-pack
WORKDIR /src
COPY Quizanchos/Quizanchos.Common/ Quizanchos/Quizanchos.Common/
COPY Quizanchos/Quizanchos.Core/   Quizanchos/Quizanchos.Core/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet pack Quizanchos/Quizanchos.Common/Quizanchos.Common.csproj -c Release -o /feed && \
    dotnet pack Quizanchos/Quizanchos.Core/Quizanchos.Core.csproj     -c Release -o /feed

# ---------- host-publish: build & publish Quizanchos.WebApi ----------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_TAG} AS host-publish
WORKDIR /src

# csproj-only copy first for restore caching.
COPY Quizanchos/Quizanchos.slnx Quizanchos/
COPY Quizanchos/Quizanchos.Common/Quizanchos.Common.csproj                                     Quizanchos/Quizanchos.Common/
COPY Quizanchos/Quizanchos.Core/Quizanchos.Core.csproj                                         Quizanchos/Quizanchos.Core/
COPY Quizanchos/Quizanchos.Domain/Quizanchos.Domain.csproj                                     Quizanchos/Quizanchos.Domain/
COPY Quizanchos/Quizanchos.Migrations/Quizanchos.Migrations.csproj                             Quizanchos/Quizanchos.Migrations/
COPY Quizanchos/Quizanchos.WebApi/Quizanchos.WebApi.csproj                                     Quizanchos/Quizanchos.WebApi/
COPY Quizanchos/Minigames/Quiz/Quizanchos.Quiz/Quizanchos.Quiz.csproj                          Quizanchos/Minigames/Quiz/Quizanchos.Quiz/
COPY Quizanchos/Minigames/Quiz/Quizanchos.Quiz.DbUpdater/Quizanchos.Quiz.DbUpdater.csproj      Quizanchos/Minigames/Quiz/Quizanchos.Quiz.DbUpdater/
COPY Quizanchos/Minigames/QuizMultiplayer/Quizanchos.QuizMultiplayer/Quizanchos.QuizMultiplayer.csproj  Quizanchos/Minigames/QuizMultiplayer/Quizanchos.QuizMultiplayer/
COPY Quizanchos/Tests/Quizanchos.Quiz.Tests/Quizanchos.Quiz.Tests.csproj                       Quizanchos/Tests/Quizanchos.Quiz.Tests/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore Quizanchos/Quizanchos.slnx

COPY Quizanchos/ Quizanchos/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish Quizanchos/Quizanchos.WebApi/Quizanchos.WebApi.csproj \
        -c Release -o /app/publish --no-restore \
        /p:UseAppHost=false /p:ErrorOnDuplicatePublishOutputFiles=false

# ---------- plugins: build & publish every plugin in Minigames.slnx ----------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_TAG} AS plugins
WORKDIR /src

# Bring in the just-packed SDK feed so plugins can resolve Quizanchos.Core.
COPY --from=sdk-pack /feed /feed

# Use a NuGet.config that points at the in-image feed only (overrides the
# repo's NuGet.config which references a relative ../Quizanchos/nupkgs path
# that isn't valid inside this stage).
RUN cat <<'EOF' > /src/NuGet.Docker.config
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org"        value="https://api.nuget.org/v3/index.json" />
    <add key="quizanchos-local" value="/feed" />
  </packageSources>
</configuration>
EOF

# csproj-only copy for restore caching.
COPY Minigames/Minigames.slnx Minigames/
COPY Minigames/Quizanchos.Plugin.Caravan/Quizanchos.Plugin.Caravan.csproj                                   Minigames/Quizanchos.Plugin.Caravan/
COPY Minigames/Quizanchos.Plugin.CaravanMultiplayer/Quizanchos.Plugin.CaravanMultiplayer.csproj             Minigames/Quizanchos.Plugin.CaravanMultiplayer/
COPY Minigames/Quizanchos.Plugin.CountryGuesser/Quizanchos.Plugin.CountryGuesser.csproj                     Minigames/Quizanchos.Plugin.CountryGuesser/
COPY Minigames/Quizanchos.Plugin.CountryGuesserMultiplayer/Quizanchos.Plugin.CountryGuesserMultiplayer.csproj  Minigames/Quizanchos.Plugin.CountryGuesserMultiplayer/
COPY Minigames/Quizanchos.Plugin.Game2048/Quizanchos.Plugin.Game2048.csproj                                 Minigames/Quizanchos.Plugin.Game2048/
COPY Minigames/Quizanchos.Plugin.TicketToRideEurope/Quizanchos.Plugin.TicketToRideEurope.csproj             Minigames/Quizanchos.Plugin.TicketToRideEurope/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore Minigames/Minigames.slnx --configfile /src/NuGet.Docker.config

COPY Minigames/ Minigames/

# Publish each plugin into its own folder under /app/plugins.
# The folder name (Caravan, CountryGuesser, ...) is what the host's PluginLoader
# uses as the plugin id and exposes at /minigames/<gamekey>/.
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    set -eux; \
    for proj in Minigames/Quizanchos.Plugin.*/Quizanchos.Plugin.*.csproj; do \
        name="$(basename "$(dirname "$proj")" | sed 's/^Quizanchos\.Plugin\.//')"; \
        dotnet publish "$proj" -c Release -o "/app/plugins/$name" --no-restore \
            /p:UseAppHost=false /p:ErrorOnDuplicatePublishOutputFiles=false \
            /p:CopyLocalLockFileAssemblies=true; \
    done; \
    # Strip SDK assemblies the host already provides. PluginLoadContext defers
    # all Microsoft.* / System.* loads to the default context (see
    # IsHostSharedAssembly) so the duplicates would just be dead weight.
    find /app/plugins -maxdepth 2 -type f \( \
        -name 'Quizanchos.Core.dll' -o \
        -name 'Quizanchos.Core.pdb' -o \
        -name 'Quizanchos.Common.dll' -o \
        -name 'Quizanchos.Common.pdb' -o \
        -name 'Microsoft.Extensions.DependencyInjection.Abstractions.dll' -o \
        -name 'Microsoft.Extensions.Logging.Abstractions.dll' \
    \) -delete

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_RUNTIME_TAG} AS runtime
WORKDIR /app

# The aspnet image ships a non-root `app` user (UID 1000).
# libgssapi-krb5-2: silences Npgsql's Kerberos probe warning at startup.
# wget: lets us hit /health from a HEALTHCHECK without curl.
# Pre-create writable dirs with app ownership so docker volumes inherit perms.
RUN apt-get update \
 && apt-get install -y --no-install-recommends libgssapi-krb5-2 wget \
 && rm -rf /var/lib/apt/lists/* \
 && mkdir -p /app/logs /app/plugins /home/app/.aspnet/DataProtection-Keys \
 && chown -R app:app /app /home/app/.aspnet

COPY --from=host-publish --chown=app:app /app/publish ./
COPY --from=plugins      --chown=app:app /app/plugins ./plugins/

USER app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    DOTNET_RUNNING_IN_CONTAINER=true \
    Plugins__Root=/app/plugins \
    TZ=UTC

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
    CMD wget -qO- http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Quizanchos.WebApi.dll"]
