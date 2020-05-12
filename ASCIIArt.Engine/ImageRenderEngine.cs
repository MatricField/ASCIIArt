using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASCIIArt.Engine.Helpers;
using ASCIIArt.Engine.ImageCompare;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace ASCIIArt.Engine
{
    public class ImageRenderEngine :
        IDisposable
    {
        private readonly IConsoleDisplayInfo info;
        private readonly (char, Mat)[] charMats;

        public ImageRenderEngine(IConsoleDisplayInfo consoleDisplayInfo)
        {
            info = consoleDisplayInfo;
            charMats =
                consoleDisplayInfo
                .PrintableChars
                .Select(kvp =>
                {
                    var (c, b) = kvp;
                    var charMat = new Mat();
                    CvInvoke.Imdecode(b, ImreadModes.Color, charMat);
                    return (c, charMat);
                })
                .ToArray();
        }

        public string RenderImage(Mat imgMat)
        {
            var imgMatrix = new Matrix<byte>(imgMat.Rows, imgMat.Cols, imgMat.NumberOfChannels);
            imgMat.CopyTo(imgMatrix);
            var charImg = new char[info.WidthInColumns, info.HeightInRows];

            void ComparePiece((int, int) index)
            {
                var (x, y) = index;
                var loc = new Point()
                {
                    X = x * info.CharPixelWidth,
                    Y = y * info.CharPixelHeight
                };
                var rect = new Rectangle(loc, info.CharPixelSize);
                using (var piece = imgMatrix.GetSubRect(rect))
                using (var pieceMat = piece.Mat)
                {
                    var c =
                        charMats
                        .MaxBy(kvp => MSSIM.MSSIMCpu(kvp.Item2, pieceMat))
                        .Item1;
                    charImg[x, y] = c;
                }
            }
            var idx = new List<(int, int)>();
            for (var x = 0; x < info.WidthInColumns; x++)
            {
                for (var y = 0; y < info.HeightInRows; y++)
                {
                    idx.Add((x, y));
                }
            }
            using (imgMatrix)
            {
                Parallel.ForEach(idx, ComparePiece);
                var builder = new StringBuilder();
                
                for (var y = 0; y < info.HeightInRows; y++)
                {
                    for (var x = 0; x < info.WidthInColumns; x++)
                    {
                        builder.Append(charImg[x, y]);
                    }
                }
                return builder.ToString();
            }
        }

        public string RenderImage(Image<Rgb, byte> imgData)
        {
            using(var resized = imgData.Resize(info.ClientAreaSize.Width, info.ClientAreaSize.Height, Inter.Linear))
            using(var resizedMat = resized.Mat)
            {
                return RenderImage(resizedMat);
            }
        }

        public string RenderImage(string filePath)
        {
            using (var mat = CvInvoke.Imread(filePath, ImreadModes.Color))
            using (var img = mat.ToImage<Rgb, byte>())
            {
                return RenderImage(img);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach(var (_, m) in charMats)
                    {
                        m.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
