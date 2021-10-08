using System;

namespace AutoABF
{
    class Program
    {
        static void Main(string[] args)
        {
            var watcher = new FolderWatcher(@"X:\Lab Documents\network\autoAnalysisFolders2.txt");
            
            watcher.UpdateFolderList();
            Console.WriteLine($"Watching {watcher.Folders.Count} folders");
            watcher.CreateTifs();
        }
    }
}
