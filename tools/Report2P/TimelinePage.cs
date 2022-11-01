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
        Log.Info($"\nCreating 2P Report HTML page for: {folderOf2pFolders}");

        TimelineItem[] items2p = GetTimelineItems2P(folderOf2pFolders);
        Log.Debug($"2P timeline items: {items2p.Length}");

        TimelineItem[] itemsABF = GetTimelineItemsAbf(folderOf2pFolders);
        Log.Debug($"ABF timeline items: {items2p.Length}");
        TimelineItem[] items = items2p.Concat(itemsABF).ToArray();

        if (items.Length == 0)
        {
            Log.Warn($"no timeline items found in: {folderOf2pFolders}");
        }
        else
        {
            MakeIndexPage(folderOf2pFolders, items);
        }
    }

    private static TimelineItem[] GetTimelineItemsAbf(string folderOf2pFolders)
    {
        string abfFolder = Path.GetFullPath(Path.Combine(folderOf2pFolders, "../abfs"));
        if (!Directory.Exists(abfFolder))
            return Array.Empty<TimelineItem>();

        List<TimelineItem> timelineItems = new();

        foreach (string abfPath in Directory.GetFiles(abfFolder, "*.abf").Where(x => x.EndsWith(".abf")))
        {
            Log.Debug($"Analyzing ABF folder: {abfPath}");
            AbfSharp.ABFFIO.ABF abf = new(abfPath, preloadSweepData: false);
            DateTime abfDateTime = GetAbfDateTime(abf);

            TimelineItem abfItem = new()
            {
                Title = $"{Path.GetFileName(abfPath)} ({Path.GetFileNameWithoutExtension(abf.Header.sProtocolPath)})",
                Content = $"<div><code>{abfPath}</code></div>" + $"<div><code>{abf}</code></div>",
                DateTime = abfDateTime,
                Icon = TimelineIcon.Ephys,
            };

            string autoAnalysisPath = Path.Combine(Path.GetDirectoryName(abfPath)!, "_autoanalysis");
            if (Directory.Exists(autoAnalysisPath))
            {
                string[] paths = Directory.GetFiles(autoAnalysisPath, Path.GetFileNameWithoutExtension(abfPath) + "*.png");
                if (paths.Length == 0)
                    continue;

                string[] urls = paths
                    .Select(x => x.Replace("X:", "http://192.168.1.9/X"))
                    .Select(x => x.Replace("\\", "/"))
                    .ToArray();

                ResultsFiles images = new()
                {
                    Title = "ABF Analyses",
                    Paths = urls,
                };

                abfItem.ImageGroups = new ResultsFiles[] { images };
            }

            timelineItems.Add(abfItem);
        }

        return timelineItems.ToArray();
    }

    private static TimelineItem? GetTimelineItem(string folder)
    {
        IExperiment? experiment = ExperimentFactory.GetExperiment(folder);

        if (experiment is null)
            return null;

        return new TimelineItem()
        {
            Title = Path.GetFileName(experiment.Path),
            Content = experiment.Details,
            DateTime = experiment.DateTime,
            Icon = GetExperimentIcon(experiment),
            ImageGroups = experiment.GetResultFiles(),
        };
    }

    private static TimelineItem[] GetTimelineItems2P(string folderOf2pFolders)
    {
        return Directory.GetDirectories(folderOf2pFolders)
            .Select(x => GetTimelineItem(x))
            .Where(x => x is not null)
            .Cast<TimelineItem>()
            .ToArray();
    }

    private static TimelineIcon GetExperimentIcon(IExperiment experiment)
    {
        return experiment switch
        {
            Linescan => TimelineIcon.Linescan,
            MarkPoints => TimelineIcon.MarkPoints,
            SingleImage => TimelineIcon.SingleImage,
            TImageSeries => TimelineIcon.TSeries,
            ZSeries => TimelineIcon.ZSeries,
            _ => TimelineIcon.Line,
        };
    }

    private static void MakeIndexPage(string folderOf2pFolders, TimelineItem[] timelineItems)
    {
        TimelineItem[] sortedTimelineItems = timelineItems.OrderBy(x => x.DateTime).ToArray();
        if (sortedTimelineItems.Length == 0)
            throw new InvalidOperationException("no timeline items found");

        string templateFolder = Templates.TemplatePaths.GetTemplateFolder();
        ReportBuilder report = new(templateFolder, folderOf2pFolders);

        string[] experimentFilePaths =
        {
            Path.Combine(folderOf2pFolders, "experiment.txt"),
            Path.Combine(Path.GetFullPath(folderOf2pFolders+"/../"), "experiment.txt"),
            Path.Combine(Path.GetFullPath(folderOf2pFolders+"/../../"), "experiment.txt"),
        };

        if (experimentFilePaths.Length == 0)
            Log.Debug($"experiment notes file does not exist");

        foreach (string path in experimentFilePaths)
        {
            if (File.Exists(path))
            {
                Log.Debug($"adding experiment notes: {path}");
                report.AddExperimentNotes(path);
            }
        }

        report.DivStart("my-5");
        DateTime lastItemTime = sortedTimelineItems.First().DateTime;
        DateTime experimentStartTime = sortedTimelineItems.First().DateTime;

        foreach (TimelineItem item in sortedTimelineItems)
        {
            Log.Debug($"generating HTML for timeline item: {item.Title}");
            if (item.DateTime - lastItemTime > TimeSpan.FromMinutes(10))
            {
                report.Add(new TimelineItem() { Icon = TimelineIcon.Break });
                lastItemTime = item.DateTime;
                experimentStartTime = item.DateTime;
            }

            item.ExperimentTime = item.DateTime - experimentStartTime;
            report.Add(item);
        }
        report.DivEnd();

        string outputFilePath = Path.Combine(folderOf2pFolders, "index.html");
        report.Save(outputFilePath);
        Log.Debug($"Saved: {outputFilePath}");
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
