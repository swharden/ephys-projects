using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace SciTIF
{
    class Program
    {
        static void Main(string[] args)
        {
            string folderPath = @"X:\Data\C57\GRABNE\2021-09-23-ne-washon\TSeries-09232021-1216-1850-ne-washon";
            string[] tifPaths = Directory.GetFiles(folderPath, "TSeries-09232021-1216-1850_Cycle00001_Ch2*.tif");
            Project(tifPaths);
        }

        public static void Project(string[] tifPaths)
        {
            List<Image> images = new();
            Console.WriteLine($"Projecting {tifPaths.Length} tifs...");
            foreach (string tifPath in tifPaths)
            {
                Console.WriteLine($"  loading: {Path.GetFileName(tifPath)}");
                images.Add(new Image(tifPath));
            }

            Image proj = ProjectMax(images.ToArray());
            proj.AutoScale();
            string saveFilePath = @"C:\Users\swharden\Documents\temp\test.bmp";
            Console.WriteLine($"  saving: {saveFilePath}");
            proj.SaveBmp(saveFilePath);
        }

        public static Image ProjectMax(Image[] images)
        {
            Console.WriteLine($"Projecting {images.Length} images...");

            int width = images[0].Width;
            int height = images[0].Height;

            bool uniformWidths = images.Select(x => x.Width).Distinct().Count() == 1;
            bool uniformHeights = images.Select(x => x.Height).Distinct().Count() == 1;
            if (!uniformWidths || !uniformHeights)
                throw new InvalidOperationException("images must all have the same dimensions");

            double[,] projectionValues = new double[height, width];
            foreach (Image image in images)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (image.Values[y, x] > projectionValues[y, x])
                            projectionValues[y, x] = image.Values[y, x];
                    }
                }
            }

            return new Image(projectionValues);
        }

        public static void TifToPng(string tifFilePath, bool autoScale = true)
        {
            var img = new Image("../../data/tifs/05.tif");
            Console.WriteLine(img);

            string saveFilePath = System.IO.Path.GetFullPath("test.bmp");
            if (autoScale)
                img.AutoScale();
            img.SaveBmp(saveFilePath);
            Console.WriteLine(saveFilePath);
        }
    }
}