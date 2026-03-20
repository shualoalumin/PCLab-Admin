namespace PCLab.Client.Models;

public sealed class MockSession
{
    public string StudentId { get; init; } = string.Empty;

    public string StudentName { get; init; } = string.Empty;

    public DateTimeOffset LoggedInAt { get; init; }
}
