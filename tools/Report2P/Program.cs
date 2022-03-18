using Report2P.Experiment;
using Report2P.PvXml;

namespace Report2P;

public static class Program
{
    public static void Main()
    {
        MakeIndex(@"X:/Data/OT-Cre/OT-Tom-uncaging/2022-02-23-ap5/2p");
        MakeIndex(@"X:\Data\OT-Cre\OT-Tom-uncaging\2022-02-27-NMDA\2p\");
        MakeIndex(@"X:\Data\C57\FOS-TRAP\nodose-injection\gcamp\2P");
    }

    private static void MakeIndex(string folderOf2pFolders)
    {
        List<TimelineItem> timelineItems = new();
        timelineItems.AddRange(GetTimelineItems2P(folderOf2pFolders));
        timelineItems.AddRange(GetTimelineItemsAbf(folderOf2pFolders));

        MakeIndexPage(folderOf2pFolders, timelineItems.ToArray());
    }

    private static TimelineItem[] GetTimelineItemsAbf(string folderOf2pFolders)
    {
        string abfFolder = Path.GetFullPath(Path.Combine(folderOf2pFolders, "../abfs"));
        Console.WriteLine(abfFolder);
        if (!Directory.Exists(abfFolder))
            return Array.Empty<TimelineItem>();

        List<TimelineItem> timelineItems = new();

        foreach (string abfPath in Directory.GetFiles(abfFolder, "*.abf").Where(x => x.EndsWith(".abf")))
        {
            Console.WriteLine($"Analyzing: {abfPath}");
            AbfSharp.ABFFIO.ABF abf = new(abfPath, preloadSweepData: false);

            TimelineItem item = new()
            {
                Title = Path.GetFileName(abfPath),
                Content = $"<pre>{abf}</pre><br><br>abf auto-analysis images will go here",
                DateTime = Abf.AbfTools.StartDateTime(abf),
                Icon = "abf",
            };

            timelineItems.Add(item);
        }

        return timelineItems.ToArray();
    }

    private static TimelineItem[] GetTimelineItems2P(string folderOf2pFolders)
    {
        List<TimelineItem> timelineItems = new();

        foreach (string folder in Directory.GetDirectories(folderOf2pFolders))
        {
            try
            {
                IExperiment experiment = ExperimentFactory.GetExperiment(folder);
                Console.WriteLine($"Analyzing: {experiment.Path}");
                experiment.Analyze();

                TimelineItem item = new()
                {
                    Title = Path.GetFileName(experiment.Path),
                    Content = experiment.Details,
                    DateTime = experiment.DateTime,
                    Icon = "abf",
                    ImageGroups = experiment.ImageGroups.ToArray(),
                };

                timelineItems.Add(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        return timelineItems.ToArray();
    }

    private static void MakeIndexPage(string folderOf2pFolders, TimelineItem[] timelineItems)
    {
        TimelineItem[] sortedTimelineItems = timelineItems.OrderBy(x => x.DateTime).ToArray(); ;

        string templateFolder = "../../../Templates";
        Html.ReportBuilder page = new(templateFolder, folderOf2pFolders);

        page.DivStart("my-5");
        DateTime lastItemTime = sortedTimelineItems.First().DateTime;
        DateTime experimentStartTime = sortedTimelineItems.First().DateTime;

        foreach (TimelineItem item in sortedTimelineItems)
        {
            if (item.DateTime - lastItemTime > TimeSpan.FromMinutes(10))
            {
                page.Add(new TimelineItems.Spacer());
                lastItemTime = item.DateTime;
                experimentStartTime = item.DateTime;
            }

            item.ExperimentTime = item.DateTime - experimentStartTime;
            page.Add(item);
        }
        page.DivEnd();

        string outputFilePath = Path.Combine(folderOf2pFolders, "index.html");
        page.Save(outputFilePath);
        Console.WriteLine(outputFilePath);
    }
}