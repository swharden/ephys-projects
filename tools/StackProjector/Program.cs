// This program creates maximum intensity projections for ZSeries folders
// created by PrairieView and saves the output as a PNG file in the References subfolder.

string folder = @"X:\Data\zProjects\Oxytocin Biosensor\experiments\patch clamp stimulation\2022-09-30 rat\MCN3";
ProjectAll(folder);

void ProjectAll(string folder)
{
    string[] folderPaths = Directory.GetDirectories(folder, "ZSeries-*");
    foreach(string folderPath in folderPaths)
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

    string saveFolder = Path.Combine(folder, "References");
    string saveAs = Path.Combine(saveFolder, $"projection_Ch{channel}.png");
    Console.WriteLine(saveAs);
    projection.Save(saveAs);
}