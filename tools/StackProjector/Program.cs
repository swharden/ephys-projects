// This program creates maximum intensity projections for ZSeries folders
// created by PrairieView and saves the output as a PNG file in the References subfolder.

if (args.Length == 1)
{
    if (Directory.Exists(args[0]))
    {
        ProjectAll(args[0]);
    }
    else
    {
        throw new DirectoryNotFoundException(args[0]);
    }
}
else
{
    Console.WriteLine("Arugment required: path to folder containing ZSeries sub-folders");
}


void ProjectAll(string folder)
{
    string[] folderPaths = Directory.GetDirectories(folder, "ZSeries-*");
    foreach (string folderPath in folderPaths)
    {
        Project(folderPath, 1);
        Project(folderPath, 2);
    }
}

void Project(string folder, int channel = 1)
{
    string[] tifPaths = Directory.GetFiles(folder, $"*_Ch{channel}_*.tif");
    Console.WriteLine($"{Path.GetFileName(folder)}: Projecting {tifPaths.Length} images...");

    SciTIF.ImageStack stack = new(tifPaths);
    SciTIF.Image projection = stack.ProjectMax();
    projection /= 16;

    string referencesFolderPath = Path.Combine(folder, "References");
	string parentFolderPath = Path.GetDirectoryName(folder) ?? string.Empty;
    string folderName = Path.GetFileName(folder);
    string saveFilename = $"{folderName}_projection_Ch{channel}.png";
    projection.Save(Path.Combine(referencesFolderPath, saveFilename));
    projection.Save(Path.Combine(parentFolderPath, saveFilename));
}