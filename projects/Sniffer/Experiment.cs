using System;
using System.Text;
using System.Collections.Generic;

namespace Sniffer
{
    class Experiment
    {
        public readonly string ID;
        private readonly Dictionary<string, string> Dict = new();
        public string Path => Dict["pathsafe"];
        public string RepImageUrl => Dict.ContainsKey("mergeImage") ? Path + "/" + Dict["mergeImage"] : "UNKNOWN";
        public string RoiImageUrl => Dict.ContainsKey("roiImage") ? Path + "/" + Dict["roiImage"] : "UNKNOWN";
        public string VideoUrl => Dict.ContainsKey("video") ? Path + "/" + Dict["video"] : "UNKNOWN";

        public Experiment(string id)
        {
            ID = id;
        }

        public void Add(string key, string value)
        {
            Dict[key] = value;
            if (key == "path")
                Dict["pathsafe"] = value.Replace("\\", "/").Replace("//spike/X_Drive", "X:");
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"Experiment '{ID}'");

            foreach (string key in Dict.Keys)
                sb.AppendLine($" {key}: {Dict[key]}");

            return sb.ToString();
        }

        public string GetHtml()
        {
            StringBuilder sb = new();

            sb.AppendLine($"<h2 class='mt-5'>{ID}</h2>");
            sb.AppendLine($"<div><code>{Path}</code></div>");
            sb.AppendLine($"<div><table><tr>");

            sb.AppendLine($"<td><a href='{RepImageUrl}'><img src='{RepImageUrl}' style='max-height: 300px;' /></a></td>");
            sb.AppendLine($"<td><a href='{RoiImageUrl}'><img src='{RoiImageUrl}' style='max-height: 300px;' /></a></td>");
            sb.AppendLine($"<td><video controls src='{VideoUrl}' ></video></td>");

            sb.AppendLine($"</tr></table></div>");
            return sb.ToString();
        }
    }
}