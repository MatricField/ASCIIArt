using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;

namespace ASCIIArt.Engine.ImageCompare
{
    public static class MSSIM
    {
        private const float defaultC1 = 6.5025f;
        private const float defaultC2 = 58.5225f;

        /// <remarks>
        /// // Converted Implementation of MSSIM from
        /// https://docs.opencv.org/2.4/doc/tutorials/gpu/gpu-basics-similarity/gpu-basics-similarity.html
        /// </remarks>
        public static double MSSIMCpu(Mat i1, Mat i2, double C1 = defaultC1, double C2 = defaultC2)
        {
            using (var I1 = new Mat())
            using (var I2 = new Mat())

            using (var I1_2 = new Mat())
            using (var I2_2 = new Mat())
            using (var I1_I2 = new Mat())

            using (var mu1 = new Mat())
            using (var mu2 = new Mat())

            using (var mu1_2 = new Mat())
            using (var mu2_2 = new Mat())
            using (var mu1_mu2 = new Mat())

            using (var sigma1_2 = new Mat())
            using (var sigma2_2 = new Mat())
            using (var sigma12 = new Mat())

            using (var t1 = new Mat())
            using (var t2 = new Mat())
            using (var t3 = new Mat())
            using (var ssim_map = new Mat())

            using (var ones = Mat.Ones(i1.Rows, i1.Cols, DepthType.Cv32F, i1.NumberOfChannels))
            {
                i1.ConvertTo(I1, DepthType.Cv32F);
                i2.ConvertTo(I2, DepthType.Cv32F);

                CvInvoke.Multiply(I1, I1, I1_2);
                CvInvoke.Multiply(I2, I2, I2_2);
                CvInvoke.Multiply(I1, I2, I1_I2);

                CvInvoke.GaussianBlur(I1, mu1, new Size(11, 11), 1.5);
                CvInvoke.GaussianBlur(I2, mu2, new Size(11, 11), 1.5);

                CvInvoke.Multiply(mu1, mu1, mu1_2);
                CvInvoke.Multiply(mu2, mu2, mu2_2);
                CvInvoke.Multiply(mu1, mu2, mu1_mu2);

                CvInvoke.GaussianBlur(I1_2, sigma1_2, new Size(11, 11), 1.5);
                CvInvoke.GaussianBlur(I2_2, sigma2_2, new Size(11, 11), 1.5);
                CvInvoke.GaussianBlur(I1_I2, sigma12, new Size(11, 11), 1.5);

                //sigma1_2 = sigma1_2 - mu1_2
                CvInvoke.AddWeighted(sigma1_2, 1, mu1_2, -1, 0, sigma1_2);

                //sigma2_2 = sigma2_2 - mu2_2
                CvInvoke.AddWeighted(sigma2_2, 1, mu2_2, -1, 0, sigma2_2);

                //sigma12 = sigma12 - mu1_mu2
                CvInvoke.AddWeighted(sigma12, 1, mu1_mu2, -1, 0, sigma12);

                // t1 = 2 * mu1_mu2 + C1
                CvInvoke.AddWeighted(mu1_mu2, 2, ones, C1, 0, t1);
                // t2 = 2 * sigma12 + C2
                CvInvoke.AddWeighted(mu1_mu2, 2, ones, C1, 0, t2);
                CvInvoke.Multiply(t1, t2, t3);

                //t1 = mu1_2 + mu2_2 + C1;
                CvInvoke.AddWeighted(mu1_2, 1, mu2_2, 1, C1, t1);
                //t2 = sigma1_2m + sigma2_2m + C2;
                CvInvoke.AddWeighted(sigma1_2, 1, sigma2_2, 1, C2, t2);
                CvInvoke.Multiply(t1, t2, t1);
                CvInvoke.Divide(t3, t1, ssim_map);

                var mssim = CvInvoke.Mean(ssim_map);
                return Math.Sqrt(
                    mssim.V0 * mssim.V0 +
                    mssim.V1 * mssim.V1 +
                    mssim.V2 * mssim.V2 +
                    mssim.V3 * mssim.V3
                    );

            }
        }
    }
}
