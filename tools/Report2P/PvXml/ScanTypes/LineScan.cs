using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Report2P.PvXml.ScanTypes;

internal class LineScan : IScan
{
    public ScanType ScanType => ScanType.LineScan;

    public PVState PVState { get; private set; }

    public readonly int WidthSpacePx = -1;
    public double WidthSpaceMicrons => PVState.MicronsPerPixelX * WidthSpacePx;

    public readonly int HeightTimePx = -1;
    public readonly double ScanLinePeriod = double.NaN;
    public double HeightTimeSec => ScanLinePeriod * HeightTimePx;

    public readonly int SequenceCount = -1;

    public LineScan(string path)
    {
        PVState = new PVState(path);
        XDocument xmlDoc = PVState.XmlDoc;
        AssertValidScan(xmlDoc);
        //FrameTimes = GetFrameTimes(xmlDoc);

        SequenceCount = xmlDoc.XPathSelectElements("/PVScan/Sequence").Count();

        var stateValueElements = xmlDoc.XPathSelectElements("/PVScan/Sequence").First()
            .Elements("Frame").First()
            .Elements("PVStateShard").First()
            .Elements("PVStateValue");

        foreach (var stateValueElement in stateValueElements)
        {
            string key = stateValueElement.Attribute("key").Value;
            double val = double.Parse(stateValueElement.Attribute("value").Value);

            if (key == "pixelsPerLine")
                WidthSpacePx = (int)val;

            if (key == "linesPerFrame")
                HeightTimePx = (int)val;

            if (key == "scanLinePeriod")
                ScanLinePeriod = val;
        }
    }

    private void AssertValidScan(XDocument xmlDoc)
    {
        var sequenceElements = xmlDoc.XPathSelectElements("/PVScan/Sequence");
        if (sequenceElements.Count() == 0)
            throw new InvalidOperationException($"no sequences in: {PVState.XmlFilePath}");

        string expectedSequenceType = "Linescan";
        var sequenceElement = sequenceElements.First();
        if (sequenceElement.Attribute("type").Value != expectedSequenceType)
            throw new InvalidOperationException($"Expected series of type: {expectedSequenceType}");
    }

    public string GetSummary()
    {
        StringBuilder sb = new(PVState.GetSummary());
        sb.AppendLine($"LineScan sequences: {SequenceCount}");
        sb.AppendLine($"LineScan space: {WidthSpaceMicrons:N2} microns ({WidthSpacePx} pixels)");
        sb.AppendLine($"LineScan time per image: {HeightTimeSec:N2} seconds ({HeightTimePx} pixels)");
        sb.AppendLine($"LineScan time per line: {ScanLinePeriod * 1000} ms");
        sb.AppendLine($"LineScan image size: {HeightTimePx} x {HeightTimePx}");
        return sb.ToString();
    }

    public static string[] GetScanTifs(string folder, int channel = 1)
    {
        return System.IO.Directory.GetFiles(folder, "*.tif")
            .Where(x => x.Contains($"_Ch{channel}_00"))
            .ToArray();
    }
}