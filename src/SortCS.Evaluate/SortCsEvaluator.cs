using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using IniParser;

namespace SortCS.Evaluate
{
    public class SortCsEvaluator
    {
        private readonly DirectoryInfo _dataFolderMot;

        private readonly DirectoryInfo _destinationDir;

        public SortCsEvaluator(DirectoryInfo dataFolder, string benchmark, string splitToEval)
        {
            _dataFolderMot = new DirectoryInfo(Path.Combine($"{dataFolder}", "gt", "mot_challenge", $"{benchmark}-{splitToEval}"));
            _destinationDir = new DirectoryInfo(Path.Combine($"{dataFolder}", "trackers", "mot_challenge", $"{benchmark}-{splitToEval}", "SortCS", "data"));
            if (_destinationDir.Exists)
            {
                _destinationDir.Delete(true);
            }
            _destinationDir.Create();
        }

        public async Task EvaluateAsync()
        {
            var tasks = new List<Task>();
            foreach (var benchmarkDir in _dataFolderMot.GetDirectories())
            {
                tasks.Add(EvaluateBenchMark(benchmarkDir));
            }
            await Task.WhenAll(tasks);
        }

        private async Task EvaluateBenchMark(DirectoryInfo benchmarkFolder)
        {
            try
            {
                var gtFile = new FileInfo(Path.Combine($"{benchmarkFolder}", "gt", "gt.txt"));
                var sequenceIniFile = new FileInfo(Path.Combine($"{benchmarkFolder}", "seqinfo.ini"));

                if (!gtFile.Exists)
                {
                    Console.WriteLine($"Benchmark folder {benchmarkFolder} has no GroundTruth file (gt/gt.txt)");
                    return;
                }

                var iniString = File.ReadAllText(sequenceIniFile.FullName);
                var parser = new IniDataParser();
                var data = parser.Parse(iniString);
                var benchmarkKey = data["Sequence"]["name"];

                // GT file format (no header): <frame>, <id>, <bb_left>, <bb_top>, <bb_width>, <bb_height>, <conf>, <x>, <y>, <z>
                var lines = await File.ReadAllLinesAsync(gtFile.FullName);

                var frames = new Dictionary<int, List<BoundingBox>>();
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    var frameId = int.Parse(parts[0]);
                    var gtTrackId = int.Parse(parts[1]);
                    var bbLeft = float.Parse(parts[2]);
                    var bbTop = float.Parse(parts[3]);
                    var bbWidth = float.Parse(parts[4]);
                    var bbHeight = float.Parse(parts[5]);
                    if (!frames.ContainsKey(frameId))
                    {
                        frames.Add(frameId, new List<BoundingBox>());
                    }

                    frames[frameId].Add(new BoundingBox(0, "", bbTop, bbLeft, bbTop + bbHeight, bbLeft + bbWidth, 1));
                }

                Console.WriteLine($"{frames.Count}");

                var path = Path.Combine(_destinationDir.ToString(), $"{benchmarkKey}.txt");
                Console.WriteLine(path);
                using var file = new StreamWriter(path, false);

                //ITracker tracker = new SimpleBoxTracker(); // or SortTracker
                ITracker tracker = new SortTracker();
                foreach (var frame in frames)
                {
                    var tracks = tracker.Track(frame.Value);
                    foreach (var track in tracks)
                    {
                        if (track.State == TrackState.Started || track.State == TrackState.Active)
                        {
                            var lastBox = track.History.Last();
                            //<frame>, <id>, <bb_left>, <bb_top>, <bb_width>, <bb_height>, <conf>, <x>, <y>, <z>
                            var line = $"{frame.Key:0.},{track.TrackId:0.},{lastBox.Box.Left:0.},{lastBox.Box.Top:0.},{lastBox.Box.Width:0.},{lastBox.Box.Height:0.},1,-1,-1,-1";
                            await file.WriteLineAsync(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception evaluating benchmark {benchmarkFolder}: {ex.Message}");
                throw;
            }
        }
    }
}
