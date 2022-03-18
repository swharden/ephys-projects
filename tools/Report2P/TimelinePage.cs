using Report2P.Experiment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P;

internal class TimelinePage
{
    public static void MakeIndex(string folderOf2pFolders)
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
                DateTime = GetAbfDateTime(abf),
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
                    Icon = GetExperimentIcon(experiment),
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

    private static string GetExperimentIcon(IExperiment experiment)
    {
        return experiment switch
        {
            Linescan => "linescan",
            MarkPoints => "markpoints",
            SingleImage => "singleimage",
            TSeries => "tseries",
            ZSeries => "zseries",
            _ => "line",
        };
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

    private static DateTime GetAbfDateTime(AbfSharp.ABFFIO.ABF abf)
    {
        int datecode = (int)abf.Header.uFileStartDate;

        int day = datecode % 100;
        datecode /= 100;

        int month = datecode % 100;
        datecode /= 100;

        int year = datecode;

        try
        {
            if (year < 1980 || year >= 2080)
                throw new InvalidOperationException("unexpected creation date year in header");
            return new DateTime(year, month, day).AddMilliseconds(abf.Header.uFileStartTimeMS);
        }
        catch
        {
            return new DateTime(0);
        }
    }
}
