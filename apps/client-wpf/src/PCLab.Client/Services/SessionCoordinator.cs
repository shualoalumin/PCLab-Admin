using PCLab.Client.Models;

namespace PCLab.Client.Services;

public sealed class SessionCoordinator : IDisposable
{
    private readonly ApiClient _apiClient;
    private readonly DeviceIdentity _deviceIdentity;
    private readonly LocalStateStore _localStateStore;
    private readonly IdleTracker _idleTracker;
    private ActiveSessionInfo? _activeSession;

    public SessionCoordinator(
        ApiClient apiClient,
        DeviceIdentityService deviceIdentityService,
        LocalStateStore localStateStore,
        IdleTracker idleTracker)
    {
        _apiClient = apiClient;
        _deviceIdentity = deviceIdentityService.GetCurrentDevice();
        _localStateStore = localStateStore;
        _idleTracker = idleTracker;
        _idleTracker.IdleMinutesChanged += OnIdleMinutesChanged;
    }

    public event EventHandler<SessionStateChangedEventArgs>? SessionStateChanged;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        LocalSessionState? localState = await _localStateStore.LoadAsync(cancellationToken);

        try
        {
            CurrentSessionResponse remoteState = await _apiClient.GetCurrentAsync(
                _deviceIdentity,
                cancellationToken);

            if (remoteState.ActiveSession is not null)
            {
                int recoveredIdleMinutes = localState?.IdleMinutes ?? 0;
                await _apiClient.LogoutAsync(
                    remoteState.ActiveSession.SessionId,
                    recoveredIdleMinutes,
                    "app_recovery",
                    cancellationToken);
            }

            await _localStateStore.ClearAsync();
            PublishLocked("Ready for student check-in.");
        }
        catch (Exception ex)
        {
            await _localStateStore.ClearAsync();
            PublishLocked($"Locked mode active. Recovery check failed: {ex.Message}");
        }
    }

    public async Task CheckInAsync(string studentId, CancellationToken cancellationToken = default)
    {
        CheckInResponse response = await _apiClient.CheckInAsync(
            studentId,
            _deviceIdentity,
            cancellationToken);

        _activeSession = new ActiveSessionInfo
        {
            SessionId = response.SessionId,
            StudentId = response.Student.StudentId,
            StudentName = response.Student.Name,
            StartAt = response.StartAt,
            IdleMinutes = 0,
        };

        await PersistLocalStateAsync(cancellationToken);
        _idleTracker.Start(0);
        PublishActive("Session active.");
    }

    public async Task LogoutAsync(
        string endedReason,
        CancellationToken cancellationToken = default)
    {
        if (_activeSession is null)
        {
            PublishLocked("Ready for student check-in.");
            return;
        }

        await _apiClient.LogoutAsync(
            _activeSession.SessionId,
            _activeSession.IdleMinutes,
            endedReason,
            cancellationToken);

        _idleTracker.Stop();
        _activeSession = null;
        await _localStateStore.ClearAsync();

        PublishLocked("Ready for student check-in.");
    }

    public void Dispose()
    {
        _idleTracker.IdleMinutesChanged -= OnIdleMinutesChanged;
        _idleTracker.Dispose();
        _apiClient.Dispose();
    }

    private async void OnIdleMinutesChanged(object? sender, int idleMinutes)
    {
        if (_activeSession is null)
        {
            return;
        }

        _activeSession = new ActiveSessionInfo
        {
            SessionId = _activeSession.SessionId,
            StudentId = _activeSession.StudentId,
            StudentName = _activeSession.StudentName,
            StartAt = _activeSession.StartAt,
            IdleMinutes = idleMinutes,
        };

        await PersistLocalStateAsync();
        PublishActive("Session active.");
    }

    private async Task PersistLocalStateAsync(CancellationToken cancellationToken = default)
    {
        if (_activeSession is null)
        {
            return;
        }

        await _localStateStore.SaveAsync(
            new LocalSessionState
            {
                SessionId = _activeSession.SessionId,
                StudentId = _activeSession.StudentId,
                StudentName = _activeSession.StudentName,
                StartAt = _activeSession.StartAt,
                IdleMinutes = _activeSession.IdleMinutes,
            },
            cancellationToken);
    }

    private void PublishLocked(string statusMessage)
    {
        SessionStateChanged?.Invoke(
            this,
            new SessionStateChangedEventArgs(null, statusMessage));
    }

    private void PublishActive(string statusMessage)
    {
        SessionStateChanged?.Invoke(
            this,
            new SessionStateChangedEventArgs(_activeSession, statusMessage));
    }
}

public sealed class SessionStateChangedEventArgs : EventArgs
{
    public SessionStateChangedEventArgs(ActiveSessionInfo? activeSession, string statusMessage)
    {
        ActiveSession = activeSession;
        StatusMessage = statusMessage;
    }

    public ActiveSessionInfo? ActiveSession { get; }

    public string StatusMessage { get; }
}
