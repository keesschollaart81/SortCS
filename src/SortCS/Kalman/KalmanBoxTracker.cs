using System;
using System.Collections.Generic;

namespace SortCS.Kalman
{
    internal class KalmanBoxTracker
    {
        private readonly KalmanFilter _filter;
        private readonly List<BoundingBox> _history = new List<BoundingBox>();
        private int _timeSinceUpdate;
        private int _hits;
        private int _hitStreak;
        private int _age;

        public KalmanBoxTracker(BoundingBox box)
        {
            _filter = new KalmanFilter(7, 4)
            {
                StateTransitionMatrix = new Matrix(
                    new double[,]
                    {
                        { 1, 0, 0, 0, 1, 0, 0 },
                        { 0, 1, 0, 0, 0, 1, 0 },
                        { 0, 0, 1, 0, 0, 0, 1 },
                        { 0, 0, 0, 1, 0, 0, 0 },
                        { 0, 0, 0, 0, 1, 0, 0 },
                        { 0, 0, 0, 0, 0, 1, 0 },
                        { 0, 0, 0, 0, 0, 0, 1 }
                    }),
                MeasurementFunction = new Matrix(
                    new double[,]
                    {
                        { 1, 0, 0, 0, 0, 0, 0 },
                        { 0, 1, 0, 0, 0, 0, 0 },
                        { 0, 0, 1, 0, 0, 0, 0 },
                        { 0, 0, 0, 1, 0, 0, 0 }
                    }),
                UncertaintyCovariances = new Matrix(
                    new double[,]
                    {
                        { 10, 0, 0, 0, 0, 0, 0 },
                        { 0, 10, 0, 0, 0, 0, 0 },
                        { 0, 0, 10, 0, 0, 0, 0 },
                        { 0, 0, 0, 10, 0, 0, 0 },
                        { 0, 0, 0, 0, 10000, 0, 0 },
                        { 0, 0, 0, 0, 0, 10000, 0 },
                        { 0, 0, 0, 0, 0, 0, 10000 }
                    }),
                MeasurementUncertainty = new Matrix(new double[,]
                {
                    { 1, 0, 0, 0 },
                    { 0, 1, 0, 0 },
                    { 0, 0, 10, 0 },
                    { 0, 0, 0, 10 },
                }),
                ProcessUncertainty = new Matrix(
                    new double[,]
                    {
                        { 1, 0, 0, 0, 0, 0, 0 },
                        { 0, 1, 0, 0, 0, 0, 0 },
                        { 0, 0, 1, 0, 0, 0, 0 },
                        { 0, 0, 0, 1, 0, 0, 0 },
                        { 0, 0, 0, 0, .01, 0, 0 },
                        { 0, 0, 0, 0, 0, .01, 0 },
                        { 0, 0, 0, 0, 0, 0, .0001 }
                    }),
                CurrentState = ToMeasurement(box).Append(0, 0, 0)
            };
        }

        public void Update(BoundingBox box)
        {
            _timeSinceUpdate = 0;
            _history.Clear();
            _hits++;
            _hitStreak++;
            _filter.Update(ToMeasurement(box));
        }

        public BoundingBox Predict()
        {
            if (_filter.CurrentState[6] + _filter.CurrentState[2] <= 0)
            {
                var state = _filter.CurrentState.ToArray();
                state[6] = 0;
                _filter.CurrentState = new Vector(state);
            }

            _filter.Predict();
            _age++;

            if (_timeSinceUpdate > 0)
            {
                _hitStreak = 0;
            }

            _timeSinceUpdate++;

            var prediction = ToBoundingBox(_filter.CurrentState);

            _history.Add(prediction);

            return prediction;
        }

        private static Vector ToMeasurement(BoundingBox box)
        {
            return new Vector(box.Center.X, box.Center.Y, box.Box.Width * (double)box.Box.Height, box.Box.Width / (double)box.Box.Height);
        }

        private static BoundingBox ToBoundingBox(Vector currentState)
        {
            var w = Math.Sqrt(currentState[2] * currentState[3]);
            var h = currentState[2] / w;

            return new BoundingBox(
                0,
                string.Empty,
                (float)(currentState[0] - (w / 2)),
                (float)(currentState[1] - (h / 2)),
                (float)w,
                (float)h,
                0);
        }
    }
}