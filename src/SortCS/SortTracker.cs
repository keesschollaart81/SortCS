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
        private readonly List<(Track Track, KalmanBoxTracker Tracker)> _trackers;
        private int _frameCount;
        private int _trackerIndex;

        public SortTracker()
        {
            _trackers = new List<(Track, KalmanBoxTracker)>();
            _frameCount = 0;
        }

        public int MaxAge { get; init; } = 1;

        public int MinHits { get; init; } = 3;

        public float IouThreshold { get; init; } = 0.3f;

        public IEnumerable<Track> Track(IEnumerable<BoundingBox> boxes)
        {
            _frameCount++;

            var trackedBoxes = new List<BoundingBox>();

            foreach (var tracker in _trackers)
            {
                var box = tracker.Tracker.Predict();
                trackedBoxes.Add(box);
            }

            var boxesArray = boxes.ToArray();

            var (matchedBoxes, unmatchedBoxes) = MatchDetectionsWithTrackers(boxesArray, trackedBoxes);

            foreach (var item in matchedBoxes)
            {
                var track = _trackers[item.Key];
                track.Track.History.Add(item.Value);
                track.Track.Misses = 0;
                track.Track.State = TrackState.Active;
                track.Tracker.Update(item.Value);
            }

            var missedTracks = _trackers.Where(x => !matchedBoxes.ContainsKey(x.Track.TrackId));
            foreach (var missedTrack in missedTracks)
            {
                missedTrack.Track.Misses++;
                missedTrack.Track.TotalMisses++;
                missedTrack.Track.State = TrackState.Ending;
            }

            var toRemove = _trackers.Where(x => x.Track.Misses > MaxAge).ToList();
            foreach (var tr in toRemove)
            {
                tr.Track.State = TrackState.Ended;
                _trackers.Remove(tr);
            }

            foreach (var unmatchedBox in unmatchedBoxes)
            {
                var track = new Track
                {
                    TrackId = _trackerIndex++,
                    Class = unmatchedBox.Class,
                    ClassName = unmatchedBox.ClassName,
                    History = new List<BoundingBox>(),
                    Misses = 0,
                    State = TrackState.Started,
                    TotalMisses = 0
                };
                _trackers.Add((track, new KalmanBoxTracker(unmatchedBox)));
            }

            var result = _trackers.Select(x => x.Track).Concat(toRemove.Select(y => y.Track));
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
