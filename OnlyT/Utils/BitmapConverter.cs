namespace OnlyT.Utils
{
    using System.Drawing;
    using System.IO;
    using System.Windows.Media.Imaging;

    internal static class BitmapConverter
    {
        public static BitmapImage Convert(Bitmap src)
        {
            var ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
