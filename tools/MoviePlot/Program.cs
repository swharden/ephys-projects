using System;
using System.Linq;
using System.IO;
using System.Drawing;

namespace MoviePlot
{
    class Program
    {
        static void Main(string[] args)
        {
            // read ROI values from ImageJ output file
            string csvFile = @"X:\Data\C57\GRABNE\2021-10-04-ne-washon\TSeries-10042021-1257-1853\Analysis\Results.csv";
            double samplePeriod = 75.4936441; // seconds
            double[] roiValues = ReadSingleRoiValues(csvFile);
            double[] roiTimes = Enumerable.Range(0, roiValues.Length)
                .Select(x => x * samplePeriod / 60)
                .ToArray();
            double[] dff = CalculateDFF(roiValues, roiTimes, 5, 8);

            // scottplot
            var plt = new ScottPlot.Plot();

            var hline = plt.AddHorizontalLine(0);
            hline.Color = Color.Black;
            hline.LineStyle = ScottPlot.LineStyle.Dash;

            var thinLine = plt.AddScatter(roiTimes, roiValues);
            thinLine.Color = Color.Blue;

            var highlightLine = plt.AddScatter(roiTimes, roiValues);
            highlightLine.LineWidth = 10;
            highlightLine.MarkerSize = 0;
            highlightLine.Color = Color.FromArgb(100, Color.Green);

            plt.AddHorizontalSpan(10, 15, Color.FromArgb(20, Color.Black));
            plt.AddHorizontalSpan(26, 31, Color.FromArgb(20, Color.Black));

            plt.YLabel("ΔF/F (%)");
            plt.XLabel("Time (minutes)");

            // load images
            string inputFolder = @"X:\Data\C57\GRABNE\2021-10-04-ne-washon\TSeries-10042021-1257-1853\Analysis\small-source-frames";
            string outputFolder = @"X:\Data\C57\GRABNE\2021-10-04-ne-washon\TSeries-10042021-1257-1853\Analysis\small-annotated";
            string[] inputImages = Directory.GetFiles(inputFolder, "*.png");
            for (int i = 0; i < inputImages.Length; i++)
            {
                string inputImage = inputImages[i];
                using Bitmap micrograph = new Bitmap(inputImage);

                using Bitmap frame = new Bitmap(854, 480);
                using Graphics gfx = Graphics.FromImage(frame);
                gfx.Clear(Color.White);

                RectangleF micrographRect = new(10, 10, 460, 460);
                gfx.DrawImage(micrograph, micrographRect);
                
                RectangleF graphRect = new(470, 0, 390, 480);
                plt.Resize(graphRect.Width, graphRect.Height);
                highlightLine.MaxRenderIndex = i;
                using Bitmap graph = plt.GetBitmap();
                gfx.DrawImage(graph, graphRect);

                string outputImage = Path.Combine(outputFolder, Path.GetFileName(inputImage));
                frame.Save(outputImage, System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine(outputImage);
            }
        }

        static double[] ReadSingleRoiValues(string filePath) => File.ReadLines(filePath)
                .Where(x => x.Contains(","))
                .Where(x => !x.StartsWith(" "))
                .Select(x => x.Split(",")[1])
                .Select(x => double.Parse(x))
                .ToArray();

        static double[] CalculateDFF(double[] afu, double[] times, double baselineTime1, double baselineTime2, bool percentage = true)
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
