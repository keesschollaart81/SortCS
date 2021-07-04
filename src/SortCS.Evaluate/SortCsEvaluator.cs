using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using IniParser;
using System.Globalization;
using System.Drawing;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SortCS.Evaluate
{
    public class SortCsEvaluator
    {
        private readonly DirectoryInfo _dataFolderMot;

        private readonly DirectoryInfo _destinationDir;
        private readonly ILogger<SortTracker> _logger;

        public SortCsEvaluator(DirectoryInfo dataFolder, string benchmark, string splitToEval, ILogger<SortTracker> logger)
        {
            _dataFolderMot = new DirectoryInfo(Path.Combine($"{dataFolder}", "gt", "mot_challenge", $"{benchmark}-{splitToEval}"));
            _destinationDir = new DirectoryInfo(Path.Combine($"{dataFolder}", "trackers", "mot_challenge", $"{benchmark}-{splitToEval}", "SortCS", "data"));
            if (_destinationDir.Exists)
            {
                _destinationDir.Delete(true);
            }
            _destinationDir.Create();
            _logger = logger;
        }

        public async Task EvaluateAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task<int>>();
            foreach (var benchmarkDir in _dataFolderMot.GetDirectories())
            {
                tasks.Add(EvaluateBenchMark(benchmarkDir));
            }
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            var totalFrames = tasks.Sum(x => x.Result);
            _logger.LogInformation("Finished evaluating {totalFrames} frames in {totalSeconds:0.} seconds ({fps} fps)", totalFrames, stopwatch.Elapsed.TotalSeconds, totalFrames / stopwatch.Elapsed.TotalSeconds);
        }

        private async Task<int> EvaluateBenchMark(DirectoryInfo benchmarkFolder)
        {
            try
            {
                var detFile = new FileInfo(Path.Combine($"{benchmarkFolder}", "gt", "gt.txt"));
                var sequenceIniFile = new FileInfo(Path.Combine($"{benchmarkFolder}", "seqinfo.ini"));

                if (!detFile.Exists)
                {
                    detFile = new FileInfo(Path.Combine($"{benchmarkFolder}", "det", "det.txt"));
                    if (!detFile.Exists)
                    {
                        _logger.LogWarning("Benchmark folder {benchmarkFolder} has no GroundTruth file (gt/gt.txt)", benchmarkFolder);
                        return 0;
                    }
                }

                var iniString = File.ReadAllText(sequenceIniFile.FullName);
                var parser = new IniDataParser();
                var data = parser.Parse(iniString);
                var benchmarkKey = data["Sequence"]["name"];

                // GT file format (no header): <frame>, <id>, <bb_left>, <bb_top>, <bb_width>, <bb_height>, <conf>, <x>, <y>, <z>
                var lines = await File.ReadAllLinesAsync(detFile.FullName);

                var frames = new Dictionary<int, List<RectangleF>>();
                var numberInfo = new NumberFormatInfo() { NumberDecimalSeparator = "." };
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    var frameId = int.Parse(parts[0]);
                    var gtTrackId = int.Parse(parts[1]);
                    var bbLeft = float.Parse(parts[2], numberInfo);
                    var bbTop = float.Parse(parts[3], numberInfo);
                    var bbWidth = float.Parse(parts[4], numberInfo);
                    var bbHeight = float.Parse(parts[5], numberInfo);
                    var bbConf = float.Parse(parts[6], numberInfo);
                    if (!frames.ContainsKey(frameId))
                    {
                        frames.Add(frameId, new List<RectangleF>());
                    }
                    if (bbConf > 0)
                    {
                        frames[frameId].Add(new RectangleF(bbLeft, bbTop, bbWidth, bbHeight));
                    }
                }


                var path = Path.Combine(_destinationDir.ToString(), $"{benchmarkKey}.txt");
                _logger.LogInformation("Read {framesCount} frames, output to {outputFile}", frames.Count, path);
                using var file = new StreamWriter(path, false);

                //ITracker tracker = new SimpleBoxTracker(); // or SortTracker
                ITracker tracker = new SortTracker(_logger);
                foreach (var frame in frames)
                {
                    var tracks = tracker.Track(frame.Value);
                    foreach (var track in tracks)
                    {
                        if (track.State == TrackState.Started || track.State == TrackState.Active)
                        {
                            //var boxForLog = track.History.Last();
                            var boxForLog = track.Prediction;
                            //<frame>, <id>, <bb_left>, <bb_top>, <bb_width>, <bb_height>, <conf>, <x>, <y>, <z>
                            var line = $"{frame.Key:0.},{track.TrackId:0.},{boxForLog.Left:0.},{boxForLog.Top:0.},{boxForLog.Width:0.},{boxForLog.Height:0.},1,-1,-1,-1";
                            await file.WriteLineAsync(line);
                        }
                    }
                }
                return frames.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception evaluating benchmark {benchmarkFolder}: {ex.Message}", benchmarkFolder, ex.Message);
                throw;
            }
        }
    }
}
