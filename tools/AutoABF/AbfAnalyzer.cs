using System;
using System.IO;
using System.Linq;

namespace AutoABF
{
    public static class AbfAnalyzer
    {
        public static void Analyze(string abfFilePath)
        {
            abfFilePath = Path.GetFullPath(abfFilePath);
            string abfFolder = Path.GetDirectoryName(abfFilePath);
            string outputFolder = Path.Combine(abfFolder, "_autoanalysis");
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            var abf = new AbfSharp.ABF(abfFilePath, preloadData: false);
            string protocolName = Path.GetFileNameWithoutExtension(abf.Header.sProtocolPath);
            string protocolPrefix = protocolName.Split(" ")[0];

            switch (protocolPrefix)
            {
                case "0201":
                    var mt = new Analysis.Memtest();
                    mt.Analyze(abf, outputFolder);
                    break;
                default:
                    throw new NotImplementedException($"unsupported protocol: {protocolPrefix}");
            }
        }
    }
}