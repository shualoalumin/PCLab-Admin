using System.Windows;

namespace PCLab.Client.Views;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();
    }

    public void ActivateOverlay()
    {
        Topmost = true;
        Activate();
        Focus();
    }

    private void Window_OnLoaded(object sender, RoutedEventArgs e)
    {
        ActivateOverlay();
    }

    private void Window_OnDeactivated(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(ActivateOverlay));
    }
}
