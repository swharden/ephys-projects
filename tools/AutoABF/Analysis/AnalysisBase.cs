using System;
using System.IO;

namespace AutoABF.Analysis
{
    public abstract class AnalysisBase
    {
        protected double[] ToDouble(float[] input)
        {
            double[] output = new double[input.Length];
            for (int i = 0; i < input.Length; i++)
                output[i] = input[i];
            return output;
        }
    }
}