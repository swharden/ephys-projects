namespace PVInfo
{
    class Program
    {
        static void Main()
        {
            string folderOfScans = @"X:\Data\OT-Cre\OT-Tom-F5-NE\2022-01-10-isoproterenol\2p";
            //ReferenceTif.ConvertMultiFolder(folderOfScans);
            //Video.CreateMultiFolderLinescanVideos(folderOfScans);
            //Plot.PlotIntensityMultiFolder(folderOfScans);
            Report.CreateMultiFolderIndex(folderOfScans);
        }
    }
}