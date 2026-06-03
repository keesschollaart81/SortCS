using System.Drawing;

namespace SortCS.Tests;

public class Frame(List<RectangleF> boundingBoxes)
{
    public List<RectangleF> BoundingBoxes { get; set; } = boundingBoxes;
}
