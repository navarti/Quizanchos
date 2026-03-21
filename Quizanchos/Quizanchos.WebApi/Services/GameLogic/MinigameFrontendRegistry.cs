using Quizanchos.Core;

namespace Quizanchos.WebApi.Services.GameLogic;

/// <summary>
/// Registry implementation for managing minigame frontend descriptors.
/// </summary>
public class MinigameFrontendRegistry : IMinigameFrontendRegistry
{
    private readonly Dictionary<string, IMinigameFrontendDescriptor> _descriptors = new();
    private readonly object _lockObject = new object();

    public void Register(IMinigameFrontendDescriptor descriptor)
    {
        if (descriptor == null)
            throw new ArgumentNullException(nameof(descriptor));

        if (string.IsNullOrWhiteSpace(descriptor.GameKey))
            throw new ArgumentException("GameKey cannot be empty or whitespace", nameof(descriptor));

        lock (_lockObject)
        {
            _descriptors[descriptor.GameKey] = descriptor;
        }
    }

    public IMinigameFrontendDescriptor? GetDescriptor(string gameKey)
    {
        if (string.IsNullOrWhiteSpace(gameKey))
            return null;

        lock (_lockObject)
        {
            _descriptors.TryGetValue(gameKey, out var descriptor);
            return descriptor;
        }
    }

    public IReadOnlyDictionary<string, IMinigameFrontendDescriptor> GetAllDescriptors()
    {
        lock (_lockObject)
        {
            return _descriptors.AsReadOnly();
        }
    }

    public bool IsRegistered(string gameKey)
    {
        if (string.IsNullOrWhiteSpace(gameKey))
            return false;

        lock (_lockObject)
        {
            return _descriptors.ContainsKey(gameKey);
        }
    }
}
