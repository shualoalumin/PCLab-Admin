namespace PCLab.Client.Models;

public sealed class StudentRecord
{
    public string Id { get; init; } = string.Empty;

    public string StudentId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}

public sealed class DeviceRecord
{
    public string Id { get; init; } = string.Empty;

    public string Hostname { get; init; } = string.Empty;
}

public sealed class CheckInResponse
{
    public string SessionId { get; init; } = string.Empty;

    public StudentRecord Student { get; init; } = new();

    public DeviceRecord Device { get; init; } = new();

    public DateTimeOffset StartAt { get; init; }
}

public sealed class LogoutResponse
{
    public string SessionId { get; init; } = string.Empty;

    public DateTimeOffset EndAt { get; init; }
}

public sealed class CurrentSessionResponse
{
    public RemoteActiveSession? ActiveSession { get; init; }

    public string? Error { get; init; }
}

public sealed class RemoteActiveSession
{
    public string SessionId { get; init; } = string.Empty;

    public string StudentId { get; init; } = string.Empty;

    public string StudentName { get; init; } = string.Empty;

    public DateTimeOffset StartAt { get; init; }
}

public sealed class ActiveSessionInfo
{
    public string SessionId { get; init; } = string.Empty;

    public string StudentId { get; init; } = string.Empty;

    public string StudentName { get; init; } = string.Empty;

    public DateTimeOffset StartAt { get; init; }

    public int IdleMinutes { get; init; }
}

public sealed class LocalSessionState
{
    public string SessionId { get; init; } = string.Empty;

    public string StudentId { get; init; } = string.Empty;

    public string StudentName { get; init; } = string.Empty;

    public DateTimeOffset StartAt { get; init; }

    public int IdleMinutes { get; init; }
}
