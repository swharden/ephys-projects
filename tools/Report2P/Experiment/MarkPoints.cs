namespace Report2P.Experiment;

internal class MarkPoints : IExperiment
{
    public string Path { get; private set; }

    public string Details => "uncaging point shape analysis not implemented";
    public DateTime DateTime => Scan.DateTime;

    public List<ResultsFiles> ImageGroups { get; private set; } = new();

    public string AutoanalysisFolder => System.IO.Path.Combine(Path, "autoanalysis");

    private readonly PvXml.ScanTypes.MarkPoints Scan;

    public MarkPoints(string folder)
    {
        Path = System.IO.Path.GetFullPath(folder);
        Scan = new PvXml.ScanTypes.MarkPoints(folder);
    }

    public ResultsFiles[] GetResultFiles()
    {
        List<ResultsFiles> groups = new();

        groups.Add(
            new ResultsFiles()
            {
                Title = "Reference Images",
                Paths = Directory.GetFiles(AutoanalysisFolder, "ref_*.png")
                    .Select(x => System.IO.Path.GetFileName(Path) + "/autoanalysis/" + System.IO.Path.GetFileName(x))
                    .ToArray(),
            }
        );

        return groups.ToArray();
    }

    public void Analyze(bool clear = false)
    {
    }
}
