using PCLab.Client.Services;

namespace PCLab.Client.ViewModels;

public sealed class ShellViewModel : ObservableObject
{
    private string _statusMessage = "Ready for student check-in.";

    public ShellViewModel(SessionCoordinator sessionCoordinator)
    {
        CheckIn = new CheckInViewModel(sessionCoordinator);
    }

    public CheckInViewModel CheckIn { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
}
