<h1 align="center">

<img src="https://raw.githubusercontent.com/keesschollaart81/SortCS/main/resources/logo.png" width=150 alt="SortCS"/>
<br/>
SortCS - A Multiple Object Tracker
</h1>

<div align="center">
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/keesschollaart81/CaseOnline.Azure.WebJobs.Extensions.Mqtt/blob/master/LICENSE)
[![BCH compliance](https://bettercodehub.com/edge/badge/keesschollaart81/CaseOnline.Azure.WebJobs.Extensions.Mqtt?branch=master)](https://bettercodehub.com/)
[![Code Coverage](https://sonarcloud.io/api/project_badges/measure?project=CaseOnline.Azure.WebJobs.Extensions.Mqtt&metric=coverage)](https://sonarcloud.io/dashboard?id=CaseOnline.Azure.WebJobs.Extensions.Mqtt)
[![Maintainability](https://sonarcloud.io/api/project_badges/measure?project=CaseOnline.Azure.WebJobs.Extensions.Mqtt&metric=sqale_rating)]()
</div>

SortCS is a 'Multiple Object Tracker' as described in [this paper](https://arxiv.org/abs/1602.00763), implemented in C#.

> SORT is a barebones implementation of a visual multiple object tracking framework based on rudimentary data association and state estimation techniques. It is designed for online tracking applications where only past and current frames are available and the method produces object identities on the fly. While this minimalistic tracker doesn't handle occlusion or re-entering objects its purpose is to serve as a baseline and testbed for the development of future trackers.

> SORT was initially described in this paper. At the time of the initial publication, SORT was ranked the best open source multiple object tracker on the MOT benchmark.

## Using

```cs
using SortCS;

ITracker tracker = new SortTracker(iouThreshold = 0.3f, maxMisses = 3);
tracker.Track(new[]
{
    new RectangleF(1695,383,159,343),
    new RectangleF(1293,455,83,213)
});
tracker.Track(new[]
{
    new RectangleF(1699,383,159,341),
    new RectangleF(1293,455,83,213)
});
tracker.Track(new[]
{
    new RectangleF(1697,383,159,343),
    new RectangleF(1293,455,83,213)
});
var tracks = tracker.Track(new[]
{
    new RectangleF(1695,383,159,343),
    new RectangleF(1293,455,83,213)
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

Preliminary results for SortCS & MOT20 Train:
```
All sequences for SortCS finished in 103.38 seconds

HOTA: SortCS-pedestrian            HOTA      DetA      AssA      DetRe     DetPr     AssRe     AssPr     LocA      RHOTA     HOTA(0)   LocA(0)   HOTALocA(0)
MOT20-01                           97.52     97.299    97.75     98.397    98.397    98.616    98.616    96.449    98.067    100       96.333    96.333
MOT20-02                           76.905    97.8      60.478    98.736    98.737    65.202    86.718    96.909    77.271    78.464    96.803    75.955
MOT20-03                           66.813    96.08     46.481    97.64     97.641    48.266    84.487    96.139    67.367    68.979    95.845    66.113
MOT20-05                           64.614    96.537    43.255    98.026    98.03     46.88     79.206    96.548    65.114    66.432    96.217    63.919
COMBINED                           67.705    96.586    47.471    98.023    98.025    50.691    82.04     96.483    68.213    69.6      96.196    66.953

CLEAR: SortCS-pedestrian           MOTA      MOTP      MODA      CLR_Re    CLR_Pr    MTR       PTR       MLR       sMOTA     CLR_TP    CLR_FN    CLR_FP    IDSW      MT        PT        ML        Frag      
MOT20-01                           100       96.333    100       100       100       100       0         0         96.333    19870     0         0         0         74        0         0         0
MOT20-02                           99.806    96.819    99.941    99.97     99.971    100       0         0         96.626    154695    47        45        208       270       0         0         32
MOT20-03                           98.894    95.925    99.616    99.807    99.808    100       0         0         94.827    313054    604       601       2264      702       0         0         533
MOT20-05                           98.905    96.355    99.557    99.777    99.78     99.914    0.085543  0         95.268    644900    1444      1419      4216      1168      1         0         1102
COMBINED                           99.044    96.299    99.633    99.815    99.818    99.955    0.045147  0         95.35     1132519   2095      2065      6688      2214      1         0         1667

Identity: SortCS-pedestrian        IDF1      IDR       IDP       IDTP      IDFN      IDFP
MOT20-01                           100       100       100       19870     0         0
MOT20-02                           71.141    71.14     71.141    110084    44658     44656
MOT20-03                           60.351    60.351    60.352    189296    124362    124359
MOT20-05                           57.143    57.142    57.144    369332    277012    276987
COMBINED                           60.689    60.689    60.69     688582    446032    446002

VACE: SortCS-pedestrian            SFDA      ATA
MOT20-01                           96.356    100
MOT20-02                           96.795    58.253
MOT20-03                           95.943    31.567
MOT20-05                           96.295    32.092
COMBINED                           96.359    35.234

Count: SortCS-pedestrian           Dets      GT_Dets   IDs       GT_IDs
MOT20-01                           19870     19870     74        74
MOT20-02                           154740    154742    403       270
MOT20-03                           313655    313658    2096      702
MOT20-05                           646319    646344    3547      1169
COMBINED                           1134584   1134614   6120      2215
```

# Attributions

- [MaartenX](https://github.com/MaartenX/), for implementing some of the core pieces of this repo
- [abewley's Python implementation of Sort](https://github.com/abewley/sort), used for validation/reference
