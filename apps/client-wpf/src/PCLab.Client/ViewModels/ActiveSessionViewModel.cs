using System.Windows.Threading;
using PCLab.Client.Models;
using PCLab.Client.Services;

namespace PCLab.Client.ViewModels;

public sealed class ActiveSessionViewModel : ObservableObject
{
    private readonly SessionCoordinator _sessionCoordinator;
    private readonly DispatcherTimer _clockTimer;
    private ActiveSessionInfo? _activeSession;
    private string _studentName = "No active session";
    private string _studentId = string.Empty;
    private string _startedAtText = "-";
    private string _elapsedText = "0m";
    private string _idleText = "0m idle";
    private bool _isBusy;

    public ActiveSessionViewModel(SessionCoordinator sessionCoordinator)
    {
        _sessionCoordinator = sessionCoordinator;
        LogoutCommand = new AsyncCommand(LogoutAsync, () => _activeSession is not null && !IsBusy);

        _clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        _clockTimer.Tick += (_, _) => RefreshElapsedText();
    }

    public string StudentName
    {
        get => _studentName;
        private set => SetProperty(ref _studentName, value);
    }

    public string StudentId
    {
        get => _studentId;
        private set => SetProperty(ref _studentId, value);
    }

    public string StartedAtText
    {
        get => _startedAtText;
        private set => SetProperty(ref _startedAtText, value);
    }

    public string ElapsedText
    {
        get => _elapsedText;
        private set => SetProperty(ref _elapsedText, value);
    }

    public string IdleText
    {
        get => _idleText;
        private set => SetProperty(ref _idleText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                LogoutCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public AsyncCommand LogoutCommand { get; }

    public void ApplySession(ActiveSessionInfo activeSession)
    {
        _activeSession = activeSession;
        StudentName = activeSession.StudentName;
        StudentId = activeSession.StudentId;
        StartedAtText = activeSession.StartAt.ToLocalTime().ToString("g");
        IdleText = $"{activeSession.IdleMinutes}m idle";
        RefreshElapsedText();
        _clockTimer.Start();
        LogoutCommand.RaiseCanExecuteChanged();
    }

    public void ClearSession()
    {
        _activeSession = null;
        StudentName = "No active session";
        StudentId = string.Empty;
        StartedAtText = "-";
        ElapsedText = "0m";
        IdleText = "0m idle";
        IsBusy = false;
        _clockTimer.Stop();
        LogoutCommand.RaiseCanExecuteChanged();
    }

    private async Task LogoutAsync()
    {
        if (_activeSession is null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            await _sessionCoordinator.LogoutAsync("student_logout");
        }
        catch (Exception ex)
        {
            IdleText = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RefreshElapsedText()
    {
        if (_activeSession is null)
        {
            ElapsedText = "0m";
            return;
        }

        TimeSpan elapsed = DateTimeOffset.UtcNow - _activeSession.StartAt.ToUniversalTime();
        int totalMinutes = Math.Max(0, (int)elapsed.TotalMinutes);
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        ElapsedText = hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
        IdleText = $"{_activeSession.IdleMinutes}m idle";
    }
}
