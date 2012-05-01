using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectTable.Code.Kinect.Structures
{
    public class DepthImage
    {
        public DepthImage(int[] DepthData, int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            Data = ConvertDataToXYArray(DepthData, Width, Height);
        }

        public int[,] Data;

        public int Width;
        public int Height;

        public static int[,] ConvertDataToXYArray(int[] DepthData, int Width, int Height)
        {
            if (DepthData.Length != Width*Height)
                throw new Exception("DepthData Length does not match Width*height");

            int[,] result = new int[Width,Height];
            var index = 0;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x<Width; x++)
                {
                    result[x, y] = DepthData[index];
                    index++;
                }
            }

            return result;
        }

        public int[] RawData
        {
            get
            {
                //recreate the initial array
                int index = 0;
                int[] dataCopied = new int[Width * Height];

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        dataCopied[index] = Data[x, y];
                        index++;
                    }
                }
                return dataCopied;
            }
        }
        public DepthImage Clone()
        {
            //recreate the initial array
            int index = 0;
            int[] dataCopied = new int[Width*Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    dataCopied[index] = Data[x, y];
                    index++;
                }
            }

            return new DepthImage(dataCopied, Width, Height);
        }
    }
}
