using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using PCLab.Client.Models;

namespace PCLab.Client.Services;

public sealed class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public ApiClient(string baseUrl, int httpTimeoutSeconds)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(EnsureTrailingSlash(baseUrl)),
            Timeout = TimeSpan.FromSeconds(httpTimeoutSeconds),
        };
    }

    public async Task<CheckInResponse> CheckInAsync(
        string studentId,
        DeviceIdentity deviceIdentity,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/client/check-in",
            new
            {
                studentId,
                hostname = deviceIdentity.Hostname,
                macAddress = deviceIdentity.MacAddress,
            },
            cancellationToken);

        return await DeserializeResponseAsync<CheckInResponse>(response, cancellationToken);
    }

    public async Task<CurrentSessionResponse> GetCurrentAsync(
        DeviceIdentity deviceIdentity,
        CancellationToken cancellationToken = default)
    {
        string requestUri =
            $"api/client/current?hostname={Uri.EscapeDataString(deviceIdentity.Hostname)}&macAddress={Uri.EscapeDataString(deviceIdentity.MacAddress)}";

        using HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new CurrentSessionResponse();
        }

        return await DeserializeResponseAsync<CurrentSessionResponse>(response, cancellationToken);
    }

    public async Task<LogoutResponse> LogoutAsync(
        string sessionId,
        int idleMinutes,
        string endedReason,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/client/logout",
            new
            {
                sessionId,
                idleMinutes,
                endedReason,
            },
            cancellationToken);

        return await DeserializeResponseAsync<LogoutResponse>(response, cancellationToken);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private async Task<T> DeserializeResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(ExtractErrorMessage(content));
        }

        T? result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
        if (result is null)
        {
            throw new InvalidOperationException("The server returned an empty response.");
        }

        return result;
    }

    private static string EnsureTrailingSlash(string baseUrl)
    {
        return baseUrl.EndsWith("/") ? baseUrl : $"{baseUrl}/";
    }

    private string ExtractErrorMessage(string content)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(content);
            if (document.RootElement.TryGetProperty("error", out JsonElement errorElement))
            {
                return errorElement.GetString() ?? "The server returned an error.";
            }
        }
        catch
        {
            // Fall back to raw content.
        }

        return string.IsNullOrWhiteSpace(content)
            ? "The server returned an error."
            : content;
    }
}
