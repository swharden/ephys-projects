namespace Report2P.Experiment;

public interface IExperiment
{
    string Path { get; }
    string AutoanalysisFolder { get; }
    string Details { get; }
    ImageGroup ImageGroups { get; }
    public void Analyze(bool clear);
}