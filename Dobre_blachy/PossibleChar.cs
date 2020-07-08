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
    public class PossibleChar
    {
        public VectorOfPoint contour { get; set; }

        public Rectangle boundingRect { get; set; }

        public int intCenterX { get; set; }
        public int intCenterY { get; set; }

        public double dblDiagonalSize { get; set; }
        public double dblAspectRatio { get; set; }

        public int intRectArea { get; set; }

        public PossibleChar(VectorOfPoint _contour)
        {
            contour = _contour;

            boundingRect = CvInvoke.BoundingRectangle(contour);

            intCenterX = (int)((boundingRect.Left + boundingRect.Right) / 2);
            intCenterY = (int)((boundingRect.Top + boundingRect.Bottom) / 2);

            dblDiagonalSize = Math.Sqrt(Math.Pow(boundingRect.Width , 2) + Math.Pow(boundingRect.Height , 2));

            dblAspectRatio = (double)(boundingRect.Width) / (double)(boundingRect.Height);

            intRectArea = boundingRect.Width * boundingRect.Height;

        }


    }
}
