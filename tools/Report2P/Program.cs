using Report2P.Experiment;
using Report2P.PvXml;

namespace Report2P;

public static class Program
{
    public static void Main()
    {
        TimelinePage.MakeIndex(@"X:/Data/OT-Cre/OT-Tom-uncaging/2022-02-23-ap5/2p");
        TimelinePage.MakeIndex(@"X:\Data\OT-Cre\OT-Tom-uncaging\2022-02-27-NMDA\2p\");
        TimelinePage.MakeIndex(@"X:\Data\C57\FOS-TRAP\nodose-injection\gcamp\2P");
    }
}