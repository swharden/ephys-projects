using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace PVInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            MakeIndex(@"X:\Data\C57\GRABNE\2021-09-23-ne-washon");
        }

        static void MakeIndex(string folderPath)
        {
            StringBuilder sb = new();

            foreach (var pvFolderPath in Directory.GetDirectories(folderPath))
            {
                PVScan.IScan scan = PVScan.ScanFactory.FromPVFolder(pvFolderPath);
                if (scan is not null)
                {
                    Console.WriteLine($"Analyzing: {pvFolderPath}");
                    sb.AppendLine("<div class='my-5 p-3 bg-light shadow border rounded bg-white'>");
                    sb.AppendLine($"<h1>{scan.ScanType}: {Path.GetFileName(pvFolderPath)}</h1>");
                    AddHeading(sb, $"Scan Information");
                    Code(sb, scan.GetSummary());
                    ShowImages(pvFolderPath, sb);
                    sb.AppendLine("</div>");
                }
                else
                {
                    Console.WriteLine($"Skipping: {pvFolderPath}");
                }
            }

            string template = File.ReadAllText("template.html");
            string html = template.Replace("{{CONTENT}}", sb.ToString());

            string reportFilePath = Path.Combine(folderPath, "report.html");
            File.WriteAllText(reportFilePath, html);
            Console.WriteLine(reportFilePath);
        }

        static void AddHeading(StringBuilder sb, string heading)
        {
            sb.AppendLine($"<h3>{heading}</h3>");
        }

        static void CodeStart(StringBuilder sb) => sb.Append($"\n<pre class='p-2'>");
        static void CodeEnd(StringBuilder sb) => sb.Append($"</pre>\n");
        static void Code(StringBuilder sb, string code)
        {
            CodeStart(sb);
            sb.Append(code);
            CodeEnd(sb);
        }

        static void ShowImages(string pvFolderPath, StringBuilder sb)
        {
            string pvFolderName = Path.GetFileName(pvFolderPath);
            List<string> tifFiles = new();
            List<string> imageFiles = new();
            List<string> notesFiles = new();
            List<string> videoFiles = new();

            string[] supportedExtensions = { ".jpg", ".gif", ".png" };
            foreach (string subFolderPath in Directory.GetDirectories(pvFolderPath))
            {
                foreach (string filePath in Directory.GetFiles(subFolderPath, "*.*"))
                {
                    string ext = Path.GetExtension(filePath);

                    if (ext == ".tif")
                        tifFiles.Add(filePath);

                    if (ext == ".txt")
                        notesFiles.Add(filePath);

                    if (ext == ".mp4")
                        videoFiles.Add(filePath);

                    if (ext == ".jpg" || ext == ".gif" || ext == ".png")
                        imageFiles.Add(filePath);
                }
            }

            AddHeading(sb, $"Experiment Notes ({notesFiles.Count})");
            CodeStart(sb);
            foreach (string notesFile in notesFiles)
                sb.AppendLine(File.ReadAllText(notesFile));
            CodeEnd(sb);

            AddHeading(sb, $"Processed TIFs ({tifFiles.Count}):");
            CodeStart(sb);
            foreach (string tif in tifFiles)
                sb.AppendLine(tif);
            CodeEnd(sb);

            AddHeading(sb, $"Processed Images ({imageFiles.Count}):");
            CodeStart(sb);
            foreach (string imageFile in imageFiles)
                sb.AppendLine(imageFile);
            CodeEnd(sb);
            foreach (string imageFile in imageFiles)
            {
                string imageFolderName = Path.GetFileName(Path.GetDirectoryName(imageFile));
                string imageFileName = Path.GetFileName(imageFile);
                string imageUrl = $"{pvFolderName}/{imageFolderName}/{imageFileName}";
                sb.AppendLine($"<a href='{imageUrl}'><img src='{imageUrl}'></a>");
            }

            AddHeading(sb, $"Processed Videos ({videoFiles.Count}):");
            CodeStart(sb);
            foreach (string videoFile in videoFiles)
                sb.AppendLine(Path.GetDirectoryName(pvFolderPath) + "/" + videoFile);
            CodeEnd(sb);

            foreach (string videoFile in videoFiles)
            {
                string videoFolderName = Path.GetFileName(Path.GetDirectoryName(videoFile));
                string videoFileName = Path.GetFileName(videoFile);
                string videoUrl = $"{pvFolderName}/{videoFolderName}/{videoFileName}";
                sb.AppendLine($"<video class='m-3' controls><source src='{videoUrl}' type='video/mp4'></video>");
            }
        }
    }
}