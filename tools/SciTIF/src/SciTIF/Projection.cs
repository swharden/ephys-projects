using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTIF
{
    public static class Projection
    {
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
    }
}
