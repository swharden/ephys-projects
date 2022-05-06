using System;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Report2P.PvXml.ScanTypes;

public class TSeries : IScan
{
    public PVState PVState { get; private set; }
    public ScanType ScanType => ScanType.TSeries;
    public readonly double[] FrameTimes;
    public int FrameCount => FrameTimes.Length;
    public double FrameAcquisitionTime => FrameTimes.Length >= 2 ? FrameTimes[1] - FrameTimes[0] : double.NaN;
    public double TotalAcquisitionTime => FrameAcquisitionTime * FrameCount;
    public readonly int Width;
    public readonly int Height;

    public TSeries(string path)
    {
        PVState = new PVState(path);
        XDocument xmlDoc = PVState.XmlDoc;
        AssertValidScan(xmlDoc);
        FrameTimes = GetFrameTimes(xmlDoc);
        (Width, Height) = GetDimensions(xmlDoc);
    }

    public string GetSummary()
    {
        StringBuilder sb = new(PVState.GetSummary());
        sb.AppendLine($"TSeries Image resolution: {Width}x{Height}");
        sb.AppendLine($"TSeries Image count: {FrameCount}");
        sb.AppendLine($"TSeries Image time: {FrameAcquisitionTime} sec ({1.0 / FrameAcquisitionTime:N2} FPS)");
        sb.AppendLine($"TSeries Total time: {TotalAcquisitionTime} ({Math.Round(TotalAcquisitionTime / 60, 3)} min)");
        return sb.ToString();
    }

    private void AssertValidScan(XDocument xmlDoc)
    {
        var sequenceElements = xmlDoc.XPathSelectElements("/PVScan/Sequence");
        if (sequenceElements.Count() != 1)
            throw new InvalidOperationException($"expected just 1 sequence: {PVState.XmlFilePath}");

        string expectedSequenceType = "TSeries Timed Element";
        var sequenceElement = sequenceElements.First();
        if (sequenceElement.Attribute("type").Value != expectedSequenceType)
            throw new InvalidOperationException($"Expected series of type: {expectedSequenceType}");
    }

    public static double[] GetFrameTimes(XDocument xmlDoc)
    {
        return xmlDoc
           .XPathSelectElements("/PVScan/Sequence")
           .First()
           .Elements("Frame")
           .Select(x => x.Attribute("relativeTime").Value)
           .Select(x => double.Parse(x))
           .ToArray();
    }

    public static (int width, int height) GetDimensions(XDocument xmlDoc)
    {
        int width = 0;
        int height = 0;

        foreach (XElement element in xmlDoc.XPathSelectElements("PVScan/PVStateShard/PVStateValue"))
        {
            foreach (XAttribute keyAttribute in element.Attributes("key"))
            {
                XAttribute? valueAttribute = element.Attribute("value");
                if (valueAttribute is null)
                    continue;

                if (keyAttribute.Value == "linesPerFrame")
                    width = int.Parse(valueAttribute.Value);

                if (keyAttribute.Value == "pixelsPerLine")
                    height = int.Parse(valueAttribute.Value);
            }
        }

        return (width, height);
    }
}