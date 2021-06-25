using System.Collections.Generic;
using SortCS.Kalman;

namespace sortcs
{
    public class SortTracker : ITracker
    {
        private readonly List<KalmanBoxTracker> _trackers;
        private int _frameCount;

        public SortTracker()
        {
            _trackers = new List<KalmanBoxTracker>();
            _frameCount = 0;
        }

        public int MaxAge { get; init; } = 1;
        public int MinHits { get; init; } = 3;
        public float IouThreshold { get; init; } = 0.3f;

        public IEnumerable<Track> Track(IEnumerable<BoundingBox> boxes)
        {
            _frameCount++;

            var toDelete = new List<KalmanBoxTracker>();
            var trackedBoxes = new List<BoundingBox>();

            foreach (var tracker in _trackers)
            {
                var box = tracker.Predict();

                if (box.IsInvalid)
                {
                    toDelete.Add(tracker);
                }
                else
                {
                    trackedBoxes.Add(box);
                }
            }

            _trackers.RemoveAll(t => toDelete.Contains(t));

            yield break;
        }

        public void MatchDetectionsWithTrackers(
            IEnumerable<BoundingBox> boxes,
            IEnumerable<BoundingBox> trackers)
        {
            // todo: port from sort.py
        }
    }

}
