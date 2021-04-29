using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace animatorapp
{
    class Program
    {
        static string OutputFolder;
        static Bitmap GIF_Local;
        static Bitmap GIF_Main;

        static void Main(string[] args)
        {
            if (args.Length != 2 || !File.Exists(args[0]))
            {
                throw new ArgumentException("Syntax: appanimator imageFile outputFolder. " + string.Join(",", args));
            }

            string now = DateTime.Now.ToString("HH-mm-ss-ff");
            OutputFolder = Path.Combine(args[1], now);
            Directory.CreateDirectory(OutputFolder);

            GIF_Local = new(args[0]);
            GIF_Main = new(args[0]);

            ImageAnimator.Animate(GIF_Local, new EventHandler(OnAnimated));
            System.Drawing.ImageAnimator.Animate(GIF_Main, new EventHandler(OnAnimated));

            for (var frame = 0; frame < 1000; frame++)
            {
                SaveSnapshot();
                Thread.Sleep(10);
            }
        }

        private static void SaveSnapshot()
        {
            string now = DateTime.Now.ToString("HH-mm-ss-ff");
            ImageAnimator.UpdateFrames();
            System.Drawing.ImageAnimator.UpdateFrames();

            Image thumbnail_local = GIF_Local.GetThumbnailImage(128, 128, null, IntPtr.Zero);
            thumbnail_local.Save(Path.Combine(OutputFolder, $"{now}_local.png"), ImageFormat.Png);

            Image thumbnail_main = GIF_Main.GetThumbnailImage(128, 128, null, IntPtr.Zero);
            thumbnail_main.Save(Path.Combine(OutputFolder, $"{now}_main.png"), ImageFormat.Png);
        }

        private static void OnAnimated(object o, EventArgs e)
        {
            Console.WriteLine("Animating...");
        }
    }
}
