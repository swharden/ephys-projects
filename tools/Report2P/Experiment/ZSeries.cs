namespace Report2P.Experiment;

internal class ZSeries : IExperiment
{
    public string Path { get; private set; }

    public string Details => Scan.GetSummary();
    public DateTime DateTime => Scan.PVState.DateTime;

    public string AutoanalysisFolder => System.IO.Path.Combine(Path, "autoanalysis");

    private readonly PvXml.ScanTypes.ZSeries Scan;

    public ZSeries(string folder)
    {
        Path = System.IO.Path.GetFullPath(folder);
        Scan = new PvXml.ScanTypes.ZSeries(folder);
    }

    public ResultsFiles[] GetResultFiles()
    {
        ResultsFiles datFiles = new()
        {
            Title = "OriginLab Files",
            Paths = Directory.GetFiles(AutoanalysisFolder, "*.dat")
                    .ToArray(),
        };

        ResultsFiles plotImages = new()
        {
            Title = "Intensity Plots",
            Paths = Directory.GetFiles(AutoanalysisFolder, "intensity_*.png")
                .Select(x => System.IO.Path.GetFileName(Path) + "/autoanalysis/" + System.IO.Path.GetFileName(x))
                .ToArray(),
        };

        ResultsFiles maxProjections = new()
        {
            Title = "Maximum Projections",
            Paths = Directory.GetFiles(AutoanalysisFolder, "proj_*.png")
                .Select(x => System.IO.Path.GetFileName(Path) + "/autoanalysis/" + System.IO.Path.GetFileName(x))
                .ToArray(),
        };

        return new ResultsFiles[] {
            datFiles,
            plotImages,
            maxProjections,
        };
    }

    public void Analyze(bool clear = false)
    {
        bool overwrite = clear;

        if (clear && Directory.Exists(AutoanalysisFolder))
            Directory.Delete(AutoanalysisFolder, recursive: true);

        if (!Directory.Exists(AutoanalysisFolder))
            Directory.CreateDirectory(AutoanalysisFolder);

        CreateProjectionImages(overwrite);
        CreateAnalysisImages(overwrite);
        GetResultFiles();
    }

    private void CreateProjectionImages(bool overwrite)
    {
        string[] tifsCh1 = Directory.GetFiles(Path, "*.tif").Where(x => x.Contains("_Ch1_")).ToArray();
        string[] tifsCh2 = Directory.GetFiles(Path, "*.tif").Where(x => x.Contains("_Ch2_")).ToArray();

        if (tifsCh1.Any())
            ProjectMax(tifsCh1, "proj_red.png", overwrite);

        if (tifsCh2.Any())
            ProjectMax(tifsCh2, "proj_green.png", overwrite);
    }

    private void ProjectMax(string[] tifs, string filename, bool overwrite)
    {
        string outputFilePath = System.IO.Path.Combine(AutoanalysisFolder, filename);
        if (overwrite == false && File.Exists(outputFilePath))
        {
            Log.Debug($"Projection image already exists: {System.IO.Path.GetFileName(outputFilePath)}");
            return;
        }

        Log.Debug($"Projecting {tifs.Length} TIFs: {System.IO.Path.GetFileName(outputFilePath)}");
        
        Imaging.ProjectAutoscaleAndSave(tifs, outputFilePath);
    }

    private void CreateAnalysisImages(bool overwrite = false)
    {
        string[] tifPaths = Directory.GetFiles(Path, "*.ome.tif").ToArray();
        string[] tifPathsR = tifPaths.Where(x => x.Contains("_Ch1_")).ToArray();
        string[] tifPathsG = tifPaths.Where(x => x.Contains("_Ch2_")).ToArray();

        double[] redValues = PlotIntensityByFrame(tifPathsR, "intensity_red.png", overwrite, System.Drawing.Color.Red);
        double[] greenValues = PlotIntensityByFrame(tifPathsG, "intensity_green.png", overwrite, System.Drawing.Color.Green);

        string datFilePath = System.IO.Path.Combine(AutoanalysisFolder, "intensity.dat");
        if (File.Exists(datFilePath) && overwrite == false)
            return;

        double[] frameNumbers = Enumerable.Range(1, Scan.FrameCount).Select(x => (double)x).ToArray();
        OriginDatFile.SaveXRG(frameNumbers, redValues, greenValues, datFilePath, xUnit: "frame");
    }

    private double[] PlotIntensityByFrame(string[] tifPaths, string outputFilename, bool overwrite = false, System.Drawing.Color? color = null)
    {
        Log.Debug($"Creating full field intensity plot of {tifPaths.Length} TIFs: {outputFilename}");

        if (tifPaths.Length == 0)
            return Array.Empty<double>();

        string outputFilePath = System.IO.Path.Combine(AutoanalysisFolder, outputFilename);
        if (overwrite == false && File.Exists(outputFilePath))
            return Array.Empty<double>();

        double[] values = new double[tifPaths.Length];
        for (int i = 0; i < tifPaths.Length; i++)
        {
            SciTIF.TifFile tif = new(tifPaths[i]);
            SciTIF.Image img = tif.GetImage();
            values[i] = img.Values.Sum() / img.Values.Length;
        }

        double[] frameNumbers = Enumerable.Range(1, Scan.FrameCount).Select(x => (double)x).ToArray(); ;

        ScottPlot.Plot plt = new(600, 400);
        plt.AddScatter(frameNumbers, values, color);
        plt.SetAxisLimits(yMin: 0);
        plt.Title(System.IO.Path.GetFileName(Path));
        plt.YLabel("PMT Value (AFU)");
        plt.XLabel("Frame (#)");

        plt.SaveFig(outputFilePath);

        return values;
    }

    private static double GetMean(double[,] data)
    {
        double mean = 0;

        for (int y = 0; y < data.GetLength(0); y++)
            for (int x = 0; x < data.GetLength(1); x++)
                mean += data[y, x];

        return mean / data.GetLength(0) / data.GetLength(1);
    }
}
