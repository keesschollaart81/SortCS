using System;
using System.Collections.Generic;
using System.Linq;

namespace sortcs
{
    class Program
    {
        static void Main(string[] args)
        {
            var trackers = new ITracker[]{
                new SimpleBoxTracker()
                // todo: add Sort-like tracker
            };

            foreach (var tracker in trackers)
            {
                var simpleTracks = SimpleTrack(tracker);
                var simpleTrack = simpleTracks.Single();
                if (simpleTrack.State != TrackState.Ended || simpleTrack.History.Count != 4)
                {
                    throw new Exception();
                }
                var complexTracks = ComplexTrack(tracker);
                if (complexTracks.Count() != 2)
                {
                    throw new Exception();
                }
                var complexTrack1 = complexTracks.ElementAt(0);
                var complexTrack2 = complexTracks.ElementAt(1);
                if (complexTrack1.State != TrackState.Ended || complexTrack2.State != TrackState.Ended)
                {
                    throw new Exception();
                }
                var firstBoxOfTrack2 = complexTrack2.History.First();
                var lastBoxOfTrack2 = complexTrack2.History.Last();

                if (firstBoxOfTrack2.Box[0] != 0.8 || lastBoxOfTrack2.Box[0] != 0.1)
                {
                    // this is where SimpleTracker misses the boat
                    throw new Exception("Track 2 did not start/end at expect location");
                }
            }

        }

        public static IEnumerable<Track> SimpleTrack(ITracker tracker)
        {
            var simpleTrack = new List<Frame>{
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.1f, 0.1f, 0.3f, 0.3f, 1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.2f, 0.2f, 0.4f, 0.4f, 1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.3f, 0.3f, 0.5f, 0.5f, 1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.4f, 0.4f, 0.6f, 0.6f, 1)
                }),
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>())
            };
            var tracks = Enumerable.Empty<Track>();
            foreach (var frame in simpleTrack)
            {
                tracks = tracker.Track(frame.BoundingBoxes);
            }
            return tracks;
        }

        public static IEnumerable<Track> ComplexTrack(ITracker tracker)
        {
            //  x     x
            //    x  x 
            //      x    < frame 3
            //      xx   < frame 3 & 4, simple track will be confused
            //     x   x
            //     x    x
            var simpleTrack = new List<Frame>{
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.1f, 0.1f, 0.2f, 0.2f, 1),
                    new BoundingBox(1, "person", 0.8f, 0.3f, 0.9f, 0.4f, 1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.3f, 0.3f, 0.4f, 0.4f, 1),
                    new BoundingBox(1, "person", 0.6f, 0.35f, 0.7f, 0.45f, 1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.45f, 0.45f, 0.55f, 0.55f, 1),
                    new BoundingBox(1, "person", 0.4f, 0.4f, 0.5f, 0.5f, 1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.6f, 0.6f, 0.7f, 0.7f, 1),
                    new BoundingBox(1, "person", 0.25f, 0.45f, 0.35f, 0.55f, 1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.7f, 0.7f, 0.8f, 0.8f, 1),
                    new BoundingBox(1, "person", 0.1f, 0.5f, 0.2f, 0.6f, 1)
                }),
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>())
            };
            var tracks = Enumerable.Empty<Track>();
            foreach (var frame in simpleTrack)
            {
                tracks = tracker.Track(frame.BoundingBoxes);
            }
            return tracks;
        }
    }
}
