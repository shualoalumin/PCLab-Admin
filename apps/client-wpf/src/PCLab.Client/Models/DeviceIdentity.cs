namespace PCLab.Client.Models;

public sealed class DeviceIdentity
{
    public string Hostname { get; init; } = string.Empty;

    public string MacAddress { get; init; } = string.Empty;
}
