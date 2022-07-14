using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Report2P.PvXml.ScanTypes;

public class TZSeries : IScan
{
    public PVState PVState { get; private set; }
    public ScanType ScanType => ScanType.TZSeries;
    public readonly double[] SequenceTimes;
    public int SequenceCount => SequenceTimes.Length;
    public double StackAcquisitionTime => SequenceTimes[1] - SequenceTimes[0];
    public double TotalAcquisitionTime => StackAcquisitionTime * SequenceCount;

    public TZSeries(string path)
    {
        PVState = new PVState(path);
        XDocument xmlDoc = PVState.XmlDoc;
        AssertValidScan(xmlDoc);
        SequenceTimes = GetSequenceTimes(xmlDoc);
    }

    public string GetSummary()
    {
        StringBuilder sb = new(PVState.GetSummary());
        sb.AppendLine($"TZSeries Stack count: {SequenceCount}");
        sb.AppendLine($"TZSeries Stack time: {StackAcquisitionTime}");
        sb.AppendLine($"TZSeries Total time: {TotalAcquisitionTime} ({Math.Round(TotalAcquisitionTime / 60, 3)} min)");
        return sb.ToString();
    }

    private static void AssertValidScan(XDocument xmlDoc)
    {
        var sequenceElements = xmlDoc.XPathSelectElements("/PVScan/Sequence");
        if (sequenceElements.Count() < 2)
            throw new InvalidOperationException("expected multiple Sequence elements");

        string expectedSequenceType = "TSeries ZSeries Element";
        var sequenceElement = sequenceElements.First();
        if (sequenceElement.Attribute("type")?.Value != expectedSequenceType)
            throw new InvalidOperationException($"Expected series of type: {expectedSequenceType}");
    }

    public static double[] GetSequenceTimes(XDocument xmlDoc)
    {
        DateTime[] sequenceTimes = xmlDoc
           .XPathSelectElements("/PVScan/Sequence")
           .Select(x => x.Attribute("time").Value)
           .Select(x => DateTime.Parse(x))
           .ToArray();

        double[] timeDeltas = sequenceTimes
            .Select(x => x - sequenceTimes.First())
            .Select(x => x.TotalSeconds)
            .ToArray();

        return timeDeltas;
    }
}