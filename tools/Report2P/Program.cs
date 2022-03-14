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

        page.DivStart("my-5");
        foreach (var scan in scans.OrderBy(x => x.PVState.Started))
        {
            TimelineItem item = new()
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