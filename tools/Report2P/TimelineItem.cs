namespace Report2P;

internal class TimelineItem
{
    public DateTime DateTime { get; init; }
    public TimeSpan ExperimentTime { get; set; } = TimeSpan.Zero;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public TimelineIcon Icon { get; init; } = TimelineIcon.Line;
    public Experiment.ResultsFiles[] ImageGroups = Array.Empty<Experiment.ResultsFiles>();
}
