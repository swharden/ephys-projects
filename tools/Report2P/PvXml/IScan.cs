namespace Report2P.PvXml;

public interface IScan
{
    ScanType ScanType { get; }
    PVState PVState { get; }
    string GetSummary();
}