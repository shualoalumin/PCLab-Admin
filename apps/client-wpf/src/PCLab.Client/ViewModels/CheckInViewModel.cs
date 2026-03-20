using PCLab.Client.Services;

namespace PCLab.Client.ViewModels;

public sealed class CheckInViewModel : ObservableObject
{
    private readonly SessionCoordinator _sessionCoordinator;
    private string _studentId = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public CheckInViewModel(SessionCoordinator sessionCoordinator)
    {
        _sessionCoordinator = sessionCoordinator;
        CheckInCommand = new AsyncCommand(CheckInAsync, () => !IsBusy);
    }

    public string StudentId
    {
        get => _studentId;
        set => SetProperty(ref _studentId, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                CheckInCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public AsyncCommand CheckInCommand { get; }

    public void Reset()
    {
        StudentId = string.Empty;
        ErrorMessage = string.Empty;
        IsBusy = false;
    }

    private async Task CheckInAsync()
    {
        ErrorMessage = string.Empty;

        string trimmedStudentId = StudentId.Trim();
        if (string.IsNullOrWhiteSpace(trimmedStudentId))
        {
            ErrorMessage = "Student ID is required.";
            return;
        }

        try
        {
            IsBusy = true;
            await _sessionCoordinator.CheckInAsync(trimmedStudentId);
            StudentId = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
