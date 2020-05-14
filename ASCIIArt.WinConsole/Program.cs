using ASCIIArt.Engine;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing.Text;

namespace ASCIIArt.WinConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var col = new InstalledFontCollection();
            var info = new ConsoleDisplayInfo();
            using (var imgMat = CvInvoke.Imread(args[0], ImreadModes.ReducedColor2))
            using (var edge = new Mat())
            {
                Console.SetWindowSize(imgMat.Width / info.CharPixelSize.Width, imgMat.Height / info.CharPixelSize.Height);
                info = new ConsoleDisplayInfo();
                using (var renderEngine = new ImageRenderEngineCpu(info, channels:1))
                {
                    CvInvoke.Canny(imgMat, edge, 50, 200);
                    using (var img = edge.ToImage<Gray, float>())
                    {
                        Console.Write(renderEngine.RenderImage(img));
                    }
                }
            }
            Console.ReadKey();
        }
    }

}
