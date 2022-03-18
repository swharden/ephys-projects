namespace Report2P.Experiment;

internal class MarkPoints : IExperiment
{
    public string Path { get; private set; }

    public string Details => "uncaging point shape analysis not implemented";
    public DateTime DateTime => Scan.DateTime;

    public List<ImageGroup> ImageGroups { get; private set; } = new();

    public string AutoanalysisFolder => System.IO.Path.Combine(Path, "autoanalysis");

    private string ReferencesFolder => System.IO.Path.Combine(Path, "References");
    private readonly PvXml.ScanTypes.MarkPoints Scan;

    public MarkPoints(string folder)
    {
        Path = System.IO.Path.GetFullPath(folder);
        Scan = new PvXml.ScanTypes.MarkPoints(folder);
    }

    public void Analyze(bool clear = false)
    {
    }
}
