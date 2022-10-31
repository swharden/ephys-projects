using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P
{
    internal static class Imaging
    {
        public static void AutoscaleAndSave(string tifPath, string pngPath)
        {
            SciTIF.TifFile tif = new(tifPath);
            SciTIF.Image img = tif.GetImage();
            img.AutoScale();
            img.Save(pngPath);
        }

        public static void ProjectAutoscaleAndSave(string[] tifPaths, string pngPath)
        {
            SciTIF.ImageStack stack = new(tifPaths);
            SciTIF.Image projection = stack.ProjectMax();
            projection.AutoScale();
            projection.Save(pngPath);
        }
    }
}
