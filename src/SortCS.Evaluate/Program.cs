using System;
using System.IO;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using sortcs;
using IniParser;

namespace SortCS.Evaluate
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<DirectoryInfo>(
                    "--benchmark",
                    getDefaultValue: () => new DirectoryInfo(@"C:\projects\github\TrackEval\data\gt\mot_challenge\MOT17-train"),
                    description: "Location where ground truth data is stored using this format: https://github.com/JonathonLuiten/TrackEval/blob/master/docs/MOTChallenge-format.txt"),
                new Option<DirectoryInfo>(
                    "--ground-truth-folder",
                    getDefaultValue: () => new DirectoryInfo(@"C:\projects\github\TrackEval\data\gt\mot_challenge\MOT17-train"),
                    description: "Location where ground truth data is stored using this format: https://github.com/JonathonLuiten/TrackEval/blob/master/docs/MOTChallenge-format.txt"),
                new Option<DirectoryInfo>(
                    "--destination-track-folder",
                    getDefaultValue: () => new DirectoryInfo(@"./data/trackers/mot_challenge/MOT17-train/SortCS/data"),
                    description: "Where to store the generated track data"),
            };

            rootCommand.Description = "App to evaluate the SortCS tracking algoritm";
            rootCommand.Handler = CommandHandler.Create<DirectoryInfo, DirectoryInfo>(async (groundTruthFolder, destinationTrackFolder) =>
            {
                var sortCsEvaluator = new SortCsEvaluator(groundTruthFolder);
                await sortCsEvaluator.EvaluateAsync(destinationTrackFolder);
            });

            return await rootCommand.InvokeAsync(args);
        }
    }

    public class SortCsEvaluator
    {
        private readonly DirectoryInfo _groundTruthFolder;

        public SortCsEvaluator(DirectoryInfo groundTruthFolder)
        {
            _groundTruthFolder = groundTruthFolder;
        }

        public async Task EvaluateAsync(DirectoryInfo destinationTrackFolder)
        {
            destinationTrackFolder.Create();
            foreach(var benchmark in _groundTruthFolder.GetDirectories())
            {
                await EvaluateBenchMark(benchmark, destinationTrackFolder);
            }
        }

        private async Task EvaluateBenchMark(DirectoryInfo benchmarkFolder, DirectoryInfo destinationTrackFolder)
        {
            var gtFile = new FileInfo(Path.Combine($"{benchmarkFolder}", "gt", "gt.txt"));

            if (!gtFile.Exists)
            {
                Console.WriteLine($"Benchmark folder {benchmarkFolder} has no GroundTruth file (gt/gt.txt)");
                return;
            }
            var parser = new FileIniDataParser();
            var data = parser.ReadFile(gtFile.FullName);

            // GT file format (no header): <frame>, <id>, <bb_left>, <bb_top>, <bb_width>, <bb_height>, <conf>, <x>, <y>, <z>
            var lines = await File.ReadAllLinesAsync(gtFile.FullName);

            var frames = new Dictionary<long, List<BoundingBox>>();
            foreach(var line in lines)
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

                frames[frameId].Add(new BoundingBox(0, "", bbTop, bbLeft, bbTop + bbHeight, bbLeft + bbWidth, 1); 
            }

            using var file = new StreamWriter(Path.Combine(destinationTrackFolder.ToString(), "trackers", data["name"], "")

            var tracker = new SortTracker();
            foreach (var frame in frames)
            {
                var tracks = tracker.Track(frame.Value);
                foreach(var track in tracks)
                {

                }
            }
        }
    }
}
