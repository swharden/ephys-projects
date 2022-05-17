namespace Report2P;

public static class Program
{
    public static void Main(string[] args)
    {
        if (System.Diagnostics.Debugger.IsAttached)
        {
            DevAnalyzeFolders();
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

    private static void DevAnalyzeFolders()
    {
        // force reanalysis of a single 2P folder
        /*
        Analysis.AnalyzeFolder(
            folder: @"X:\Data\zProjects\Oxytocin Biosensor\experiments\patch clamp stimulation\2p\ZSeries-05132022-1311-1552",
            overwrite: true);
        */

        string[] folderPaths =
        {
            //@"X:\Data\zProjects\Oxytocin Biosensor\experiments\bath apply OXT",
            //@"X:\Data\zProjects\Oxytocin Biosensor\experiments\electrical stimulation\2p",
            //@"X:\Data\zProjects\Oxytocin Biosensor\experiments\patch clamp stimulation\2p",
            //@"X:\Data\zProjects\Oxytocin Biosensor\experiments\ChR2 stimulation\2p",
            //@"X:\Data\zProjects\Oxytocin Biosensor\experiments\raise bath potassium",
            @"X:\Data\C57\Sigma-1R\tagged-S1R\2022-05-16-METH-20uM",
        };

        foreach (string folderPath in folderPaths)
        {
            Analysis.AnalyzeAllSubfolders(folderPath, overwrite: false);
            TimelinePage.MakeIndex(folderPath);
        }
    }
}