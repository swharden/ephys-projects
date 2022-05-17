namespace Report2P.Experiment;

internal static class ExperimentFactory
{
    public static IExperiment? GetExperiment(string folder2p)
    {
        PvXml.IScan? scan = PvXml.ScanFactory.FromPVFolder(folder2p);

        if (scan is null)
            return null;

        if (scan is PvXml.ScanTypes.LineScan)
            return new Linescan(folder2p);

        if (scan is PvXml.ScanTypes.SingleImage)
            return new SingleImage(folder2p);

        if (scan is PvXml.ScanTypes.MarkPoints)
            return new MarkPoints(folder2p);

        if (scan is PvXml.ScanTypes.ZSeries)
            return new ZSeries(folder2p);

        if (scan is PvXml.ScanTypes.TSeries)
            return new TImageSeries(folder2p);

        if (scan is PvXml.ScanTypes.TZSeries)
            return new TStackSeries(folder2p);

        return null;
    }
}
