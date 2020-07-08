using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.ML;
using Emgu.CV.ML.MlEnum;
using Emgu.CV.Util;


using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Drawing;

namespace Dobre_blachy
{
    public static class DetectChars
    {
        //static frmMain frm = new frmMain();
        public static  string error;
               
        public static  int MIN_PIXEL_WIDTH = 2;
        public static int MIN_PIXEL_HEIGHT = 8;
               
        public static double MIN_ASPECT_RATIO = 0.25;
        public static double MAX_ASPECT_RATIO = 1.0;
              
        public static int MIN_RECT_AREA = 80;
              
        public static double MIN_DIAG_SIZE_MULTIPLE_AWAY = 0.3;
        public static double MAX_DIAG_SIZE_MULTIPLE_AWAY = 5.0;
               
        public static double MAX_CHANGE_IN_AREA = 0.5;
             
        public static double MAX_CHANGE_IN_WIDTH = 0.8;
        public static double MAX_CHANGE_IN_HEIGHT = 0.2;
            
        public static double MAX_ANGLE_BETWEEN_CHARS = 12.0;
             
        public static int MIN_NUMBER_OF_MATCHING_CHARS = 3;
              
        public static int RESIZED_CHAR_IMAGE_WIDTH = 20;
        public static int RESIZED_CHAR_IMAGE_HEIGHT = 30;
              
        public static MCvScalar SCALAR_GREEN = new MCvScalar(0.0, 255.0, 0.0);
        public static MCvScalar SCALAR_WHITE = new MCvScalar(255.0, 255.0, 255.0);
             
        public static KNearest kNearest = new KNearest();
              
        public static bool loadKNNDataAndTrainKNN()
        {
            Matrix<Single> mtxClassifications = new Matrix<Single>(1, 1);
            Matrix<Single> mtxTrainingImages = new Matrix<Single>(1, 1);

            List<int> intValidChars = new List<int> {(int)('0'), (int)('1'), (int)('2'), (int)('3'), (int)('4'), (int)('5'), (int)('6'), (int)('7'), (int)('8'), (int)('9'),
                                                                  (int)('A'),(int)('B'),(int)('C'), (int)('D'), (int)('E'), (int)('F'), (int)('G'), (int)('H'), (int)('I'), (int)('J'),
                                                                 (int)('K'), (int)('L'), (int)('M'), (int)('N'), (int)('O'), (int)('P'), (int)('Q'), (int)('R'), (int)('S'), (int)('T'),
                                                                  (int)('U'), (int)('V'), (int)('W'), (int)('X'), (int)('Y'), (int)('Z') };

            XmlSerializer xmlSerializer = new XmlSerializer(mtxClassifications.GetType());

            StreamReader streamReader;

            try
            {
                streamReader = new StreamReader("classifications.xml");
            }
            catch (Exception e)
            {
                error = e.ToString();
                //frm.WriteToTxtBox(e.ToString());//nie wiadomo czy dziala
                return false;
            }

            mtxClassifications = (Matrix<Single>)xmlSerializer.Deserialize(streamReader);

            streamReader.Close();

            int intNumberOfTrainingSamples = mtxClassifications.Rows;

            mtxClassifications = new Matrix<Single>(intNumberOfTrainingSamples, 1);
            mtxTrainingImages = new Matrix<Single>(intNumberOfTrainingSamples, RESIZED_CHAR_IMAGE_WIDTH * RESIZED_CHAR_IMAGE_HEIGHT);

            try
            {
                streamReader = new StreamReader("classifications.xml");
            }
            catch (Exception e)
            {
                error = e.ToString();
                //frm.WriteToTxtBox(e.ToString());//nie wiadomo czy dziala
                return false;
            }

            mtxClassifications = (Matrix<Single>)(xmlSerializer.Deserialize(streamReader));

            streamReader.Close();

            xmlSerializer = new XmlSerializer(mtxTrainingImages.GetType());

            try
            {
                streamReader = new StreamReader("images.xml");
            }
            catch (Exception e)
            {
                error = e.ToString();
                //frm.WriteToTxtBox(e.ToString());//nie wiadomo czy dziala
                return false;
            }

            mtxTrainingImages = (Matrix<Single>)(xmlSerializer.Deserialize(streamReader));

            streamReader.Close();

            kNearest.DefaultK = 1;
            kNearest.Train(mtxTrainingImages, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, mtxClassifications);

            return true;

        }//end loadKNNData

