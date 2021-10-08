using System;
using System.IO;

namespace AutoABF.Analysis
{
    public class Memtest : AnalysisBase, IAnalysis
    {
        public void Analyze(AbfSharp.ABF abf, string outputFolder)
        {
            var plt = new ScottPlot.Plot(800, 600);

            for (int i = 0; i < abf.Header.SweepCount; i++)
            {
                double[] ys = ToDouble(abf.GetSweep(i));
                plt.AddSignal(ys, abf.Header.SampleRate / 1e3);
            }
            plt.AxisAuto(0);
            plt.XLabel("Sweep Time (ms)");
            plt.YLabel("Current (pA)");
            plt.Title($"Membrane Test\n{abf.Header.AbfID}.abf");

            string outputFileName = $"{abf.Header.AbfID}_autoabf_autoanalysis.png";
            string outputFilePath = Path.Combine(outputFolder, outputFileName);
            Console.WriteLine(outputFilePath);
            plt.SaveFig(outputFilePath);
        }
    }
}