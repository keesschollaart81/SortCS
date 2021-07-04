using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using HungarianAlgorithm;
using SortCS.Kalman;

namespace SortCS
{
    public class SortTracker : ITracker
    {
        private readonly Dictionary<int, (Track Track, KalmanBoxTracker Tracker)> _trackers;
        private int _frameCount;
        private int _trackerIndex = 1;

        public SortTracker()
        {
            _trackers = new Dictionary<int, (Track, KalmanBoxTracker)>();
            _frameCount = 0;
        }

        public int MaxAge { get; init; } = 1;

        public int MinHits { get; init; } = 3;

        public float IouThreshold { get; init; } = 0.3f;

        public IEnumerable<Track> Track(IEnumerable<BoundingBox> boxes)
        {
            _frameCount++;

            var trackedBoxes = new Dictionary<int, BoundingBox>();

            foreach (var tracker in _trackers)
            {
                var box = tracker.Value.Tracker.Predict();
                trackedBoxes.Add(tracker.Key, box);
            }

            var boxesArray = boxes.ToArray();

            var (matchedBoxes, unmatchedBoxes) = MatchDetectionsWithTrackers(boxesArray, trackedBoxes.Values);

            var activeTrackids = new HashSet<int>();
            foreach (var item in matchedBoxes)
            {
                var track = _trackers[trackedBoxes.ElementAt(item.Key).Key];
                track.Track.History.Add(item.Value);
                track.Track.Misses = 0;
                track.Track.State = TrackState.Active;
                track.Tracker.Update(item.Value);

                activeTrackids.Add(track.Track.TrackId);
            }

            var missedTracks = _trackers.Where(x => !activeTrackids.Contains(x.Key));
            foreach (var missedTrack in missedTracks)
            {
                missedTrack.Value.Track.Misses++;
                missedTrack.Value.Track.TotalMisses++;
                missedTrack.Value.Track.State = TrackState.Ending;
            }

            var toRemove = _trackers.Where(x => x.Value.Track.Misses > MaxAge).ToList();
            foreach (var tr in toRemove)
            {
                tr.Value.Track.State = TrackState.Ended;
                _trackers.Remove(tr.Key);
            }

            foreach (var unmatchedBox in unmatchedBoxes)
            {
                var track = new Track
                {
                    TrackId = _trackerIndex++,
                    Class = unmatchedBox.Class,
                    ClassName = unmatchedBox.ClassName,
                    History = new List<BoundingBox>() { unmatchedBox },
                    Misses = 0,
                    State = TrackState.Started,
                    TotalMisses = 0
                };
                _trackers.Add(track.TrackId,(track, new KalmanBoxTracker(unmatchedBox)));
            }

            var result = _trackers.Select(x => x.Value.Track).Concat(toRemove.Select(y => y.Value.Track));
            Log(result);
            return result;
        }

        private void Log(IEnumerable<Track> tracks)
        {
            if (!tracks.Any())
            {
                return;
            }

            var tracksWithHistory = tracks.Where(x => x.History != null);
            var longest = tracksWithHistory.Max(x => x.History.Count);
            var anyStarted = tracksWithHistory.Any(x => x.History.Count == 1 && x.Misses == 0);
            var ended = tracks.Count(x => x.State == TrackState.Ended);
            if (anyStarted || ended > 0)
            {
                var tracksStr = tracks.Select(x => $"{x.TrackId}{(x.State == TrackState.Active ? null : $": {x.State}")}");

                Console.WriteLine($"Tracks: [{string.Join(",", tracksStr)}], Longest: {longest}, Ended: {ended}");
            }
        }

        private (Dictionary<int, BoundingBox> Matched, ICollection<BoundingBox> Unmatched) MatchDetectionsWithTrackers(
            ICollection<BoundingBox> boxes,
            ICollection<BoundingBox> trackers)
        {
            if (trackers.Count == 0)
            {
                return (new(), boxes);
            }

            var matrix = boxes.SelectMany((box) => trackers.Select((tracker) =>
            {
                var intersection = RectangleF.Intersect(box.Box, tracker.Box);
                var union = RectangleF.Union(box.Box, tracker.Box);
                var intersectionArea = (double)(intersection.Width * intersection.Height);
                var unionArea = (double)(union.Width * union.Height);

                var iou = unionArea < double.Epsilon ? 0 : intersectionArea / unionArea;

                return (int)(100 * -iou);
            })).ToArray(boxes.Count, trackers.Count);

            if (boxes.Count > trackers.Count)
            {
                var extra = new int[boxes.Count - trackers.Count];
                matrix = Enumerable.Range(0, boxes.Count)
                    .SelectMany(row => Enumerable.Range(0, trackers.Count).Select(col => matrix[row, col]).Concat(extra))
                    .ToArray(boxes.Count, boxes.Count);
            }

            var original = (int[,])matrix.Clone();
            var minimalThreshold = (int)(-IouThreshold * 100);
            var boxTrackerMapping = matrix.FindAssignments()
                .Select((ti, bi) => (bi, ti))
                .Where(bt => bt.ti < trackers.Count && original[bt.bi, bt.ti] <= minimalThreshold)
                .ToDictionary(bt => bt.bi, bt => bt.ti);

            var unmatchedBoxes = boxes.Where((_, index) => !boxTrackerMapping.ContainsKey(index)).ToArray();
            var matchedBoxes = boxes.Select((box, index) => boxTrackerMapping.TryGetValue(index, out var tracker)
                    ? (Tracker: tracker, Box: box)
                    : (Tracker: -1, Box: null))
                .Where(tb => tb.Tracker != -1)
                .ToDictionary(tb => tb.Tracker, tb => tb.Box);

            return (matchedBoxes, unmatchedBoxes);
        }
    }
}
