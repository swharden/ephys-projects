using Report2P.PvXml;

namespace Report2P;

public static class Program
{
    public static void Main()
    {
        string folder = @"X:\Data\SD\2p technique development\glutamate uncaging\2022-practice\2022-02-15-MNI\2p\LineScan-02152022-1438-235";

        HtmlTemplates.Populate("../../../Templates");
        Autoanalysis.TwoPhoton.AnalyzeDataFolder(folder);

        /*
        string folder2p = @"X:/Data/OT-Cre/OT-Tom-uncaging/2022-02-22-ap5/2p";
        string[] subfolders = Directory.GetDirectories(folder2p);
        foreach (string subfolder in subfolders)
        {
            IScan? scan = PvXml.ScanFactory.FromPVFolder(subfolder);
            if (scan is null)
            {
                Console.WriteLine($"Skipping: {subfolder}");
                continue;
            }

            Console.WriteLine($"Creating page for: {subfolder}");
            break;
        }
        */
    }

    private static void MakeTimeline(string folder2p)
    {
        List<TimelineItem> unsortedItems = new();
        unsortedItems.AddRange(GetTimelineItemsAbf(folder2p));
        unsortedItems.AddRange(GetTimelineItems2P(folder2p));

        TimelineItem[] sortedItems = SortedWithSpacers(unsortedItems.ToArray());

        string templateFolder = "../../../Templates";
        Html.ReportBuilder page = new(templateFolder, folder2p);

        page.DivStart("my-5");
        foreach (TimelineItem item in sortedItems)
        {
            Console.WriteLine(item);
            page.Add(item);
        }
        page.DivEnd();

        page.Save("test.html");
    }

    private static TimelineItem[] SortedWithSpacers(TimelineItem[] itemsIn)
    {
        TimelineItem[] itemsInSorted = itemsIn.OrderBy(x => x.Timestamp).ToArray();

        List<TimelineItem> itemsOut = new();

        for (int i = 0; i < itemsInSorted.Length; i++)
        {
            TimelineItem thisItem = itemsInSorted[i];

            if (i > 0)
            {
                TimelineItem prevItem = itemsInSorted[i - 1];
                TimeSpan delta = thisItem.Timestamp - prevItem.Timestamp;
                if (delta > TimeSpan.FromMinutes(10))
                {
                    TimelineItems.Spacer spacer = new();
                    itemsOut.Add(spacer);
                }
            }

            itemsOut.Add(thisItem);
        }

        return itemsOut.ToArray();
    }

    private static TimelineItem[] GetTimelineItemsAbf(string folder2p)
    {
        List<TimelineItem> timelineItems = new();

        string abfFolder = Path.Combine(folder2p, "../../abfs");
        if (!Directory.Exists(abfFolder))
        {
            Console.WriteLine($"ABF folder not found: {abfFolder}");
            return Array.Empty<TimelineItem>();
        }

        string[] abfPaths = Directory.GetFiles(abfFolder)
            .Where(x => x.EndsWith(".abf"))
            .ToArray();

        foreach (string abfPath in abfPaths)
        {
            Console.WriteLine($"Analyzing: {Path.GetFileName(abfPath)}");
            AbfSharp.ABFFIO.ABF abf = new(abfPath, preloadSweepData: false);

            TimelineItems.Abf item = new()
            {
                Title = Path.GetFileName(abfPath),
                Timestamp = Abf.AbfTools.StartDateTime(abf),
                Content = abf.ToString(),
            };

            timelineItems.Add(item);
        }

        return timelineItems.ToArray();
    }

    private static TimelineItem[] GetTimelineItems2P(string folder2p)
    {
        List<TimelineItem> timelineItems = new();

        foreach (string path in Directory.GetDirectories(folder2p))
        {
            IScan? scan = ScanFactory.FromPVFolder(path);

            if (scan is null)
            {
                Console.WriteLine($"skipping: {path}");
                continue;
            }

            Console.WriteLine($"analyzing: {path}");

            TimelineItems.Abf item = new()
            {
                Title = Path.GetFileName(scan.PVState.FolderPath),
                Timestamp = scan.PVState.Started,
                Content = "<pre>" + scan.GetSummary() + "</pre>",
            };

            timelineItems.Add(item);
        }

        return timelineItems.ToArray();
    }
}