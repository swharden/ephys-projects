namespace Report2P;

public static class ImageAnalysis
{
    public static double[] GetMeanIntensityByImage(string[] tifPaths)
    {
        Log.Debug($"Measuring intensity from {tifPaths.Length} TIFs...");

        if (tifPaths.Length == 0)
            return Array.Empty<double>();

        double[] values = new double[tifPaths.Length];
        for (int i = 0; i < tifPaths.Length; i++)
        {
            SciTIF.TifFile tif = new(tifPaths[i]);
            values[i] = GetImageMean(tif.Channels[0].Values);
        }

        return values;
    }

    private static double GetImageMean(double[,] data)
    {
        double mean = 0;

        for (int y = 0; y < data.GetLength(0); y++)
            for (int x = 0; x < data.GetLength(1); x++)
                mean += data[y, x];

        return mean / data.GetLength(0) / data.GetLength(1);
    }

    public static void PlotTZSeries(List<double[]> intensities, int channel, PvXml.ScanTypes.TZSeries scan, string saveAs)
    {
        ScottPlot.Drawing.Colormaps.Turbo cmap = new();

        ScottPlot.Plot plt = new();
        for (int i = 0; i < intensities.Count; i++)
        {
            double fraction = 1.0 - (double)i / (intensities.Count - 1);
            (byte r, byte g, byte b) = cmap.GetRGB((byte)(fraction * 255));
            var sig = plt.AddSignal(intensities[i]);
            sig.Color = System.Drawing.Color.FromArgb(r, g, b);
            if (i % 3 == 0)
                sig.Label = $"Cycle {i + 1}";
        }

        //plt.SetAxisLimits(yMin: 0);
        plt.YLabel("Mean Frame Intensity (AFU)");
        plt.XLabel("Z Depth (Frame #)");
        plt.Title($"Frame Intensity by Depth over Time (Ch{channel})");
        plt.Legend();

        plt.SaveFig(saveAs);
    }
}
