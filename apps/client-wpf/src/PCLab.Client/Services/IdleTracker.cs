using System.Runtime.InteropServices;
using System.Timers;

namespace PCLab.Client.Services;

public sealed class IdleTracker : IDisposable
{
    private readonly Timer _timer;
    private readonly int _idleThresholdSeconds;
    private readonly int _pollSeconds;
    private double _accumulatedIdleSeconds;
    private int _lastPublishedIdleMinutes;

    public IdleTracker(int idleThresholdSeconds, int pollSeconds)
    {
        _idleThresholdSeconds = idleThresholdSeconds;
        _pollSeconds = pollSeconds;

        _timer = new Timer(TimeSpan.FromSeconds(pollSeconds).TotalMilliseconds)
        {
            AutoReset = true,
        };
        _timer.Elapsed += OnTimerElapsed;
    }

    public event EventHandler<int>? IdleMinutesChanged;

    public int CurrentIdleMinutes => (int)Math.Floor(_accumulatedIdleSeconds / 60d);

    public void Start(int initialIdleMinutes)
    {
        _accumulatedIdleSeconds = initialIdleMinutes * 60d;
        _lastPublishedIdleMinutes = initialIdleMinutes;
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        _accumulatedIdleSeconds = 0;
        _lastPublishedIdleMinutes = 0;
    }

    public void Dispose()
    {
        _timer.Elapsed -= OnTimerElapsed;
        _timer.Dispose();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (GetIdleSeconds() < _idleThresholdSeconds)
        {
            return;
        }

        _accumulatedIdleSeconds += _pollSeconds;

        int idleMinutes = CurrentIdleMinutes;
        if (idleMinutes <= _lastPublishedIdleMinutes)
        {
            return;
        }

        _lastPublishedIdleMinutes = idleMinutes;
        IdleMinutesChanged?.Invoke(this, idleMinutes);
    }

    private static uint GetIdleSeconds()
    {
        LASTINPUTINFO lastInputInfo = new()
        {
            cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>(),
        };

        if (!GetLastInputInfo(ref lastInputInfo))
        {
            return 0;
        }

        uint tickCount = (uint)Environment.TickCount;
        return (tickCount - lastInputInfo.dwTime) / 1000;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }
}
