namespace Report2P;

internal class TimelineItem
{
    public DateTime Timestamp { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Icon { get; init; } = "line";
}