        public static List<PossiblePlate> detectCharsInPlates(List<PossiblePlate> listOfPossiblePlates)
        {
            //frmMain frm = new frmMain();

            int intPlateCounter = 0;
            Mat imgContours;
            Random random = new Random();

            if (listOfPossiblePlates == null)
            {
                return listOfPossiblePlates;
            }
            else if (listOfPossiblePlates.Count == 0)
            {
                return listOfPossiblePlates;
            }

            foreach (PossiblePlate possiblePlate in listOfPossiblePlates)
            {
                Preprocess.preprocess(possiblePlate.imgPlate, possiblePlate.imgGrayscale, possiblePlate.imgThresh);

                //if (frm.IsCheckedSteps() == true)
                //{
                //    CvInvoke.Imshow("5a", possiblePlate.imgPlate);
                //    CvInvoke.Imshow("5b", possiblePlate.imgGrayscale);
                //    CvInvoke.Imshow("5c", possiblePlate.imgThresh);
                //}

                CvInvoke.Resize(possiblePlate.imgThresh, possiblePlate.imgThresh, new Size(), 1.6, 1.6);

                CvInvoke.Threshold(possiblePlate.imgThresh, possiblePlate.imgThresh, 0.0, 255.0, ThresholdType.Binary | ThresholdType.Otsu);//??????????????????  brakuje Or'a

                //if (frm.IsCheckedSteps() == true)
                //{
                //    CvInvoke.Imshow("5d", possiblePlate.imgThresh);
                //}

                List<PossibleChar> listOfPossibleCharsInPlate = findPossibleCharsInPlate(possiblePlate.imgGrayscale, possiblePlate.imgThresh);

                //if (frm.IsCheckedSteps() == true)
                //{
                //    imgContours = new Mat(possiblePlate.imgThresh.Size, DepthType.Cv8U, 3);

                //    VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

                //    foreach (PossibleChar possibleChar in listOfPossibleCharsInPlate)
                //    {
                //        contours.Push(possibleChar.contour);
                //    }

                //    CvInvoke.DrawContours(imgContours, contours, -1, SCALAR_WHITE);

                //    CvInvoke.Imshow("6", imgContours);
                //}

                List<List<PossibleChar>> listOfListsOfMatchingCharsInPlate = findListOfListsOfMatchingChars(listOfPossibleCharsInPlate);

                //if (frm.IsCheckedSteps() == true)
                //{
                //    imgContours = new Mat(possiblePlate.imgThresh.Size, DepthType.Cv8U, 3);

                //    VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

                //    foreach (List<PossibleChar> listOfMatchingChars in listOfListsOfMatchingCharsInPlate)
                //    {
                //        int intRandomBlue = random.Next(0, 256);
                //        int intRandomGreen = random.Next(0, 256);
                //        int intRandomRed = random.Next(0, 256);

                //        foreach (PossibleChar matchingChar in listOfMatchingChars)
                //        {
                //            contours.Push(matchingChar.contour);
                //        }
                //        CvInvoke.DrawContours(imgContours, contours, -1, new MCvScalar((double)(intRandomBlue), (double)(intRandomGreen), (double)(intRandomRed)));
                //    }
                //    CvInvoke.Imshow("7", imgContours);
                //}

                if (listOfListsOfMatchingCharsInPlate == null)
                {
                    //if (frm.IsCheckedSteps() == true)
                    //{
                    //    intPlateCounter = intPlateCounter + 1;
                    //    CvInvoke.DestroyWindow("8");
                    //    CvInvoke.DestroyWindow("9");
                    //    CvInvoke.DestroyWindow("10");
                    //    CvInvoke.WaitKey(0);
                    //}
                    possiblePlate.strChars = "";
                }
                else if (listOfListsOfMatchingCharsInPlate.Count == 0)
                {
                    //if (frm.IsCheckedSteps() == true)
                    //{
                    //    intPlateCounter = intPlateCounter + 1;
                    //    CvInvoke.DestroyWindow("8");
                    //    CvInvoke.DestroyWindow("9");
                    //    CvInvoke.DestroyWindow("10");
                    //    CvInvoke.WaitKey(0);
                    //}
                    possiblePlate.strChars = "";
                }

                for (int i = 0; i <= listOfListsOfMatchingCharsInPlate.Count-1; i++)
                {
                    listOfListsOfMatchingCharsInPlate[i].Sort((oneChar, otherChar)=>oneChar.boundingRect.X.CompareTo(otherChar.boundingRect.X));

                    listOfListsOfMatchingCharsInPlate[i] = removeInnerOverlappingChars(listOfListsOfMatchingCharsInPlate[i]);
                }

                //if (frm.IsCheckedSteps() == true)
                //{
                //    imgContours = new Mat(possiblePlate.imgThresh.Size, DepthType.Cv8U, 3);

                //    foreach (var listOfMatchingChars in listOfListsOfMatchingCharsInPlate)
                //    {
                //        int intRandomBlue = random.Next(0, 256);
                //        int intRandomGreen = random.Next(0, 256);
                //        int intRandomRed = random.Next(0, 256);

                //        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();//? posprawdzac czy maja byc te same zmienne

                //        foreach (var matchingChar in listOfMatchingChars)
                //        {
                //            contours.Push(matchingChar.contour);
                //        }
                //        CvInvoke.DrawContours(imgContours, contours, -1, new MCvScalar((double)(intRandomBlue), (double)(intRandomGreen), (double)(intRandomRed)));
                //        CvInvoke.Imshow("8", imgContours);
                //    }
                //}

                int intLenOfLongestListOfChars = 0;
                int intIndexOfLongestListOfChars = 0;

                for (int i = 0; i <= listOfListsOfMatchingCharsInPlate.Count-1; i++)
                {
                    if (listOfListsOfMatchingCharsInPlate[i].Count > intLenOfLongestListOfChars)
                    {
                        intLenOfLongestListOfChars = listOfListsOfMatchingCharsInPlate[i].Count;
                        intIndexOfLongestListOfChars = i;
                    }
                }

                if (intIndexOfLongestListOfChars >0)
                {
                    List<PossibleChar> longestListOfMatchingCharsInPlate = listOfListsOfMatchingCharsInPlate[intIndexOfLongestListOfChars - 1];

                    //if (frm.IsCheckedSteps() == true)
                    //{
                    //    imgContours = new Mat(possiblePlate.imgThresh.Size, DepthType.Cv8U, 3);
                    //    VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();//? posprawdzac czy maja byc te same zmienne

                    //    foreach (var matchingChar in longestListOfMatchingCharsInPlate)
                    //    {
                    //        contours.Push(matchingChar.contour);
                    //    }

                    //    CvInvoke.DrawContours(imgContours, contours, -1, SCALAR_WHITE);
                    //    CvInvoke.Imshow("9", imgContours);
                    //}

                    possiblePlate.strChars = recognizeCharsInPlate(possiblePlate.imgThresh, longestListOfMatchingCharsInPlate);
                }
                //if (frm.IsCheckedSteps() == true)
                //{
                //    intPlateCounter = intPlateCounter + 1;
                //    CvInvoke.WaitKey(0);
                //}
            }//end foreach

            //jakis tekst do tekstboxa   ewentulnie dorobic

            return listOfPossiblePlates;
        }//end detectcharsinplate

