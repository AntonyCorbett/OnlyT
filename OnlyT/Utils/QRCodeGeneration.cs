namespace OnlyT.Utils
{
    using System.Collections.Concurrent;
    using System.Windows.Media.Imaging;
    using QRCoder;

    internal static class QRCodeGeneration
    {
        private static readonly ConcurrentDictionary<string, BitmapImage> Cache = 
            new ConcurrentDictionary<string, BitmapImage>();
        
        public static BitmapImage CreateQRCode(string url)
        {
            if (!Cache.TryGetValue(url, out var result))
            {
                using (var generator = new QRCodeGenerator())
                using (var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q))
                using (var code = new QRCode(data))
                using (var bmp = code.GetGraphic(20))
                {
                    result = BitmapConverter.Convert(bmp);
                    Cache.TryAdd(url, result);
                }
            }

            return result;
        }
    }
}
