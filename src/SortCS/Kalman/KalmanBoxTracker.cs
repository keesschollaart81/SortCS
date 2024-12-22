using System;
using System.Collections.Generic;
using System.Drawing;

namespace SortCS.Kalman
{
    internal class KalmanBoxTracker
    {
        private static Matrix _stateTransitioningMatrix = new Matrix(
            new double[,]
            {
                { 1, 0, 0, 0, 1, 0, 0 },
                { 0, 1, 0, 0, 0, 1, 0 },
                { 0, 0, 1, 0, 0, 0, 1 },
                { 0, 0, 0, 1, 0, 0, 0 },
                { 0, 0, 0, 0, 1, 0, 0 },
                { 0, 0, 0, 0, 0, 1, 0 },
                { 0, 0, 0, 0, 0, 0, 1 }
            });

        private static Matrix _measurementFunction = new Matrix(
            new double[,]
            {
                { 1, 0, 0, 0, 0, 0, 0 },
                { 0, 1, 0, 0, 0, 0, 0 },
                { 0, 0, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 0, 0, 0 }
            });

        private static Matrix _uncertaintyCovariances = new Matrix(
            new double[,]
            {
                { 10, 0, 0, 0, 0, 0, 0 },
                { 0, 10, 0, 0, 0, 0, 0 },
                { 0, 0, 10, 0, 0, 0, 0 },
                { 0, 0, 0, 10, 0, 0, 0 },
                { 0, 0, 0, 0, 10000, 0, 0 },
                { 0, 0, 0, 0, 0, 10000, 0 },
                { 0, 0, 0, 0, 0, 0, 10000 }
            });

        private static Matrix _measurementUncertainty = new Matrix(new double[,]
        {
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, 10, 0 },
            { 0, 0, 0, 10 },
        });

        private static Matrix _processUncertainty = new Matrix(
            new double[,]
            {
                { 1, 0, 0, 0, 0, 0, 0 },
                { 0, 1, 0, 0, 0, 0, 0 },
                { 0, 0, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 0, 0, 0 },
                { 0, 0, 0, 0, .01, 0, 0 },
                { 0, 0, 0, 0, 0, .01, 0 },
                { 0, 0, 0, 0, 0, 0, .0001 }
            });

        private readonly KalmanFilter _filter;

        public KalmanBoxTracker(RectangleF box)
        {
            _filter = new KalmanFilter(7, 4)
            {
                StateTransitionMatrix = _stateTransitioningMatrix,
                MeasurementFunction = _measurementFunction,
                UncertaintyCovariances = _uncertaintyCovariances,
                MeasurementUncertainty = _measurementUncertainty,
                ProcessUncertainty = _processUncertainty,
                CurrentState = ToMeasurement(box).Append(0, 0, 0)
            };
        }

        public void Update(RectangleF box)
        {
            _filter.Update(ToMeasurement(box));
        }

        public RectangleF Predict()
        {
            if (_filter.CurrentState[6] + _filter.CurrentState[2] <= 0)
            {
                _filter.SetState(6, 0);
            }

            _filter.Predict();

            var prediction = ToBoundingBox(_filter.CurrentState);

            return prediction;
        }

        private static Vector ToMeasurement(RectangleF box)
        {
            var center = new PointF(box.Left + (box.Width / 2f), box.Top + (box.Height / 2f));
            return new Vector(center.X, center.Y, box.Width * (double)box.Height, box.Width / (double)box.Height);
        }

        private static RectangleF ToBoundingBox(Vector currentState)
        {
            var w = Math.Sqrt(currentState[2] * currentState[3]);
            var h = currentState[2] / w;

            return new RectangleF(
                (float)(currentState[0] - (w / 2)),
                (float)(currentState[1] - (h / 2)),
                (float)w,
                (float)h);
        }
    }
}