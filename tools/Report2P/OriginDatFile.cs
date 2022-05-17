using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P
{
    public static class OriginDatFile
    {
        public static void Write(double[] xs, double[] red, double[] green, string filePath, string xUnit = "sec")
        {
            if (xs.Length < 2)
                return;

            Log.Debug($"Creating 2-channel intensity data as origin compatible file: {Path.GetFileName(filePath)}");

            double fps = 1.0 / xs[1];

            StringBuilder sb = new();
            sb.AppendLine("Time\tRed\tGreen");
            sb.AppendLine($"{xUnit}\tAFU\tAFU");
            sb.AppendLine($"X\timage mean\timage mean");
            for (int i = 0; i < xs.Length; i++)
            {
                double r = i < red.Length ? red[i] : 0;
                double g = i < green.Length ? green[i] : 0;
                sb.AppendLine($"{xs[i]}\t{r}\t{g}");
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
