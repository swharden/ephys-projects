using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVInfo
{
    internal static class Template
    {
        const string FILENAME = "template.html";

        public static string HTML => File.ReadAllText(Locate());

        private static string Locate(int maxLevelsUp = 10)
        {
            string folder = Path.GetFullPath("./");

            for (int i = 0; i < maxLevelsUp; i++)
            {
                string path = Path.Combine(folder, FILENAME);

                if (File.Exists(path))
                    return path;

                folder = Path.GetDirectoryName(folder);
            }

            throw new InvalidOperationException($"unable to locate {FILENAME} in {folder}");
        }
    }
}
