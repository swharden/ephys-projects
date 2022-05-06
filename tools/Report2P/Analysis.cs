﻿using Report2P.Experiment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P
{
    internal class Analysis
    {
        public static void AnalyzeAllSubfolders(string folderPath)
        {
            foreach (string subfolder in Directory.GetDirectories(folderPath))
                AnalyzeFolder(subfolder);
        }

        public static void AnalyzeFolder(string folder)
        {
            Log.Info($"\nAnalyzing: {folder}");
            IExperiment? experiment = ExperimentFactory.GetExperiment(folder);
            if (experiment is null)
            {
                Log.Warn($"WARNING: unsupported experiment folder: {folder}");
            }
            else
            {
                Log.Debug($"Analyzing folder as {experiment}");
                experiment.Analyze();
            }
        }
    }
}
