using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Drawing;

namespace ASCIIArt.Engine.ImageCompare
{
    internal sealed class MSSIMCpuParam :
        IDisposable
    {
        public Mat I1 { get; set; } = new Mat();
        public Mat I2 { get; set; } = new Mat();
        public Mat I1_2 { get; set; } = new Mat();
        public Mat I2_2 { get; set; } = new Mat();
        public Mat I1_I2 { get; set; } = new Mat();
        public Mat Mu1 { get; set; } = new Mat();
        public Mat Mu2 { get; set; } = new Mat();
        public Mat Mu1_2 { get; set; } = new Mat();
        public Mat Mu2_2 { get; set; } = new Mat();
        public Mat Mu1_Mu2 { get; set; } = new Mat();
        public Mat Sigma1_2 { get; set; } = new Mat();
        public Mat Sigma2_2 { get; set; } = new Mat();
        public Mat Sigma12 { get; set; } = new Mat();
        public Mat T1 { get; set; } = new Mat();
        public Mat T2 { get; set; } = new Mat();
        public Mat T3 { get; set; } = new Mat();
        public Mat SSIM_map { get; set; } = new Mat();

        public Mat Ones { get; }

        public DepthType DepthType { get; }

        public int Channels { get; }

        public MSSIMCpuParam(in Size imgPieceSize, DepthType depthType = DepthType.Default, int channels = 4)
        {
            DepthType = depthType;
            Channels = channels;
            Ones = Mat.Ones(imgPieceSize.Height, imgPieceSize.Width, depthType, channels);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    I1.Dispose();
                    I2.Dispose();
                    I1_2.Dispose();
                    I2_2.Dispose();
                    I1_I2.Dispose();
                    Mu1.Dispose();
                    Mu2.Dispose();
                    Mu1_2.Dispose();
                    Mu2_2.Dispose();
                    Mu1_Mu2.Dispose();
                    Sigma1_2.Dispose();
                    Sigma2_2.Dispose();
                    Sigma12.Dispose();
                    T1.Dispose();
                    T2.Dispose();
                    T3.Dispose();
                    Ones.Dispose();
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
