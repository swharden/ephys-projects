using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections.Generic;

namespace PVInfo.PVScan
{
    /// <summary>
    /// This class stores hardware configuration.
    /// It is agnostic to scan type (t-seires, z-series, etc.)
    /// </summary>
    public class PVState
    {
        public readonly string FolderPath;
        public readonly string XmlFilePath;
        public readonly string PrairieViewVersion;
        public readonly DateTime Started;
        public readonly ScanMode ScanMode;
        public readonly int BitDepth;
        public readonly double DwellTime;
        public readonly double FramePeriod;
        public double FrameRate => 1.0 / FramePeriod;
        public readonly Laser[] Lasers;
        public readonly double MicronsPerPixelX;
        public readonly double MicronsPerPixelY;
        public readonly double MicronsPerPixelZ;
        public readonly double OpticalZoom;
        public readonly double PmtGain1;
        public readonly double PmtGain2;
        public readonly int RastersPerFrame;
        public readonly ZDevice ZDevice;
        public readonly XDocument XmlDoc;

        public PVState(string path)
        {
            XmlFilePath = GetXmlFilePath(path);
            FolderPath = Path.GetDirectoryName(XmlFilePath);
            XmlDoc = XDocument.Parse(File.ReadAllText(XmlFilePath));
            PrairieViewVersion = GetVersion(XmlDoc);
            Started = DateTime.Parse(XmlDoc.Element("PVScan").Attribute("date").Value);
            ScanMode = GetActiveMode(XmlDoc);
            BitDepth = GetBitDepth(XmlDoc);
            DwellTime = GetDwellTime(XmlDoc);
            FramePeriod = GetFramePeriod(XmlDoc);
            Lasers = GetLasers(XmlDoc);
            (MicronsPerPixelX, MicronsPerPixelY, MicronsPerPixelZ) = GetMicronsPerPixel(XmlDoc);
            OpticalZoom = GetOpticalZoom(XmlDoc);
            (PmtGain1, PmtGain2) = GetPmtGains(XmlDoc);
            RastersPerFrame = GetRastersPerFrame(XmlDoc);
            ZDevice = GetZDevice(XmlDoc);
        }

        public string GetSummary()
        {
            StringBuilder sb = new();
            sb.AppendLine($"Scan folder path: {FolderPath}");
            sb.AppendLine($"Xml file path: {XmlFilePath}");
            sb.AppendLine($"Prairie View version: {PrairieViewVersion}");
            sb.AppendLine($"Scan started: {Started}");
            sb.AppendLine($"Scan mode: {ScanMode}");
            sb.AppendLine($"Bit depth: {BitDepth}");
            sb.AppendLine($"Dwell time: {DwellTime}");
            sb.AppendLine($"Frame period: {FramePeriod}");
            foreach (var laser in Lasers)
                sb.AppendLine(laser.ToString());
            sb.AppendLine($"Microns per pixel X: {MicronsPerPixelX}");
            sb.AppendLine($"Microns per pixel Y: {MicronsPerPixelY}");
            sb.AppendLine($"Microns per pixel Z: {MicronsPerPixelZ}");
            sb.AppendLine($"Optical zoom: {OpticalZoom}");
            sb.AppendLine($"PMT1 (R) gain: {PmtGain1}");
            sb.AppendLine($"PMT2 (G) gain: {PmtGain2}");
            sb.AppendLine($"Frame averaging: {RastersPerFrame}");
            sb.AppendLine($"Z device: {ZDevice}");
            return sb.ToString();
        }

        private static string GetXmlFilePath(string fileOrFolderPath)
        {
            if (fileOrFolderPath.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!File.Exists(fileOrFolderPath))
                    throw new InvalidOperationException($"file does not exist: {fileOrFolderPath}");
                return Path.GetFullPath(fileOrFolderPath);
            }
            else
            {
                string[] xmlFilePaths = Directory.GetFiles(fileOrFolderPath, "*.xml");
                if (xmlFilePaths.Length != 1)
                    throw new InvalidOperationException($"folder must have 1 XML file: {fileOrFolderPath}");
                return Path.GetFullPath(xmlFilePaths[0]);
            }
        }

