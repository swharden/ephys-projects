namespace Report2P.Experiment;

internal static class ExperimentFactory
{
    public static IExperiment GetExperiment(string folder2p)
    {
        string folderName = Path.GetFileName(folder2p);

        if (folderName.StartsWith("LineScan-"))
            return new Linescan(folder2p);

        if (folderName.StartsWith("SingleImage-"))
            return new SingleImage(folder2p);

        if (folderName.StartsWith("MarkPoints-"))
            return new MarkPoints(folder2p);

        if (folderName.StartsWith("ZSeries-"))
            return new ZSeries(folder2p);

        if (folderName.StartsWith("TSeries-"))
            return new TSeries(folder2p);

        throw new NotImplementedException($"unsupported experiment folder: {folder2p}");
    }
}
