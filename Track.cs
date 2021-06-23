using System.Collections.Generic;

namespace sortcs
{
    public class Track
    {
        public int TrackId { get; set; }

        public int TotalMisses { get; set; }

        public int Misses { get; set; }

        public int Class { get; set; }

        public string ClassName { get; set; }

        public List<BoundingBox> History { get; set; }

        public TrackState State { get; set; }
    }
}