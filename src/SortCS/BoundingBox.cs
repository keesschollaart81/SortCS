using System.Drawing;
using System.Linq;

namespace sortcs
{
    public class BoundingBox
    {
        public BoundingBox()
        {

        }
        public BoundingBox(int classIx, string className, float tly, float tlx, float w, float h, float score)
        {
            Class = classIx;
            ClassName = className;
            Box = new RectangleF(tlx, tly, w, h);
            Score = score;
            IsInvalid = Box.IsEmpty;
        }

        public RectangleF Box { get; set; }

        public PointF Center
        {
            get
            {
                return new PointF(Box.Left + (Box.Width / 2f), Box.Top + (Box.Left / 2f));
            }
        }

        public int Class { get; set; }

        public string ClassName { get; set; }

        public float Score { get; set; }
        public bool IsInvalid { get; }
    }
}