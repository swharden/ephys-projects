using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Report2P.PvXml.ScanTypes
{
    public class MarkPoints
    {
        public readonly DateTime DateTime;

        public MarkPoints(string folder)
        {
            string[] xmlFiles = Directory.GetFiles(folder, "MarkPoints-*.xml");
            string[] patternXmlFiles = xmlFiles.Where(x => Path.GetFileName(x).EndsWith("_MarkPoints.xml")).ToArray();
            string[] stateXmlFiles = xmlFiles.Where(x => !Path.GetFileName(x).EndsWith("_MarkPoints.xml")).ToArray();

            var xmlDoc = XDocument.Parse(File.ReadAllText(stateXmlFiles.First()));
            string scanDate = xmlDoc.Element("PVScan")?.Attribute("date")?.Value
                ?? throw new InvalidOperationException("date not found in XML file");
            DateTime = DateTime.Parse(scanDate);
        }
    }
}
