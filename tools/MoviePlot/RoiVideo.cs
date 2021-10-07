using System;
using System.Drawing;
using System.Linq;
using System.IO;

namespace MoviePlot
{
    public class RoiVideo
    {
        public readonly double SamplePeriod;
        public readonly double[] Times;
        public readonly double[] AFU;
        public readonly double[] DFF;
        public readonly string[] TifFiles;

        public RoiVideo(string imageFolderInput, string csvFilePath, double samplePeriodSeconds, double baselineMinutes1, double baselineMinutes2)
        {
            AFU = ReadSingleRoiValues(csvFilePath);
            SamplePeriod = samplePeriodSeconds / 60;
            Times = Enumerable.Range(0, AFU.Length).Select(x => x * SamplePeriod).ToArray();
            DFF = CalculateDFF(AFU, Times, baselineMinutes1, baselineMinutes2);
            TifFiles = Directory.GetFiles(imageFolderInput, "*.png");
        }

        public void CreateFrameImages(string outputFolder)
        {
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            for (int i = 0; i < TifFiles.Length; i++)
            {
                using Bitmap frame = new Bitmap(854, 480);
                using Graphics gfx = Graphics.FromImage(frame);
                gfx.Clear(Color.White);

                using Bitmap micrographBmp = new Bitmap(TifFiles[i]);
                Rectangle micrographRect = new(10, 10, 460, 460);
                gfx.DrawImage(micrographBmp, micrographRect);

                Rectangle graphRect = new(470, 0, 390, 480);
                Bitmap graph = GetPlotBitmap(graphRect.Width, graphRect.Height, i);
                gfx.DrawImage(graph, graphRect);

                string outputImage = Path.Combine(outputFolder, Path.GetFileName(TifFiles[i]));
                frame.Save(outputImage, System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine(outputImage);
            }
        }

        private Bitmap GetPlotBitmap(int width, int height, int frame)
        {
            var plt = new ScottPlot.Plot(width, height);

            var hline = plt.AddHorizontalLine(0);
            hline.Color = Color.Black;
            hline.LineStyle = ScottPlot.LineStyle.Dash;

            var thinLine = plt.AddScatter(Times, DFF);
            thinLine.Color = Color.Blue;

            var highlightLine = plt.AddScatter(Times, DFF);
            highlightLine.LineWidth = 10;
            highlightLine.MarkerSize = 0;
            highlightLine.Color = Color.FromArgb(100, Color.Green);
            highlightLine.MaxRenderIndex = frame;

            // drug times highlighted manually here
            plt.AddHorizontalSpan(10, 15, Color.FromArgb(20, Color.Black));
            plt.AddHorizontalSpan(26, 31, Color.FromArgb(20, Color.Black));

            plt.YLabel("ΔF/F (%)");
            plt.XLabel("Time (minutes)");

            return plt.GetBitmap();
        }

        private static double[] ReadSingleRoiValues(string filePath) => File.ReadLines(filePath)
                .Where(x => x.Contains(","))
                .Where(x => !x.StartsWith(" "))
                .Select(x => x.Split(",")[1])
                .Select(x => double.Parse(x))
                .ToArray();

        private static double[] CalculateDFF(double[] afu, double[] times, double baselineTime1, double baselineTime2, bool percentage = true)
        {
            double baselineMean = Enumerable.Range(0, afu.Length)
                .Where(x => times[x] >= baselineTime1)
                .Where(x => times[x] <= baselineTime2)
                .Select(x => afu[x])
                .Average();

            double[] dff = afu.Select(x => x / baselineMean - 1).ToArray();

            if (percentage)
                dff = dff.Select(x => x * 100).ToArray();

            return dff;
        }
    }
}