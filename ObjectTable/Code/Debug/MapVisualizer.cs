using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ObjectTable.Code.Kinect.Structures;

namespace ObjectTable.Code.Debug
{
    /// <summary>
    /// Visualizes maps by converting them to bitmaps
    /// </summary>
    public static class MapVisualizer
    {
        public static Bitmap VisualizeNeighbourMap(int[,,] map, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);

            //Get Max Values
            int max = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y=0; y<height; y++)
                {
                    if (map[x, y, 0] > max)
                        max = map[x, y, 0];
                }
            }

            //Generate scaling value
            double scale_multiplicator = (float)255.0 / Convert.ToDouble(max);

            //div by 0
            if (double.IsInfinity(scale_multiplicator))
                scale_multiplicator = 0;
            
            //Paint Bitmap
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int rgb_val = (int)Math.Round(scale_multiplicator*map[x, y, 0]);
                    Color col = Color.FromArgb(rgb_val, rgb_val, rgb_val);
                    bmp.SetPixel(x, y, col);
                }
            }

            //return
            return bmp;
        }

        public static Bitmap VisualizeBoolMap(bool[,] map, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (map[x, y])
                        bmp.SetPixel(x, y, Color.White);
                    else
                        bmp.SetPixel(x, y, Color.Black);
                }
            }

            return bmp;
        }

        public static Bitmap VisualizeDepthImage(DepthImage img, bool invert=false, bool printDepthValues = false)
        {
            //Calculate Min Value
            int[] RawData = img.RawData;

            int min = 0;
            for (int i = 1; i < RawData.Count(); i++)
            {
                if (min == 0)
                    min = RawData[i];
                if ((RawData[i] < min) && (RawData[i] != 0))
                    min = RawData[i];
            }

            //Calculate max
            int max = RawData[0];
            for (int i = 1; i < RawData.Count(); i++)
            {
                if (RawData[i] > max)
                    max = RawData[i];
            }

            if (max == 0)
                max = 1;

            //The range between min and max has to fit into 255 bit (grayscale image)
            double scale_multiplicator = (float)255.0 / Convert.ToDouble(max - min);

            //Convert each pixel
            Bitmap grayscale = new Bitmap(img.Width, img.Height);

            int indexc = 0;

            for (var y = 0; y < img.Height; y++)
            {
                for (var x = 0; x < img.Width; x++)
                {
                    int value = img.Data[x, y];
                    int rgb_value = (int)Math.Round((value - min) * scale_multiplicator);

                    if (rgb_value < 0)
                        rgb_value = 0;
                    if (rgb_value > 255)
                        rgb_value = 255;

                    if (invert)
                        rgb_value = 255 - rgb_value;

                    Color col = new Color();
                    col = Color.FromArgb(rgb_value, rgb_value, rgb_value);
                    grayscale.SetPixel(x, y, col);
                    indexc++;
                }
            }

            //Add information if desired
            if (printDepthValues)
            {
                /* Graphics g = Graphics.FromImage(grayscale);
                g.DrawString("min: " + min.ToString() + "mm max: " + max.ToString() + "mm", new Font("Tahoma", 10),
                             Brushes.Red, new PointF(0, 0));
                return new Bitmap(img.Width, img.Height, g);
                 * */
                //TODO: Bild wird schwarz, bug beheben
            }

            return grayscale;
        }

    }
}
