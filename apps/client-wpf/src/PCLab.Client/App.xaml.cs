using System.Windows;
using PCLab.Client.Services;
using PCLab.Client.ViewModels;
using PCLab.Client.Views;

namespace PCLab.Client;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShellViewModel shellViewModel = new(new MockCheckInService());
        ShellWindow shellWindow = new()
        {
            DataContext = shellViewModel,
        };

        MainWindow = shellWindow;
        shellWindow.Show();
        shellWindow.ActivateOverlay();
    }
}
