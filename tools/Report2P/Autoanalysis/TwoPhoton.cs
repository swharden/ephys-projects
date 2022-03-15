using Report2P.PvXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P.Autoanalysis
{
    public static class TwoPhoton
    {
        public const string FOLDER_NAME = "autoanalysis";

        public static void AnalyzeDataFolder(string dataFolder)
        {
            Console.WriteLine($"Analyzing: {dataFolder}");
            IScan? scan = ScanFactory.FromPVFolder(dataFolder);

            if (scan is null)
            {
                Console.WriteLine("  Not a 2P folder.");
                return;
            }

            Console.WriteLine($"  Type: {scan.GetType()}");

            CreateAnalysisFolder(dataFolder);
            CreateLinescanIndexPage(dataFolder);
        }

        private static void CreateAnalysisFolder(string dataFolder)
        {
            string analysisFolder = Path.Combine(dataFolder, FOLDER_NAME);
            if (!Directory.Exists(analysisFolder))
                Directory.CreateDirectory(analysisFolder);
        }

        private static void CreateLinescanIndexPage(string dataFolder)
        {
            string analysisFolder = Path.Combine(dataFolder, FOLDER_NAME);

            Pages.Page page = new();

            string folderName = Path.GetFileName(dataFolder);
            page.Title = folderName;
            page.Subtitle = dataFolder;

            page.Content.AppendLine("info");

            string indexFilePath = Path.Combine(analysisFolder, "index.html");
            page.Save(indexFilePath);
        }
    }
}
