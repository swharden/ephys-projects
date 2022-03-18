﻿namespace Report2P.Experiment;

internal class ZSeries : IExperiment
{
    public string Path { get; private set; }

    public string Details => Scan.GetSummary();
    public DateTime DateTime => Scan.PVState.DateTime;

    public ImageGroup ImageGroups { get; private set; } = new();

    public string AutoanalysisFolder => System.IO.Path.Combine(Path, "autoanalysis");

    private readonly PvXml.ScanTypes.ZSeries Scan;

    public ZSeries(string folder)
    {
        Path = System.IO.Path.GetFullPath(folder);
        Scan = new PvXml.ScanTypes.ZSeries(folder);
    }

    public void Analyze(bool clear = false)
    {
        if (clear && Directory.Exists(AutoanalysisFolder))
            Directory.Delete(AutoanalysisFolder, recursive: true);

        if (!Directory.Exists(AutoanalysisFolder))
            Directory.CreateDirectory(AutoanalysisFolder);

        CreateProjectionImages();
    }

    private void CreateProjectionImages()
    {
        string[] tifsCh1 = Directory.GetFiles(Path, "*.tif").Where(x => x.Contains("_Ch1_")).ToArray();
        string[] tifsCh2 = Directory.GetFiles(Path, "*.tif").Where(x => x.Contains("_Ch2_")).ToArray();

        if (tifsCh1.Any())
            ProjectMax(tifsCh1, "proj_red.png");

        if (tifsCh2.Any())
            ProjectMax(tifsCh2, "proj_green.png");
    }

    private void ProjectMax(string[] tifs, string filename, bool overwrite = false)
    {
        string outputFilePath = System.IO.Path.Combine(AutoanalysisFolder, filename);
        if (overwrite == false && File.Exists(outputFilePath))
            return;

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
