using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Stitching;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;

namespace ASCIIArt.Engine.ImageCompare
{
    internal static class MSSIM
    {
        private const float defaultC1 = 6.5025f;
        private const float defaultC2 = 58.5225f;

        /// <remarks>
        /// // Converted Implementation of MSSIM from
        /// https://docs.opencv.org/2.4/doc/tutorials/gpu/gpu-basics-similarity/gpu-basics-similarity.html
        /// </remarks>
        public static double MSSIMCpu(Mat i1, Mat i2, MSSIMCpuParam b, double C1 = defaultC1, double C2 = defaultC2)
        {
            try
            {
                var gaussianSize = new Size(3, 3);
                i1.ConvertTo(b.I1, b.DepthType);
                i2.ConvertTo(b.I2, b.DepthType);

                CvInvoke.Multiply(b.I1, b.I1, b.I1_2);
                CvInvoke.Multiply(b.I2, b.I2, b.I2_2);
                CvInvoke.Multiply(b.I1, b.I2, b.I1_I2);

                CvInvoke.GaussianBlur(b.I1, b.Mu1, gaussianSize, 1.5);
                CvInvoke.GaussianBlur(b.I2, b.Mu2, gaussianSize, 1.5);

                CvInvoke.Multiply(b.Mu1, b.Mu1, b.Mu1_2);
                CvInvoke.Multiply(b.Mu2, b.Mu2, b.Mu2_2);
                CvInvoke.Multiply(b.Mu1, b.Mu2, b.Mu1_Mu2);

                CvInvoke.GaussianBlur(b.I1_2, b.Sigma1_2, gaussianSize, 1.5);
                CvInvoke.GaussianBlur(b.I2_2, b.Sigma2_2, gaussianSize, 1.5);
                CvInvoke.GaussianBlur(b.I1_I2, b.Sigma12, gaussianSize, 1.5);

                //sigma1_2 = sigma1_2 - mu1_2
                CvInvoke.AddWeighted(b.Sigma1_2, 1, b.Mu1_2, -1, 0, b.Sigma1_2);

                //sigma2_2 = sigma2_2 - mu2_2
                CvInvoke.AddWeighted(b.Sigma2_2, 1, b.Mu2_2, -1, 0, b.Sigma2_2);

                //sigma12 = sigma12 - mu1_mu2
                CvInvoke.AddWeighted(b.Sigma12, 1, b.Mu1_Mu2, -1, 0, b.Sigma12);

                // t1 = 2 * mu1_mu2 + C1
                CvInvoke.AddWeighted(b.Mu1_Mu2, 2, b.Ones, C1, 0, b.T1);
                // t2 = 2 * sigma12 + C2
                CvInvoke.AddWeighted(b.Sigma12, 2, b.Ones, C1, 0, b.T2);
                CvInvoke.Multiply(b.T1, b.T2, b.T3);

                //t1 = mu1_2 + mu2_2 + C1;
                CvInvoke.AddWeighted(b.Mu1_2, 1, b.Mu2_2, 1, C1, b.T1);
                //t2 = sigma1_2m + sigma2_2m + C2;
                CvInvoke.AddWeighted(b.Sigma1_2, 1, b.Sigma2_2, 1, C2, b.T2);
                CvInvoke.Multiply(b.T1, b.T2, b.T1);

                CvInvoke.Divide(b.T3, b.T1, b.SSIM_map);

                var mssim = CvInvoke.Mean(b.SSIM_map);
                return Math.Sqrt(
                    mssim.V0 * mssim.V0 +
                    mssim.V1 * mssim.V1 +
                    mssim.V2 * mssim.V2
                    );
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static double MSSIMGpu(GpuMat VI1, GpuMat VI2, MSSIMGpuParam b, double C1 = defaultC1, double C2 = defaultC2)
        {
            if(!VI1.Size.Equals(VI2.Size))
            {
                throw new ArgumentException();
            }

            var gStream = new Stream();


            CudaInvoke.Multiply(VI1, VI1, b.I1_2, stream: gStream);
            CudaInvoke.Multiply(VI2, VI2, b.I2_2, stream: gStream);
            CudaInvoke.Multiply(VI1, VI2, b.I1_I2, stream: gStream);


            b.GaussianFilter.Apply(VI1, b.Mu1, stream: gStream);
            b.GaussianFilter.Apply(VI2, b.Mu2, stream: gStream);

            CudaInvoke.Multiply(b.Mu1, b.Mu1, b.Mu1_2, stream: gStream);
            CudaInvoke.Multiply(b.Mu2, b.Mu2, b.Mu2_2, stream: gStream);
            CudaInvoke.Multiply(b.Mu1, b.Mu2, b.Mu1_Mu2, stream: gStream);

            b.GaussianFilter.Apply(b.I1_2, b.Sigma1_2, stream: gStream);
            CudaInvoke.Subtract(b.Sigma1_2, b.Mu1_2, b.Sigma1_2, stream: gStream);

            b.GaussianFilter.Apply(b.I2_2, b.Sigma2_2, stream: gStream);
            CudaInvoke.Subtract(b.Sigma2_2, b.Mu2_2, b.Sigma2_2, stream: gStream);

            b.GaussianFilter.Apply(b.I1_I2, b.Sigma12, stream: gStream);
            CudaInvoke.Subtract(b.Sigma12, b.Mu1_Mu2, b.Sigma12, stream: gStream);

            CudaInvoke.AddWeighted(b.Mu1_Mu2, 2, b.Ones, C1, 0, b.T1, stream: gStream);
            CudaInvoke.AddWeighted(b.Sigma12, 2, b.Ones, C2, 0, b.T2, stream: gStream);
            CudaInvoke.Multiply(b.T1, b.T2, b.T3, stream: gStream);

            CudaInvoke.AddWeighted(b.Mu1_2, 1, b.Mu2_2, 1, C1, b.T1, stream: gStream);
            CudaInvoke.AddWeighted(b.Sigma1_2, 1, b.Sigma2_2, 1, C2, b.T1, stream: gStream);
            CudaInvoke.Multiply(b.T1, b.T2, b.T1, stream: gStream);

            CudaInvoke.Divide(b.T3, b.T1, b.SSIM_map, stream: gStream);

            gStream.WaitForCompletion();

            var mssim = CudaInvoke.AbsSum(b.SSIM_map);
            var ret = 0.0;
            foreach(var v in mssim.ToArray())
            {
                var tmp = v/(b.SSIM_map.Size.Height * b.SSIM_map.Size.Width);
                tmp *= tmp;
                ret += tmp;
            }
            return Math.Sqrt(ret);
        }
    }
}
