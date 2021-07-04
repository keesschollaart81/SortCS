using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace SortCS.Tests
{
    [TestClass]
    public class SimpleTrackerTests
    {
        [TestMethod]
        public void SimpleTracker_FourEasyTracks_TrackedToEnd()
        {
            // Arrange
            var mot15Track = new List<Frame>{
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 1703,385,157,339, 1),
                    new BoundingBox(1, "person", 1293,455,83,213,  1),
                    new BoundingBox(1, "person", 259,449,101,261,  1),
                    new BoundingBox(1, "person", 1253,529,55,127,  1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 1699,383,159,341, 1),
                    new BoundingBox(1, "person", 1293,455,83,213,  1),
                    //new BoundingBox(1, "person", 261,447,101,263,  1),
                    new BoundingBox(1, "person", 1253,529,55,127,  1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 1697,383,159,343, 1),
                    new BoundingBox(1, "person", 1293,455,83,213,  1),
                    new BoundingBox(1, "person", 263,447,101,263,  1),
                    new BoundingBox(1, "person", 1255,529,55,127,  1),
                    new BoundingBox(1, "person", 429,300,55,127,  1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 1695,383,159,343, 1),
                    new BoundingBox(1, "person", 1293,455,83,213,  1),
                    new BoundingBox(1, "person", 265,447,101,263,  1),
                    new BoundingBox(1, "person", 1257,529,55,127,  1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 1693,381,159,347, 1),
                    new BoundingBox(1, "person", 1295,455,83,213,  1),
                    new BoundingBox(1, "person", 267,447,101,263,  1),
                    new BoundingBox(1, "person", 1259, 529,55,129, 1)
                }),
            };

            var tracks = Enumerable.Empty<Track>();
            var sut = new SortTracker();

            // Act
            foreach (var frame in mot15Track)
            {
                // ToArray because otherwise the IEnumerable is not evaluated.
                tracks = sut.Track(frame.BoundingBoxes).ToArray();
            }

            // Assert
            Assert.AreEqual(4, tracks.Count());
        }

        [TestMethod]
        public void SimpleTracker_CrossingTracks_EndInCorrectEndLocation()
        {
            // Arrange 
            var crossingTrack = new List<Frame>{
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.8f, 0.3f, 0.1f, 0.1f, 1),
                    new BoundingBox(1, "person", 0.1f, 0.1f, 0.15f, 0.15f, 1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.8f, 0.35f, 0.1f, 0.1f, 1),
                    new BoundingBox(1, "person", 0.2f, 0.2f, 0.15f, 0.15f, 1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.3f, 0.3f, 0.15f, 0.15f, 1),
                    new BoundingBox(1, "person", 0.8f, 0.4f, 0.1f, 0.1f, 1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.4f, 0.4f, 0.15f, 0.15f, 1),
                    new BoundingBox(1, "person", 0.8f, 0.45f, 0.1f, 0.1f, 1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.5f, 0.5f, 0.15f, 0.15f, 1),
                    new BoundingBox(1, "person", 0.8f, 0.5f, 0.1f, 0.1f, 1)
                }),
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>()),
                new Frame(new List<BoundingBox>())
            };
            var tracks = Enumerable.Empty<Track>();

            var sut = new SortTracker();

            // Act
            foreach (var frame in crossingTrack)
            {
                var result = sut.Track(frame.BoundingBoxes).ToArray();
                if (result.Any())
                {
                    tracks = result;
                }
            }

            var complexTrack1 = tracks.ElementAt(0);
            var complexTrack2 = tracks.ElementAt(1);
            var firstBoxOfTrack2 = complexTrack2.History.FirstOrDefault();
            var lastBoxOfTrack2 = complexTrack2.History.LastOrDefault();

            // Assert
            Assert.AreEqual(TrackState.Ended, complexTrack1.State);
            Assert.AreEqual(TrackState.Ended, complexTrack2.State);
            Assert.AreEqual(0.5, lastBoxOfTrack2.Box.Top, 0.001);
            Assert.AreEqual(5, complexTrack1.History.Count);
            Assert.AreEqual(5, complexTrack2.History.Count);
        }


        [TestMethod]
        public void SimpleTracker_ThisNeverEnds()
        {
            // Arrange 
            var crossingTrack = new List<Frame>{
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.8f, 0.3f, 0.1f, 0.1f, 1),
                    new BoundingBox(1, "person", 0.1f, 0.1f, 0.15f, 0.15f, 1)
                }),
                new Frame(new List<BoundingBox>{
                    new BoundingBox(1, "person", 0.8f, 0.35f, 0.1f, 0.1f, 1),
                    new BoundingBox(1, "person", 0.9f, 0.9f, 0.15f, 0.15f, 1),
                    new BoundingBox(1, "person", 0.2f, 0.2f, 0.15f, 0.15f, 1)
                }), 
                new Frame(new List<BoundingBox>())
            };

            var sut = new SortTracker();

            // Act
            foreach (var frame in crossingTrack)
            {
                var result = sut.Track(frame.BoundingBoxes).ToArray();
                // for frame 2, we never get here because `matrix.FindAssignments()` gets into a infinite loop
            }

        }
    }
}
