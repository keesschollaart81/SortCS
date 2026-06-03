using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SortCS.Tests;

[TestClass]
public class SortTrackerTests
{
    [TestMethod]
    public void SortTracker_FourEasyTracks_TrackedToEnd()
    {
        // Arrange
        var mot15Track = new List<Frame>{
            new(new List<RectangleF>{
                new(1703,385,157,339),
                new(1293,455,83,213),
                new(259,449,101,261),
                new(1253,529,55,127)
            }),
            new(new List<RectangleF>{
                new(1699,383,159,341),
                new(1293,455,83,213),
                new(261,447,101,263),
                new(1253,529,55,127)
            }),
            new(new List<RectangleF>{
                new(1697,383,159,343),
                new(1293,455,83,213),
                new(263,447,101,263),
                new(1255,529,55,127),
                new(429,300,55,127)
            }),
            new(new List<RectangleF>{
                new(1695,383,159,343),
                new(1293,455,83,213),
                new(265,447,101,263),
                new(1257,529,55,127)
            }),
            new(new List<RectangleF>{
                new(1693,381,159,347),
                new(1295,455,83,213),
                new(267,447,101,263),
                new(1259, 529,55,129)
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
        Assert.AreEqual(4, tracks.Count(x => x.State == TrackState.Active));
    }

    [TestMethod]
    public void SortTracker_CrossingTracks_EndInCorrectEndLocation()
    {
        // Arrange 
        var crossingTrack = new List<Frame>{
            new(new List<RectangleF>{
                new(0.8f, 0.3f, 0.1f, 0.1f),
                new(0.1f, 0.1f, 0.15f, 0.15f)
            }),
            new(new List<RectangleF>{
                new(0.8f, 0.35f, 0.1f, 0.1f),
                new(0.2f, 0.2f, 0.15f, 0.15f)
            }),
            new(new List<RectangleF>{
                new(0.3f, 0.3f, 0.15f, 0.15f),
                new(0.8f, 0.4f, 0.1f, 0.1f)
            }),
            new(new List<RectangleF>{
                new(0.4f, 0.4f, 0.15f, 0.15f),
                new(0.8f, 0.45f, 0.1f, 0.1f)
            }),
            new(new List<RectangleF>{
                new(0.5f, 0.5f, 0.15f, 0.15f),
                new(0.8f, 0.5f, 0.1f, 0.1f)
            }),
            new(new List<RectangleF>()),
            new(new List<RectangleF>()),
            new(new List<RectangleF>()),
            new(new List<RectangleF>()),
            new(new List<RectangleF>())
        };
        var tracks = Enumerable.Empty<Track>();

        var sut = new SortTracker(0.2f);

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
        Assert.AreEqual(0.5, lastBoxOfTrack2.Top, 0.00);
        Assert.AreEqual(5, complexTrack1.History.Count);
        Assert.AreEqual(5, complexTrack2.History.Count);
    }

    [TestMethod]
    public void SortTracker_MaxMisses_CanBeAdjustedAtRuntime()
    {
        // Arrange: a tracker that would normally drop a track after a single missed frame.
        var sut = new SortTracker(iouThreshold: 0.3f, maxMisses: 1);
        var box = new RectangleF(100, 100, 50, 50);

        sut.Track([box]); // Started
        sut.Track([box]); // Active

        // Act: widen the allowed occlusion gap on the fly.
        sut.MaxMisses = 5;

        Track[] tracks = [];
        for (var i = 0; i < 4; i++)
        {
            tracks = sut.Track([]).ToArray(); // four consecutive misses
        }

        // Assert: with MaxMisses raised to 5, the track survives four misses instead of being removed.
        Assert.AreEqual(5, sut.MaxMisses);
        Assert.IsTrue(tracks.Any(t => t.State == TrackState.Ending));
        Assert.IsFalse(tracks.Any(t => t.State == TrackState.Ended));
    }
}