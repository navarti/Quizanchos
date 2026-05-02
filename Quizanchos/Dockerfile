# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution + all referenced csproj files first for layer caching.
COPY Quizanchos.slnx ./
COPY Quizanchos.Common/Quizanchos.Common.csproj                         Quizanchos.Common/
COPY Quizanchos.Core/Quizanchos.Core.csproj                             Quizanchos.Core/
COPY Quizanchos.Domain/Quizanchos.Domain.csproj                         Quizanchos.Domain/
COPY Quizanchos.Migrations/Quizanchos.Migrations.csproj                 Quizanchos.Migrations/
COPY Quizanchos.WebApi/Quizanchos.WebApi.csproj                         Quizanchos.WebApi/
COPY Quizanchos.Game2048/Quizanchos.Game2048.csproj                     Quizanchos.Game2048/
COPY Minigames/Quiz/Quizanchos.Quiz/Quizanchos.Quiz.csproj              Minigames/Quiz/Quizanchos.Quiz/
COPY Minigames/Quiz/Quizanchos.Quiz.DbUpdater/Quizanchos.Quiz.DbUpdater.csproj  Minigames/Quiz/Quizanchos.Quiz.DbUpdater/
COPY Minigames/QuizMultiplayer/Quizanchos.QuizMultiplayer/Quizanchos.QuizMultiplayer.csproj  Minigames/QuizMultiplayer/Quizanchos.QuizMultiplayer/
COPY Minigames/TicketToRideEurope/Quizanchos.TicketToRideEurope/Quizanchos.TicketToRideEurope.csproj  Minigames/TicketToRideEurope/Quizanchos.TicketToRideEurope/
COPY Tests/Quizanchos.Quiz.Tests/Quizanchos.Quiz.Tests.csproj           Tests/Quizanchos.Quiz.Tests/

RUN dotnet restore Quizanchos.slnx

# Now copy the rest and publish.
COPY . .
RUN dotnet publish Quizanchos.WebApi/Quizanchos.WebApi.csproj \
    -c Release -o /app/publish --no-restore \
    /p:UseAppHost=false /p:ErrorOnDuplicatePublishOutputFiles=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
# The aspnet:10.0 image ships with a non-root `app` user (UID 1000) by default.
RUN mkdir -p /app/logs && chown -R app:app /app
COPY --from=build --chown=app:app /app/publish ./
USER app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    DOTNET_RUNNING_IN_CONTAINER=true \
    TZ=UTC

EXPOSE 8080
ENTRYPOINT ["dotnet", "Quizanchos.WebApi.dll"]
