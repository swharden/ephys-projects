using System;
using System.IO;
using System.Linq;

namespace PVInfo.PVScan
{
    public static class ScanFactory
    {
        public static IScan FromPVFolder(string pvFolderPath)
        {
            string[] xmlFilePaths = Directory.GetFiles(pvFolderPath, "*.xml");
            if (xmlFilePaths.Length == 0)
                return null;

            string xmlFilePath = xmlFilePaths.First();
            string xml = File.ReadAllText(xmlFilePath);

            if (xml.Contains("type=\"TSeries Timed Element\""))
                return new TSeries(xmlFilePath);

            if (xml.Contains("type=\"TSeries ZSeries Element\""))
                return new TZSeries(xmlFilePath);

            if (xml.Contains("type=\"ZSeries\""))
                return new ZSeries(xmlFilePath);

            if (xml.Contains("type=\"Single\""))
                return new SingleImage(xmlFilePath);

            throw new NotImplementedException($"unsupported XML scan type: {xmlFilePath}");
        }
    }
}