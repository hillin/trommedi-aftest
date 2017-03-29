using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace AFTest
{
    class Program
    {
        struct FocusPoint
        {
            public double X { get; }
            public double Y { get; }

            public FocusPoint(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        private const int FocusIteration = 6;

        static void Main(string[] args)
        {
            var fileNames = Directory.GetFiles("pics", "*.jpg");
            Console.WriteLine($"{fileNames.Length} files detected.");

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var files = fileNames.Select(file => new { FileName = file, Image = (Bitmap)Image.FromFile(file) })
                                  .ToArray();

            stopwatch.Stop();
            Console.WriteLine($"{stopwatch.Elapsed} elapsed while reading files");

            var focusPoints = Program.GenerateFocusPoints();

            Console.WriteLine($"Using {FocusIteration} iterations, totally {focusPoints.Length} focus points for contrast detection");

            stopwatch.Reset();
            stopwatch.Start();

            var contrasts = files.Select(f => new
                                 {
                                     File = f.FileName,
                                     Measurement = Program.CalculateContrast(f.Image, focusPoints)
                                 })
                                 .OrderByDescending(c => c.Measurement);

            Console.WriteLine($"{"File",-20}{"Measurement",10}");
            Console.WriteLine(new string('-', 30));
            foreach (var contrast in contrasts)
            {
                Console.WriteLine($"{Path.GetFileName(contrast.File),-20}{contrast.Measurement,10}");
            }

            stopwatch.Stop();
            Console.WriteLine($"{stopwatch.Elapsed} elapsed detecting contrast");

            Console.ReadKey();
        }

        private static FocusPoint[] GenerateFocusPoints()
        {
            var focusInterval = 1.0 / Math.Pow(2, FocusIteration);
            var focusPoints = new List<FocusPoint>();

            for (var x = focusInterval; x < 1; x += focusInterval)
            {
                for (var y = focusInterval; y < 1; y += focusInterval)
                {
                    focusPoints.Add(new FocusPoint(x, y));
                }
            }

            return focusPoints.ToArray();
        }

        private static long CalculateContrast(Bitmap image, FocusPoint[] focusPoints)
        {
            var width = image.Width;
            var height = image.Height;

            return focusPoints.Sum(
                f => Program.CalculateContrast(image, (int)Math.Round(f.X * width), (int)Math.Round(f.Y * height)));
        }

        private static long CalculateContrast(Bitmap image, int x, int y)
        {
            var center = image.GetPixel(x, y).ToArgb();
            var measurement = 0L;
            for (var xOffset = -1; xOffset <= 1; ++xOffset)
            {
                for (var yOffset = -1; yOffset <= 1; ++yOffset)
                {
                    measurement += Math.Abs(center - image.GetPixel(x + xOffset, y + yOffset).ToArgb());
                }
            }

            return measurement;
        }
    }
}
