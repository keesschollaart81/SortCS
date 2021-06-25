using System;
using System.Collections.Generic;
using System.Drawing;
using SortCS.Kalman;

namespace sortcs
{
    public class SortTracker : ITracker
    {
        private readonly List<KalmanBoxTracker> _trackers;
        private int _frameCount;

        public SortTracker()
        {
            _trackers = new List<KalmanBoxTracker>();
            _frameCount = 0;
        }

        public int MaxAge { get; init; } = 1;
        public int MinHits { get; init; } = 3;
        public float IouThreshold { get; init; } = 0.3f;

        public IEnumerable<Track> Track(IEnumerable<BoundingBox> boxes)
        {
            _frameCount++;

            var toDelete = new List<KalmanBoxTracker>();
            var trackedBoxes = new List<BoundingBox>();

            foreach (var tracker in _trackers)
            {
                var box = tracker.Predict();

                if (box.IsInvalid)
                {
                    toDelete.Add(tracker);
                }
                else
                {
                    trackedBoxes.Add(box);
                }
            }

            _trackers.RemoveAll(t => toDelete.Contains(t));

            yield break;
        }

        public void MatchDetectionsWithTrackers(
            IEnumerable<BoundingBox> boxes,
            IEnumerable<BoundingBox> trackers)
        {
            if ()
            // todo: port from sort.py
        }


        // if(len(trackers)==0):
        //     return np.empty((0,2),dtype=int), np.arange(len(detections)), np.empty((0,5),dtype=int)

        //   iou_matrix = iou_batch(detections, trackers)

        //   if min(iou_matrix.shape) > 0:
        //     a = (iou_matrix > iou_threshold).astype(np.int32)
        //     if a.sum(1).max() == 1 and a.sum(0).max() == 1:
        //         matched_indices = np.stack(np.where(a), axis=1)
        //     else:
        //       matched_indices = linear_assignment(-iou_matrix)
        //   else:
        //     matched_indices = np.empty(shape=(0,2))

        //   unmatched_detections = []
        //   for d, det in enumerate(detections):
        //     if(d not in matched_indices[:,0]):
        //       unmatched_detections.append(d)
        //   unmatched_trackers = []
        //   for t, trk in enumerate(trackers):
        //     if(t not in matched_indices[:,1]):
        //       unmatched_trackers.append(t)

        //   #filter out matched with low IOU
        //   matches = []
        //   for m in matched_indices:
        //     if(iou_matrix[m[0], m[1]]<iou_threshold):
        //       unmatched_detections.append(m[0])
        //       unmatched_trackers.append(m[1])
        //     else:
        //       matches.append(m.reshape(1,2))
        //   if(len(matches)==0):
        //     matches = np.empty((0,2),dtype=int)
        //   else:
        //     matches = np.concatenate(matches,axis=0)

        //   return matches, np.array(unmatched_detections), np.array(unmatched_trackers)




        private float IntersectionOverUnion(RectangleF boundingBoxA, RectangleF boundingBoxB)
        {
            var areaA = boundingBoxA.Width * boundingBoxA.Height;

            if (areaA <= 0)
                return 0;

            var areaB = boundingBoxB.Width * boundingBoxB.Height;

            if (areaB <= 0)
                return 0;

            var minX = Math.Max(boundingBoxA.Left, boundingBoxB.Left);
            var minY = Math.Max(boundingBoxA.Top, boundingBoxB.Top);
            var maxX = Math.Min(boundingBoxA.Right, boundingBoxB.Right);
            var maxY = Math.Min(boundingBoxA.Bottom, boundingBoxB.Bottom);

            var intersectionArea = Math.Max(maxY - minY, 0) * Math.Max(maxX - minX, 0);

            return intersectionArea / (areaA + areaB - intersectionArea);
        }
    }

}
