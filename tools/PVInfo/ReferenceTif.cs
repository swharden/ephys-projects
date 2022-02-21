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
                ConvertFirstLinescan(dir);

                string referenceFolder = Path.Combine(dir, "References");
                if (Directory.Exists(referenceFolder))
                    ConvertFolder(referenceFolder);
            }
        }

        static void ConvertFirstLinescan(string linescanFolder)
        {
            if (!Path.GetFileName(linescanFolder).StartsWith("LineScan-"))
                return;

            foreach (string path in Directory.GetFiles(linescanFolder, "*000001.ome.tif"))
            {
                string refFolder = Path.Combine(Path.GetDirectoryName(path), "References");
                ConvertFile(path, refFolder);
            }
        }

        static void ConvertFolder(string tifFolder)
        {
            string[] paths = Directory
                .GetFiles(tifFolder, "*.tif")
                .Where(x => x.EndsWith(".tif", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            foreach (string path in paths)
                ConvertFile(path);
        }

        static void ConvertFile(string tifpath, string outputFolder = null, bool overwrite = false)
        {
            string pngPath = tifpath + ".png";

            if ((overwrite is false) && (File.Exists(pngPath)))
                return;

            if (outputFolder is not null)
                pngPath = Path.Combine(outputFolder, Path.GetFileName(pngPath));

            Console.WriteLine($"Converting to PNG: {tifpath}");
            var img = SciTIF.Image.FromTif(tifpath);
            img.AutoScale();
            img.SavePng(pngPath);
        }
    }
}
