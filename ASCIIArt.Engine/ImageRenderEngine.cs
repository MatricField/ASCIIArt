using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace ASCIIArt.Engine
{
    public abstract class ImageRenderEngine :
        IDisposable
    {
        protected IConsoleDisplayInfo info { get; private set; }

        public ImageRenderEngine(IConsoleDisplayInfo consoleDisplayInfo)
        {
            info = consoleDisplayInfo;
        }

        public abstract string RenderImage(Mat imgMat);

        public virtual string RenderImage<TColor, TDepth>(Image<TColor, TDepth> imgData)
            where TColor : struct, IColor
            where TDepth : new()
        {
            using(var resized = imgData.Resize(info.ClientAreaSize.Width, info.ClientAreaSize.Height, Inter.Linear))
            using(var resizedMat = resized.Mat)
            {
                return RenderImage(resizedMat);
            }
        }

        public virtual string RenderImage(string filePath)
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
