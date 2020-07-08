using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;

namespace Dobre_blachy
{
    public class PossiblePlate
    {
        public Mat imgPlate { get; set; }
        public Mat imgGrayscale { get; set; }
        public Mat imgThresh { get; set; }

        public RotatedRect rrLocationOfPlateInScene { get; set; }

        public string strChars { get; set; }

        public PossiblePlate()
        {
            imgPlate = new Mat();
            imgGrayscale = new Mat();
            imgThresh = new Mat();

            rrLocationOfPlateInScene = new RotatedRect();

            strChars = "";
        }
    }
}
