using System.Windows;

namespace PCLab.Client.Views;

public partial class SessionBarWindow : Window
{
    public SessionBarWindow()
    {
        InitializeComponent();
    }

    public void ActivateBar()
    {
        PositionWindow();
        Topmost = true;
    }

    private void Window_OnLoaded(object sender, RoutedEventArgs e)
    {
        PositionWindow();
    }

    private void PositionWindow()
    {
        Rect workArea = SystemParameters.WorkArea;
        Left = Math.Max(workArea.Left + 16, workArea.Right - Width - 16);
        Top = workArea.Top + 16;
    }
}
