using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SciTIF.Tests
{
    class AutoConvert
    {
        string DATA_FOLDER => Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../../data/tifs/"));
        string OUTPUT_FOLDER => Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../../data/tifs/output"));

        [Test]
        public void Test_AutoConvert_AllSampleImages()
        {
            foreach (string tifPath in Directory.GetFiles(DATA_FOLDER, "*.tif"))
            {
                Console.WriteLine(tifPath);
                string outputFilename = Path.GetFileNameWithoutExtension(tifPath) + ".png";
                string outputPath = Path.Combine(OUTPUT_FOLDER, outputFilename);
                SciTIF.Convert.FileToPng(tifPath, outputPath, autoScale: true);
            }
        }
    }
}
