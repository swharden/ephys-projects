using System;

namespace SciTIF
{
    class Program
    {
        static void Main(string[] args)
        {
            var img = new Image("../../data/tifs/05.tif");
            Console.WriteLine(img);

            string saveFilePath = System.IO.Path.GetFullPath("test.bmp");
            img.AutoScale();
            img.SaveBmp(saveFilePath);
            Console.WriteLine(saveFilePath);
        }
    }
}