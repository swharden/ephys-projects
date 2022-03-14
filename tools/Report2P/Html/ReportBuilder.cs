using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P.Html;

internal class ReportBuilder
{
    public string Title = "2P Report";

    private readonly string TemplateBase;

    private readonly string TemplateTimelineItem;

    private readonly StringBuilder Content = new();

    public ReportBuilder(string templateFolder, string folder2p)
    {
        templateFolder = Path.GetFullPath(templateFolder);
        if (!Directory.Exists(templateFolder))
            throw new DirectoryNotFoundException(templateFolder);

        folder2p = Path.GetFullPath(folder2p);
        if (!Directory.Exists(folder2p))
            throw new DirectoryNotFoundException(folder2p);

        TemplateBase = File.ReadAllText(Path.Combine(templateFolder, "base.html"));
        TemplateTimelineItem = File.ReadAllText(Path.Combine(templateFolder, "timeline-item-details.html"));

        string header = File.ReadAllText(Path.Combine(templateFolder, "header.html"))
            .Replace("{{TITLE}}", "2P Report")
            .Replace("{{SUBTITLE}}", folder2p);

        Content.AppendLine(header);
    }

    public void Add(TimelineItem item, bool open = false)
    {
        string timeOnly = string.IsNullOrEmpty(item.Timestamp)
            ? ""
            : DateTime.Parse(item.Timestamp).ToShortTimeString();

        string line = TemplateTimelineItem
            .Replace("{{TITLE}}", item.Title)
            .Replace("{{TIMESTAMP}}", timeOnly)
            .Replace("{{CONTENT}}", item.Content)
            .Replace("{{ICON}}", item.Icon)
            .Replace("{{OPEN}}", open ? "open" : string.Empty);

        Content.AppendLine(line);
    }

    public void Save(string path)
    {
        string html = TemplateBase
            .Replace("{{TITLE}}", Title)
            .Replace("{{CONTENT}}", Content.ToString());

        path = Path.GetFullPath(path);
        File.WriteAllText(path, html);
        Console.WriteLine(path);
    }

    public void DivStart(string classes)
    {
        Content.Append($"<div class='{classes}'>");
    }

    public void DivEnd()
    {
        Content.Append("</div>");
    }
}