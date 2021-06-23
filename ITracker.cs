using System.Collections.Generic;

namespace sortcs
{
    public interface ITracker
    {
        public IEnumerable<Track> Track(IEnumerable<BoundingBox> boxes);
    }
}