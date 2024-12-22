using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SortCS;
using System.Drawing;

BenchmarkRunner.Run<SortCSBenchmarks>();

[MemoryDiagnoser]
public class SortCSBenchmarks
{
    private ITracker _tracker;
    private List<RectangleF[]> _frames;

    [GlobalSetup]
    public void Setup()
    {
        _tracker = new SortTracker();
        _frames = GenerateTestFrames(100, 10);
    }

    [Benchmark]
    public void TrackMultipleFrames()
    {
        foreach (var frame in _frames)
        {
            _tracker.Track(frame);
        }
    }

    private List<RectangleF[]> GenerateTestFrames(int numFrames, int objectsPerFrame)
    {
        var random = new Random(42);
        var frames = new List<RectangleF[]>();

        for (var i = 0; i < numFrames; i++)
        {
            var objects = new RectangleF[objectsPerFrame];
            for (var j = 0; j < objectsPerFrame; j++)
            {
                objects[j] = new RectangleF(
                    random.Next(0, 1000),
                    random.Next(0, 1000),
                    random.Next(50, 200),
                    random.Next(50, 200)
                );
            }
            frames.Add(objects);
        }

        return frames;
    }
}