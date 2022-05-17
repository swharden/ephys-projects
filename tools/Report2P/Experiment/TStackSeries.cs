namespace Report2P.Experiment;

internal class TStackSeries : IExperiment
{
    public string Path { get; private set; }

    public string Details => Scan.GetSummary();
    public DateTime DateTime => Scan.PVState.DateTime;

    public string AutoanalysisFolder => System.IO.Path.Combine(Path, "autoanalysis");

    private string ReferencesFolder => System.IO.Path.Combine(Path, "References");

    private readonly PvXml.ScanTypes.TZSeries Scan;

    public TStackSeries(string folder)
    {
        Path = System.IO.Path.GetFullPath(folder);
        Scan = new PvXml.ScanTypes.TZSeries(folder);
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

        ResultsFiles referenceImages = new()
        {
            Title = "Reference Images",
            Paths = Directory.GetFiles(AutoanalysisFolder, "ref_*.png")
                .Select(x => System.IO.Path.GetFileName(Path) + "/autoanalysis/" + System.IO.Path.GetFileName(x))
                .ToArray(),
        };

        return new ResultsFiles[]
        {
            datFiles,
            plotImages,
            referenceImages,
        };
    }

    public void Analyze(bool clear = false)
    {
        if (clear && Directory.Exists(AutoanalysisFolder))
            Directory.Delete(AutoanalysisFolder, recursive: true);

        if (!Directory.Exists(AutoanalysisFolder))
            Directory.CreateDirectory(AutoanalysisFolder);

        CreateReferenceImages();
        CreateAnalysisImages();
    }

    private void CreateAnalysisImages(bool overwrite = false)
    {
        string[] tifPaths = Directory.GetFiles(Path, "*.ome.tif").ToArray();
        string[] cycles = tifPaths.Select(x => System.IO.Path.GetFileName(x).Split("_Ch")[0]).Distinct().ToArray();

        List<double[]> intensities1 = new();
        List<double[]> intensities2 = new();
        foreach (string cycle in cycles)
        {
            Log.Debug($"Analyzing cycle: {cycle}");
            string[] stackFiles = tifPaths.Where(x => x.Contains(cycle)).ToArray();
            string[] stackFiles1 = stackFiles.Where(x => x.Contains("_Ch1_")).ToArray();
            string[] stackFiles2 = stackFiles.Where(x => x.Contains("_Ch2_")).ToArray();

            if (stackFiles1.Any())
                intensities1.Add(ImageAnalysis.GetMeanIntensityByImage(stackFiles1));

            if (stackFiles2.Any())
                intensities2.Add(ImageAnalysis.GetMeanIntensityByImage(stackFiles2));
        }

        if (intensities1.Any())
        {
            Log.Debug($"Creating Ch1 plot...");
            string saveAs1 = System.IO.Path.Combine(AutoanalysisFolder, "intensity_depth_over_time_ch1");
            ImageAnalysis.PlotTZSeries(intensities1, 1, Scan, saveAs1 + ".png");
            OriginDatFile.SaveYs(intensities1, saveAs1 + ".dat");
        }

        if (intensities2.Any())
        {
            Log.Debug($"Creating Ch2 plot...");
            string saveAs2 = System.IO.Path.Combine(AutoanalysisFolder, "intensity_depth_over_time_ch2");
            ImageAnalysis.PlotTZSeries(intensities2, 2, Scan, saveAs2 + ".png");
            OriginDatFile.SaveYs(intensities2, saveAs2 + ".dat");
        }
    }

    private void ConvertTif(string tifPath, string prefix, bool overwrite = false)
    {
        Log.Debug($"Converting TIF to PNG: {System.IO.Path.GetFileName(tifPath)}");

        string outputFileName = prefix + System.IO.Path.GetFileName(tifPath) + ".png";
        string outputFilePath = System.IO.Path.Combine(AutoanalysisFolder, outputFileName);

        if (overwrite == false && File.Exists(outputFilePath))
            return;

        if (new FileInfo(tifPath).Length > 5_000_000)
        {
            Log.Warn($"Skipping TIF that is >5 MB: {System.IO.Path.GetFileName(tifPath)}");
            return;
        }

        SciTIF.TifFile tif = new(tifPath);
        tif.SavePng(outputFilePath, autoScale: true);
    }

    private void CreateReferenceImages()
    {
        string[] windowTifs = Directory
            .GetFiles(ReferencesFolder, "*.tif")
            .Where(x => x.Contains("Window") || x.Contains("Reference"))
            .ToArray();

        foreach (string tifPath in windowTifs)
        {
            ConvertTif(tifPath, "ref_");
        }
    }
}
