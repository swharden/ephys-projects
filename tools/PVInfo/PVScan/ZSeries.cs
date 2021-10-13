using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PVInfo.PVScan
{
    public class ZSeries : IScan
    {
        public PVState PVState { get; private set; }
        public ScanType ScanType => ScanType.ZSeries;
        public readonly double[] FrameTimes;
        public int FrameCount => FrameTimes.Length;
        public double FrameAcquisitionTime => FrameTimes.Length > 1 ? Math.Round(FrameTimes[1] - FrameTimes[0], 5) : double.NaN;
        public double TotalAcquisitionTime => Math.Round(FrameAcquisitionTime * FrameCount, 5);

        public ZSeries(string path)
        {
            PVState = new PVState(path);
            XDocument xmlDoc = PVState.XmlDoc;
            AssertValidScan(xmlDoc);
            FrameTimes = GetFrameTimes(xmlDoc);
        }

        public string GetSummary()
        {
            StringBuilder sb = new(PVState.GetSummary());
            sb.AppendLine($"ZSeries Frame count: {FrameCount}");
            sb.AppendLine($"ZSeries Frame time: {FrameAcquisitionTime}");
            sb.AppendLine($"ZSeries Total time: {TotalAcquisitionTime}");
            return sb.ToString();
        }

        private static void AssertValidScan(XDocument xmlDoc)
        {
            var sequenceElements = xmlDoc.XPathSelectElements("/PVScan/Sequence");
            if (sequenceElements.Count() != 1)
                throw new InvalidOperationException("expected single Sequence element");

            var sequenceElement = sequenceElements.First();
            if (sequenceElement.Attribute("type").Value != "ZSeries")
                throw new InvalidOperationException("Expected series of type ZSeries");
        }

        public static double[] GetFrameTimes(XDocument xmlDoc) =>
            xmlDoc
               .XPathSelectElements("/PVScan/Sequence")
               .First()
               .Elements("Frame")
               .Select(x => x.Attribute("absoluteTime").Value)
               .Select(x => double.Parse(x))
               .Select(x => Math.Round(x, 6))
               .ToArray();
    }
}