using System.Drawing;

namespace SortCS;

public interface ITracker
{
    IEnumerable<Track> Track(IEnumerable<RectangleF> boxes);
}
