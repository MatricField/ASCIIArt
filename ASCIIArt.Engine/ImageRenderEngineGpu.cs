using ASCIIArt.Engine.Helpers;
using ASCIIArt.Engine.ImageCompare;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace ASCIIArt.Engine
{
    public class ImageRenderEngineGpu :
        ImageRenderEngine
    {
        private readonly Dictionary<string, GpuMat> charMats;
        public ImageRenderEngineGpu(IConsoleDisplayInfo consoleDisplayInfo) :
            base(consoleDisplayInfo)
        {
            var charMat = new Dictionary<string, GpuMat>();
            using (var tmp = new GpuMat())
            using (var mat = new Mat())
            {
                foreach (var kvp in consoleDisplayInfo.PrintableChars)
                {
                    var c = kvp.Key;
                    var data = kvp.Value;
                    CvInvoke.Imdecode(data, ImreadModes.Grayscale, mat);
                    tmp.Upload(mat);
                    var gMat = new GpuMat();
                    tmp.ConvertTo(gMat, DepthType.Cv32F);
                    charMat.Add(c, gMat);
                }
            }
            this.charMats = charMat;
        }

        public override string RenderImage(Mat imgMat)
        {
            const DepthType cDepthType = DepthType.Cv32F;

            using (var tmp = new GpuMat())
            using (var gImgMat = new GpuMat())
            using (var param = new DisposingThreadLocal<MSSIMGpuParam>(
                () => new MSSIMGpuParam(info.CharPixelSize, cDepthType, channels: imgMat.NumberOfChannels)))
            {
                tmp.Upload(imgMat);
                tmp.ConvertTo(gImgMat, cDepthType);
                var idx = new List<(int, int)>();
                for (var y = 0; y < info.HeightInRows; y++)
                {
                    for (var x = 0; x < info.WidthInColumns; x++)
                    {
                        idx.Add((y, x));
                    }
                }
                var charImg = new string[info.HeightInRows, info.WidthInColumns];
                Parallel.ForEach(idx, ComparePiece);
                void ComparePiece((int, int) i)
                {
                    var (y, x) = i;
                    var rowRange = new Range(y * info.CharPixelSize.Height, (y + 1) * info.CharPixelSize.Height);
                    var colRange = new Range(x * info.CharPixelSize.Width, (x + 1) * info.CharPixelSize.Width);
                    using (var piece = new GpuMat(gImgMat, rowRange, colRange))
                    {
                        var c =
                            charMats
                            .MaxBy(kvp => MSSIM.MSSIMGpu(kvp.Value, piece, param.Value))
                            .Key;
                        charImg[y, x] = c;
                    }
                }

                return string.Concat(charImg);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                foreach(var kvp in charMats)
                {
                    kvp.Value.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
