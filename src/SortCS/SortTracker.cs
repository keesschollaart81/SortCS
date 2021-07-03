using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using HungarianAlgorithm;
using SortCS.Kalman;

namespace SortCS
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

                if (box.Box.IsEmpty)
                {
                    toDelete.Add(tracker);
                }
                else
                {
                    trackedBoxes.Add(box);
                }
            }

            _trackers.RemoveAll(t => toDelete.Contains(t));

            var boxesArray = boxes.ToArray();

            var (matchedBoxes, unmatchedBoxes) = MatchDetectionsWithTrackers(boxesArray, trackedBoxes);

            foreach (var item in matchedBoxes)
            {
                _trackers[item.Key].Update(item.Value);
            }

            foreach (var unmatchedBox in unmatchedBoxes)
            {
                _trackers.Add(new KalmanBoxTracker(unmatchedBox));
            }

            return _trackers.Select(x => new SortCS.Track
            {
                State = TrackState.Active,
                TrackId = x.Id,
                History = new List<BoundingBox> { x.LastBoundingBox },
                Class = 0,
                ClassName = "",
                Misses = 0,
                TotalMisses = 0
            });
        }

        private (Dictionary<int, BoundingBox> Matched, ICollection<BoundingBox> Unmatched) MatchDetectionsWithTrackers(
            ICollection<BoundingBox> boxes,
            ICollection<BoundingBox> trackers)
        {
            if (trackers.Count == 0)
            {
                return (new(), boxes);
            }

            var ious = trackers.SelectMany((tracker) => boxes.Select((box) =>
            {
                var intersection = RectangleF.Intersect(box.Box, tracker.Box);
                var union = RectangleF.Union(box.Box, tracker.Box);
                var intersectionArea = (double)(intersection.Width * intersection.Height);
                var unionArea = (double)(union.Width * union.Height);

                var iou = unionArea < double.Epsilon ? 0 : intersectionArea / unionArea;

                return (int)((1 - iou) * 100); // int costs?
            }));
            var matrix = ious.ToArray(boxes.Count, trackers.Count);
            var matrix2 = ious.ToArray(boxes.Count, trackers.Count);

            var matchedBoxIndices = matrix.FindAssignments();

            // here we filter the matches that did not have a cost of 100
            // todo: filter before `FindAssignments()` so that all matches with a cost of 100 are ignored / not part of the computation
            var matchedBoxIndicesWithOverlap = matchedBoxIndices.ToDictionary(x => x, boxIx =>
            {
                for (var trackIx = 0; trackIx < matrix2.GetLength(0); trackIx++)
                {
                    if (matrix2[trackIx, boxIx] < 100)
                    {
                        return (int?)trackIx;
                    }
                }

                return null;
            });

            var matchedBoxes = new Dictionary<int, BoundingBox>();
            for (var bi = 0; bi < boxes.Count; bi++)
            {
                if (!matchedBoxIndicesWithOverlap.ContainsKey(bi))
                {
                    continue;
                }

                var trackId = matchedBoxIndicesWithOverlap[bi];
                if (trackId.HasValue)
                {
                    matchedBoxes.Add(trackId.Value, boxes.ElementAt(bi));
                }
            }

            var unmatched = boxes
                .Where((b, index) => !matchedBoxIndicesWithOverlap.ContainsKey(index) || !matchedBoxIndicesWithOverlap[index].HasValue)
                .ToList();

            return (matchedBoxes, unmatched);
        }
         }
}
