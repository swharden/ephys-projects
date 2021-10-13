using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTIF
{
    public static class Convert
    {
        public static void FileToPng(string tifFilePath, string pngFilePath, bool autoScale = true)
        {
            Console.WriteLine($"Converting {Path.GetFileName(tifFilePath)}...");
            var img = new Image(tifFilePath);
            if (autoScale)
                img.AutoScale(.05, 99.95);
            img.SavePng(pngFilePath);
        }

        public static void FolderToPng(string folderPath, string outputFolderPath, bool autoScale = true)
        {
            outputFolderPath = Path.GetFullPath(outputFolderPath);
            if (!Directory.Exists(outputFolderPath))
                Directory.CreateDirectory(outputFolderPath);

            foreach (string tifPath in Directory.GetFiles(folderPath, "*.tif"))
            {
                string saveFileName = Path.GetFileNameWithoutExtension(tifPath) + ".png";
                string saveFilePath = Path.Combine(outputFolderPath, saveFileName);
                FileToPng(tifPath, saveFilePath, autoScale);
            }
        }
    }
}
