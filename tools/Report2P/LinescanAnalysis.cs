using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P
{
    internal static class LinescanAnalysis
    {
        public static void PlotCurves(string folderPath, PvXml.ScanTypes.LineScan? scan = null)
        {
            if (scan is null)
                scan = new PvXml.ScanTypes.LineScan(folderPath);

            string outputFolder = Path.Combine(folderPath, "References");

            string[] tifPathsRed = PvXml.ScanTypes.LineScan.GetScanTifs(folderPath, 1);
            string[] tifPathsGreen = PvXml.ScanTypes.LineScan.GetScanTifs(folderPath, 2);
            string[] tifPaths = tifPathsRed.Concat(tifPathsGreen).ToArray();

            foreach (string tifPath in tifPaths)
            {
            }
        }

        private static double[] CollapseHorizontally(double[,] values)
        {
            double[] collapsed = new double[values.GetLength(0)];
            for (int y = 0; y < values.GetLength(0); y++)
            {
                double xSum = 0;
                for (int x = 0; x < values.GetLength(1); x++)
                {
                    xSum += values[y, x];
                }
                double xMean = xSum / values.GetLength(1);
                collapsed[y] = xMean;
            }
            return collapsed;
        }

        private static (double min, double max) GetPercentiles(double[] values, double minPercentile, double maxPercentile)
        {
            values = values.OrderBy(x => x).ToArray();
            double minFrac = minPercentile / 100;
            double maxFrac = maxPercentile / 100;
            int minIndex = (int)(values.Length * minFrac);
            int maxIndex = (int)(values.Length * maxFrac);
            return (values[minIndex], values[maxIndex]);
        }

        public static void MakeOriginDAT(double[] xs, double[] red, double[] green, string filename)
        {
            double fps = 1.0 / xs[1];

            StringBuilder sb = new();
            sb.AppendLine("Time\tRed\tGreen");
            sb.AppendLine("sec\tAFU\tAFU");
            sb.AppendLine($"{fps:N3} FPS\timage mean\timage mean");
            for (int i = 0; i < xs.Length; i++)
            {
                sb.AppendLine($"{xs[i]}\t{red[i]}\t{green[i]}");
            }

            File.WriteAllText(filename, sb.ToString());
            Console.WriteLine($"Wrote: {filename}");
        }
    }
}
