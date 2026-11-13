namespace Venther.App.Models;

public class Device
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DeviceStatus Status { get; set; } = DeviceStatus.Normal;
    public string LastReading { get; set; } = string.Empty;
}
