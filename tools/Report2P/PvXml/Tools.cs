using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P.PvXml
{
    internal static class Tools
    {
        public static string GetXmlFilePath(string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath, "*.xml");

            if (files.Length == 0)
                throw new InvalidOperationException($"folder does not have XML files: {folderPath}");

            return files.First();
        }

        public static void GetTime(string xmlFilePath)
        {

        }
    }
}
