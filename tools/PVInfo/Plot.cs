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
        public static void PlotIntensityMultiFolder(string folderPath)
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
    }
}
