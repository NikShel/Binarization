using System;
using System.Threading;
using System.Diagnostics;

namespace AdaptiveTheresholding
{
    class Program
    {
        private static void PrintTime(Stopwatch timer)
        {
            Console.WriteLine("Time: {0}", timer.Elapsed.ToString(@"hh\:mm\:ss"));
        }

        static void Main(string[] args)
        {
            long ms = 0;
            Stopwatch timer = new Stopwatch();
            timer.Start();

            Console.WriteLine("Reading");
            var image = new ImageBinarizer("..\\..\\..\\test4.jpg");
            PrintTime(timer);
            ms += timer.ElapsedMilliseconds;
            timer.Restart();

            Console.WriteLine("Binarization");
            image.StartThresholding();
            PrintTime(timer);
            ms += timer.ElapsedMilliseconds;
            timer.Restart();

            Console.WriteLine("Writing");
            image.SaveTo("..\\..\\..\\out.bmp");

            timer.Stop();
            ms += timer.ElapsedMilliseconds;
            PrintTime(timer);

            Console.WriteLine("Full time: " + ms);
            Thread.Sleep(3000);
            //Console.ReadKey();
        }
    }
}
