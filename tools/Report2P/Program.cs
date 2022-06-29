namespace Report2P;

public static class Program
{
    public static void Main(string[] args)
    {
        if (System.Diagnostics.Debugger.IsAttached)
        {
            DeveloperMain();
            return;
        }
        else if (args.Length == 1)
        {
            Console.WriteLine($"Analyzing 2P Folders in: {args[0]}");
            AnalyzeAndIndex(args[0], overwrite: false);
        }
        else
        {
            Console.WriteLine("ERROR: Argument required (path of folder containing 2P folders)");
        }

        Console.WriteLine("press ENTER to exit...");
        _ = Console.ReadLine();
    }

    /// <summary>
    /// This method is called when the program starts on a developer computer (where debugger is attached)
    /// </summary>
    static void DeveloperMain()
    {
        //AnalyzeAndIndexEverySubfolder(@"X:\Data\C57\Sigma-1R\tagged-S1R", true);
        AnalyzeAndIndex(@"X:\Data\C57\practice\Nadine\2P\2P practice", true);
    }

    /// <summary>
    /// Analyze a folder where every subfolder contains multiple experiment folders
    /// </summary>
    private static void AnalyzeAndIndexEverySubfolder(string folder, bool overwrite)
    {
        foreach (string subFolder in Directory.GetDirectories(folder))
        {
            AnalyzeAndIndex(subFolder, overwrite);
        }
    }

    /// <summary>
    /// Given a folder containing many 2P experiment folders,
    /// analyze all 2P experiments and generate an index.html
    /// </summary>
    private static void AnalyzeAndIndex(string folder, bool overwrite)
    {
        Analysis.AnalyzeAllSubfolders(folder, overwrite);
        TimelinePage.MakeIndex(folder);
    }
}