using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using System;
using System.Drawing;

namespace ASCIIArt.Engine.ImageCompare
{
    internal sealed class MSSIMGpuParam :
        IDisposable
    {
        public GpuMat I1_2 { get; set; } = new GpuMat();
        public GpuMat I2_2 { get; set; } = new GpuMat();
        public GpuMat I1_I2 { get; set; } = new GpuMat();
        public GpuMat Mu1 { get; set; } = new GpuMat();
        public GpuMat Mu2 { get; set; } = new GpuMat();
        public GpuMat Mu1_2 { get; set; } = new GpuMat();
        public GpuMat Mu2_2 { get; set; } = new GpuMat();
        public GpuMat Mu1_Mu2 { get; set; } = new GpuMat();
        public GpuMat Sigma1_2 { get; set; } = new GpuMat();
        public GpuMat Sigma2_2 { get; set; } = new GpuMat();
        public GpuMat Sigma12 { get; set; } = new GpuMat();
        public GpuMat T1 { get; set; } = new GpuMat();
        public GpuMat T2 { get; set; } = new GpuMat();
        public GpuMat T3 { get; set; } = new GpuMat();
        public GpuMat SSIM_map { get; set; } = new GpuMat();

        public GpuMat Ones { get; }

        public CudaGaussianFilter GaussianFilter { get; }

        public MSSIMGpuParam(in Size imgPieceSize, DepthType depthType = DepthType.Default, int channels = 4)
        {
            GaussianFilter = new CudaGaussianFilter(
                depthType, channels,
                depthType, channels,
                new Size(11, 11), 1.5);
            Ones = new GpuMat();
            using(var mat = Mat.Ones(imgPieceSize.Height, imgPieceSize.Width, depthType, channels))
            {
                Ones.Upload(mat);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    
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
