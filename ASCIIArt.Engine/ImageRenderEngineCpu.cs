using ASCIIArt.Engine.Helpers;
using ASCIIArt.Engine.ImageCompare;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Stitching;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASCIIArt.Engine
{
    public class ImageRenderEngineCpu :
        ImageRenderEngine
    {
        private readonly DisposingThreadLocal<MSSIMCpuParam> _Param;

        private readonly CollectionDisposer _CharMats;

        protected ThreadLocal<MSSIMCpuParam> Param => _Param;

        protected DepthType Depth { get; }

        protected (string, Mat)[] CharMats { get; }

        public ImageRenderEngineCpu(IConsoleDisplayInfo consoleDisplayInfo,
            DepthType depthType = DepthType.Cv32F, int channels = 4) :
            base(consoleDisplayInfo)
        {
            _Param = new DisposingThreadLocal<MSSIMCpuParam>(
                () => new MSSIMCpuParam(consoleDisplayInfo.CharPixelSize, depthType, channels: channels));
            Depth = depthType;

            (string, Mat) ConvertCharMat((string, byte[]) pair)
            {
                var (c, b) = pair;
                var charMat = new Mat();
                using (var tmp = new Mat(0, 0, Depth, 4))
                {
                    switch (channels)
                    {
                        case 1:
                            CvInvoke.Imdecode(b, ImreadModes.Grayscale, tmp);
                            break;
                        case 2:
                        case 3:
                            CvInvoke.Imdecode(b, ImreadModes.Color, tmp);
                            break;
                        case 4:
                            CvInvoke.Imdecode(b, ImreadModes.Unchanged | ImreadModes.Color, charMat);
                            if (charMat.NumberOfChannels < 4)
                            {
                                using (var vec = new VectorOfMat())
                                using (var aChannel = new Mat(charMat.Size, Depth, 4))
                                {
                                    CvInvoke.Split(charMat, vec);
                                    aChannel.SetTo(new MCvScalar(255));
                                    vec.Push(aChannel);
                                    CvInvoke.Merge(vec, tmp);
                                }
                            }
                            else
                            {
                                charMat.CopyTo(tmp);
                            }
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    tmp.ConvertTo(charMat, Depth);
                }
                return (c, charMat);
            }

            var charMats =
                info
                .PrintableChars
                .Select(ConvertCharMat)
                .ToArray();

            IEnumerable<IDisposable> SelectMat()
            {
                foreach(var (_, m) in charMats)
                {
                    yield return m;
                }    
            }
            _CharMats = new CollectionDisposer(SelectMat);
            this.CharMats = charMats;
        }

        public override string RenderImage(Mat originalImgMat)
        {
            Mat imgMat;
            if(originalImgMat.Depth != Depth)
            {
                imgMat = new Mat();
                originalImgMat.ConvertTo(imgMat, Depth);
            }
            else
            {
                imgMat = new Mat(originalImgMat, Range.All, Range.All);
            }
            using (imgMat)
            {
                
                var idx = new List<(int, int)>();
                for (var y = 0; y < info.HeightInRows; y++)
                {
                    for (var x = 0; x < info.WidthInColumns; x++)
                    {
                        idx.Add((y, x));
                    }
                }
                var charImg = new string[info.HeightInRows, info.WidthInColumns];
                var result = Parallel.ForEach(idx, ComparePiece);
                void ComparePiece((int, int) i)
                {
                    var (y, x) = i;
                    var loc = new Point()
                    {
                        X = x * info.CharPixelWidth,
                        Y = y * info.CharPixelHeight
                    };
                    var rect = new Rectangle(loc, info.CharPixelSize);
                    using (var piece = new Mat(imgMat, rect))
                    {
                        var c =
                            CharMats
                            .MaxBy(kvp => MSSIM.MSSIMCpu(kvp.Item2, piece, Param.Value))
                            .Item1;
                        charImg[y, x] = c;
                    }
                }
                return string.Concat(charImg.Cast<string>());
            }
        }
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                _Param.Dispose();
                _CharMats.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
