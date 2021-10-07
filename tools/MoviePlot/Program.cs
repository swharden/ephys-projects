namespace MoviePlot
{
    class Program
    {
        static void Main(string[] args)
        {
            var vid = new RoiVideo(
                imageFolderInput: @"X:\Data\C57\GRABNE\2021-10-04-ne-washon\TSeries-10042021-1257-1853\Analysis\small-source-frames",
                csvFilePath: @"X:\Data\C57\GRABNE\2021-10-04-ne-washon\TSeries-10042021-1257-1853\Analysis\Results.csv",
                samplePeriodSeconds: 75.4936441,
                baselineMinutes1: 5,
                baselineMinutes2: 8
            );

            vid.CreateFrameImages(@"X:\Data\C57\GRABNE\2021-10-04-ne-washon\TSeries-10042021-1257-1853\Analysis\small-annotated");
        }
    }
}
