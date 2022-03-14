using Report2P.PvXml.ScanTypes;

namespace Report2P.PvXml;

internal class ScanFactory
{
    public static IScan? FromPVFolder(string pvFolderPath)
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

        if (xml.Contains("type=\"Linescan\""))
            return new LineScan(xmlFilePath);

        if (xml.Contains("type=\"MarkPoints\""))
            return null;

        if (xml.Contains("type=\"Point Scan\""))
            return null;

        throw new NotImplementedException($"unsupported XML scan type: {xmlFilePath}");
    }
}