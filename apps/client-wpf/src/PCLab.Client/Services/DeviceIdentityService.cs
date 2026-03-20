using System.Net.NetworkInformation;
using PCLab.Client.Models;

namespace PCLab.Client.Services;

public sealed class DeviceIdentityService
{
    public DeviceIdentity GetCurrentDevice()
    {
        string? macAddress = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(networkInterface =>
                networkInterface.OperationalStatus == OperationalStatus.Up &&
                networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                networkInterface.GetPhysicalAddress().GetAddressBytes().Length == 6)
            .Select(networkInterface => networkInterface.GetPhysicalAddress().ToString())
            .Where(address => !string.IsNullOrWhiteSpace(address))
            .Select(address => FormatMacAddress(address!))
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(macAddress))
        {
            throw new InvalidOperationException("Unable to determine a physical MAC address for this PC.");
        }

        return new DeviceIdentity
        {
            Hostname = Environment.MachineName.ToUpperInvariant(),
            MacAddress = macAddress.ToUpperInvariant(),
        };
    }

    private static string FormatMacAddress(string address)
    {
        return string.Join(
            "-",
            Enumerable.Range(0, address.Length / 2)
                .Select(index => address.Substring(index * 2, 2)));
    }
}
