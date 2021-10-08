using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace AutoABF
{
    public class FolderWatcher
    {
        private const string ANALYSIS_FOLDER = "_autoanalysis";
        public readonly string WatchFile;
        public readonly List<string> Folders = new List<string>();

        public FolderWatcher(string watchFile)
        {
            if (!File.Exists(watchFile))
                throw new ArgumentException($"file does not exist: {watchFile}");

            WatchFile = Path.GetFullPath(watchFile);
            UpdateFolderList();
        }

        public void UpdateFolderList()
        {
            string[] lines = File.ReadAllLines(WatchFile);
            foreach (string line in lines)
            {
                string folderPath = Path.GetFullPath(line);
                if (!Folders.Contains(folderPath))
                    Folders.Add(folderPath);
            }
        }

        public void CreateTifs()
        {
            foreach (string folder in Folders)
            {
                CreateTifs(folder);
            }
        }

        public void CreateTifs(string folder)
        {
            foreach (string tifPath in Directory.GetFiles(folder, "*.tif"))
            {
                CreateTif(tifPath);
            }
        }

        private string CreateAnalysisFolder(string baseFolder)
        {
            string analysisFolder = Path.Combine(baseFolder, ANALYSIS_FOLDER);
            if (!Directory.Exists(analysisFolder))
                Directory.CreateDirectory(analysisFolder);
            return analysisFolder;
        }

        public void CreateTif(string tifFilePath)
        {
            string analysisFolder = CreateAnalysisFolder(Path.GetDirectoryName(tifFilePath));
            string outFile = Path.Combine(analysisFolder, Path.GetFileName(tifFilePath) + "_SciTIF.png");
            if (!File.Exists(outFile))
            {
                Console.WriteLine(outFile);
                SciTIF.Convert.FileToPng(tifFilePath, outFile);
            }
        }
    }
}