using Emgu.CV.Cuda;
using System;

namespace ASCIIArt.Engine.ImageCompare
{
    internal sealed class GpuMSSIMBuffer :
        IDisposable
    {
        public GpuMat GI1 { get; set; } = new GpuMat();
        public GpuMat GI2 { get; set; } = new GpuMat();
        public GpuMat Gs { get; set; } = new GpuMat();
        public GpuMat Tmp1 { get; set; } = new GpuMat();
        public GpuMat Tmp2 { get; set; } = new GpuMat();
        public GpuMat I1_2 { get; set; } = new GpuMat();
        public GpuMat I2_2 { get; set; } = new GpuMat();
        public GpuMat I1_I2 { get; set; } = new GpuMat();
        public GpuMat Mu1 { get; set; } = new GpuMat();
        public GpuMat Mu2 { get; set; } = new GpuMat();
        public GpuMat Mu1_2 { get; set; } = new GpuMat();
        public GpuMat Mu2_2 { get; set; } = new GpuMat();
        public GpuMat Mu1_mu2 { get; set; } = new GpuMat();
        public GpuMat Sigma1_2 { get; set; } = new GpuMat();
        public GpuMat Sigma2_2 { get; set; } = new GpuMat();
        public GpuMat Sigma12 { get; set; } = new GpuMat();
        public GpuMat T3 { get; set; } = new GpuMat();
        public GpuMat Ssim_map { get; set; } = new GpuMat();
        public GpuMat Buf { get; set; } = new GpuMat();

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
