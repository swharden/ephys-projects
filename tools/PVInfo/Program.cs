using System;
using System.IO;

namespace PVInfo
{
    class Program
    {
        static void Main()
        {
            GenerateReport(@"X:\Data\SD\2p technique development\glutamate uncaging\2022-practice\2022-02-17-MNI");
            //LaunchPointScanGraph(@"X:\Data\SD\2p technique development\problem\2022-02-18 Mira vs X3\PointScan-02182022-1204-008\PointScan-02182022-1204-008_Cycle00001.csv");
        }

        static void GenerateReport(string folderOfScans)
        {
            ReferenceTif.ConvertMultiFolder(folderOfScans);
            Video.CreateMultiFolderLinescanVideos(folderOfScans);
            Plot.PlotIntensityMultiFolder(folderOfScans);
            Report.CreateMultiFolderIndex(folderOfScans);
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