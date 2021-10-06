using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace SciTIF
{
    class Program
    {
        static void Main(string[] args)
        {
            string folderPath = @"C:\Users\swharden\Documents\GitHub\ephys-projects\tools\SciTIF\data\tifs";
            Convert.FolderToPng(folderPath, Path.Combine(folderPath, "output"));
        }
    }
}