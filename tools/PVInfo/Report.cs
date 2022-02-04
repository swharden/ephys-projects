using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVInfo
{
    internal static class Report
    {
        public static void CreateMultiFolderIndex(string folderPath)
        {
            StringBuilder sb = new();

            sb.AppendLine("<div class='alert alert-primary mt-5 shadow border'>");
            sb.AppendLine($"<h1>Multi-Folder 2P Report</h1>");
            sb.AppendLine($"<div>{folderPath}</div>");
            sb.AppendLine($"<div>Generated {DateTime.Now}</div>");
            sb.AppendLine("</div>");

            string[] imageExtensions = { ".png", ".gif", ".jpg", ".jpeg" };
            string[] imageFilenames = Directory.GetFiles(folderPath)
                .Select(x => Path.GetFileName(x))
                .Where(x => imageExtensions.Contains(Path.GetExtension(x).ToLowerInvariant()))
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
                    Code(sb, scan.GetSummary());
                    ShowImages(pvFolderPath, sb);
                    sb.AppendLine("</div>");
                }
                else
                {
                    Console.WriteLine($"Skipping: {pvFolderPath}");
                }
            }

            string html = Template.HTML.Replace("{{CONTENT}}", sb.ToString());

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
            List<string> tifFiles = new();
            List<string> imageFiles = new();
            List<string> notesFiles = new();
            List<string> videoFiles = new();
            List<string> originFiles = new();

            foreach (string subFolderPath in Directory.GetDirectories(pvFolderPath))
            {
                foreach (string filePath in Directory.GetFiles(subFolderPath, "*.*"))
                {
                    string ext = Path.GetExtension(filePath).ToLowerInvariant();
                    if (ext == ".tif")
                        tifFiles.Add(filePath);
                    else if (ext == ".txt")
                        notesFiles.Add(filePath);
                    else if (ext == ".mp4")
                        videoFiles.Add(filePath);
                    else if (ext == ".jpg" || ext == ".gif" || ext == ".png")
                        imageFiles.Add(filePath);
                    else if (ext == ".dat")
                        originFiles.Add(filePath);
                }
            }

            ListNotesFiles(sb, notesFiles, pvFolderPath);
            ListVideos(sb, videoFiles, pvFolderPath);
            ListImages(sb, imageFiles, pvFolderPath);
            ListOriginFiles(sb, originFiles, pvFolderPath);
            ListTifFiles(sb, tifFiles, pvFolderPath);
        }

        static void ListNotesFiles(StringBuilder sb, List<string> notesFiles, string pvFolderPath)
        {
            if (notesFiles.Count == 0)
                return;

            AddHeading(sb, $"Experiment Notes ({notesFiles.Count})");
            CodeStart(sb);
            foreach (string notesFile in notesFiles)
                sb.AppendLine(File.ReadAllText(notesFile));
            CodeEnd(sb);
        }

        static void ListTifFiles(StringBuilder sb, List<string> tifFiles, string pvFolderPath)
        {
            if (tifFiles.Count == 0)
                return;

            AddHeading(sb, $"Reference TIFs ({tifFiles.Count}):");
            CodeStart(sb);
            foreach (string tif in tifFiles)
                sb.AppendLine(tif);
            CodeEnd(sb);
        }

        static void ListOriginFiles(StringBuilder sb, List<string> originFiles, string pvFolderPath)
        {
            if (originFiles.Count == 0)
                return;

            AddHeading(sb, $"Origin Files ({originFiles.Count}):");
            CodeStart(sb);
            foreach (string path in originFiles)
                sb.AppendLine(path);
            CodeEnd(sb);
        }

        static void ListImages(StringBuilder sb, List<string> imageFiles, string pvFolderPath)
        {
            if (imageFiles.Count == 0)
                return;

            AddHeading(sb, $"Processed Images ({imageFiles.Count}):");
            CodeStart(sb);
            foreach (string imageFile in imageFiles)
                sb.AppendLine(imageFile);
            CodeEnd(sb);
            foreach (string imageFile in imageFiles)
            {
                string imageFolderName = Path.GetFileName(Path.GetDirectoryName(imageFile));
                string imageFileName = Path.GetFileName(imageFile);
                string imageUrl = $"{Path.GetFileName(pvFolderPath)}/{imageFolderName}/{imageFileName}";
                sb.AppendLine($"<a href='{imageUrl}'><img src='{imageUrl}' class='m-2' style='max-width: 300px'></a>");
            }
        }

        static void ListVideos(StringBuilder sb, List<string> videoFiles, string pvFolderPath)
        {
            if (videoFiles.Count == 0)
                return;

            AddHeading(sb, $"Processed Videos ({videoFiles.Count}):");

            CodeStart(sb);
            foreach (string videoFile in videoFiles)
                sb.AppendLine(Path.GetDirectoryName(pvFolderPath) + "/" + videoFile);
            CodeEnd(sb);

            foreach (string videoFile in videoFiles)
            {
                string videoFolderName = Path.GetFileName(Path.GetDirectoryName(videoFile));
                string videoFileName = Path.GetFileName(videoFile);
                string videoUrl = $"{Path.GetFileName(pvFolderPath)}/{videoFolderName}/{videoFileName}";
                sb.AppendLine($"<video class='m-3' controls><source src='{videoUrl}' type='video/mp4'></video>");
            }
        }

        static string GetImageHtml(string folderPath)
        {
            StringBuilder sb = new();

            string[] extensions = { ".png", ".gif", ".jpg", ".jpeg" };
            string[] filenames = Directory.GetFiles(folderPath)
                .Select(x => Path.GetFileName(x))
                .Where(x => extensions.Contains(Path.GetExtension(x).ToLowerInvariant()))
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
                .Where(x => extensions.Contains(Path.GetExtension(x).ToLowerInvariant()))
                .ToArray();

            foreach (var filename in filenames)
                sb.AppendLine($"<video class='m-3' height='300' controls><source src='{filename}' type='video/mp4'></video>");

            return sb.ToString();
        }

    }
}
