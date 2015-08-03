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

        private bool IsInImage(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < sumImage.Length && y < sumImage[0].Length);
        }

        public Tuple<double, int> GetMean(int x, int y)  //Среднее значение пикселя в radius-окрестности, количество пикселей
        {
            if (!IsInImage(x, y))
                throw new ArgumentOutOfRangeException();

            Rectangle rect;
            if (!IsInImage(x + radius, y + radius) || !IsInImage(x - radius, y - radius))
                rect = GetCorrectRectangle(x, y);
            else
                rect = new Rectangle() { X = x - radius, Y = y - radius, Height = 2 * radius - 1, Width = 2 * radius - 1};
            return new Tuple<double, int> (GetRectangleSum(rect, sumImage) / (rect.Width * rect.Height), (rect.Width * rect.Height));
        }

        public double GetSumOfSquares(int x, int y)  //Сумма квадратов пикселей из radius-окрестности 
        {
            if (!IsInImage(x, y))
                throw new ArgumentOutOfRangeException();

            Rectangle rect;
            if (!IsInImage(x + radius, y + radius) || !IsInImage(x - radius, y - radius))
                rect = GetCorrectRectangle(x, y);
            else
                rect = new Rectangle() { X = x - radius, Y = y - radius, Height = 2 * radius - 1, Width = 2 * radius - 1};
            return GetRectangleSum(rect, sqrImage);
        }

        private double GetRectangleSum(Rectangle rect, long[][] integralImage)  //Сумма пикселей в прямоугольнике rect 
        {
            return integralImage[rect.Right][rect.Bottom] + integralImage[rect.Left][rect.Top]
                - integralImage[rect.Right][rect.Top] - integralImage[rect.Left][rect.Bottom];
        }

        private Rectangle GetCorrectRectangle(int x, int y)
        {
            var mainRectangle = new Rectangle() { Width = sumImage.Length - 1, Height = sumImage[0].Length - 1};
            var currentRectangle = new Rectangle() {X = x - radius, Y = y - radius, Width = 2 * radius - 1, Height = 2 * radius - 1};
            currentRectangle.Intersect(mainRectangle);
            return currentRectangle;
        }
    }
}
