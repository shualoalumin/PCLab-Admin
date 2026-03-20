using PCLab.Client.Models;

namespace PCLab.Client.Services;

public sealed class MockCheckInService
{
    public async Task<MockSession> LoginAsync(string studentId, string studentName)
    {
        await Task.Delay(300);

        return new MockSession
        {
            StudentId = studentId.Trim(),
            StudentName = studentName.Trim(),
            LoggedInAt = DateTimeOffset.Now,
        };
    }

    public async Task LogoutAsync()
    {
        await Task.Delay(150);
    }
}
