using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Sniffer
{
    class Program
    {
        static void Main(string[] args)
        {
            string basePath = @"X:\Data Analysis\_Core Projects\Sniffer PVN";
            string yamlFilePath = Path.Combine(basePath, "experiments.yaml");
            string htmlOutputPath = Path.Combine(basePath, "report.html");

            List<Experiment> experiments = new();
            Experiment exp = null;

            foreach (string line in File.ReadLines(yamlFilePath))
            {
                if (line.StartsWith("#"))
                    continue;

                if (line.StartsWith("  \"") && line.Contains(":"))
                {
                    string id = line.Split("\"")[1];
                    exp = new Experiment(id);
                    experiments.Add(exp);
                    continue;
                }

                if (line.StartsWith("    ") && line.Contains(":"))
                {
                    string[] parts = line.Split(":", 2);
                    string key = parts[0].Trim();
                    string val = parts[1].Trim().Trim('\'');
                    exp.Add(key, val);
                }
            }

            StringBuilder sb = new();
            foreach (Experiment ex in experiments)
                sb.AppendLine(ex.GetHtml());
            SavePage(sb.ToString(), htmlOutputPath);
        }

        private static void SavePage(string html, string outFile)
        {
            string template = @"<!doctype html>
<html lang='en'>
  <head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.1.1/dist/css/bootstrap.min.css' rel='stylesheet' integrity='sha384-F3w7mX95PdgyTmZZMECAngseQB83DfGTowi0iMjiWaeVhAn4FJkqJByhZMI3AhiU' crossorigin='anonymous'>
    <title>Sniffer Report</title>
  </head>
  <body>
    <div class='container'>
    <h1>Sniffer Experiments (K-Glu)</h1>
    {{CONTENT}}
    </div>
    <script src='https://cdn.jsdelivr.net/npm/bootstrap@5.1.1/dist/js/bootstrap.bundle.min.js' integrity='sha384-/bQdsTh/da6pkI1MST/rWKFNjaCP5gBSY4sEBT38Q/9RBh9AH40zEOg7Hlq2THRZ' crossorigin='anonymous'></script>
  </body>
</html>";
            File.WriteAllText(outFile, template.Replace("{{CONTENT}}", html));
            Console.WriteLine($"Wrote: {outFile}");
        }
    }
}