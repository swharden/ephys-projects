using System;
using System.IO;

namespace PVInfo
{
    class Program
    {
        static void Main()
        {
            GenerateReport(@"X:\Data\C57\FOS-TRAP\nodose-injection\gcamp\2P");
        }

        static void GenerateReport(string folderOfScans)
        {
            /*
            ReferenceTif.ConvertMultiFolder(folderOfScans);
            Video.CreateMultiFolderLinescanVideos(folderOfScans);
            Plot.CreateMultiFolderLinescanCurves(folderOfScans);
            Plot.TSeriesIntensity(folderOfScans);
            */
            Report.CreateMultiFolderIndex(folderOfScans);
        }
    }
}