using PCLab.Client.Models;
using PCLab.Client.Services;

namespace PCLab.Client.ViewModels;

public sealed class ShellViewModel : ObservableObject
{
    private readonly MockCheckInService _mockCheckInService;
    private string _studentIdInput = string.Empty;
    private string _studentNameInput = string.Empty;
    private string _errorMessage = string.Empty;
    private string _statusMessage = "Ready for local prototype check-in.";
    private string _currentStudentId = string.Empty;
    private string _currentStudentName = string.Empty;
    private string _loggedInAtText = "-";
    private bool _isLocked = true;
    private bool _isBusy;

    public ShellViewModel(MockCheckInService mockCheckInService)
    {
        _mockCheckInService = mockCheckInService;
        LoginCommand = new AsyncCommand(LoginAsync, () => IsLocked && !IsBusy);
        LogoutCommand = new AsyncCommand(LogoutAsync, () => !IsLocked && !IsBusy);
    }

    public string StudentIdInput
    {
        get => _studentIdInput;
        set => SetProperty(ref _studentIdInput, value);
    }

    public string StudentNameInput
    {
        get => _studentNameInput;
        set => SetProperty(ref _studentNameInput, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string CurrentStudentId
    {
        get => _currentStudentId;
        private set => SetProperty(ref _currentStudentId, value);
    }

    public string CurrentStudentName
    {
        get => _currentStudentName;
        private set => SetProperty(ref _currentStudentName, value);
    }

    public string LoggedInAtText
    {
        get => _loggedInAtText;
        private set => SetProperty(ref _loggedInAtText, value);
    }

    public bool IsLocked
    {
        get => _isLocked;
        private set
        {
            if (SetProperty(ref _isLocked, value))
            {
                OnPropertyChanged(nameof(IsUnlocked));
                LoginCommand.RaiseCanExecuteChanged();
                LogoutCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsUnlocked => !IsLocked;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                LoginCommand.RaiseCanExecuteChanged();
                LogoutCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public AsyncCommand LoginCommand { get; }

    public AsyncCommand LogoutCommand { get; }

    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        string studentId = StudentIdInput.Trim();
        string studentName = StudentNameInput.Trim();

        if (string.IsNullOrWhiteSpace(studentId))
        {
            ErrorMessage = "Student ID is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(studentName))
        {
            ErrorMessage = "Student name is required.";
            return;
        }

        try
        {
            IsBusy = true;
            MockSession session = await _mockCheckInService.LoginAsync(studentId, studentName);

            CurrentStudentId = session.StudentId;
            CurrentStudentName = session.StudentName;
            LoggedInAtText = session.LoggedInAt.ToString("g");
            StatusMessage = "Prototype unlocked state active.";
            StudentIdInput = string.Empty;
            StudentNameInput = string.Empty;
            IsLocked = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LogoutAsync()
    {
        try
        {
            IsBusy = true;
            await _mockCheckInService.LogoutAsync();
            CurrentStudentId = string.Empty;
            CurrentStudentName = string.Empty;
            LoggedInAtText = "-";
            StatusMessage = "Returned to lock/check-in state.";
            ErrorMessage = string.Empty;
            IsLocked = true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
