using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;

namespace Dobre_blachy
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            
        }
        public Single IMAGE_BOX_PCT_SHOW_STEPS_NOT_CHECKED = 75;
        public Single TEXT_BOX_PCT_SHOW_STEPS_NOT_CHECKED = 25;

        public Single IMAGE_BOX_PCT_SHOW_STEPS_CHECKED = 55;
        public Single TEXT_BOX_PCT_SHOW_STEPS_CHECKED = 45;

        public MCvScalar SCALAR_RED = new MCvScalar(0.0, 0.0, 255.0);
        public MCvScalar SCALAR_YELLOW = new MCvScalar(0.0, 255.0, 255.0);

        private void frmMain_Load(object sender, EventArgs e) 
        {
            bool blnKNNTrainingSuccessful = DetectChars.loadKNNDataAndTrainKNN();

            if (blnKNNTrainingSuccessful == false)
            {
                MessageBox.Show("Failed to load TrainData :(");
                txtInfo.Text= "Failed to load TrainData :(";
                btnOpenFile.Enabled = false;
                return;
            }
        }

        public bool IsCheckedSteps()
        {
            if (cbShowSteps.Checked)
                return true;
            else
                return false;
                //statuscheckbox = true;
        }

        public void WriteToTxtBox(string txt)
        {
            txtInfo.AppendText(txt);
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            Mat imgOriginalScene = new Mat();
           
                OpenFileDialog openFileDialog = new OpenFileDialog();

                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "Img Files|*.bmp;*.png;*.jpg;*.jpeg";
                openFileDialog.DefaultExt = ".png";

            

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                //Image<Bgr, byte> input_img = new Image<Bgr, byte>(openFileDialog.FileName);
                //imgOriginalScene = input_img.Mat;
                //ibOriginal.Image = imgOriginalScene;
                lblChosenFile.Text = openFileDialog.FileName;
                CvInvoke.DestroyAllWindows();
                imgOriginalScene = CvInvoke.Imread(openFileDialog.FileName);
                ibOriginal.Image = imgOriginalScene;
                }

            List<PossiblePlate> listOfPossiblePlates = DetectPlate.detectPlatesInScene(imgOriginalScene);
            listOfPossiblePlates = DetectChars.detectCharsInPlates(listOfPossiblePlates);

            if (listOfPossiblePlates == null)
            {
                MessageBox.Show("No detected :( :(");
            }
            else if (listOfPossiblePlates.Count == 0)
            {
                MessageBox.Show("No detected :( :(");
            }
            else
            {
                listOfPossiblePlates.Sort((onePlate, otherPlate) => otherPlate.strChars.Length.CompareTo(onePlate.strChars.Length));

                PossiblePlate licPlate = listOfPossiblePlates[0];

                CvInvoke.Imshow("final imgPlate", licPlate.imgPlate);
                CvInvoke.Imshow("final imgThresh", licPlate.imgThresh);

                if (licPlate.strChars.Length == 0)
                {
                    MessageBox.Show("No detected characters :( :(");
                }
                txtInfo.Text = DetectPlate.tmptxt;
                //drawRedRectangleAroundPlate(imgOriginalScene, licPlate);    do dorobienia funkcja

                drawRedRectangleAroundPlate(imgOriginalScene, licPlate);

                MessageBox.Show(licPlate.strChars);

                writeLicensePlateCharsOnImage(imgOriginalScene, licPlate);

                ibOriginal.Image = imgOriginalScene;

                CvInvoke.Imwrite("imgOriginalScene.png", imgOriginalScene);
            }
        }//end btnOpneFile

        private void drawRedRectangleAroundPlate(Mat imgOriginalScene, PossiblePlate licPlate)
        {
            PointF[] ptfRectPoints = new PointF[4];

            ptfRectPoints = licPlate.rrLocationOfPlateInScene.GetVertices();

            Point pt0 = new Point((int)(ptfRectPoints[0].X), (int)(ptfRectPoints[0].Y));
            Point pt1 = new Point((int)(ptfRectPoints[1].X), (int)(ptfRectPoints[1].Y));
            Point pt2 = new Point((int)(ptfRectPoints[2].X), (int)(ptfRectPoints[2].Y));
            Point pt3 = new Point((int)(ptfRectPoints[3].X), (int)(ptfRectPoints[3].Y));

            CvInvoke.Line(imgOriginalScene, pt0, pt1, SCALAR_RED, 2);
            CvInvoke.Line(imgOriginalScene, pt1, pt2, SCALAR_RED, 2);
            CvInvoke.Line(imgOriginalScene, pt2, pt3, SCALAR_RED, 2);
            CvInvoke.Line(imgOriginalScene, pt3, pt0, SCALAR_RED, 2);

        }//end

        private void writeLicensePlateCharsOnImage(Mat imgOriginalScene, PossiblePlate licPlate)
        {
            Point ptCenterOfTextArea = new Point();
            Point ptLowerLeftTextOrigin = new Point();

            FontFace fontFace = FontFace.HersheySimplex;
            double dblFontScale = licPlate.imgPlate.Height / 30;
            int intFontThickness = (int)(dblFontScale * 1.5);
            Size textSize = new Size();

            textSize.Width = (int)(dblFontScale * 18.5 * licPlate.strChars.Length);
            textSize.Height = (int)(dblFontScale * 25);

            ptCenterOfTextArea.X = (int)(licPlate.rrLocationOfPlateInScene.Center.X);

            if (licPlate.rrLocationOfPlateInScene.Center.Y < (imgOriginalScene.Height * 0.75))
            {
                ptCenterOfTextArea.Y = (int)(licPlate.rrLocationOfPlateInScene.Center.Y + (int)((double)(licPlate.rrLocationOfPlateInScene.MinAreaRect().Height) * 1.6));
            }
            else
            {
                ptCenterOfTextArea.Y = (int)(licPlate.rrLocationOfPlateInScene.Center.Y - (int)((double)(licPlate.rrLocationOfPlateInScene.MinAreaRect().Height) * 1.6));
            }

            ptLowerLeftTextOrigin.X = (int)(ptCenterOfTextArea.X - (textSize.Width / 2));
            ptLowerLeftTextOrigin.Y = (int)(ptCenterOfTextArea.Y + (textSize.Height / 2));

            CvInvoke.PutText(imgOriginalScene, licPlate.strChars, ptLowerLeftTextOrigin, fontFace, dblFontScale, SCALAR_YELLOW, intFontThickness);
        }
    }
}
