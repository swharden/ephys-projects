using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVInfo
{
    internal static class Plot
    {
        public static void CreateMultiFolderLinescanCurves(string folderPath)
        {
            foreach (var pvFolderPath in Directory.GetDirectories(folderPath))
            {
                PVScan.IScan scan = PVScan.ScanFactory.FromPVFolder(pvFolderPath);
                if (scan is PVScan.LineScan lineScan)
                {
                    LinescanCurves(pvFolderPath, lineScan);
                }
            }
        }

        public static void LinescanCurves(string folderPath, PVScan.LineScan scan = null)
        {
            if (scan is null)
                scan = new PVScan.LineScan(folderPath);

            string outputFolder = Path.Combine(folderPath, "References");

            string[] tifPathsRed = PVScan.LineScan.GetScanTifs(folderPath, 1);
            string[] tifPathsGreen = PVScan.LineScan.GetScanTifs(folderPath, 2);
            string[] tifPaths = tifPathsRed.Concat(tifPathsGreen).ToArray();

            foreach (string tifPath in tifPaths)
            {
                Console.WriteLine(tifPath);
                var tif = new SciTIF.Image(tifPath);
                double[] data = ImageOp.CollapseHorizontally(tif.Values);

                var plt = new ScottPlot.Plot(400, 300);
                var sig = plt.AddSignal(data, 1.0 / scan.ScanLinePeriod);
                sig.Color = Path.GetFileName(tifPath).Contains("_Ch1_") ? Color.Red : Color.Green;

                var p = ImageOp.GetPercentiles(data, 0, 99);
                plt.SetAxisLimitsY(0, p.max * 1.1);

                string saveFileName = Path.GetFileName(tifPath) + "_curve.png";
                string saveFilePath = Path.Combine(outputFolder, saveFileName);
                plt.SaveFig(saveFilePath);
                Console.WriteLine(saveFilePath);
            }
        }

        public static void TSeriesIntensity(string folderPath)
        {
            foreach (var pvFolderPath in Directory.GetDirectories(folderPath))
            {
                PVScan.IScan scan = PVScan.ScanFactory.FromPVFolder(pvFolderPath);
                if (scan is PVScan.TSeries)
                {
                    PVScan.TSeries scan2 = (PVScan.TSeries)scan;
                    string linescanFolder = scan.PVState.FolderPath;
                    string[] filePathsRed = Directory.GetFiles(linescanFolder, $"*Ch1*.tif");
                    string[] filePathsGreen = Directory.GetFiles(linescanFolder, $"*Ch2*.tif");
                    if (filePathsRed.Length == 0)
                        continue;

                    PlotIntensity(filePathsRed, filePathsGreen, scan2.FrameTimes);
                }
            }
        }

        static void PlotIntensity(string[] filePathsRed, string[] filePathsGreen, double[] frameTimes)
        {
            string linescanFolder = Path.GetDirectoryName(filePathsRed[0]);

            double[] afuRed = new double[filePathsRed.Length];
            double[] afuGreen = new double[filePathsGreen.Length];

            for (int i = 0; i < filePathsRed.Length; i++)
            {
                Console.CursorLeft = 0;
                Console.WriteLine($"Analyzing {i + 1} of {filePathsRed.Length} ...");

                SciTIF.Image imgRed = new(filePathsRed[i]);
                afuRed[i] = imgRed.GetMean();

                SciTIF.Image imgGreen = new(filePathsGreen[i]);
                afuGreen[i] = imgGreen.GetMean();
            }

            var plt = new ScottPlot.Plot(600, 400);
            plt.AddScatter(frameTimes, afuRed, Color.Red);
            plt.AddScatter(frameTimes, afuGreen, Color.Green);
            plt.YLabel("Mean Image Intensity (AFU)");
            plt.XLabel("Time (seconds)");
            plt.Title(Path.GetFileName(linescanFolder));
            string plotFigurePath = Path.Combine(linescanFolder, $"References/afu.png");
            plt.SaveFig(plotFigurePath);
            Console.WriteLine(plotFigurePath);

            string dataFilePath = Path.Combine(linescanFolder, $"References/afu.dat");
            MakeOriginDAT(frameTimes, afuRed, afuGreen, dataFilePath);
        }

        static void MakeOriginDAT(double[] xs, double[] red, double[] green, string filename)
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

        static void LaunchPointScanGraph(string csvFilePath)
        {
            var data = PointScanAnalysis(csvFilePath);

            ScottPlot.Plot plt = new();
            plt.AddSignal(data.ch2);

            ScottPlot.FormsPlotViewer viewer = new(plt);
            viewer.ShowDialog();
        }

        static (double[] times, double[] ch1, double[] ch2) PointScanAnalysis(string csvFilePath)
        {
            Console.WriteLine($"Loading: {csvFilePath}");

            System.Diagnostics.Stopwatch sw = new();
            sw.Start();

            string[] lines = File.ReadAllLines(csvFilePath);

            int readingCount = lines.Length;
            double[] times = new double[readingCount];
            double[] ch1 = new double[readingCount];
            double[] ch2 = new double[readingCount];

            for (int i = 0; i < readingCount; i++)
            {
                string[] parts = lines[i].Split(",");
                if (parts.Length == 0)
                    continue;

                times[i] = double.Parse(parts[0]);
                ch1[i] = int.Parse(parts[1]);
                ch2[i] = int.Parse(parts[2]);
            }

            Console.WriteLine($"Read {readingCount} values in {sw.Elapsed.TotalMilliseconds:N3} ms");
            return (times, ch1, ch2);
        }
    }
}
