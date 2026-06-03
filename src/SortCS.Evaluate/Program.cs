using System.CommandLine;
using System.IO.Compression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SortCS;
using SortCS.Evaluate;

var services = new ServiceCollection();
services.AddLogging(loggerBuilder =>
{
    loggerBuilder.ClearProviders();
    loggerBuilder.AddConsole();
});
var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

var dataFolderOption = new Option<DirectoryInfo>("--data-folder")
{
    Description = "Location where data is stored using this format: https://github.com/JonathonLuiten/TrackEval/blob/master/docs/MOTChallenge-format.txt",
    DefaultValueFactory = _ => new DirectoryInfo(@"../../../../../../TrackEval/data") // Assuming SortCS/src/SortCS.Evaluate/bin/debug/net10.0 is working directory
};

var benchmarkOption = new Option<string>("--benchmark")
{
    Description = "Name of the benchmark, e.g. MOT15, MO16, MOT17 or MOT20 (default : MOT20)",
    DefaultValueFactory = _ => "MOT20"
};

var splitOption = new Option<string>("--split-to-eval")
{
    Description = "Data split on which to evalute e.g. train, test (default : train)",
    DefaultValueFactory = _ => "train"
};

var rootCommand = new RootCommand("App to evaluate the SortCS tracking algoritm")
{
    dataFolderOption,
    benchmarkOption,
    splitOption
};

rootCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var dataFolder = parseResult.GetValue(dataFolderOption)!;
    var benchmark = parseResult.GetValue(benchmarkOption)!;
    var splitToEval = parseResult.GetValue(splitOption)!;

    if (!dataFolder.Exists || !dataFolder.GetDirectories().Any())
    {
        await DownloadTrackEvalExampleAsync(dataFolder, cancellationToken);
    }

    var sortCsEvaluator = new SortCsEvaluator(dataFolder, benchmark, splitToEval, serviceProvider.GetRequiredService<ILogger<SortTracker>>());
    await sortCsEvaluator.EvaluateAsync();
});

return await rootCommand.Parse(args).InvokeAsync();

async Task DownloadTrackEvalExampleAsync(DirectoryInfo groundTruthFolder, CancellationToken cancellationToken)
{
    const string dataZipUrl = "https://omnomnom.vision.rwth-aachen.de/data/TrackEval/data.zip";
    groundTruthFolder.Create();
    var targetZipFile = Path.Combine(groundTruthFolder.ToString(), "..", "data.zip");
    logger.LogInformation("{TargetZipFile}", Path.GetFullPath(targetZipFile));

    logger.LogInformation("Downloading data.zip (150mb) from {DataZipUrl} to {TargetZipFile}", dataZipUrl, targetZipFile);
    using var httpClient = new HttpClient();
    var zipStream = await httpClient.GetStreamAsync(dataZipUrl, cancellationToken);
    await using var fs = new FileStream(targetZipFile, FileMode.CreateNew);
    await zipStream.CopyToAsync(fs, cancellationToken);
    ZipFile.ExtractToDirectory(targetZipFile, Path.Combine(groundTruthFolder.ToString(), ".."));
    logger.LogInformation("data.zip downloaded & extracted");
}
