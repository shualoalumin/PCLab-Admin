namespace PCLab.Client.Models;

public sealed class ClientConfiguration
{
    public string ApiBaseUrl { get; init; } = "http://localhost:3000";

    public int IdleThresholdSeconds { get; init; } = 60;

    public int IdlePollSeconds { get; init; } = 5;

    public int HttpTimeoutSeconds { get; init; } = 10;
}
