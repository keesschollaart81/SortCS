using System.Drawing;
using System.Linq;

namespace sortcs
{
    public class BoundingBox
    {
        public BoundingBox()
        {

        }
        public BoundingBox(int classIx, string className, float tly, float tlx, float bry, float brx, float score)
        {
            Class = classIx;
            ClassName = className;
            Box = new float[] { tly, tlx, bry, brx };
            Score = score;
            IsInvalid = Box.Any(b => float.IsNaN(b));
        }

        public float[] Box { get; set; }

        public int Class { get; set; }

        public string ClassName { get; set; }

        public float Score { get; set; }
        public bool IsInvalid { get; }
        public float Width { get => Box[3] - Box[1]; }

        public float Height { get => Box[2] - Box[0]; }

        public PointF Center { get => new PointF(Box[1] + ((Box[3] - Box[1]) / 2), Box[0] + ((Box[2] - Box[0]) / 2)); }
    }
}