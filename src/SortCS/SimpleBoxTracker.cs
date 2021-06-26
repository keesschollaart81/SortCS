using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SortCS
{
    public class SimpleBoxTracker : ITracker
    {
        private readonly int _maxTrackLength;
        private readonly int _endTrackAfterNMisses;
        private readonly double _minFirstBoxScore;
        private int trackIndex = 1;

        public SimpleBoxTracker(int maxGap = 4)
        {
            _maxTrackLength = 40;
            _endTrackAfterNMisses = maxGap;
            _minFirstBoxScore = 0.3;
        }

        private List<Track> Tracks { get; } = new List<Track>();

        public IEnumerable<Track> Track(IEnumerable<BoundingBox> boxes)
        {
            var boxesCopy = boxes.ToList();

            var tracksAppendedThisFrame = new List<int>();
            foreach (var track in Tracks)
            {
                var lastBoxForTrack = track.History.Last();

                BoundingBox newBoxForTrack = null;
                double shortestDistanceFound = double.MaxValue;
                foreach (var box in boxesCopy.Where(x => x.Class == track.Class))
                {
                    var distance = GetDistance(lastBoxForTrack.Center, box.Center);

                    var foundCloserBoxInOtherTrack = FindCloserBoxInRemainingTracks(box, distance, tracksAppendedThisFrame);
                    if (foundCloserBoxInOtherTrack)
                    {
                        continue;
                    }

                    if (distance > shortestDistanceFound)
                    {
                        continue;
                    }

                    shortestDistanceFound = distance;
                    newBoxForTrack = box;
                }

                if (newBoxForTrack != null)
                {
                    boxesCopy.Remove(newBoxForTrack);
                    track.Misses = 0;
                    if (track.History.Count == _maxTrackLength)
                    {
                        track.History.RemoveAt(0);
                    }

                    track.History.Add(newBoxForTrack);
                }
                else
                {
                    track.TotalMisses++;
                    track.Misses++;
                }

                tracksAppendedThisFrame.Add(track.TrackId);
            }

            foreach (var track in Tracks)
            {
                if (track.Misses > _endTrackAfterNMisses)
                {
                    track.State = TrackState.Ended;
                }
                else if (track.History.Count == 1 && track.Misses == 0)
                {
                    track.State = TrackState.Started;
                }
                else if (track.Misses > 0)
                {
                    track.State = TrackState.Ending;
                }
                else
                {
                    track.State = TrackState.Active;
                }
            }

            var endedTracks = Tracks.Where(x => x.State == TrackState.Ended).ToList(); // create a new list with references to ended tracks
            Tracks.RemoveAll(x => x.State == TrackState.Ended); // remove references to tracks from 'active tracks' before we reset the Misses on the 'ended tracks'

            foreach (var t in endedTracks)
            {
                // the sequence of misses at the end of the track does not count
                t.TotalMisses -= t.Misses;
                t.Misses = 0;
            }

            Tracks.AddRange(CreateNewTracks(boxesCopy));

            var result = Tracks.Concat(endedTracks);

            if (result.Any())
            {
                var tracksWithHistory = result.Where(x => x.History != null);
                var longest = tracksWithHistory.Max(x => x.History.Count < _maxTrackLength ? x.History.Count : 0);
                var anyStarted = tracksWithHistory.Any(x => x.History.Count == 1 && x.Misses == 0);
                var ended = endedTracks.Count;
                if (anyStarted || ended > 0)
                {
                    var tracks = result.Select(x => $"{x.TrackId}{(x.State == TrackState.Active ? null : $": {x.State}")}");

                    Console.WriteLine($"Tracks: [{string.Join(",", tracks)}], Longest: {longest}, Ended: {ended}");
                }
            }

            return result;
        }

        private bool FindCloserBoxInRemainingTracks(BoundingBox boxToCompareWith, double withinDistance, List<int> visitedTracks)
        {
            foreach (var otherTrack in Tracks.Where(x => !visitedTracks.Contains(x.TrackId)))
            {
                var lastBoxForOtherTrack = otherTrack.History.Last();
                if (lastBoxForOtherTrack.Class != boxToCompareWith.Class)
                {
                    continue;
                }

                var distanceInOtherTrack = GetDistance(lastBoxForOtherTrack.Center, boxToCompareWith.Center);
                if (distanceInOtherTrack < withinDistance)
                {
                    // we found a box in another track that is closer, stop searching
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<Track> CreateNewTracks(IEnumerable<BoundingBox> boxes)
        {
            foreach (var box in boxes)
            {
                if (box.Score < _minFirstBoxScore)
                {
                    continue;
                }

                yield return new Track
                {
                    History = new List<BoundingBox> { box },
                    TrackId = trackIndex++,
                    Misses = 0,
                    Class = box.Class,
                    ClassName = box.ClassName
                };
            }
        }

        private double GetDistance(PointF a, PointF b)
        {
            return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }
    }
}