namespace Report2P.Experiment;

internal class ZSeries : IExperiment
{
    public string Path { get; private set; }

    public string Details => Scan.GetSummary();
    public DateTime DateTime => Scan.PVState.DateTime;

    // TODO: make this a method that scans at call time
    public List<ResultsFiles> ImageGroups { get; private set; } = new();

    public string AutoanalysisFolder => System.IO.Path.Combine(Path, "autoanalysis");

    private readonly PvXml.ScanTypes.ZSeries Scan;

    public ZSeries(string folder)
    {
        Path = System.IO.Path.GetFullPath(folder);
        Scan = new PvXml.ScanTypes.ZSeries(folder);
    }

    public ResultsFiles[] GetResultFiles()
    {
        List<ResultsFiles> groups = new();

        groups.Add(
            new ResultsFiles()
            {
                Title = "Maximum Projections",
                Paths = Directory.GetFiles(AutoanalysisFolder, "proj_*.png")
                    .Select(x => System.IO.Path.GetFileName(Path) + "/autoanalysis/" + System.IO.Path.GetFileName(x))
                    .ToArray(),
            }
        );

        return groups.ToArray();
    }

    public void Analyze(bool clear = false)
    {
        bool overwrite = clear;

        if (clear && Directory.Exists(AutoanalysisFolder))
            Directory.Delete(AutoanalysisFolder, recursive: true);

        if (!Directory.Exists(AutoanalysisFolder))
            Directory.CreateDirectory(AutoanalysisFolder);

        CreateProjectionImages(overwrite);
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

        SciTIF.TifFile tifMax = new(tifs[0]);

        for (int i = 1; i < tifs.Length; i++)
        {
            SciTIF.TifFile tif = new(tifs[i]);
            for (int y = 0; y < tif.Height; y++)
            {
                for (int x = 0; x < tif.Width; x++)
                {
                    tifMax.Channels[0].Values[y, x] += tif.Channels[0].Values[y, x];
                }
            }
        }

        tifMax.SavePng(outputFilePath, autoScale: true);
    }
}
