using System;
using System.Diagnostics.CodeAnalysis;

namespace SortCS.Kalman
{
    [SuppressMessage("Major Code Smell", "S3928:Parameter names used into ArgumentException constructors should match an existing one ",
        Justification = "Properties throw ArgumentException for 'value'")]
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
        private Matrix _pht;
        private Matrix _s;
        private Matrix _si;
        private Matrix _k;
        private Matrix _kh;
        private Matrix _ikh;

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
            set => _currentState = value.Size == _stateSize
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
            init => _measurementUncertainty = value.Rows == _measurementSize && value.Columns == _measurementSize
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

        public void SetState(int index, double values)
        {
            _currentState[index] = values;
        }

        public void Predict()
        {
            _currentState = StateTransitionMatrix.Dot(CurrentState);
            _uncertaintyCovariances = (_alphaSq * StateTransitionMatrix * UncertaintyCovariances * StateTransitionMatrix.Transposed) +
                                      ProcessUncertainty;
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter",
            Justification = "These are well known abbreviations for the Kalman Filter")]
        public void Update(Vector measurement)
        {
            _pht ??= UncertaintyCovariances * MeasurementFunction.Transposed;
            _s ??= (MeasurementFunction * _pht) + MeasurementUncertainty;
            _si ??= _s.Inverted;
            _k ??= _pht * _si;
            _kh ??= _k * MeasurementFunction;
            _ikh ??= _identity - _kh;

            var y = measurement - MeasurementFunction.Dot(CurrentState);

            _currentState += _k.Dot(y);

            _uncertaintyCovariances = (_ikh * UncertaintyCovariances * _ikh.Transposed) + (_k * MeasurementUncertainty * _k.Transposed);
        }
    }
}