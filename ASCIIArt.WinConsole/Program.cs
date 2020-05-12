using ASCIIArt.Engine;
using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace ASCIIArt.WinConsole
{
    class Program
    {
        static readonly string DESKTOP = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        static void Main(string[] args)
        {
            var info = new ConsoleDisplayInfo();
            using (var renderEngine = new ImageRenderEngine(info))
            using (var imgMat = CvInvoke.Imread(args[0]))
            using (var edge = new Mat())
            {
                CvInvoke.Canny(imgMat, edge, 50, 200);
                using (var img = edge.ToImage<Rgb, byte>())
                {
                    Console.Write(renderEngine.RenderImage(img));
                }
            }
            Console.ReadKey();
        }
    }

}
