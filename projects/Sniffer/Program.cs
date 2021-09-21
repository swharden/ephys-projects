using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Sniffer
{
    class Program
    {
        static void Main(string[] args)
        {
            string basePath = @"X:\Data Analysis\_Core Projects\Sniffer PVN";
            string yamlFilePath = Path.Combine(basePath, "experiments.yaml");
            string htmlOutputPath = Path.Combine(basePath, "report.html");

            List<Experiment> experiments = new();
            Experiment experiment = null;
            foreach (string line in File.ReadLines(yamlFilePath))
            {
                if (line.StartsWith("#"))
                    continue;

                if (line.StartsWith("      - "))
                {
                    if (experiment is not null)
                        experiments.Add(experiment);

                    string id = line.Split("\"")[1];
                    experiment = new Experiment(id);
                }

                if (line.StartsWith("          - "))
                {
                    experiment.Process(line);
                }
            }

            Console.WriteLine($"Read {experiments.Count} experiments");
            WriteReport(experiments.ToArray(), htmlOutputPath);
        }

        static void WriteReport(Experiment[] experiments, string filePath)
        {
            StringBuilder sb = new();
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("img {max-height: 400px;}");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<table>");
            AddHeader(sb);
            foreach (Experiment experiment in experiments)
                AddRow(sb, experiment);
            sb.AppendLine("</table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            File.WriteAllText(filePath, sb.ToString());
            Console.WriteLine($"wrote: {filePath}");
        }

        static void AddHeader(StringBuilder sb)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Reference Image</th>");
            sb.AppendLine("<th>ROIs</th>");
            sb.AppendLine("<th>Video</th>");
            sb.AppendLine("<tr>");
        }

        static void AddRow(StringBuilder sb, Experiment experiment)
        {
            sb.AppendLine("<tr>");
            try
            {
                sb.AppendLine($"<td><a href='{experiment.MergeImageFile}'><img src='{experiment.MergeImageFile}' /></a></td>");
                sb.AppendLine($"<td><a href='{experiment.RoiImageFile}'><img src='{experiment.RoiImageFile}' /></a></td>");
                sb.AppendLine($"<td><video controls src='{experiment.VideoFile}' ></video></td>");
                Console.WriteLine($"Experiment {experiment.ID}: added");
            }
            catch
            {
                Console.WriteLine($"Experiment {experiment.ID}: ERROR");
            }
            sb.AppendLine("</tr>");
        }
    }

    class Experiment
    {
        public readonly string ID;

        string BasePath;

        string TSeries;
        public DirectoryInfo TSeriesFolder => new(Path.Combine(BasePath, TSeries));

        string MergeImage;
        public string MergeImageFile => Path.Combine(BasePath, MergeImage);

        string RoiImage;
        public string RoiImageFile => Path.Combine(BasePath, RoiImage);

        string Video;
        public string VideoFile => Path.Combine(BasePath, Video);

        string Notes;

        public Experiment(string id)
        {
            ID = id;
        }

        public void Process(string line)
        {
            line = line.Trim().TrimStart('-').Trim();
            string[] parts = line.Split(":", 2);
            string key = parts[0].Trim();
            string val = parts[1].Trim().Trim('\'');

            switch (key)
            {
                case "path":
                    BasePath = val.Replace("\\", "/").Replace("//spike/X_Drive/", "X:/");
                    break;
                case "tseries":
                    TSeries = val;
                    break;
                case "mergeImage":
                    MergeImage = val;
                    break;
                case "roiImage":
                    RoiImage = val;
                    break;
                case "video":
                    Video = val;
                    break;
            }
        }
    }

}

