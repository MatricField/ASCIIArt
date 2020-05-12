using ASCIIArt.Engine.Helpers;
using ASCIIArt.Engine.ImageCompare;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ASCIIArt.Engine
{
    public class ImageRenderEngineCpu :
        ImageRenderEngine
    {

        public ImageRenderEngineCpu(IConsoleDisplayInfo consoleDisplayInfo) :
            base(consoleDisplayInfo)
        {

        }

        public override string RenderImage(Mat originalImgMat)
        {
            const DepthType cDepthType = DepthType.Cv32F;
            var charMats =
                info
                .PrintableChars
                .Select(kvp =>
                {
                    var (c, b) = kvp;
                    var charMat = new Mat();
                    switch (originalImgMat.NumberOfChannels)
                    {
                        case 1:
                            CvInvoke.Imdecode(b, ImreadModes.Grayscale, charMat);
                            break;
                        case 2:
                        case 3:
                            CvInvoke.Imdecode(b, ImreadModes.Color, charMat);
                            break;
                        case 4:
                            CvInvoke.Imdecode(b, ImreadModes.Unchanged | ImreadModes.Color, charMat);
                            if(charMat.NumberOfChannels < 4)
                            {
                                using (var tmp = new Mat(0, 0, charMat.Depth, 4))
                                using (var vec = new VectorOfMat())
                                using (var aChannel = new Mat(charMat.Size, charMat.Depth, 4))
                                {
                                    CvInvoke.Split(tmp, vec);
                                    vec.Push(aChannel);
                                    aChannel.SetTo(new MCvScalar(255));
                                    CvInvoke.Merge(vec, tmp);
                                    tmp.CopyTo(charMat);
                                }
                            }
                            break;
                    }
                        
                    return (c, charMat);
                })
                .ToArray();
            using (var charMatHandle = CollectionDisposer.Create(charMats, pair => Enumerable.Repeat(pair.Item2, 1)))
            using (var imgMat = new Mat())
            using (var param = new DisposingThreadLocal<MSSIMCpuParam>(
                () => new MSSIMCpuParam(info.CharPixelSize, cDepthType, originalImgMat.NumberOfChannels)))
            {
                originalImgMat.ConvertTo(imgMat, cDepthType);
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
                            charMats
                            .MaxBy(kvp => MSSIM.MSSIMCpu(kvp.Item2, piece, param.Value))
                            .Item1;
                        charImg[y, x] = c;
                    }
                }
                return string.Concat(charImg.Cast<string>());
            }
        }

    }
}
