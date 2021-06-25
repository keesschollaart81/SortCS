using System.Collections.Generic;

namespace sortcs
{
    public class Frame
    {
        public Frame(List<BoundingBox> boundingBoxes)
        {
            BoundingBoxes = boundingBoxes;
        }
        public List<BoundingBox> BoundingBoxes { get; set; }
    }
}
