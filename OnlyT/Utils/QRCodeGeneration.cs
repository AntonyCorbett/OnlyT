using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using QRCoder;

namespace OnlyT.Utils
{
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
