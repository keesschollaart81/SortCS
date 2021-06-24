using System.Collections.Generic;

namespace sortcs
{
    public interface ITracker
    {
        IEnumerable<Track> Track(IEnumerable<BoundingBox> boxes);
    }
}