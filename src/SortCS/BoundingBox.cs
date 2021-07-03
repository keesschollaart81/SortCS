using System.Drawing;

namespace SortCS
{
    public record BoundingBox
    {
        public BoundingBox()
        {
        }

        public BoundingBox(int classIx, string className, float tlx, float tly, float w, float h, float score)
        {
            Class = classIx;
            ClassName = className;
            Box = new RectangleF(tlx, tly, w, h);
            Score = score;
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
    }
}