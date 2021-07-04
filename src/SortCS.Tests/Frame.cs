using System.Collections.Generic;
using System.Drawing;

namespace SortCS.Tests
{
    public class Frame
    {
        public Frame(List<RectangleF> boundingBoxes)
        {
            BoundingBoxes = boundingBoxes;
        }
        public List<RectangleF> BoundingBoxes { get; set; }
    }
}
