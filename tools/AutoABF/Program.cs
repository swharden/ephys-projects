using System;

namespace AutoABF
{
    class Program
    {
        static void Main(string[] args)
        {
            string abfFilePath = @"X:/Data/SD/practice/Scott/2021-10-08-AON-VC-L368/2021_10_08_0007.abf";
            AbfAnalyzer.Analyze(abfFilePath);

            /*
            var watcher = new FolderWatcher(@"X:\Lab Documents\network\autoAnalysisFolders2.txt");
            watcher.UpdateFolderList();
            Console.WriteLine($"Watching {watcher.Folders.Count} folders");
            watcher.CreateTifs();
            */
        }
    }
}
