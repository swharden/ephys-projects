using Report2P.PvXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P.Experiment
{
    internal class Linescan : IExperiment
    {
        public string Path { get; private set; }

        public string Details { get; private set; }

        public string AutoanalysisFolder => System.IO.Path.Combine(Path, "autoanalysis");

        private string ReferencesFolder => System.IO.Path.Combine(Path, "References");

        public ImageGroup ImageGroups { get; private set; } = new();

        public Linescan(string folder)
        {
            folder = System.IO.Path.GetFullPath(folder);

            Path = folder;

            IScan scan = ScanFactory.FromPVFolder(folder)
                ?? throw new InvalidOperationException($"Invalid scan: {folder}");

            Details = scan.GetSummary();
        }

        public void Analyze(bool clear = false)
        {
            if (clear && Directory.Exists(AutoanalysisFolder))
                Directory.Delete(AutoanalysisFolder, recursive: true);

            if (!Directory.Exists(AutoanalysisFolder))
                Directory.CreateDirectory(AutoanalysisFolder);

            CreateReferenceImages();
            CreateDataImages();
        }

        private void ConvertTif(string tifPath, string prefix, bool overwrite = false)
        {
            string outputFileName = prefix + System.IO.Path.GetFileName(tifPath) + ".png";
            string outputFilePath = System.IO.Path.Combine(AutoanalysisFolder, outputFileName);

            if (overwrite == false && File.Exists(outputFilePath))
                return;

            Console.WriteLine($"  Converting: {outputFilePath}");
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
}
