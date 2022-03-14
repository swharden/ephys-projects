using Report2P.PvXml;

namespace Report2P;

public static class Program
{
    public static void Main()
    {
        string templateFolder = "../../../Templates";
        string folder2p = @"X:/Data/OT-Cre/OT-Tom-uncaging/2022-02-22-ap5/2p";
        Html.ReportBuilder page = new(templateFolder, folder2p);


        List<IScan> scans = new();

        foreach (string path in Directory.GetDirectories(folder2p))
        {
            IScan scan = PvXml.ScanFactory.FromPVFolder(path);

            if (scan is null)
            {
                Console.WriteLine($"skipping: {path}");
            }
            else
            {
                Console.WriteLine($"analyzing: {path}");
                scans.Add(scan);
            }
        }

        IScan[] orderedScans = scans.OrderBy(x => x.PVState.Started).ToArray();

        page.DivStart("my-5");

        for (int i = 0; i < scans.Count; i++)
        {
            if (i > 0)
            {
                TimeSpan delta = orderedScans[i].PVState.Started - orderedScans[i - 1].PVState.Started;
                if (delta > TimeSpan.FromMinutes(10))
                {
                    TimelineItems.Spacer spacer = new();
                    page.Add(spacer);
                }
            }

            IScan scan = orderedScans[i];

            TimelineItems.Abf item = new()
            {
                Title = Path.GetFileName(scan.PVState.FolderPath),
                Timestamp = scan.PVState.Started.ToString(),
                Content = "<pre>" + scan.GetSummary() + "</pre>",
            };
            page.Add(item);
        }
        page.DivEnd();

        page.Save("test.html");
    }
}