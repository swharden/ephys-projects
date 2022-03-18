﻿namespace Report2P.Experiment;

public interface IExperiment
{
    string Path { get; }
    string AutoanalysisFolder { get; }
    string Details { get; }
    ImageGroup ImageGroups { get; }
    DateTime DateTime { get; }
    public void Analyze(bool clear = false);
}