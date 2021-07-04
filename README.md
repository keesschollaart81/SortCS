# SortCS 

SortCS is a 'Multiple Object Tracker' as described in [this paper](https://arxiv.org/abs/1602.00763), implemented in C#.

> SORT is a barebones implementation of a visual multiple object tracking framework based on rudimentary data association and state estimation techniques. It is designed for online tracking applications where only past and current frames are available and the method produces object identities on the fly. While this minimalistic tracker doesn't handle occlusion or re-entering objects its purpose is to serve as a baseline and testbed for the development of future trackers.

> SORT was initially described in this paper. At the time of the initial publication, SORT was ranked the best open source multiple object tracker on the MOT benchmark.

## Using

```cs
using SortCS;

ITracker tracker = new SortTracker();
tracker.Track(new[]
{
    new BoundingBox(1, "person", 1695,383,159,343, 1),
    new BoundingBox(1, "person", 1293,455,83,213,  1)
});
tracker.Track(new[]
{
    new BoundingBox(1, "person", 1699,383,159,341, 1),
    new BoundingBox(1, "person", 1293,455,83,213,  1)
});
tracker.Track(new[]
{
    new BoundingBox(1, "person", 1697,383,159,343, 1),
    new BoundingBox(1, "person", 1293,455,83,213,  1)
});
var tracks = tracker.Track(new[]
{
    new BoundingBox(1, "person", 1695,383,159,343, 1),
    new BoundingBox(1, "person", 1293,455,83,213,  1)
});

Assert.AreEqual(2 tracks.Count());
Assert.AreEqual(TrackState.Active, tracks.First().State);
Assert.AreEqual(4, tracks.First().History.Count);

```

## Evaluation

The performance of this implementation can be evaluation using the 'SortCS.Evaluate' Console Application.
The output can be used for https://motchallenge.net/ and their [TrackEval SDK](https://github.com/JonathonLuiten/TrackEval/). 
Brief instructions:
- Clone this repo and the TrackEval in the same folder / next to each other
- Run the SortCS.Evaluate Console app. The `--data-folder` arguments needs to point to the data folder in the `TrackEval` repo.
- Make sure that the outputs (tracks+detections) will be stored in `TrackEval/data/trackers/mot_challenge/MOT15-train/SortCS/data`
- Run TrackEval (according to their [readme](https://github.com/JonathonLuiten/TrackEval/blob/master/docs/MOTChallenge-Official/Readme.md)):
  `python scripts/run_mot_challenge.py --BENCHMARK MOT15 --SPLIT_TO_EVAL train --TRACKERS_TO_EVAL SortCS --METRICS HOTA CLEAR Identity VACE --USE_PARALLEL False --NUM_PARALLEL_CORES 1`

Example results for SortCS & MOT15:
```
All sequences for SortCs finished in 5.05 seconds

HOTA: SortCS-pedestrian            HOTA      DetA      AssA      DetRe     DetPr     AssRe     AssPr     LocA      RHOTA     HOTA(0)   LocA(0)   HOTALocA(0)
ADL-Rundle-6                       98.384    100       96.794    100       100       96.794    100       100       98.384    98.384    100       98.384
ADL-Rundle-8                       100       100       100       100       100       100       100       100       100       100       100       100
ETH-Bahnhof                        75.855    70.513    81.601    99.928    70.549    97.145    83.768    99.969    90.301    75.916    99.928    75.862
ETH-Pedcross2                      94.877    92.497    97.318    99.9      92.583    98.908    98.238    99.957    98.601    95.006    99.899    94.91
ETH-Sunnyday                       98.479    97.738    99.225    100       97.738    100       99.225    100       99.612    98.479    100       98.479
KITTI-13                           0.13798   0.043755  0.43509   0.096698  0.079316  0.43509   10.526    90.526    0.20512   1.3108    10        0.13108
KITTI-17                           0.077672  0.036049  0.16735   0.077059  0.067304  0.16735   10.526    90.535    0.11356   0.73788   10.086    0.074424
PETS09-S2L1                        0.014927  0.0023074 0.096572  0.0047034 0.0045274 0.096572  10.526    90.526    0.021312  0.14181   10        0.014181
TUD-Campus                         73.576    73.813    73.351    84.929    84.929    73.351    100       99.742    78.925    74.383    97.41     72.456
TUD-Stadtmitte                     0.041713  0.0068323 0.25467   0.013659  0.013659  0.25467   15.789    86.785    0.058978  0.26418   16.306    0.043079
Venice-2                           100       100       100       100       100       100       100       100       100       100       100       100
COMBINED                           78.957    65.169    95.662    82.108    75.955    98.577    96.939    99.98     88.627    79.003    99.903    78.926
```

# Attributions

- [MaartenX](https://github.com/MaartenX/), for implementing some of the core pieces of this repo
- [abewley's Python implementation of Sort](https://github.com/abewley/sort), used for validation/reference
