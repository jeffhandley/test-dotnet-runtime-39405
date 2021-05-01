using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace animatorapp
{
    class Program
    {
        static DateTime StartTime;
        static string OutputFolder;
        static Bitmap GIF;
        static int FrameCount;
        static int LastAnimationFrame = 0;

        static void Main(string[] args)
        {
            if (args.Length < 2 || !File.Exists(args[0]))
            {
                throw new ArgumentException("Syntax: appanimator imageFile outputFolder [seconds]. " + string.Join(",", args));
            }

            if (args.Length > 2 && args[2] == "--debug")
            {
                System.Diagnostics.Debugger.Launch();
            }

            GIF = new(args[0]);
            FrameCount = GIF.GetFrameCount(FrameDimension.Time);

            StartTime = DateTime.Now;
            OutputFolder = Path.Combine(args[1], StartTime.ToString("yyyy-MM-dd-HH-mm"));
            Directory.CreateDirectory(OutputFolder);

            TakeSnapshot(GIF, 0, 0);

            ImageAnimator.Animate(GIF, OnAnimated);

            while (LastAnimationFrame < FrameCount)
            {
                Thread.Sleep(1000);
            }

            ImageAnimator.StopAnimate(GIF, OnAnimated);
        }

        private static void TakeSnapshot(Image image, int frame, int animationTime)
        {
            string filename = Path.Combine(OutputFolder, $"{frame}_{animationTime.ToString("00000")}.png");
            image.Save(filename, ImageFormat.Png);
        }

        private static void OnAnimated(object o, EventArgs e)
        {
            ImageAnimator.ImageInfo imageInfo = (ImageAnimator.ImageInfo)o;

            if (imageInfo.Frame > LastAnimationFrame)
            {
                LastAnimationFrame = imageInfo.Frame;
                ImageAnimator.UpdateFrames(imageInfo._image);
                TakeSnapshot(imageInfo._image, imageInfo.Frame, imageInfo.AnimationTimer);
            }
            else
            {
                LastAnimationFrame = FrameCount;
            }
        }
    }
}
