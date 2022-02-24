using System;
using System.Linq;

namespace PVInfo;

public static class ImageOp
{
    public static double[] CollapseHorizontally(double[,] values)
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

    public static (double min, double max) GetPercentiles(double[,] values, double minPercentile, double maxPercentile)
    {
        int Height = values.GetLength(0);
        int Width = values.GetLength(1);
        int i = 0;
        double[] values2 = new double[Width * Height];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                values2[i++] = values[y, x];
            }
        }

        return GetPercentiles(values2, minPercentile, maxPercentile);
    }

    public static (double min, double max) GetPercentiles(double[] values, double minPercentile, double maxPercentile)
    {
        values = values.OrderBy(x => x).ToArray();
        double minFrac = minPercentile / 100;
        double maxFrac = maxPercentile / 100;
        int minIndex = (int)(values.Length * minFrac);
        int maxIndex = (int)(values.Length * maxFrac);
        return (values[minIndex], values[maxIndex]);
    }
}
