namespace Report2P;

public static class Program
{
    public static void Main()
    {
        // force reanalysis of a single 2P folder
        //Analysis.AnalyzeFolder(@"X:\Data\zProjects\Oxytocin Biosensor\experiments\bath apply OXT\TSeries-05102022-1208-1948", overwrite: true);

        string[] folderPaths =
        {
            @"X:\Data\zProjects\Oxytocin Biosensor\experiments\bath apply OXT",
            @"X:\Data\zProjects\Oxytocin Biosensor\experiments\electrical stimulation",
            @"X:\Data\zProjects\Oxytocin Biosensor\experiments\patch clamp stimulation",
        };

        foreach (string folderPath in folderPaths)
            AnalyzeFolder(folderPath, overwrite: true);
    }

    public static void AnalyzeFolder(string folderPath, bool overwrite)
    {
        Analysis.AnalyzeAllSubfolders(folderPath, overwrite);
        TimelinePage.MakeIndex(folderPath);
    }
}