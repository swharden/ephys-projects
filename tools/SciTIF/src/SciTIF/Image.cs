using System;
using BitMiracle.LibTiff.Classic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

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
        public readonly int SamplesPerPixel;
        public readonly int BitsPerSample;
        public readonly string ColorFormat;
        public readonly string SampleFormat;

        public Image(string tifFilePath)
        {
            tifFilePath = System.IO.Path.GetFullPath(tifFilePath);
            if (!System.IO.File.Exists(tifFilePath))
                throw new ArgumentException($"file does not exist: {tifFilePath}");

            Tiff.SetErrorHandler(new SilentHandler());
            using Tiff tif = Tiff.Open(tifFilePath, "r");

            SamplesPerPixel = 1;
            if (tif.GetField(TiffTag.SAMPLESPERPIXEL) != null)
                SamplesPerPixel = tif.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt();

            BitsPerSample = tif.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
            SampleFormat = tif.GetFieldDefaulted(TiffTag.SAMPLEFORMAT)[0].ToString();
            ColorFormat = tif.GetField(TiffTag.PHOTOMETRIC)[0].ToString();

            if (ColorFormat == "RGB")
            {
                Values = SamplesPerPixel switch
                {
                    4 => ReadPixels_ARGB_AVG(tif),
                    3 => ReadPixels_RGB_AVG(tif),
                    _ => throw new InvalidOperationException($"unsupported samples per pixel: {SamplesPerPixel}")
                };
            }
            else if (ColorFormat == "MINISBLACK")
            {
                Values = BitsPerSample switch
                {
                    32 => ReadPixels_Float32(tif),
                    16 => ReadPixels_Int16(tif),
                    8 => ReadPixels_Int8(tif),
                    _ => throw new NotImplementedException($"unsupported TIF depth: {BitsPerSample}-bit"),
                };
            }
            else
            {
                throw new InvalidOperationException($"unsupported color format: {ColorFormat}");
            }
        }

        public Image(double[,] values)
        {
            Values = values;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new();
            sb.AppendLine($"Size: {Width}x{Height}");
            sb.AppendLine($"SamplesPerPixel: {SamplesPerPixel}");
            sb.AppendLine($"BitsPerSample: {BitsPerSample}");
            sb.AppendLine($"ColorFormat: {ColorFormat}");
            sb.AppendLine($"SampleFormat: {SampleFormat}");
            return sb.ToString();
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

        public (double min, double max) GetPercentiles(double minPercentile, double maxPercentile)
        {
            int i = 0;
            double[] values = new double[Width * Height];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    values[i++] = Values[y, x];
                }
            }
            Array.Sort(values);

            double minFrac = minPercentile / 100;
            double maxFrac = maxPercentile / 100;
            int minIndex = (int)(values.Length * minFrac);
            int maxIndex = (int)(values.Length * maxFrac);

            return (values[minIndex], values[maxIndex]);
        }

        public void Divide(double factor)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Values[y, x] = Values[y, x] / factor;
        }

        public void AutoScale(double percentileLow = 0, double percentileHigh = 100)
        {
            double newMax = 255;
            double min;
            double max;

            if (percentileLow == 0 && percentileHigh == 100)
            {
                (min, max) = GetMinMax();
            }
            else
            {
                (min, max) = GetPercentiles(percentileLow, percentileHigh);
            }

            double scale = newMax / (max - min);

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Values[y, x] = (Values[y, x] - min) * scale;
        }

        public double GetMean()
        {
            double sum = 0;

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    sum += Values[y, x];

            return sum / (Width * Height);
        }

        private static double[,] ReadPixels_ARGB_AVG(Tiff image)
        {
            int width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            double[,] pixelValues = new double[height, width];

            int[] raster = new int[height * width];
            image.ReadRGBAImage(width, height, raster, true);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * width + x;
                    int a = Tiff.GetA(raster[offset]);
                    int r = Tiff.GetR(raster[offset]);
                    int g = Tiff.GetG(raster[offset]);
                    int b = Tiff.GetB(raster[offset]);
                    pixelValues[y, x] = (r + g + b) / 3;
                }
            }

            return pixelValues;
        }

        private static double[,] ReadPixels_RGB_AVG(Tiff image)
        {
            int width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            double[,] pixelValues = new double[height, width];

            int[] raster = new int[height * width];
            image.ReadRGBAImage(width, height, raster, true);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * width + x;
                    int r = Tiff.GetR(raster[offset]);
                    int g = Tiff.GetG(raster[offset]);
                    int b = Tiff.GetB(raster[offset]);
                    pixelValues[y, x] = (r + g + b) / 3;
                }
            }

            return pixelValues;
        }

        private static double[,] ReadPixels_Float32(Tiff image)
        {
            const int bytesPerPixel = 4;

            int width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            double[,] pixelValues = new double[height, width];

            byte[] lineBytes = new byte[image.ScanlineSize()];
            byte[] pixelBytes = new byte[bytesPerPixel];
            for (int y = 0; y < height; y++)
            {
                image.ReadScanline(lineBytes, y);
                for (int x = 0; x < width; x++)
                {
                    Array.Copy(lineBytes, x * bytesPerPixel, pixelBytes, 0, bytesPerPixel);
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

        private static double[,] ReadPixels_Int8(Tiff image)
        {
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
                    int offset = (y * width + x);
                    pixelValues[y, x] = bytes[offset];
                }
            }

            return pixelValues;
        }

        public void SaveBmp(string path)
        {
            using Bitmap bmp = GetBitmap(Values);
            bmp.Save(path);
        }

        public void SavePng(string path)
        {
            using Bitmap bmp = GetBitmap(Values);
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
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

        public Bitmap GetBitmapIndexed() => GetBitmap(Values);

        public Bitmap GetBitmapRGB()
        {
            Bitmap bmpIndexed = GetBitmapIndexed();
            Bitmap bmp = new(bmpIndexed.Width, bmpIndexed.Height, PixelFormat.Format32bppRgb);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.DrawImage(bmpIndexed, 0, 0);
            return bmp;
        }

        public static Image FromTif(string tifFilePath) => new Image(tifFilePath);

        public static Bitmap GetBitmap(double[,] values)
        {
            // create and fill a pixel array for the 8-bit final image
            int width = values.GetLength(1);
            int height = values.GetLength(0);

            // Image bytes in memory always assume the width is a multiple of 4.
            // This "width in memory" is called stride width.
            int strideMultiple = 4;
            int strideOverhang = width % strideMultiple;
            int stridePadNeeded = strideMultiple - strideOverhang;
            int strideWidth = width + stridePadNeeded;
            byte[] pixelsOutput = new byte[strideWidth * height];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pixelsOutput[y * strideWidth + x] = Clamp(values[y, x]);

            // create the output bitmap (8-bit indexed color)
            Bitmap bmp = new(strideWidth, height, PixelFormat.Format8bppIndexed);
            Rectangle rect = new(0, 0, strideWidth, height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            Marshal.Copy(pixelsOutput, 0, bmpData.Scan0, pixelsOutput.Length);
            bmp.UnlockBits(bmpData);

            // Create a grayscale palette, although other colors and LUTs could go here
            ColorPalette pal = bmp.Palette;
            for (int i = 0; i < 256; i++)
                pal.Entries[i] = Color.FromArgb(255, i, i, i);
            bmp.Palette = pal;

            // Return RGB image
            Bitmap bmpFinal = new(width, height, PixelFormat.Format32bppPArgb);
            Graphics gfx = Graphics.FromImage(bmpFinal);
            gfx.DrawImage(bmp, 0, 0);
            return bmpFinal;
        }
    }
}