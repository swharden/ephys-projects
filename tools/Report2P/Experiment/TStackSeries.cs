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
