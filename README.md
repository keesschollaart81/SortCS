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
- Make sure that the outputs (tracks+detections) will be stored in `TrackEval/data/trackers/mot_challenge/MOT20-train/SortCS/data`
- Run TrackEval (according to their [readme](https://github.com/JonathonLuiten/TrackEval/blob/master/docs/MOTChallenge-Official/Readme.md)):
  `python scripts/run_mot_challenge.py --BENCHMARK MOT20 --SPLIT_TO_EVAL train --TRACKERS_TO_EVAL SortCS --METRICS HOTA CLEAR Identity VACE --USE_PARALLEL False --NUM_PARALLEL_CORES 1`

Example results for SortCS & MOT20:
```
All sequences for SortCS finished in 83.10 seconds

HOTA: SortCS-pedestrian            HOTA      DetA      AssA      DetRe     DetPr     AssRe     AssPr     LocA      RHOTA     HOTA(0)   LocA(0)   HOTALocA(0)
MOT20-01                           100       100       100       100       100       100       100       100       100       100       100       100
MOT20-02                           78.251    100       61.232    100       100       65.497    88.4      100       78.251    78.251    100       78.251
MOT20-03                           69.046    99.999    47.674    100       100       48.984    86.758    100       69.046    69.047    100       69.046
MOT20-05                           66.401    99.798    44.18     99.899    99.899    47.377    81.006    99.965    66.435    66.459    99.904    66.395
COMBINED                           69.567    99.885    48.452    99.942    99.942    51.216    83.939    99.98     69.587    69.6      99.945    69.562

CLEAR: SortCS-pedestrian           MOTA      MOTP      MODA      CLR_Re    CLR_Pr    MTR       PTR       MLR       sMOTA     CLR_TP    CLR_FN    CLR_FP    IDSW      MT        PT        ML        Frag      
MOT20-01                           100       100       100       100       100       100       0         0         100       19870     0         0         0         74        0         0         0
MOT20-02                           99.866    100       100       100       100       100       0         0         99.866    154742    0         0         207       270       0         0         0
MOT20-03                           99.283    99.974    100       100       100       100       0         0         99.258    313658    0         0         2248      702       0         0         0
MOT20-05                           99.338    99.929    100       100       100       100       0         0         99.267    646344    0         0         4280      1169      0         0         0
COMBINED                           99.406    99.952    100       100       100       100       0         0         99.359    1134614   0         0         6735      2215      0         0         0

Identity: SortCS-pedestrian        IDF1      IDR       IDP       IDTP      IDFN      IDFP
MOT20-01                           100       100       100       19870     0         0
MOT20-02                           70.672    70.672    70.672    109359    45383     45383
MOT20-03                           60.385    60.385    60.385    189401    124257    124257
MOT20-05                           57.169    57.169    57.169    369506    276838    276838
COMBINED                           60.649    60.649    60.649    688136    446478    446478

VACE: SortCS-pedestrian            SFDA      ATA
MOT20-01                           100       100
MOT20-02                           100       57.835
MOT20-03                           100       31.045
MOT20-05                           100       31.712
COMBINED                           100       34.767

Count: SortCS-pedestrian           Dets      GT_Dets   IDs       GT_IDs
MOT20-01                           19870     19870     74        74
MOT20-02                           154742    154742    408       270
MOT20-03                           313658    313658    2149      702
MOT20-05                           646344    646344    3635      1169
COMBINED                           1134614   1134614   6266      2215
```

# Attributions

- [MaartenX](https://github.com/MaartenX/), for implementing some of the core pieces of this repo
- [abewley's Python implementation of Sort](https://github.com/abewley/sort), used for validation/reference
