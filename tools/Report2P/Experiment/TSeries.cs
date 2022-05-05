namespace Report2P.Experiment;

internal class TSeries : IExperiment
{
    public string Path { get; private set; }

    public string Details => Scan.GetSummary();
    public DateTime DateTime => Scan.PVState.DateTime;

    public List<ImageGroup> ImageGroups { get; private set; } = new();

    public string AutoanalysisFolder => System.IO.Path.Combine(Path, "autoanalysis");

    private string ReferencesFolder => System.IO.Path.Combine(Path, "References");

    private readonly PvXml.ScanTypes.TSeries Scan;

    public TSeries(string folder)
    {
        Path = System.IO.Path.GetFullPath(folder);
        Scan = new PvXml.ScanTypes.TSeries(folder);
    }

    public void Analyze(bool clear = false)
    {
        if (clear && Directory.Exists(AutoanalysisFolder))
            Directory.Delete(AutoanalysisFolder, recursive: true);

        if (!Directory.Exists(AutoanalysisFolder))
            Directory.CreateDirectory(AutoanalysisFolder);

        CreateReferenceImages();
        CreateAnalysisImages();

        ImageGroups.Add(
            new ImageGroup()
            {
                Title = "Analyses",
                Paths = Directory.GetFiles(AutoanalysisFolder, "intensity_*.png")
                    .Select(x => System.IO.Path.GetFileName(Path) + "/autoanalysis/" + System.IO.Path.GetFileName(x))
                    .ToArray(),
            }
        );

        ImageGroups.Add(
            new ImageGroup()
            {
                Title = "Reference Images",
                Paths = Directory.GetFiles(AutoanalysisFolder, "ref_*.png")
                    .Select(x => System.IO.Path.GetFileName(Path) + "/autoanalysis/" + System.IO.Path.GetFileName(x))
                    .ToArray(),
            }
        );
    }

    private void CreateAnalysisImages(bool overwrite = true)
    {
        string[] tifPaths = Directory.GetFiles(Path, "*.ome.tif").ToArray();
        string[] tifPathsR = tifPaths.Where(x => x.Contains("_Ch1_")).ToArray();
        string[] tifPathsG = tifPaths.Where(x => x.Contains("_Ch2_")).ToArray();

        double[] redValues = PlotIntensityOverTime(tifPathsR, "intensity_red.png", overwrite, System.Drawing.Color.Red);
        double[] greenValues = PlotIntensityOverTime(tifPathsG, "intensity_green.png", overwrite, System.Drawing.Color.Green);

        string datFilePath = System.IO.Path.Combine(AutoanalysisFolder, "intensity.dat");
        OriginDatFile.Write(Scan.FrameTimes, redValues, greenValues, datFilePath);
    }

    private double[] PlotIntensityOverTime(string[] tifPaths, string outputFilename, bool overwrite = false, System.Drawing.Color? color = null)
    {
        if (tifPaths.Length == 0)
            return Array.Empty<double>();

        string outputFilePath = System.IO.Path.Combine(AutoanalysisFolder, outputFilename);
        if (overwrite == false && File.Exists(outputFilePath))
            return Array.Empty<double>();

        double[] values = new double[tifPaths.Length];
        for (int i = 0; i < tifPaths.Length; i++)
        {
            SciTIF.TifFile tif = new(tifPaths[i]);
            values[i] = GetMean(tif.Channels[0].Values);
        }

        ScottPlot.Plot plt = new(600, 400);
        plt.AddScatter(Scan.FrameTimes, values, color);
        plt.SetAxisLimits(yMin: 0);
        plt.Title(System.IO.Path.GetFileName(Path));
        plt.YLabel("PMT Value (AFU)");
        plt.XLabel("Time (seconds)");

        plt.SaveFig(outputFilePath);

        return values;
    }

    private double GetMean(double[,] data)
    {
        double mean = 0;

        for (int y = 0; y < data.GetLength(0); y++)
            for (int x = 0; x < data.GetLength(1); x++)
                mean += data[y, x];

        return mean / data.GetLength(0) / data.GetLength(1);
    }

    private void ConvertTif(string tifPath, string prefix, bool overwrite = false)
    {
        string outputFileName = prefix + System.IO.Path.GetFileName(tifPath) + ".png";
        string outputFilePath = System.IO.Path.Combine(AutoanalysisFolder, outputFileName);

        if (overwrite == false && File.Exists(outputFilePath))
            return;

        SciTIF.TifFile tif = new(tifPath);
        tif.SavePng(outputFilePath, autoScale: true);
    }

    private void CreateReferenceImages()
    {
        string[] windowTifs = Directory.GetFiles(ReferencesFolder, "*Window*.tif").ToArray();

        foreach (string tifPath in windowTifs)
        {
            ConvertTif(tifPath, "ref_");
        }
    }
}
