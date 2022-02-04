namespace PVInfo
{
    class Program
    {
        static void Main()
        {
            string folderOfScans = @"X:\Data\OT-Cre\OT-Tom-F5-NE\2022-01-03-practice\2p";
            Video.CreateMultiFolderLinescanVideos(folderOfScans, true);
            Report.CreateMultiFolderIndex(folderOfScans);
        }
    }
}