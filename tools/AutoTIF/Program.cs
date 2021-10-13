using System;
using System.IO;

string[] arguments = System.Environment.GetCommandLineArgs();
if (arguments.Length != 3)
    throw new ArgumentException("arguments required: inputPath, outputPath");
string pathIn = arguments[1];
string pathOut = arguments[2];

if (Directory.Exists(pathIn))
{
    Console.WriteLine("Converting a FOLDER of TIFs...");
    ConvertFolder(pathIn, pathOut);
}
else if (File.Exists(pathIn))
{
    Console.WriteLine("Converting a SINGLE TIF...");
    ConvertFile(pathIn, pathOut);
}
else
{
    throw new ArgumentException("invalid path(s)");
}

static void ConvertFolder(string sourceFolder, string destFolder)
{
    if (!Directory.Exists(destFolder))
        throw new ArgumentException("output path must be an existing folder");

    string[] sourceFilePaths = Directory.GetFiles(sourceFolder, "*.tif");
    foreach (string sourceFilePath in sourceFilePaths)
    {
        string sourceFilename = Path.GetFileName(sourceFilePath);
        string destFilename = sourceFilename + ".png";
        string destFilePath = Path.Combine(destFolder, destFilename);
        ConvertFile(sourceFilePath, destFilePath);
    }
}

static void ConvertFile(string sourceFilePath, string destFilePath)
{
    if (!sourceFilePath.EndsWith(".tif", StringComparison.InvariantCultureIgnoreCase))
        throw new ArgumentException("input file must end with .TIF");

    if (Directory.Exists(destFilePath))
        throw new ArgumentException("output path must be a file");

    Console.WriteLine($"{sourceFilePath} -> {Path.GetFileName(destFilePath)}");

    SciTIF.Convert.FileToPng(sourceFilePath, destFilePath);
}