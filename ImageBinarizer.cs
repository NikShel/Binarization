using System;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AdaptiveTheresholding
{
    class ImageBinarizer
    {
        private bool[][] thresholdedImage;

        private const int RADIUS = 10;
        private const int OFFSET = 0;
        private const double COEFFICIENT = 0.2; //0.1

        private IntegralImage[] IntegralImages = new IntegralImage[3];
        private byte[][][] RGBChannelsImages = new byte[3][][];

        public ImageBinarizer(string filename)
        {
            var bmp = (Bitmap)Bitmap.FromFile(filename);
            RGBChannelsImages = GetRGBImages(bmp);
            for (int i = 0; i < 3; i++)
                IntegralImages[i] = new IntegralImage(RGBChannelsImages[i], RADIUS);
        }

        public void SaveTo(string filename)
        {
            Bitmap bmp = new Bitmap(thresholdedImage.Length, thresholdedImage[0].Length) ;
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                for (int y = 0; y < bmp.Height; y++)
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        byte color = (thresholdedImage[x][y] ? (byte)255 : (byte)0);
                        ptr[(x * 3) + y * stride] = color;
                        ptr[(x * 3) + y * stride + 1] = color;
                        ptr[(x * 3) + y * stride + 2] = color;
                    }
            }
            bmp.UnlockBits(data);
            bmp.Save(filename);
        }

        private T[][] GenerateArray<T>(int length1, int length2)
        {
            var array = new T[length1][];
            for (int i = 0; i < length1; i++)
                array[i] = new T[length2];
            return array;
        }

        private byte[][][] GetRGBImages(Bitmap bmp)
        {
            var rectangle = new Rectangle(0, 0, bmp.Width, bmp.Height);
            if (bmp.PixelFormat != PixelFormat.Format24bppRgb)
                bmp = bmp.Clone(rectangle, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(rectangle, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            byte[] bmpBytes = new byte[bmpData.Stride * bmpData.Height];
            Marshal.Copy(bmpData.Scan0, bmpBytes, 0, bmpBytes.Length);
            bmp.UnlockBits(bmpData);

            byte[][] redChannel = GenerateArray<byte>(bmp.Width, bmp.Height);
            byte[][] greenChannel = GenerateArray<byte>(bmp.Width, bmp.Height);
            byte[][] blueChannel = GenerateArray<byte>(bmp.Width, bmp.Height);

            for (int x = 0; x < bmp.Width; x++)
                for (int y = 0; y < bmp.Height; y++)
                {
                    byte R = bmpBytes[bmpData.Stride * y + 3 * x + 2];
                    byte G = bmpBytes[bmpData.Stride * y + 3 * x + 1];
                    byte B = bmpBytes[bmpData.Stride * y + 3 * x + 0];
                    redChannel[x][y] = R;
                    greenChannel[x][y] = G;
                    blueChannel[x][y] = B;
                }
            return new byte[3][][] {redChannel, greenChannel, blueChannel};
        }

        private byte GetThreshold(int x, int y, IntegralImage integralImage)
        {
            int count;
            long sum, sqrSum;
            integralImage.GetData(x, y, out count, out sum, out sqrSum);
            double mean = (double)sum / count;
            double tmp = sqrSum - (2 * mean * sum) + (mean * mean * count);
            double standartDeviation = Math.Sqrt(1.0 / (count * count) * tmp);
            double threshold = mean * (1 + COEFFICIENT * ((standartDeviation / 128) - 1));
            threshold = Math.Round(threshold);
            if (threshold > 255)
                //throw new Exception();
                return (byte)255;
            return (byte)threshold;
        }

        public void StartThresholding()
        {
            thresholdedImage = GenerateArray<bool>(RGBChannelsImages[0].Length, RGBChannelsImages[0][0].Length);
            int maxY = thresholdedImage[0].Length;
            int maxX = thresholdedImage.Length;

            Parallel.For(0, maxX, x =>
            {
                for (int y = 0; y < maxY; y++)
                {
                    bool isWhite = true;
                    for (int i = 0; i < 3; i++)
                    {
                        byte threshold = GetThreshold(x, y, IntegralImages[i]);
                        //threshold -= offsetConstant;
                        isWhite = isWhite && (RGBChannelsImages[i][x][y] >= threshold);
                    }
                    thresholdedImage[x][y] = isWhite;
                }
            });
            //for (int x = 0; x < maxX; x++)
            //{
            //    for (int y = 0; y < maxY; y++)
            //    {
            //        bool isWhite = true;
            //        for (int i = 0; i < 3; i++)
            //        {
            //            byte threshold = GetThreshold1(x, y, IntegralImages[i]);
            //            //threshold -= offsetConstant;
            //            isWhite = isWhite && (RGBChannelsImages[i][x][y] >= threshold);
            //        }
            //        thresholdedImage[x][y] = isWhite;
            //    }
            //}
        }
    }
}
