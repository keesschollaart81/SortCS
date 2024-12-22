using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using HungarianAlgorithm;
using Microsoft.Extensions.Logging;
using SortCS.Kalman;

namespace SortCS
{
    public class SortTracker : ITracker
    {
        private readonly Dictionary<int, (Track Track, KalmanBoxTracker Tracker)> _trackers;
        private readonly ILogger<SortTracker> _logger;
        private int _trackerIndex = 1; // MOT Evaluations requires a start index of 1

        public SortTracker(float iouThreshold = 0.3f, int maxMisses = 3)
        {
            _trackers = new Dictionary<int, (Track, KalmanBoxTracker)>();
            IouThreshold = iouThreshold;
            MaxMisses = maxMisses;
        }

        public SortTracker(ILogger<SortTracker> logger, float iouThreshold = 0.3f, int maxMisses = 3)
            : this(iouThreshold, maxMisses)
        {
            _logger = logger;
        }

        public float IouThreshold { get; private init; }

        public int MaxMisses { get; private init; }

        public IEnumerable<Track> Track(IEnumerable<RectangleF> boxes)
        {
            var predictions = new Dictionary<int, RectangleF>();

            foreach (var tracker in _trackers)
            {
                var prediction = tracker.Value.Tracker.Predict();
                predictions.Add(tracker.Key, prediction);
            }

            var boxesArray = boxes.ToArray();

            var (matchedBoxes, unmatchedBoxes) = MatchDetectionsWithPredictions(boxesArray, predictions.Values);

            var activeTrackids = new HashSet<int>();
            foreach (var item in matchedBoxes)
            {
                var prediction = predictions.ElementAt(item.Key);
                var track = _trackers[prediction.Key];
                track.Track.History.Add(item.Value);
                track.Track.Misses = 0;
                track.Track.State = TrackState.Active;
                track.Tracker.Update(item.Value);
                track.Track.Prediction = prediction.Value;

                activeTrackids.Add(track.Track.TrackId);
            }

            var missedTracks = _trackers.Where(x => !activeTrackids.Contains(x.Key));
            foreach (var missedTrack in missedTracks)
            {
                missedTrack.Value.Track.Misses++;
                missedTrack.Value.Track.TotalMisses++;
                missedTrack.Value.Track.State = TrackState.Ending;
            }

            var toRemove = _trackers.Where(x => x.Value.Track.Misses > MaxMisses).ToList();
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
                    History = new List<RectangleF>() { unmatchedBox },
                    Misses = 0,
                    State = TrackState.Started,
                    TotalMisses = 0,
                    Prediction = unmatchedBox
                };
                _trackers.Add(track.TrackId, (track, new KalmanBoxTracker(unmatchedBox)));
            }

            var result = _trackers.Select(x => x.Value.Track).Concat(toRemove.Select(y => y.Value.Track));
            Log(result);
            return result;
        }

        private void Log(IEnumerable<Track> tracks)
        {
            if (_logger == null || !tracks.Any())
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

                _logger.LogDebug("Tracks: [{tracks}], Longest: {longest}, Ended: {ended}", string.Join(",", tracksStr), longest, ended);
            }
        }

        private (Dictionary<int, RectangleF> Matched, ICollection<RectangleF> Unmatched) MatchDetectionsWithPredictions(
            RectangleF[] boxes,
            ICollection<RectangleF> trackPredictions)
        {
            if (trackPredictions.Count == 0)
            {
                return (new(), boxes);
            }

            var matrix = new int[boxes.Length, trackPredictions.Count];
            var trackPredictionsArray = trackPredictions.ToArray();

            for (int i = 0; i < boxes.Length; i++)
            {
                for (int j = 0; j < trackPredictionsArray.Length; j++)
                {
                    matrix[i, j] = (int)(-100 * IoU(boxes[i], trackPredictionsArray[j]));
                }
            }

            if (boxes.Length > trackPredictions.Count)
            {
                var extra = new int[boxes.Length - trackPredictions.Count];
                matrix = Enumerable.Range(0, boxes.Length)
                    .SelectMany(row => Enumerable.Range(0, trackPredictions.Count).Select(col => matrix[row, col]).Concat(extra))
                    .ToArray(boxes.Length, boxes.Length);
            }

            var original = (int[,])matrix.Clone();
            var minimalThreshold = (int)(-IouThreshold * 100);
            var boxTrackerMapping = matrix.FindAssignments()
                .Select((ti, bi) => (bi, ti))
                .Where(bt => bt.ti < trackPredictions.Count && original[bt.bi, bt.ti] <= minimalThreshold)
                .ToDictionary(bt => bt.bi, bt => bt.ti);

            var unmatchedBoxes = boxes.Where((_, index) => !boxTrackerMapping.ContainsKey(index)).ToArray();
            var matchedBoxes = boxes.Select((box, index) => boxTrackerMapping.TryGetValue(index, out var tracker)
                   ? (Tracker: tracker, Box: box)
                   : (Tracker: -1, Box: RectangleF.Empty))
                .Where(tb => tb.Tracker != -1)
                .ToDictionary(tb => tb.Tracker, tb => tb.Box);

            return (matchedBoxes, unmatchedBoxes);
        }

        private double IoU(RectangleF a, RectangleF b)
        {
            RectangleF intersection = RectangleF.Intersect(a, b);
            if (intersection.IsEmpty)
            {
                return 0;
            }

            double intersectArea = (1.0 + intersection.Width) * (1.0 + intersection.Height);
            double unionArea = ((1.0 + a.Width) * (1.0 + a.Height)) + ((1.0 + b.Width) * (1.0 + b.Height)) - intersectArea;
            return intersectArea / (unionArea + 1e-5);
        }
    }
}