        public static List<PossibleChar> findPossibleCharsInPlate(Mat imgGrayscale, Mat imgThresh)
        {
            List<PossibleChar> listOfPossibleChars = new List<PossibleChar>();

            Mat imgThreshCopy = new Mat();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            imgThreshCopy = imgThresh.Clone();

            CvInvoke.FindContours(imgThreshCopy, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i <= contours.Size-1; i++)
            {
                PossibleChar possibleChar = new PossibleChar(contours[i]);

                if (checkIfPossibleChar(possibleChar))
                {
                    listOfPossibleChars.Add(possibleChar);
                }
            }
            return listOfPossibleChars;
        }// end findpossiblecharinplate

        public static bool checkIfPossibleChar(PossibleChar possibleChar)
        {
            if ((possibleChar.intRectArea > MIN_RECT_AREA &&
            possibleChar.boundingRect.Width > MIN_PIXEL_WIDTH && possibleChar.boundingRect.Height > MIN_PIXEL_HEIGHT &&
            MIN_ASPECT_RATIO < possibleChar.dblAspectRatio && possibleChar.dblAspectRatio < MAX_ASPECT_RATIO))
                return true;
            else
                return false;
        }// end checkifpossiblechar

        public static List<List<PossibleChar>> findListOfListsOfMatchingChars(List<PossibleChar> listOfPossibleChars)
        {
            List<List<PossibleChar>> listOfListsOfMatchingChars = new List<List<PossibleChar>>();

            foreach (PossibleChar possibleChar in listOfPossibleChars)
            {
                List<PossibleChar> listOfMatchingChars = findListOfMatchingChars(possibleChar, listOfPossibleChars);

                listOfMatchingChars.Add(possibleChar);

                if (listOfMatchingChars.Count > MIN_NUMBER_OF_MATCHING_CHARS)
                {
                    listOfListsOfMatchingChars.Add(listOfMatchingChars);

                    List<PossibleChar> listOfPossibleCharsWithCurrentMatchesRemoved = listOfPossibleChars.Except(listOfMatchingChars).ToList();

                    List<List<PossibleChar>> recursiveListOfListsOfMatchingChars = new List<List<PossibleChar>>();

                    recursiveListOfListsOfMatchingChars = findListOfListsOfMatchingChars(listOfPossibleCharsWithCurrentMatchesRemoved);

                    foreach (var recursiveListOfMatchingChars in recursiveListOfListsOfMatchingChars)
                    {
                        listOfListsOfMatchingChars.Add(recursiveListOfMatchingChars);
                    }
                }

            }//end foreach

            return listOfListsOfMatchingChars;

        }//end findListOfListsOfMatchingChars

