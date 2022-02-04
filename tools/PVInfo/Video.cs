using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Extend;
using FFMpegCore.Pipes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVInfo
{
    internal class Video
    {
        public static void CreateMultiFolderLinescanVideos(string folderPath, bool overwrite = false)
        {
            foreach (var pvFolderPath in Directory.GetDirectories(folderPath))
            {
                PVScan.IScan scan = PVScan.ScanFactory.FromPVFolder(pvFolderPath);
                if (scan is PVScan.TSeries)
                {
                    PVScan.TSeries scan2 = (PVScan.TSeries)scan;

                    Console.WriteLine();
                    Console.WriteLine("Generating MP4 video from T-Series:");
                    Console.WriteLine(scan.PVState.FolderPath);
                    CreateLinescanMP4(scan.PVState.FolderPath, 1, scan2.FrameTimes, overwrite);
                    CreateLinescanMP4(scan.PVState.FolderPath, 2, scan2.FrameTimes, overwrite);
                }
            }
        }

        public static void CreateLinescanMP4(string linescanFolder, int channel, double[] frameTimes, bool overwrite = false)
        {
            if (!(channel == 1 || channel == 2))
                throw new ArgumentException("only channels 1 and 2 are supported");

            string filePathOut = Path.Combine(linescanFolder, $"References/ch{channel}.mp4");
            if (File.Exists(filePathOut) && overwrite == false)
                return;

            string[] filePaths = Directory.GetFiles(linescanFolder, $"*Ch{channel}*.tif");

            SciTIF.Image firstImage = new(filePaths[0]);

            var frames = CreateFrames(filePaths, frameTimes);
            RawVideoPipeSource videoFramesSource = new(frames) { FrameRate = 1.0 / frameTimes[1] };

            _ = FFMpegArguments
                .FromPipeInput(videoFramesSource)
                .OutputToFile(filePathOut)
                .ProcessSynchronously();
        }

        static IEnumerable<BitmapVideoFrameWrapper> CreateFrames(string[] filePaths, double[] frameTimes)
        {
            for (int i = 0; i < filePaths.Length; i++)
            {
                Console.CursorLeft = 0;
                Console.Write($"Encoding {i + 1} of {filePaths.Length} ...");
                SciTIF.Image img = new(filePaths[i]);
                img.Divide(1 << 5);
                Bitmap bmp = img.GetBitmapRGB();
                Graphics gfx = Graphics.FromImage(bmp);
                Font fnt = new("consolas", 12);
                gfx.DrawString($"Frame: {i + 1:N0}\nTime: {frameTimes[i]:N3}", fnt, Brushes.Yellow, 2, 2);
                BitmapVideoFrameWrapper bmp2 = new(bmp);
                yield return bmp2;
            }
            Console.WriteLine();
        }
    }
}
