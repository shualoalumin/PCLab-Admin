using System.IO;
using System.Windows;
using System.Windows.Threading;
using PCLab.Client.Models;
using PCLab.Client.Services;
using PCLab.Client.ViewModels;
using PCLab.Client.Views;

namespace PCLab.Client;

public partial class App : Application
{
    private SessionCoordinator? _sessionCoordinator;
    private ShellWindow? _shellWindow;
    private SessionBarWindow? _sessionBarWindow;
    private ShellViewModel? _shellViewModel;
    private ActiveSessionViewModel? _activeSessionViewModel;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;

        try
        {
            ClientConfiguration configuration = AppConfigurationLoader.Load();
            ApiClient apiClient = new(configuration.ApiBaseUrl, configuration.HttpTimeoutSeconds);
            DeviceIdentityService deviceIdentityService = new();
            LocalStateStore localStateStore = new();
            IdleTracker idleTracker = new(
                configuration.IdleThresholdSeconds,
                configuration.IdlePollSeconds);

            _sessionCoordinator = new SessionCoordinator(
                apiClient,
                deviceIdentityService,
                localStateStore,
                idleTracker);
            _shellViewModel = new ShellViewModel(_sessionCoordinator);
            _activeSessionViewModel = new ActiveSessionViewModel(_sessionCoordinator);

            _shellWindow = new ShellWindow
            {
                DataContext = _shellViewModel,
            };

            _sessionBarWindow = new SessionBarWindow
            {
                DataContext = _activeSessionViewModel,
            };

            _sessionCoordinator.SessionStateChanged += OnSessionStateChanged;

            _shellWindow.Show();
            await _sessionCoordinator.InitializeAsync();
        }
        catch (Exception ex)
        {
            LogFatalError(ex);
            Shutdown(-1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        if (_sessionCoordinator is not null)
        {
            _sessionCoordinator.SessionStateChanged -= OnSessionStateChanged;
            _sessionCoordinator.Dispose();
        }
    }

    private void OnSessionStateChanged(object? sender, SessionStateChangedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (_shellWindow is null || _shellViewModel is null || _activeSessionViewModel is null)
            {
                return;
            }

            _shellViewModel.StatusMessage = e.StatusMessage;

            if (e.ActiveSession is null)
            {
                _shellViewModel.CheckIn.Reset();
                _activeSessionViewModel.ClearSession();
                _sessionBarWindow?.Hide();
                _shellWindow.Show();
                _shellWindow.ActivateOverlay();
                return;
            }

            _activeSessionViewModel.ApplySession(e.ActiveSession);
            _shellWindow.Hide();
            _sessionBarWindow?.Show();
            _sessionBarWindow?.ActivateBar();
        });
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogFatalError(e.Exception);
        e.Handled = false;
    }

    private static void LogFatalError(Exception exception)
    {
        try
        {
            string logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PCLab.Client");
            Directory.CreateDirectory(logDirectory);

            string logPath = Path.Combine(logDirectory, "client-errors.log");
            File.AppendAllText(
                logPath,
                $"{DateTimeOffset.UtcNow:u} {exception}\n");
        }
        catch
        {
            // Ignore secondary logging failures.
        }
    }
}
