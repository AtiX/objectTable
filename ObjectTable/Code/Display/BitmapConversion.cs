using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ObjectTable.Code.Display
{
    public static class BitmapConversion
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static Bitmap ToWinFormsBitmap(this BitmapSource bitmapsource)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(stream);

                using (var tempBitmap = new Bitmap(stream))
                {
                    // According to MSDN, one "must keep the stream open for the lifetime of the Bitmap."
                    // So we return a copy of the new bitmap, allowing us to dispose both the bitmap and the stream.
                    return new Bitmap(tempBitmap);
                }
            }
        }

        public static BitmapSource ToWpfBitmap(this Bitmap bitmap)
        {
            if (bitmap == null)
                return null;

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                stream.Close();
                stream.Dispose();
                return result;
            }
        }

        /// <summary>
        /// Second method with (hopefully) no memory leaks
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static BitmapSource ToWpfBitmapVar2(this Bitmap bmp)
        {
            BitmapSource source = null;
            IntPtr hBitmap = bmp.GetHbitmap();
            try 
            {
                source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
            return source;
        }
    }

}
