using System;
using System.IO;

namespace PVInfo
{
    class Program
    {
        static void Main()
        {
            //Plot.LinescanCurves(@"X:\Data\OT-Cre\OT-Tom-uncaging\2022-02-23-ap5\2p\LineScan-02232022-1106-357");
            GenerateReport(@"X:/Data/OT-Cre/OT-Tom-uncaging/2022-02-24-acsf-mg/2p");
            //LaunchPointScanGraph(@"X:\Data\SD\2p technique development\problem\2022-02-18 Mira vs X3\PointScan-02182022-1204-008\PointScan-02182022-1204-008_Cycle00001.csv");
        }

        static void GenerateReport(string folderOfScans)
        {
            ReferenceTif.ConvertMultiFolder(folderOfScans);
            Video.CreateMultiFolderLinescanVideos(folderOfScans);
            Plot.CreateMultiFolderLinescanCurves(folderOfScans);
            Plot.TSeriesIntensity(folderOfScans);
            Report.CreateMultiFolderIndex(folderOfScans);
        }
    }
}