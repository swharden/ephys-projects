using System;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PVInfo.PVScan
{
    public class TSeries : IScan
    {
        public PVState PVState { get; private set; }
        public ScanType ScanType => ScanType.TSeries;
        public readonly double[] FrameTimes;
        public int FrameCount => FrameTimes.Length;
        public double FrameAcquisitionTime => FrameTimes[1] - FrameTimes[0];
        public double TotalAcquisitionTime => FrameAcquisitionTime * FrameCount;

        public TSeries(string path)
        {
            PVState = new PVState(path);
            XDocument xmlDoc = PVState.XmlDoc;
            AssertValidScan(xmlDoc);
            FrameTimes = GetFrameTimes(xmlDoc);
        }

        public string GetSummary()
        {
            StringBuilder sb = new(PVState.GetSummary());
            sb.AppendLine($"TSeries Image count: {FrameCount}");
            sb.AppendLine($"TSeries Image time: {FrameAcquisitionTime}");
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
    }
}