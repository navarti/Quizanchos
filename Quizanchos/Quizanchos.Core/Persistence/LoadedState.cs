namespace Quizanchos.Core;

/// <summary>
/// Result of <see cref="IGameStatePersistence.LoadAsync"/>: the persisted state JSON
/// alongside the host-tracked session metadata (player IDs, finished/winner) that the
/// plugin needs to reconstitute its <see cref="IGameState"/> on load.
/// </summary>
public sealed record LoadedState(
    string StateJson,
    IReadOnlyList<string> PlayerIds,
    bool IsFinished,
    string? Winner);
