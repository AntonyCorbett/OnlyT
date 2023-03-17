using System.Collections.Concurrent;
using System.Windows.Media.Imaging;
using QRCoder;

namespace OnlyT.Utils;

internal static class QRCodeGeneration
{
    private static readonly ConcurrentDictionary<string, BitmapImage> Cache = new();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static BitmapImage CreateQRCode(string url)
    {
        if (!Cache.TryGetValue(url, out var result))
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var code = new QRCode(data);
            using var bmp = code.GetGraphic(20);

            result = BitmapConverter.Convert(bmp);
            Cache.TryAdd(url, result);
        }

        return result;
    }
}