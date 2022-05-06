namespace Report2P;

public static class Program
{
    public static void Main()
    {
        string folderPath = @"X:\Data\zProjects\Oxytocin Biosensor\experiments\bath apply OXT";
        //Analysis.AnalyzeAllSubfolders(folderPath);
        TimelinePage.MakeIndex(folderPath);
    }
}