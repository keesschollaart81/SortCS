using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SortCS.Kalman;
using HungarianAlgorithm;

namespace SortCS
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

                if (box.Box.IsEmpty) // todo @ Maarten... Ik heb dit omgegooid naar the IsEmpty property van de RectangleF. Werkt dit nu nog??
                {
                    toDelete.Add(tracker);
                }
                else
                {
                    trackedBoxes.Add(box);
                }
            }

            _trackers.RemoveAll(t => toDelete.Contains(t));
            
            MatchDetectionsWithTrackers(boxes.ToArray(), trackedBoxes);
            yield break;
        }
        
        private void MatchDetectionsWithTrackers(
            ICollection<BoundingBox> boxes,
            ICollection<BoundingBox> trackers)
        {
            var matrix = trackers.SelectMany((tracker) => boxes.Select((box) =>
            {
                var intersection = RectangleF.Intersect(box.Box, tracker.Box);
                var union = RectangleF.Union(box.Box, tracker.Box);
                var intersectionArea = (double) (intersection.Width * intersection.Height);
                var unionArea = (double) (union.Width * union.Height);

                var iou = unionArea < double.Epsilon ? 0 : intersectionArea / unionArea;

                return (int)((1-iou) * 100); // int costs?
            })).ToArray(boxes.Count, trackers.Count);
            
            var matched = matrix.FindAssignments();

            var unmatched = boxes.Where((b, index) => !matched.Contains(index));
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
    }

    public static class EnumerableExtensions
    {

        public static T[,] ToArray<T>(this IEnumerable<T> source, int firstDimensionLength, int secondDimensionLength)
        {
            var array = source.ToArray();
            var result = new T[firstDimensionLength, secondDimensionLength];

            for (var i = 0; i < array.Length; i++)
            {
                result[i / secondDimensionLength, i % secondDimensionLength] = array[i];
            }

            return result;
        } 
    }

}
