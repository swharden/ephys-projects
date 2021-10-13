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
            /*
            string folderOfScans = @"X:\Data\C57\GRABNE\2021-10-04-ne-washon";
            
            if (args.Length == 1) {
                folderOfScans = args[0];
            }

            MakeIndex(folderOfScans);
            */

            // ffmpeg.exe -y -i video.avi -c:v libx264 -pix_fmt yuv420p video.mp4

            string[] folders = {
                "X:/Data/OT-Cre/OT-GCaMP-nonspecific/04-03-19 evoke OT/19421000",
                "X:/Data/OT-Cre/OT-GCaMP-nonspecific/04-03-19 evoke OT/19528000",
                "X:/Data/OT-Cre/OT-GCaMP-nonspecific/04-03-19 evoke OT/04-30-2020 K-GLU analyze/19528021",
                "X:/Data/OT-Cre/OT-GCaMP-nonspecific/04-03-19 evoke OT/19610000",
                "X:/Data/OT-Cre/OT-GCaMP-nonspecific/04-03-19 evoke OT/04-30-2020 K-GLU analyze/19610009",
                "X:/Data/OT-Cre/OT-GCaMP-nonspecific/04-03-19 evoke OT/19523000",
                "X:/Data/OT-Cre/OT-GCaMP-nonspecific/04-03-19 evoke OT/04-30-2020 K-GLU analyze/19523009",
                "X:/Data/OT-Cre/OT-GCaMP-nonspecific/04-03-19 evoke OT/19523019",
                "X:/Data/OT-Cre/OT-GCaMP-nonspecific/04-03-19 evoke OT/04-30-2020 K-GLU analyze/19d09000",
                "X:/Data/OT-Cre/OT-GCaMP-nonspecific/04-03-19 evoke OT/04-30-2020 K-GLU analyze/20207000",
                "X:/Data/OT-Cre/OT-GCaMP-nonspecific/04-03-19 evoke OT/04-30-2020 K-GLU analyze/20207019",
                "X:/Data/OT-Cre/OT-GCaMP-nonspecific/04-03-19 evoke OT/04-30-2020 K-GLU analyze/20214011",
                "X:/Data/OT-Cre/OT-GCaMP-nonspecific/04-03-19 evoke OT/04-30-2020 K-GLU analyze/20214022",
            };

            foreach (string folder in folders)
            {
                MakeIndex(folder);
            }
        }

        static string GetImageHtml(string folderPath)
        {
            StringBuilder sb = new();

            string[] extensions = { ".png", ".gif", ".jpg", ".jpeg" };
            string[] filenames = Directory.GetFiles(folderPath)
                .Select(x => Path.GetFileName(x))
                .Where(x => extensions.Contains(Path.GetExtension(x)))
                .ToArray();

            foreach (var filename in filenames)
                sb.AppendLine($"<a href='{filename}'><img src='{filename}' height='300' class='shadow border m-3' /></a>");

            return sb.ToString();
        }

        static string GetVideoHtml(string folderPath)
        {
            StringBuilder sb = new();

            string[] extensions = { /*".avi",*/ ".mp4" };
            string[] filenames = Directory.GetFiles(folderPath)
                .Select(x => Path.GetFileName(x))
                .Where(x => extensions.Contains(Path.GetExtension(x)))
                .ToArray();

            foreach (var filename in filenames)
                sb.AppendLine($"<video class='m-3' height='300' controls><source src='{filename}' type='video/mp4'></video>");

            return sb.ToString();
        }

        static void MakeIndex(string folderPath)
        {
            StringBuilder sb = new();

            sb.AppendLine($"<h3><code>{folderPath}</code></h3>");

            string[] imageExtensions = { ".png", ".gif", ".jpg", ".jpeg" };
            string[] imageFilenames = Directory.GetFiles(folderPath)
                .Select(x => Path.GetFileName(x))
                .Where(x => imageExtensions.Contains(Path.GetExtension(x)))
                .ToArray();

            sb.AppendLine("<div class='bg-light my-3 d-flex'>");
            sb.AppendLine(GetImageHtml(folderPath));
            sb.AppendLine(GetVideoHtml(folderPath));
            sb.AppendLine("</div>");

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

            string reportFilePath = Path.Combine(folderPath, "index.html");
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