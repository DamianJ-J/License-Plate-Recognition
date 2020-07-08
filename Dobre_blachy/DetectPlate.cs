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
    public static class DetectPlate
    {
        //static frmMain frm = new frmMain();

        public static string tmptxt;
        public static  double PLATE_WIDTH_PADDING_FACTOR = 1.3;
        public static  double PLATE_HEIGHT_PADDING_FACTOR = 1.5;
             
        public static  MCvScalar SCALAR_RED = new MCvScalar(0.0, 0.0, 255.0);
        public static  MCvScalar SCALAR_WHITE = new MCvScalar(255.0, 255.0, 255.0);

        public static List<PossiblePlate> detectPlatesInScene(Mat imgOriginalScene)
        {
            List<PossiblePlate> listOfPossiblePlates = new List<PossiblePlate>();

            Mat imgGrayscaleScene = new Mat();
            Mat imgThreshScene = new Mat();
            Mat imgContours = new Mat(imgOriginalScene.Size, DepthType.Cv8U, 3);

            Random random = new Random();

            CvInvoke.DestroyAllWindows();

            //if(frm.IsCheckedSteps()==true)
            //{
            //    CvInvoke.Imshow("0", imgOriginalScene);
            //}
         
            Preprocess.preprocess(imgOriginalScene, imgGrayscaleScene, imgThreshScene);

            //if (frm.IsCheckedSteps() == true)
            //{
            //     CvInvoke.Imshow("1a", imgGrayscaleScene);
            //     CvInvoke.Imshow("1b", imgThreshScene);  
            //}

            List<PossibleChar> listOfPossibleCharsInScene = findPossibleCharsInScene(imgThreshScene);

            //if (frm.IsCheckedSteps() == true)
            //{
            //    VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            //    foreach (PossibleChar possibleChar in listOfPossibleCharsInScene)
            //    {
            //        contours.Push(possibleChar.contour);
            //    }

            //    CvInvoke.DrawContours(imgContours, contours, -1, SCALAR_WHITE);
            //    CvInvoke.Imshow("2b", imgContours);
            //}

            List<List<PossibleChar>> listOfListsOfMatchingCharsInScene = DetectChars.findListOfListsOfMatchingChars(listOfPossibleCharsInScene);



            //if (frm.IsCheckedSteps() == true)
            //{
            //    imgContours = new Mat(imgOriginalScene.Size, DepthType.Cv8U, 3);

            //    foreach (var listOfMatchingChars in listOfListsOfMatchingCharsInScene)
            //    {
            //        int intRandomBlue = random.Next(0, 256);
            //        int intRandomGreen = random.Next(0, 256);
            //        int intRandomRed = random.Next(0, 256);

            //        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            //        foreach (PossibleChar matchingChar in listOfMatchingChars)
            //        {
            //            contours.Push(matchingChar.contour);
            //        }

            //        CvInvoke.DrawContours(imgContours, contours, -1, new MCvScalar((double)(intRandomBlue), (double)(intRandomGreen), (double)(intRandomRed)));

            //    }
            //    CvInvoke.Imshow("3", imgContours);
            //}

            PossiblePlate possiblePlate;//?????????????
            foreach (var listOfMatchingChars in listOfListsOfMatchingCharsInScene)
            {
                possiblePlate = extractPlate(imgOriginalScene, listOfMatchingChars);//??????????????????

                if (possiblePlate.imgPlate != null)
                {
                    listOfPossiblePlates.Add(possiblePlate);
                }
            }

            tmptxt = $"possible plates: {listOfPossiblePlates.Count}";
            // dorobic wpis do txtboxa ile jest tablic mozliwych

            //if (frm.IsCheckedSteps() == true)
            //{
            //    CvInvoke.Imshow("4a", imgContours);

            //    for (int i = 0; i <= listOfPossiblePlates.Count - 1; i++)
            //    {
            //        PointF[] ptfRectPoints = new PointF[4];

            //        ptfRectPoints = listOfPossiblePlates[i].rrLocationOfPlateInScene.GetVertices();

            //        Point pt0 = new Point((int)(ptfRectPoints[0].X), (int)(ptfRectPoints[0].Y));
            //        Point pt1 = new Point((int)(ptfRectPoints[1].X), (int)(ptfRectPoints[1].Y));
            //        Point pt2 = new Point((int)(ptfRectPoints[2].X), (int)(ptfRectPoints[2].Y));
            //        Point pt3 = new Point((int)(ptfRectPoints[3].X), (int)(ptfRectPoints[3].Y));

            //        CvInvoke.Line(imgContours, pt0, pt1, SCALAR_RED, 2);
            //        CvInvoke.Line(imgContours, pt1, pt2, SCALAR_RED, 2);
            //        CvInvoke.Line(imgContours, pt2, pt3, SCALAR_RED, 2);
            //        CvInvoke.Line(imgContours, pt3, pt0, SCALAR_RED, 2);

            //        CvInvoke.Imshow("4a", imgContours);
            //        CvInvoke.Imshow("4b", listOfPossiblePlates[i].imgPlate);
            //        CvInvoke.WaitKey(0);   
            //    }
            //}
            return listOfPossiblePlates;
        }//end detectplatesinscene

        public static List<PossibleChar> findPossibleCharsInScene(Mat imgThresh)
        {
            List<PossibleChar> listOfPossibleChars = new List<PossibleChar>();

            Mat imgContours = new Mat(imgThresh.Size, DepthType.Cv8U, 3);
            int intCountOfPossibleChars = 0;

            Mat imgThreshCopy = imgThresh.Clone();

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            CvInvoke.FindContours(imgThreshCopy, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);


            for (int i = 0; i < contours.Size-1; i++)
            {
                //if (frm.IsCheckedSteps() == true)
                //{
                //    CvInvoke.DrawContours(imgContours, contours, i, SCALAR_WHITE);
                //}

                PossibleChar possibleChar = new PossibleChar(contours[i]);

                if (DetectChars.checkIfPossibleChar(possibleChar))
                {
                    intCountOfPossibleChars=intCountOfPossibleChars+1;
                    listOfPossibleChars.Add(possibleChar);
                }
            }

            //if (frm.IsCheckedSteps() == true)
            //{
            //    CvInvoke.Imshow("2a", imgContours);
            //}
            return listOfPossibleChars;
        }//end possibleplateinscene

        public static PossiblePlate extractPlate(Mat imgOriginal, List<PossibleChar> listOfMatchingChars)
        {
            PossiblePlate possiblePlate = new PossiblePlate();

            listOfMatchingChars.Sort((firstChar, secondChar) => firstChar.intCenterX.CompareTo(secondChar.intCenterX));//??????????????????????

            double dblPlateCenterX = (double)(listOfMatchingChars[0].intCenterX + listOfMatchingChars[listOfMatchingChars.Count - 1].intCenterX) / 2.0;
            double dblPlateCenterY = (double)(listOfMatchingChars[0].intCenterY + listOfMatchingChars[listOfMatchingChars.Count - 1].intCenterY) / 2.0;
            PointF ptfPlateCenter = new PointF((float)dblPlateCenterX, (float)dblPlateCenterY);

            int intPlateWidth = (int)((double)(listOfMatchingChars[listOfMatchingChars.Count - 1].boundingRect.X + listOfMatchingChars[listOfMatchingChars.Count - 1].boundingRect.Width - listOfMatchingChars[0].boundingRect.X) * PLATE_WIDTH_PADDING_FACTOR);

            int intTotalOfCharHeights = 0;

            foreach (PossibleChar matchingChar in listOfMatchingChars)
            {
                intTotalOfCharHeights = intTotalOfCharHeights + matchingChar.boundingRect.Height;
            }

            double dblAverageCharHeight = (double)(intTotalOfCharHeights) / (double)(listOfMatchingChars.Count);

            int intPlateHeight = (int)(dblAverageCharHeight * PLATE_HEIGHT_PADDING_FACTOR);

            double dblOpposite = listOfMatchingChars[listOfMatchingChars.Count - 1].intCenterY - listOfMatchingChars[0].intCenterY;
            double dblHypotenuse = DetectChars.distanceBetweenChars(listOfMatchingChars[0], listOfMatchingChars[listOfMatchingChars.Count - 1]);
            double dblCorrectionAngleInRad = Math.Asin(dblOpposite / dblHypotenuse);
            double dblCorrectionAngleInDeg = dblCorrectionAngleInRad * (180.0 / Math.PI);

            possiblePlate.rrLocationOfPlateInScene = new RotatedRect(ptfPlateCenter, new SizeF((Single)intPlateWidth, (Single)intPlateHeight), (Single)dblCorrectionAngleInDeg);

            Mat rotationMatrix = new Mat();
            Mat imgRotated = new Mat();
            Mat imgCropped = new Mat();

            CvInvoke.GetRotationMatrix2D(ptfPlateCenter, dblCorrectionAngleInDeg, 1.0, rotationMatrix);

            CvInvoke.WarpAffine(imgOriginal, imgRotated, rotationMatrix, imgOriginal.Size);

            CvInvoke.GetRectSubPix(imgRotated, possiblePlate.rrLocationOfPlateInScene.MinAreaRect().Size, possiblePlate.rrLocationOfPlateInScene.Center, imgCropped);

            possiblePlate.imgPlate = imgCropped;

            return possiblePlate;
        }//end extractplate
    }
}
