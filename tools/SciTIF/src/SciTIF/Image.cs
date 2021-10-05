using System;
using BitMiracle.LibTiff.Classic;
using System.Drawing;

namespace SciTIF
{
    class SilentHandler : TiffErrorHandler
    {
        public override void WarningHandler(Tiff tif, string method, string format, params object[] args) { }
        public override void ErrorHandler(Tiff tif, string method, string format, params object[] args) { }
    }

    public class Image
    {
        public int Width => Values.GetLength(1);
        public int Height => Values.GetLength(0);
        public readonly double[,] Values;
        public readonly int BitsPerPixel;

        public Image(string tifFilePath)
        {
            tifFilePath = System.IO.Path.GetFullPath(tifFilePath);
            if (!System.IO.File.Exists(tifFilePath))
                throw new ArgumentException($"file does not exist: {tifFilePath}");

            Tiff.SetErrorHandler(new SilentHandler());
            using Tiff tif = Tiff.Open(tifFilePath, "r");
            BitsPerPixel = tif.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
            switch (BitsPerPixel)
            {
                case 32:
                    Values = ReadPixels_Float32(tif);
                    break;
                case 16:
                    Values = ReadPixels_Int16(tif);
                    break;
                default:
                    throw new NotImplementedException($"unsupported TIF depth: {BitsPerPixel}-bit");
            }
        }

        public Image(double[,] values)
        {
            Values = values;
        }

        public override string ToString()
        {
            return $"Image {Width}x{Height}";
        }

        public (double min, double max) GetMinMax()
        {
            double max = double.NegativeInfinity;
            double min = double.PositiveInfinity;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    max = Math.Max(max, Values[y, x]);
                    min = Math.Min(min, Values[y, x]);
                }
            }

            return (min, max);
        }

        public void AutoScale(double newMax = 255)
        {
            (double min, double max) = GetMinMax();

            double scale = newMax / (max - min);

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Values[y, x] = (Values[y, x] - min) * scale;
        }

        private static double[,] ReadPixels_Float32(Tiff image)
        {
            const int bytesPerPixel = 4;

            int width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            double[,] pixelValues = new double[height, width];

            byte[] stripBytes = new byte[image.StripSize()];
            byte[] pixelBytes = new byte[bytesPerPixel];
            for (int y = 0; y < pixelValues.GetLength(0); y++)
            {
                image.ReadRawStrip(y, stripBytes, 0, stripBytes.Length);
                for (int x = 0; x < pixelValues.GetLength(1); x++)
                {
                    Array.Copy(stripBytes, x * bytesPerPixel, pixelBytes, 0, bytesPerPixel);
                    Array.Reverse(pixelBytes);
                    pixelValues[y, x] = BitConverter.ToSingle(pixelBytes, 0);
                }
            }

            return pixelValues;
        }

        private static double[,] ReadPixels_Int16(Tiff image)
        {
            const int bytesPerPixel = 2;

            int width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            double[,] pixelValues = new double[height, width];

            int numberOfStrips = image.NumberOfStrips();
            int stripSize = image.StripSize();

            byte[] bytes = new byte[numberOfStrips * stripSize];
            for (int i = 0; i < numberOfStrips; ++i)
            {
                image.ReadRawStrip(i, bytes, i * stripSize, stripSize);
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = (y * width + x) * bytesPerPixel;
                    pixelValues[y, x] = 0;
                    pixelValues[y, x] += bytes[offset];
                    pixelValues[y, x] += bytes[offset + 1] << 8;
                }
            }

            return pixelValues;
        }

        public void SaveBmp(string path)
        {
            using Bitmap bmp = GetBitmap(Values);
            bmp.Save(path);
        }

        private static byte Clamp(double x, byte min = 0, byte max = 255)
        {
            if (x < min)
                return min;
            else if (x > max)
                return max;
            else
                return (byte)x;
        }

        public static Bitmap GetBitmap(double[,] values)
        {
            // create and fill a pixel array for the 8-bit final image
            int width = values.GetLength(1);
            int height = values.GetLength(0);
            int pixelCount = width * height;

            // TODO: proper stride (width must be multiple of 4)
            byte[] pixelsOutput = new byte[pixelCount];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixelsOutput[y * width + x] = Clamp(values[y, x]);
                }
            }

            // create the output bitmap (8-bit indexed color)
            var formatOutput = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            Bitmap bmp = new Bitmap(width, height, formatOutput);

            // Create a grayscale palette, although other colors and LUTs could go here
            System.Drawing.Imaging.ColorPalette pal = bmp.Palette;
            for (int i = 0; i < 256; i++)
                pal.Entries[i] = System.Drawing.Color.FromArgb(255, i, i, i);
            bmp.Palette = pal;

            // copy the new pixel data into the data of our output bitmap
            var rect = new Rectangle(0, 0, width, height);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, formatOutput);
            System.Runtime.InteropServices.Marshal.Copy(pixelsOutput, 0, bmpData.Scan0, pixelsOutput.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }
    }
}