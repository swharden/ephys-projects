using System.Text;

namespace Report2P
{
    public static class OriginDatFile
    {
        public static void SaveXRG(double[] xs, double[] red, double[] green, string filePath, string xUnit = "sec")
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

        public static void SaveXY(double[] xs, double[] ys, string filePath)
        {
            Log.Debug($"Creating XY DAT file: {Path.GetFileName(filePath)}");

            string xLabel = string.Empty;
            string yLabel = string.Empty;

            string xUnit = string.Empty;
            string yUnit = string.Empty;

            string xComment = string.Empty;
            string yComment = string.Empty;

            StringBuilder sb = new();
            sb.AppendLine($"{xLabel}\t{yLabel}");
            sb.AppendLine($"{xUnit}\t{yUnit}");
            sb.AppendLine($"{xComment}\t{yComment}");

            for (int i = 0; i < xs.Length; i++)
                sb.AppendLine($"{xs[i]}\t{ys[i]}");

            File.WriteAllText(filePath, sb.ToString());
        }

        public static void SaveY(double[] ys, string filePath)
        {
            double[] xs = Enumerable.Range(0, ys.Length).Select(x => (double)x).ToArray();
            SaveXY(xs, ys, filePath);
        }

        public static void SaveYs(List<double[]> yColumnData, string filePath)
        {
            Log.Debug($"Creating X(Y)n DAT file: {Path.GetFileName(filePath)}");

            double[,] data = new double[yColumnData.Count(), yColumnData.Select(x => x.Length).Max()];

            for (int i = 0; i < yColumnData.Count; i++)
            {
                double[] ys = yColumnData[i];
                for (int j = 0; j < ys.Length; j++)
                {
                    data[i, j] = ys[j];
                }
            }

            File.WriteAllText(filePath, MatrixToTSV(data));
            File.WriteAllText(filePath + "-rotated.dat", MatrixToTSV(Rotate(data)));
        }

        private static double[,] Rotate(double[,] input)
        {
            double[,] output = new double[input.GetLength(1), input.GetLength(0)];

            for (int y = 0; y < input.GetLength(0); y++)
            {
                for (int x = 0; x < input.GetLength(1); x++)
                {
                    output[x, y] = input[y, x];
                }
            }

            return output;
        }

        private static string MatrixToTSV(double[,] data)
        {
            StringBuilder sb = new();
            for (int y = 0; y < data.GetLength(0); y++)
            {
                sb.Append($"{y}\t");
                for (int x = 0; x < data.GetLength(1); x++)
                {
                    sb.Append(data[y, x]);
                    sb.Append("\t");
                }
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
