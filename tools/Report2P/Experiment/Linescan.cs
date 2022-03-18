﻿namespace Report2P.Experiment;

internal class Linescan : IExperiment
{
    public string Path { get; private set; }

    public string Details => Scan.GetSummary();
    public DateTime DateTime => Scan.PVState.DateTime;

    public List<ImageGroup> ImageGroups { get; private set; } = new();

    public string AutoanalysisFolder => System.IO.Path.Combine(Path, "autoanalysis");

    private string ReferencesFolder => System.IO.Path.Combine(Path, "References");

    private readonly PvXml.ScanTypes.LineScan Scan;

    public Linescan(string folder)
    {
        Path = System.IO.Path.GetFullPath(folder);
        Scan = new PvXml.ScanTypes.LineScan(folder);
    }

    public void Analyze(bool clear = false)
    {
        if (clear && Directory.Exists(AutoanalysisFolder))
            Directory.Delete(AutoanalysisFolder, recursive: true);

        if (!Directory.Exists(AutoanalysisFolder))
            Directory.CreateDirectory(AutoanalysisFolder);

        CreateReferenceImages();
        CreateDataImages();
        CreateAnalysisImages();

        ImageGroups.Add(
            new ImageGroup()
            {
                Title = "Reference Images",
                Paths = Directory.GetFiles(AutoanalysisFolder, "ref_*.png")
                    .Select(x => System.IO.Path.GetFileName(Path) + "/autoanalysis/" + System.IO.Path.GetFileName(x))
                    .ToArray(),
            }
        );

        ImageGroups.Add(
            new ImageGroup()
            {
                Title = "Linescan Images",
                Paths = Directory.GetFiles(AutoanalysisFolder, "data_*.png")
                    .Select(x => System.IO.Path.GetFileName(Path) + "/autoanalysis/" + System.IO.Path.GetFileName(x))
                    .ToArray(),
            }
        );

        ImageGroups.Add(
            new ImageGroup()
            {
                Title = "Linescan Analyses",
                Paths = Directory.GetFiles(AutoanalysisFolder, "linescan_*.png")
                    .Select(x => System.IO.Path.GetFileName(Path) + "/autoanalysis/" + System.IO.Path.GetFileName(x))
                    .ToArray(),
            }
        );
    }

    private void CreateAnalysisImages(bool overwrite = false)
    {
        string saveFilePath = System.IO.Path.Combine(AutoanalysisFolder, "linescan_curves.png");
        if ((overwrite == false) && (File.Exists(saveFilePath)))
            return;

        string[] tifPaths = Directory.GetFiles(Path, "*.ome.tif").ToArray();
        string[] tifPathsR = tifPaths.Where(x => x.Contains("_Ch1_")).ToArray();
        string[] tifPathsG = tifPaths.Where(x => x.Contains("_Ch2_")).ToArray();

        for (int i = 0; i < tifPathsR.Length; i++)
        {
            SciTIF.TifFile tifR = new(tifPathsR[i]);
            SciTIF.TifFile tifG = new(tifPathsG[i]);

            double[] dataR = CollapseHorizontally(tifR.Channels[0].Values);
            double[] dataG = CollapseHorizontally(tifG.Channels[0].Values);

            ScottPlot.Plot plt = new(600, 400);

            plt.AddSignal(dataR, 1.0 / Scan.ScanLinePeriod, System.Drawing.Color.Red);
            plt.AddSignal(dataG, 1.0 / Scan.ScanLinePeriod, System.Drawing.Color.Green);
            plt.SetAxisLimits(yMin: 0);
            plt.Title(System.IO.Path.GetFileName(Path));
            plt.YLabel("PMT Value (AFU)");
            plt.XLabel("Time (seconds)");

            plt.SaveFig(saveFilePath);
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

    private void CreateDataImages()
    {
        string[] tifPaths = Directory.GetFiles(Path, "*.ome.tif").ToArray();

        string[] ch1Paths = tifPaths.Where(x => x.Contains("_Ch1_")).ToArray();
        if (ch1Paths.Any())
            ConvertTif(ch1Paths.First(), "data_ch1_");

        string[] ch2Paths = tifPaths.Where(x => x.Contains("_Ch2_")).ToArray();
        if (ch2Paths.Any())
            ConvertTif(ch1Paths.First(), "data_ch2_");
    }
}