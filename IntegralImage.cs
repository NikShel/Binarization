using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace AdaptiveTheresholding
{
    class IntegralImage  //Интегральное изображение позволяет быстро посчитать сумму значений пикселей в прямоугольнике
    {
        private long[][] sumImage;  //интегральные суммы
        private long[][] sqrImage;  //интегральные суммы квадратов
        private int radius;

        public IntegralImage(byte[][] image, int radius)
        {
            this.radius = radius;
            sumImage = GenerateLongArray(image.Length, image[0].Length);
            sqrImage = GenerateLongArray(image.Length, image[0].Length);

            //заполнение границ
            sumImage[0][0] = image[0][0];
            sqrImage[0][0] = Square(image[0][0]);
            for (int x = 1; x < sumImage.Length; x++)
            {
                sumImage[x][0] = image[x][0] + sumImage[x - 1][0];
                sqrImage[x][0] = Square(image[x][0]) + sqrImage[x - 1][0];
            }
            for (int y = 1; y < sumImage[0].Length; y++)
            {
                sumImage[0][y] = image[0][y] + sumImage[0][y - 1];
                sqrImage[0][y] = Square(image[0][y]) + sqrImage[0][y - 1];
            }

            //заполнение остального изображения
            for (int x = 1; x < sumImage.Length; x++)
                for (int y = 1; y < sumImage[0].Length; y++)
                {
                    sumImage[x][y] = image[x][y] - sumImage[x - 1][y - 1] + sumImage[x][y - 1]
                        + sumImage[x - 1][y];
                    sqrImage[x][y] = Square(image[x][y]) - sqrImage[x - 1][y - 1] + sqrImage[x][y - 1]
                        + sqrImage[x - 1][y];
                }
        }

        private int Square(byte value)
        {
            return value * value;
        }

        private long[][] GenerateLongArray(int length1, int length2)
        {
            long[][] array = new long[length1][];
            for (int i = 0; i < length1; i++)
                array[i] = new long[length2];
            return array;
        }

        public void GetData(int x, int y, out int count, out long sum, out long sumSqr)
        {
            int leftBorder = Math.Max(0, x - radius);
            int rightBorder = Math.Min(sqrImage.Length - 1, x + radius);
            int topBorder = Math.Max(0, y - radius);
            int bottomBorder = Math.Min(sqrImage[0].Length - 1, y + radius);

            count = (rightBorder - leftBorder) * (bottomBorder - topBorder);
            sum = GetRectangleSum(leftBorder, rightBorder, topBorder, bottomBorder, sumImage);
            sumSqr = GetRectangleSum(leftBorder, rightBorder, topBorder, bottomBorder, sqrImage);
        }

        private long GetRectangleSum(int left, int right, int top, int bottom, long[][] integralImage)  //Сумма пикселей в прямоугольнике
        {
            return integralImage[right][bottom] + integralImage[left][top]
                - integralImage[right][top] - integralImage[left][bottom];
        }
    }
}
