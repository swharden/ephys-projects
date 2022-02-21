using System;
using System.Linq;
using System.IO;
using NUnit.Framework;
using BitMiracle.LibTiff.Classic;

namespace SciTIF.Tests
{
    public class DataTests
    {
        string DATA_FOLDER => Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../../data/tifs/"));

        [TestCase("01.tif", 1392, 1040, 32, new int[] { 1932, 1903, 1910 })]
        [TestCase("02.tif", 1392, 1040, 32, new int[] { 384, 382, 390 })]
        [TestCase("03.tif", 256, 256, 16, new int[] { 297, 285, 633 })]
        //[TestCase("04.tif", 512, 512, 32, new int[] { 297, 285, 633 })] // RGB
        [TestCase("05.tif", 512, 512, 16, new int[] { 1053, 1120, 954 })]
        [TestCase("11.tif", 641, 500, 16, new int[] { 141, 206, 211 })]
        public void Test_Grayscale_PixelValues_FirstRow(string filename, int width, int height, int bitsPerPixel, int[] values)
        {
            string filePath = Path.Combine(DATA_FOLDER, filename);
            
            SciTIF.Image image = new(filePath);
            Assert.AreEqual(width, image.Width);
            Assert.AreEqual(height, image.Height);
            Assert.AreEqual(bitsPerPixel, image.BitsPerSample);

            for (int i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(values[i], image.Values[0, i], $"failed at pixel index {i}");
            }
        }

        [TestCase("11.tif", 27, 17, 6783)]
        public void Test_Grayscale_PixelValues_SpecifiedLocation(string filename, int x, int y, int value)
        {
            string filePath = Path.Combine(DATA_FOLDER, filename);
            SciTIF.Image image = new(filePath);
            Assert.AreEqual(value, image.Values[y, x]);
        }
    }
}