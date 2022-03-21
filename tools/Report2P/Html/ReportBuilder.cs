﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P.Html;

internal class ReportBuilder
{
    public readonly string Title;

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

        Title = "2P Report";
        string header = File.ReadAllText(Path.Combine(templateFolder, "header.html"))
            .Replace("{{TITLE}}", Title)
            .Replace("{{SUBTITLE}}", folder2p)
            .Replace("{{DATE}}", DateTime.Now.ToShortDateString())
            .Replace("{{TIME}}", DateTime.Now.ToShortTimeString());

        Content.AppendLine(header);
    }

    public void Add(TimelineItem item, bool open = false)
    {
        StringBuilder sb = new();
        string timestamp = string.Empty;

        if (item.DateTime != DateTime.MinValue)
        {
            timestamp = $"<div style='font-size: .8em;'>" +
                $"<div>{TimeOnly.FromDateTime(item.DateTime).ToLongTimeString()}</div>" +
                $"<div>(+{item.ExperimentTime.Minutes}:{item.ExperimentTime.Seconds:00})</div>" +
                $"</div>";

            sb.AppendLine($"<h3>PrairieView Configuration</h3>");
            sb.AppendLine($"<pre>{item.Content}</pre>");

            foreach (Experiment.ImageGroup grp in item.ImageGroups)
            {
                sb.AppendLine($"<h3>{grp.Title}</h3>");
                foreach (string path in grp.Paths)
                {
                    sb.AppendLine($"<a href='{path}'><img src='{path}' height='300'></a>");
                }
            }
        }

        string line = TemplateTimelineItem
            .Replace("{{TITLE}}", item.Title)
            .Replace("{{TIMESTAMP}}", timestamp)
            .Replace("{{CONTENT}}", sb.ToString())
            .Replace("{{ICON}}", item.Icon.ToString().ToLower())
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