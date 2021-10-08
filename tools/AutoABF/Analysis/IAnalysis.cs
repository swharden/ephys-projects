namespace AutoABF.Analysis
{
    public interface IAnalysis
    {
        void Analyze(AbfSharp.ABF abf, string outputFolder);
    }
}