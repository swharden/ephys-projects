namespace PVInfo.PVScan
{
    public interface IScan
    {
        ScanType ScanType { get; }
        PVState PVState { get; }
        string GetSummary();
    }
}