        private static string GetVersion(XDocument xmlDoc)
        {
            string version = xmlDoc.Element("PVScan").Attribute("version").Value;
            string[] supportedVersions = { "5.4.64.500", "5.5.64.500" };
            if (!supportedVersions.Contains(version))
                throw new NotImplementedException($"unsupported PV version: {version}");
            return version;
        }

        private static string GetPVStateValue(XDocument xmlDoc, string key)
        {
            var sharedStateElement = xmlDoc.XPathSelectElement("/PVScan/PVStateShard");
            foreach (var stateValueElement in sharedStateElement.Elements())
            {
                if (stateValueElement.Attribute("key").Value == key)
                {
                    string value = stateValueElement.Attribute("value").Value;
                    if (value is null)
                        throw new InvalidOperationException($"key does not have single value: {key}");
                    return value;
                }
            }
            throw new InvalidOperationException($"key not found: {key}");
        }

        private static IndexedValue[] GetPVStateIndexedValues(XDocument xmlDoc, string key)
        {
            List<IndexedValue> values = new();

            var sharedStateElement = xmlDoc.XPathSelectElement("/PVScan/PVStateShard");
            foreach (var stateValueElement in sharedStateElement.Elements())
            {
                if (stateValueElement.Attribute("key").Value == key)
                {
                    foreach (var indexedValueElement in stateValueElement.Elements())
                    {
                        string index = indexedValueElement.Attribute("index").Value;
                        string value = indexedValueElement.Attribute("value").Value;
                        string description = indexedValueElement.Attribute("description")?.Value;
                        values.Add(new IndexedValue(index, value, description));
                    }
                }
            }

            if (values.Count == 0)
                throw new InvalidOperationException($"no indexed values found for key: {key}");

            return values.ToArray();
        }

        private static ScanMode GetActiveMode(XDocument xmlDoc) =>
            GetPVStateValue(xmlDoc, "activeMode") switch
            {
                "Galvo" => ScanMode.GalvoGalvo,
                "ResonantGalvo" => ScanMode.ResonantGalvo,
                _ => throw new NotImplementedException("unsupported activeMode")
            };

        private static int GetBitDepth(XDocument xmlDoc) =>
            int.Parse(GetPVStateValue(xmlDoc, "bitDepth"));

        private static double GetDwellTime(XDocument xmlDoc) =>
            double.Parse(GetPVStateValue(xmlDoc, "dwellTime"));

        private static double GetFramePeriod(XDocument xmlDoc) =>
            double.Parse(GetPVStateValue(xmlDoc, "framePeriod"));

        public static Laser[] GetLasers(XDocument xmlDoc) =>
            GetPVStateIndexedValues(xmlDoc, "laserPower")
                .Select(x => new Laser(int.Parse(x.Index), x.Description, double.Parse(x.Value)))
                .ToArray();

        public static (double x, double y, double z) GetMicronsPerPixel(XDocument xmlDoc)
        {
            double x = double.NaN;
            double y = double.NaN;
            double z = double.NaN;
            foreach (IndexedValue val in GetPVStateIndexedValues(xmlDoc, "micronsPerPixel"))
            {
                if (val.Index == "XAxis")
                    x = double.Parse(val.Value);
                else if (val.Index == "YAxis")
                    y = double.Parse(val.Value);
                else if (val.Index == "ZAxis")
                    z = double.Parse(val.Value);
            }
            return (x, y, z);
        }

        private static double GetOpticalZoom(XDocument xmlDoc) =>
            double.Parse(GetPVStateValue(xmlDoc, "opticalZoom"));

        private static (double gain1, double gain2) GetPmtGains(XDocument xmlDoc)
        {
            IndexedValue[] values = GetPVStateIndexedValues(xmlDoc, "pmtGain");
            return (double.Parse(values[0].Value), double.Parse(values[1].Value));
        }

        private static int GetRastersPerFrame(XDocument xmlDoc) =>
            int.Parse(GetPVStateValue(xmlDoc, "rastersPerFrame"));

        private static int GetZDeviceIndex(XDocument xmlDoc) =>
            int.Parse(GetPVStateValue(xmlDoc, "zDevice"));

        private static ZDevice GetZDevice(XDocument xmlDoc) =>
            GetZDeviceIndex(xmlDoc) switch
            {
                0 => ZDevice.Motor,
                1 => ZDevice.Piezo,
                _ => ZDevice.Unknown,
            };
    }
}