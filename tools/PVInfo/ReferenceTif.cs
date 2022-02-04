using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PVInfo
{
    internal static class ReferenceTif
    {
        public static void ConvertMultiFolder(string folderContainingScans)
        {
            foreach (string dir in Directory.GetDirectories(folderContainingScans))
            {
                string referenceFolder = Path.Combine(dir, "References");
                if (Directory.Exists(referenceFolder))
                    ConvertFolder(referenceFolder);
            }
        }

        static void ConvertFolder(string tifFolder)
        {
            Console.WriteLine($"Converting TIF to PNG in {tifFolder}");
            string[] paths = Directory
                .GetFiles(tifFolder, "*.tif")
                .Where(x => x.EndsWith(".tif", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            foreach (string path in paths)
                ConvertFile(path);
        }

        static void ConvertFile(string tifpath)
        {
            string pngPath = tifpath + ".png";
            var img = SciTIF.Image.FromTif(tifpath);
            img.AutoScale();
            img.SavePng(pngPath);
        }
    }
}
