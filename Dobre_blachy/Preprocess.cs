using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;

namespace Dobre_blachy
{
    public static class Preprocess
    {
        public static  int GAUSSIAN_BLUR_FILTER_SIZE = 5;
        public static  int ADAPTIVE_THRESH_BLOCK_SIZE = 19;
        public static  int ADAPTIVE_THRESH_WEIGHT = 9;

        public static void preprocess(Mat imgOriginal, Mat imgGrayscale, Mat imgThresh)//przerobic na kontruktor??????
        {
            imgGrayscale = extractValue(imgOriginal);

            Mat imgMaxContrastGrayscale = maximizeContrast(imgGrayscale);

            Mat imgBlurred = new Mat();

            CvInvoke.GaussianBlur(imgMaxContrastGrayscale, imgBlurred, new Size(GAUSSIAN_BLUR_FILTER_SIZE, GAUSSIAN_BLUR_FILTER_SIZE), 0);

            CvInvoke.AdaptiveThreshold(imgBlurred, imgThresh, 255.0, AdaptiveThresholdType.GaussianC, ThresholdType.BinaryInv, ADAPTIVE_THRESH_BLOCK_SIZE, ADAPTIVE_THRESH_WEIGHT);
        }

        public static Mat extractValue(Mat imgOriginal)
        {
            Mat imgHSV = new Mat();
            VectorOfMat vectorOfHSVImages = new VectorOfMat();
            Mat imgValue = new Mat();

            CvInvoke.CvtColor(imgOriginal, imgHSV, ColorConversion.Bgr2Hsv);

            CvInvoke.Split(imgHSV, vectorOfHSVImages);

            imgValue = vectorOfHSVImages[2];

            return imgValue;
        }

        public static Mat maximizeContrast(Mat imgGrayscale)
        {
            Mat imgTopHat = new Mat();
            Mat imgBlackHat = new Mat();
            Mat imgGrayscalePlusTopHat = new Mat();
            Mat imgGrayscalePlusTopHatMinusBlackHat = new Mat();

            Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));

            CvInvoke.MorphologyEx(imgGrayscale, imgTopHat, MorphOp.Tophat, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            CvInvoke.MorphologyEx(imgGrayscale, imgBlackHat, MorphOp.Blackhat, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            CvInvoke.Add(imgGrayscale, imgTopHat, imgGrayscalePlusTopHat);
            CvInvoke.Subtract(imgGrayscalePlusTopHat, imgBlackHat, imgGrayscalePlusTopHatMinusBlackHat);


            return imgGrayscalePlusTopHatMinusBlackHat;
        }
    }
}
