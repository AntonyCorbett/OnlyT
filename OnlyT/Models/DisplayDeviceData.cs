namespace OnlyT.Models
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    /// <summary>
    /// Used when reading monitor device info from system
    /// </summary>
    public class DisplayDeviceData
    {
        public string Name { get; init; } = null!;

        public string DeviceId { get; init; } = null!;

        public string DeviceString { get; init; } = null!;

        public string DeviceKey { get; init; } = null!;
    }
}
