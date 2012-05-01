using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;

namespace ObjectTable.Code.Kinect
{
    public class DepthFrameVisualizer
    {
        public int global_min = 0;
        public int global_max = 0;

        private static int GetMinValue(int[] img)
        {
            int min = 0;
            for (int i = 1; i < img.Count(); i++)
            {
                if (min == 0)
                    min = img[i];
                if ((img[i] < min) && (img[i] != 0))
                    min = img[i];
            }
            return min;
        }
        private static int GetMaxValue(int[] img)
        {
            int max = img[0];
            for (int i = 1; i < img.Count(); i++)
            {
                if (img[i] > max)
                    max = img[i];
            }
            return max;
        }

        public void RecalculateMinMax(int[] deptharray)
        {
            global_max = GetMaxValue(deptharray);
            global_min = GetMinValue(deptharray);
            if (global_max == 0)
                global_max = 1;
        }

        public Bitmap ConvertToGrayscale(int[] deptharray, int Width, int Height,bool invert=false)
        {
            //The range between min and max has to fit into 255 bit (grayscale image)
            double scale_multiplicator = (float)255.0 / Convert.ToDouble(global_max - global_min);

            //Convert each pixel
            Bitmap grayscale = new Bitmap(Width, Height);

            int indexc = 0;

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    int value = deptharray[indexc];
                    int rgb_value = (int)Math.Round((value - global_min) * scale_multiplicator);

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
            return grayscale;
        }
    }
}
