using System.IO;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.IO.Compression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SortCS.Evaluate;

class Program
{
    private static ILogger<Program> _logger;

    static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddLogging(loggerBuilder =>
        {
            loggerBuilder.ClearProviders();
            loggerBuilder.AddConsole();
        });
        var serviceProvider = services.BuildServiceProvider();

        _logger = serviceProvider.GetService<ILogger<Program>>();

        var rootCommand = new RootCommand
        {
            new Option<DirectoryInfo>(
                "--data-folder",
                getDefaultValue: () => new DirectoryInfo(@"../../../../../../TrackEval/data"), // Sssuming SortCS/src/SortCS.Evaluate/bin/debug/net5.0 is working directory 
                description: "Location where data is stored using this format: https://github.com/JonathonLuiten/TrackEval/blob/master/docs/MOTChallenge-format.txt"),
            new Option<string>(
                "--benchmark",
                getDefaultValue: () => "MOT20",
                description: "Name of the benchmark, e.g. MOT15, MO16, MOT17 or MOT20 (default : MOT20)"),
            new Option<string>(
                "--split-to-eval",
                getDefaultValue: () => "train",
                description: "Data split on which to evalute e.g. train, test (default : train)"),
        };

        rootCommand.Description = "App to evaluate the SortCS tracking algoritm";
        rootCommand.Handler = CommandHandler.Create<DirectoryInfo, string, string>(async (dataFolder, benchmark, splitToEval) =>
        {
            if (!dataFolder.Exists || !dataFolder.GetDirectories().Any())
            {
                await DownloadTrackEvalExampleAsync(dataFolder);
            }
            var sortCsEvaluator = new SortCsEvaluator(dataFolder, benchmark, splitToEval, serviceProvider.GetService<ILogger<SortTracker>>());
            await sortCsEvaluator.EvaluateAsync();
        });

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task DownloadTrackEvalExampleAsync(DirectoryInfo groundTruthFolder)
    {
        var dataZipUrl = "https://omnomnom.vision.rwth-aachen.de/data/TrackEval/data.zip";
        groundTruthFolder.Create();
        var targetZipFile = Path.Combine(groundTruthFolder.ToString(), "..", "data.zip");
        _logger.LogInformation(Path.GetFullPath(targetZipFile));

        _logger.LogInformation($"Downloading data.zip (150mb) from {dataZipUrl} to {targetZipFile}");
        using var httpClient = new HttpClient();
        var zipStream = await httpClient.GetStreamAsync(dataZipUrl);
        using var fs = new FileStream(targetZipFile, FileMode.CreateNew);
        await zipStream.CopyToAsync(fs);
        ZipFile.ExtractToDirectory(targetZipFile, Path.Combine(groundTruthFolder.ToString(), ".."));
        _logger.LogInformation("data.zip downloaded & extracted");
    }
}