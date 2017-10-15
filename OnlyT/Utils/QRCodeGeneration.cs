using System;
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
        public static BitmapImage CreateQRCode(string url)
        {
            using (var generator = new QRCodeGenerator())
            using (var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q))
            using (var code = new QRCode(data))
            using (var bmp = code.GetGraphic(20))
            {
                return BitmapConverter.Convert(bmp);
            }
        }
    }
}
