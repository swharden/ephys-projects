using System;
using System.Linq;
using System.IO;
using NUnit.Framework;
using BitMiracle.LibTiff.Classic;

namespace SciTIF.Tests
{
    public class DataTests
    {
        private string DataFolder => Path.GetFullPath("../../../../../data/tifs/");

        [TestCase("01.tif", 1392, 1040, 32, new int[] { 1932, 1903, 1910 })]
        [TestCase("02.tif", 1392, 1040, 32, new int[] { 384, 382, 390 })]
        [TestCase("03.tif", 256, 256, 16, new int[] { 297, 285, 633 })]
        //[TestCase("04.tif", 512, 512, 32, new int[] { 297, 285, 633 })] // RGB
        [TestCase("05.tif", 512, 512, 16, new int[] { 1053, 1120, 954 })]
        public void Test_PixelValues_FirstRow(string filename, int width, int height, int bitsPerPixel, int[] values)
        {
            string filePath = Path.Combine(DataFolder, filename);
            
            SciTIF.Image image = new(filePath);
            Assert.AreEqual(width, image.Width);
            Assert.AreEqual(height, image.Height);
            Assert.AreEqual(bitsPerPixel, image.BitsPerPixel);

            for (int i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(values[i], image.Values[0, i], $"failed at pixel index {i}");
            }
        }
    }
}