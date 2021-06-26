using System.Collections.Generic;

namespace SortCS
{
    public record Track
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