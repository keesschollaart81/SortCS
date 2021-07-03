using System;

namespace SortCS.Kalman
{
    internal class KalmanFilter
    {
        private readonly int _stateSize;
        private readonly int _measurementSize;
        private readonly Matrix _identity;
        private readonly Matrix _processUncertainty;
        private readonly Matrix _stateTransitionMatrix;
        private readonly Matrix _measurementFunction;
        private readonly Matrix _measurementUncertainty;
        private readonly double _alphaSq;

        private Vector _currentState;
        private Matrix _uncertaintyCovariances;

        public KalmanFilter(int stateSize, int measurementSize)
        {
            _stateSize = stateSize;
            _measurementSize = measurementSize;
            _identity = Matrix.Identity(stateSize);
            _alphaSq = 1.0d;

            StateTransitionMatrix = _identity; // F 
            MeasurementFunction = new Matrix(_measurementSize, _stateSize); //  H
            UncertaintyCovariances = Matrix.Identity(_stateSize); // P
            MeasurementUncertainty = Matrix.Identity(_measurementSize); // R
            ProcessUncertainty = _identity; // Q
            CurrentState = new Vector(stateSize);
        }

        /// <summary>
        /// Gets or sets the current state.
        /// </summary>
        public Vector CurrentState
        {
            get => _currentState;
            set => _currentState = value.Length == _stateSize
                ? value
                : throw new ArgumentException($"Vector must be of size {_stateSize}.", nameof(value));
        }

        /// <summary>
        /// Gets the uncertainty covariances.
        /// </summary>
        public Matrix UncertaintyCovariances
        {
            get => _uncertaintyCovariances;
            init => _uncertaintyCovariances = value.Rows == _stateSize && value.Columns == _stateSize
                ? value
                : throw new ArgumentException($"Matrix must be of size {_stateSize}x{_stateSize}.", nameof(value));
        }

        /// <summary>
        /// Gets the process uncertainty.
        /// </summary>
        public Matrix ProcessUncertainty
        {
            get => _processUncertainty;
            init => _processUncertainty = value.Rows == _stateSize && value.Columns == _stateSize
                ? value
                : throw new ArgumentException($"Matrix must be of size {_stateSize}x{_stateSize}.", nameof(value));
        }

        public Matrix MeasurementUncertainty
        {
            get => _measurementUncertainty;
            init=>_measurementUncertainty = value.Rows == _measurementSize && value.Columns == _measurementSize
                ? value
                : throw new ArgumentException($"Matrix must be of size {_measurementSize}x{_measurementSize}.", nameof(value));
        }

        /// <summary>
        /// Gets the state transition matrix.
        /// </summary>
        public Matrix StateTransitionMatrix
        {
            get => _stateTransitionMatrix;
            init => _stateTransitionMatrix = value.Rows == _stateSize && value.Columns == _stateSize
                ? value
                : throw new ArgumentException($"Matrix must be of size {_stateSize}x{_stateSize}.", nameof(value));
        }

        /// <summary>
        /// Gets the measurement function.
        /// </summary>
        public Matrix MeasurementFunction
        {
            get => _measurementFunction;
            init => _measurementFunction = value.Rows == _measurementSize && value.Columns == _stateSize
                ? value
                : throw new ArgumentException($"Matrix must be of size {_measurementSize}x{_stateSize}.", nameof(value));
        }

        public void Predict(Matrix stateTransitionMatrix = null, Matrix processNoiseMatrix = null)
        {
            stateTransitionMatrix ??= StateTransitionMatrix;
            processNoiseMatrix ??= ProcessUncertainty;

            _currentState = stateTransitionMatrix.Dot(CurrentState);
            _uncertaintyCovariances = _alphaSq * stateTransitionMatrix * UncertaintyCovariances * stateTransitionMatrix.Transposed + processNoiseMatrix;
        }

        public void Update(Vector measurement, Matrix measurementNoise = null, Matrix measurementFunction = null)
        {
            measurementNoise ??= MeasurementUncertainty;
            measurementFunction ??= MeasurementFunction;

            var y = measurement - measurementFunction.Dot(CurrentState);
            var pht = UncertaintyCovariances * measurementFunction.Transposed;
            var S = (measurementFunction * pht) + measurementNoise;
            var SI = S.Inverted;
            var K = pht * SI;

            _currentState += K.Dot(y);

            var I_KH = _identity - (K * measurementFunction);

            _uncertaintyCovariances = (I_KH * UncertaintyCovariances * I_KH.Transposed) + (K * measurementNoise * K.Transposed);
        }
    }
}