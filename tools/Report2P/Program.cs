namespace Report2P;

public static class Program
{
    public static void Main(string[] args)
    {
        if (System.Diagnostics.Debugger.IsAttached)
        {
            DeveloperAnalysis();
            return;
        }
        else if (args.Length == 1)
        {
            Console.WriteLine("Analyzing subfolders in: {}");
            Analysis.AnalyzeAllSubfolders(args[1], overwrite: false);
            TimelinePage.MakeIndex(args[1]);
        }
        else
        {
            Console.WriteLine("ERROR: Argument required (path of folder containing 2P folders)");
        }

        Console.WriteLine("press ENTER to exit...");
        _ = Console.ReadLine();
    }

    private static void DeveloperAnalysis()
    {
        // force reanalysis of a single 2P folder
        //Analysis.AnalyzeFolder(@"X:\Data\zProjects\Oxytocin Biosensor\experiments\bath apply OXT\TSeries-05102022-1208-1948", overwrite: true);

        string[] folderPaths =
        {
            //@"X:\Data\zProjects\Oxytocin Biosensor\experiments\bath apply OXT",
            //@"X:\Data\zProjects\Oxytocin Biosensor\experiments\electrical stimulation\2p",
            @"X:\Data\zProjects\Oxytocin Biosensor\experiments\patch clamp stimulation\2p",
            @"X:\Data\zProjects\Oxytocin Biosensor\experiments\ChR2 stimulation\2p",
        };

        foreach (string folderPath in folderPaths)
        {
            Analysis.AnalyzeAllSubfolders(folderPath, overwrite: false);
            TimelinePage.MakeIndex(folderPath);
        }
    }
}