        public static List<PossibleChar> findListOfMatchingChars(PossibleChar possibleChar, List<PossibleChar> listOfChars)
        {
            List<PossibleChar> listOfMatchingChars = new List<PossibleChar>();

            foreach (PossibleChar possibleMatchingChar in listOfChars)
            {
                if (!possibleMatchingChar.Equals(possibleChar))
                //if (possibleMatchingChar!=possibleChar)
                {
                    //break;//sprobowac jak wyzej


                    double dblDistanceBetweenChars = distanceBetweenChars(possibleChar, possibleMatchingChar);
                    double dblAngleBetweenChars = angleBetweenChars(possibleChar, possibleMatchingChar);
                    double dblChangeInArea = Math.Abs(possibleMatchingChar.intRectArea - possibleChar.intRectArea) / possibleChar.intRectArea;


                    double dblChangeInWidth = Math.Abs(possibleMatchingChar.boundingRect.Width - possibleChar.boundingRect.Width) / possibleChar.boundingRect.Width;
                    double dblChangeInHeight = Math.Abs(possibleMatchingChar.boundingRect.Height - possibleChar.boundingRect.Height) / possibleChar.boundingRect.Height;

                    if (dblDistanceBetweenChars < (possibleChar.dblDiagonalSize * MAX_DIAG_SIZE_MULTIPLE_AWAY) &&
                        dblAngleBetweenChars < MAX_ANGLE_BETWEEN_CHARS &&
                        dblChangeInArea < MAX_CHANGE_IN_AREA &&
                        dblChangeInWidth < MAX_CHANGE_IN_WIDTH &&
                        dblChangeInHeight < MAX_CHANGE_IN_HEIGHT)
                    {
                        listOfMatchingChars.Add(possibleMatchingChar);
                    }
                }
            }
            return listOfMatchingChars;
        }//end findListOfMatchingChars

