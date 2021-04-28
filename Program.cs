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
        static Bitmap GIF;

        static void Main(string[] args)
        {
            if (args.Length != 2 || !File.Exists(args[0]))
            {
                throw new ArgumentException("Syntax: appanimator imageFile outputFolder");
            }

            string now = DateTime.Now.ToString("HH-mm-ss-ff");
            OutputFolder = Path.Combine(args[1], now);
            Directory.CreateDirectory(OutputFolder);

            GIF = new(args[0]);
            System.Drawing.ImageAnimator.Animate(GIF, new EventHandler(OnAnimated));

            for (var frame = 0; frame < 1000; frame++)
            {
                SaveSnapshot();
                Thread.Sleep(10);
            }

            //ImageAnimator.StopAnimate(GIF, new EventHandler(OnAnimated));
        }

        private static void SaveSnapshot()
        {
            string now = DateTime.Now.ToString("HH-mm-ss-ff");
            System.Drawing.ImageAnimator.UpdateFrames();
            Image thumbnail = GIF.GetThumbnailImage(128, 128, null, IntPtr.Zero);

            thumbnail.Save(Path.Combine(OutputFolder, $"{now}.png"), ImageFormat.Png);
        }

        private static void OnAnimated(object o, EventArgs e)
        {
            Console.WriteLine("Animating...");
        }
    }
}
