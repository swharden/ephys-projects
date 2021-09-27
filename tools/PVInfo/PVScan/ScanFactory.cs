using System;
using System.IO;
using System.Linq;

namespace PVInfo.PVScan
{
    public static class ScanFactory
    {
        public static IScan FromPVFolder(string pvFolderPath)
        {
            string xmlFilePath = Directory.GetFiles(pvFolderPath, "*.xml").First();
            string xml = File.ReadAllText(xmlFilePath);

            if (xml.Contains("type=\"TSeries ZSeries Element\""))
                return new TZSeries(xmlFilePath);

            if (xml.Contains("type=\"ZSeries\""))
                return new ZSeries(xmlFilePath);

            throw new NotImplementedException("unsupported XML scan type");
        }
    }
}