# SortCS 

SortCS is a 'Multiple Object Tracker' as described in [this paper](https://arxiv.org/abs/1602.00763), implemented in C#.

> SORT is a barebones implementation of a visual multiple object tracking framework based on rudimentary data association and state estimation techniques. It is designed for online tracking applications where only past and current frames are available and the method produces object identities on the fly. While this minimalistic tracker doesn't handle occlusion or re-entering objects its purpose is to serve as a baseline and testbed for the development of future trackers.
> SORT was initially described in this paper. At the time of the initial publication, SORT was ranked the best open source multiple object tracker on the MOT benchmark.

## Using

```cs
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
```

## Evaluation

The performance of this implementation can be evaluation using the 'SortCS.Evaluate' Console Application.
The output can be used for https://motchallenge.net/ and their [TrackEval SDK](https://github.com/JonathonLuiten/TrackEval/). 
Brief instructions:
- Clone this repo and the TrackEval in the same folder / next to each other
- Run the SortCS.Evaluate Console app. The `--data-folder` arguments needs to point to the `TrackEval/data` folder.
- Make sure that the outputs (tracks+detections) will be stored in `TrackEval/data/trackers/mot_challenge/MOT15-train/SortCS/data`
- Run TrackEval (according to their [readme](https://github.com/JonathonLuiten/TrackEval/blob/master/docs/MOTChallenge-Official/Readme.md)):
  `python scripts/run_mot_challenge.py --BENCHMARK MOT15 --SPLIT_TO_EVAL train --TRACKERS_TO_EVAL SortCS --METRICS HOTA CLEAR Identity VACE --USE_PARALLEL False --NUM_PARALLEL_CORES 1`

Example results for SortCS & MOT15:
```
All sequences for SortCs finished in 5.05 seconds

HOTA: SortCs-pedestrian            HOTA      DetA      AssA      DetRe     DetPr     AssRe     AssPr     LocA      RHOTA     HOTA(0)   LocA(0)   HOTALocA(0)
ADL-Rundle-6                       4.3363    2.571     7.8576    4.493     4.493     11.031    13.115    67.316    5.7713    32.43     13.747    4.4582
ADL-Rundle-8                       2.7022    1.3631    5.7027    2.4776    2.4776    8.548     9.183     75.478    3.6616    24.717    11.356    2.807
ETH-Bahnhof                        3.4095    1.9645    7.0184    4.1503    2.9301    11.361    9.8899    69.038    4.9861    31.03     11.59     3.5963
ETH-Pedcross2                      13.797    9.0305    25.56     13.965    12.942    33.587    33.834    59.481    17.5      67.366    19.91     13.412
ETH-Sunnyday                       5.6667    4.412     7.6376    6.3481    6.2045    10.226    9.5539    78.567    6.8993    64.843    10.092    6.544
KITTI-13                           0         0         0         0         0         0         0         100       0         0         100       0
KITTI-17                           0         0         0         0         0         0         0         100       0         0         100       0
PETS09-S2L1                        0         0         0         0         0         0         0         100       0         0         100       0
TUD-Campus                         6.9847    5.8945    8.4395    8.4592    8.4592    11.046    11.208    78.839    8.4223    59.717    13.222    7.896
TUD-Stadtmitte                     0.013086  0.0022774 0.075188  0.0045529 0.0045529 0.08489   0.5848    95.107    0.018502  0.24863   7.0346    0.01749
Venice-2                           3.7967    2.9194    5.7248    4.9897    4.9897    8.0368    11.904    67.227    5.0247    27.561    13.008    3.5852
COMBINED                           6.1129    2.7381    19.632    5.0048    4.6297    27.623    28.204    58.592    8.3781    35.171    14.588    5.1308
```

# Attributions

- [MaartenX, for implementing some of the core pieces of this repo](https://github.com/MaartenX/)
- [abewley's Python implementation of Sort, used for validation/reference](https://github.com/abewley/sort)
