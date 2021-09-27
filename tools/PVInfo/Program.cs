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
                sb.AppendLine($"<h1>{scan.ScanType}: {Path.GetFileName(pvFolderPath)}</h1>");
                sb.AppendLine($"<pre>{scan.GetSummary()}</pre>");
                ShowImages(pvFolderPath, sb);
                sb.AppendLine("<hr>");
            }

            string reportFilePath = Path.Combine(folderPath, "report.html");
            File.WriteAllText(reportFilePath, sb.ToString());
            Console.WriteLine(reportFilePath);
        }

        static void ShowImages(string pvFolderPath, StringBuilder sb)
        {
            string pvFolderName = Path.GetFileName(pvFolderPath);
            List<string> tifFiles = new();
            List<string> imageFiles = new();
            List<string> notesFiles = new();

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

                    string imageUrl = pvFolderName + "/" + Path.GetFileName(subFolderPath) + "/" + Path.GetFileName(filePath);
                    if (supportedExtensions.Contains(ext))
                        imageFiles.Add(imageUrl);
                }
            }

            if (notesFiles.Count > 0)
            {
                sb.AppendLine("<pre>");
                sb.AppendLine($"Experiment Notes:");
                foreach (string notesFile in notesFiles)
                    sb.AppendLine(File.ReadAllText(notesFile));
                sb.AppendLine("</pre>");
            }

            sb.AppendLine("<pre>");
            sb.AppendLine($"Processed TIFs ({tifFiles.Count}):");
            foreach (string tif in tifFiles)
                sb.AppendLine(tif);
            sb.AppendLine("</pre>");

            sb.AppendLine("<pre>");
            sb.AppendLine($"Processed Images ({imageFiles.Count}):");
            foreach (string imgUrl in imageFiles)
                sb.AppendLine(Path.GetDirectoryName(pvFolderPath) + "/" + imgUrl);
            sb.AppendLine("</pre>");
            foreach (string imgUrl in imageFiles)
                sb.AppendLine($"<a href='{imgUrl}'><img src='{imgUrl}'></a>");
        }
    }
}