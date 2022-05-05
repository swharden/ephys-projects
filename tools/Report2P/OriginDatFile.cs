using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P
{
    public static class OriginDatFile
    {
        public static void Write(double[] xs, double[] red, double[] green, string filePath)
        {
            if (xs.Length == 0)
                return;

            double fps = 1.0 / xs[1];

            StringBuilder sb = new();
            sb.AppendLine("Time\tRed\tGreen");
            sb.AppendLine("sec\tAFU\tAFU");
            sb.AppendLine($"{fps:N3} FPS\timage mean\timage mean");
            for (int i = 0; i < xs.Length; i++)
            {
                sb.AppendLine($"{xs[i]}\t{red[i]}\t{green[i]}");
            }

            File.WriteAllText(filePath, sb.ToString());
            Console.WriteLine($"Wrote: {filePath}");
        }
    }
}
