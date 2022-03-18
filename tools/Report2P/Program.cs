using Report2P.Experiment;
using Report2P.PvXml;

namespace Report2P;

public static class Program
{
    public static void Main()
    {
        string folderOf2pFolders = @"X:/Data/OT-Cre/OT-Tom-uncaging/2022-02-23-ap5/2p";
        //string folderOf2pFolders = @"X:\Data\OT-Cre\OT-Tom-uncaging\2022-02-27-NMDA\2p\";
        //string folderOf2pFolders = @"X:\Data\C57\FOS-TRAP\nodose-injection\gcamp\2P";

        foreach (string folder in Directory.GetDirectories(folderOf2pFolders))
        {
            try
            {
                IExperiment experiment = ExperimentFactory.GetExperiment(folder);
                Console.WriteLine($"Analyzing: {experiment.Path}");
                experiment.Analyze();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}