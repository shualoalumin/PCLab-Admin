using System.IO;
using System.Text.Json;
using PCLab.Client.Models;

namespace PCLab.Client.Services;

public sealed class LocalStateStore
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _stateFilePath;

    public LocalStateStore()
    {
        string stateDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PCLab.Client");
        Directory.CreateDirectory(stateDirectory);

        _stateFilePath = Path.Combine(stateDirectory, "session-state.json");
    }

    public async Task<LocalSessionState?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_stateFilePath))
        {
            return null;
        }

        await using FileStream stream = File.OpenRead(_stateFilePath);
        return await JsonSerializer.DeserializeAsync<LocalSessionState>(
            stream,
            _jsonOptions,
            cancellationToken);
    }

    public async Task SaveAsync(
        LocalSessionState state,
        CancellationToken cancellationToken = default)
    {
        await using FileStream stream = File.Create(_stateFilePath);
        await JsonSerializer.SerializeAsync(stream, state, _jsonOptions, cancellationToken);
    }

    public Task ClearAsync()
    {
        if (File.Exists(_stateFilePath))
        {
            File.Delete(_stateFilePath);
        }

        return Task.CompletedTask;
    }
}