        public static double distanceBetweenChars(PossibleChar firstChar, PossibleChar secondChar)
        {
            int intX = Math.Abs(firstChar.intCenterX - secondChar.intCenterX);
            int intY = Math.Abs(firstChar.intCenterY - secondChar.intCenterY);

            return Math.Sqrt(Math.Pow(intX , 2) + Math.Pow(intY , 2));
        }//end

        public static double angleBetweenChars(PossibleChar firstChar, PossibleChar secondChar)
        {
            double dblAdj = (double)(Math.Abs(firstChar.intCenterX - secondChar.intCenterX));
            double dblOpp = (double)(Math.Abs(firstChar.intCenterY - secondChar.intCenterY));

            double dblAngleInRad = Math.Atan(dblOpp / dblAdj);
            double dblAngleInDeg = dblAngleInRad * (180.0 / Math.PI);

            return dblAngleInDeg;
        }//end

        public static List<PossibleChar> removeInnerOverlappingChars(List<PossibleChar> listOfMatchingChars)
        {
            List<PossibleChar> listOfMatchingCharsWithInnerCharRemoved = new List<PossibleChar>(listOfMatchingChars);

            foreach (PossibleChar currentChar in listOfMatchingChars)
            {
                foreach (PossibleChar otherChar in listOfMatchingChars)
                {
                    if (!currentChar.Equals(otherChar))
                    {
                        if (distanceBetweenChars(currentChar, otherChar) < (currentChar.dblDiagonalSize * MIN_DIAG_SIZE_MULTIPLE_AWAY))
                        {
                            if (currentChar.intRectArea < otherChar.intRectArea)
                            {
                                if(listOfMatchingCharsWithInnerCharRemoved.Contains(currentChar))
                                {
                                    listOfMatchingCharsWithInnerCharRemoved.Remove(currentChar);
                                }                               
                            }
                            else if (listOfMatchingCharsWithInnerCharRemoved.Contains(otherChar))
                            {
                                listOfMatchingCharsWithInnerCharRemoved.Remove(otherChar);
                            }
                        }
                    }
                }
            }
            return listOfMatchingCharsWithInnerCharRemoved;
        }//end

        public static string recognizeCharsInPlate(Mat imgThresh, List<PossibleChar> listOfMatchingChars)
        {
            string strChars = "";
            Mat imgThreshColor = new Mat();

            listOfMatchingChars.Sort((firstChar, secondChar) => firstChar.boundingRect.X.CompareTo(secondChar.boundingRect.X));

            CvInvoke.CvtColor(imgThresh, imgThreshColor, ColorConversion.Gray2Bgr);

            foreach (PossibleChar currentChar in listOfMatchingChars)
            {
                CvInvoke.Rectangle(imgThreshColor, currentChar.boundingRect, SCALAR_GREEN, 2);

                Mat imgROItoBeCloned = new Mat(imgThresh, currentChar.boundingRect);

                Mat imgROI = imgROItoBeCloned.Clone();

                Mat imgROIResized = new Mat();

                CvInvoke.Resize(imgROI, imgROIResized, new Size(RESIZED_CHAR_IMAGE_WIDTH, RESIZED_CHAR_IMAGE_HEIGHT));

                Matrix<Single> mtxTemp = new Matrix<Single>(imgROIResized.Size);

                Matrix<Single> mtxTempReshaped = new Matrix<Single>(1, RESIZED_CHAR_IMAGE_WIDTH * RESIZED_CHAR_IMAGE_HEIGHT);

                imgROIResized.ConvertTo(mtxTemp, DepthType.Cv32F);

                for (int intRow = 0; intRow <= RESIZED_CHAR_IMAGE_HEIGHT-1; intRow++)
                {
                    for (int intCol = 0; intCol <= RESIZED_CHAR_IMAGE_WIDTH-1; intCol++)
                    {
                        mtxTempReshaped[0, (intRow * RESIZED_CHAR_IMAGE_WIDTH) + intCol] = mtxTemp[intRow, intCol];
                    }
                }

                Single sngCurrentChar;

                sngCurrentChar = kNearest.Predict(mtxTempReshaped);


                strChars = strChars + (char)(Convert.ToInt32(sngCurrentChar));
            }
            //CvInvoke.Imshow("10", imgThreshColor);

            return strChars;
        }//end


    }
}
