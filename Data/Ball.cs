//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor
        internal Ball(Vector initialPosition, Vector initialVelocity, double tableWidth, double tableHeight, double radius)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            _tableWidth = tableWidth;
            _tableHeight = tableHeight;
            Mass = new Random().NextDouble() * 3 + 3;
            Radius = radius;
            _isRunning = true;
            _moveThread = null!;
        }
        #endregion

        #region IBall
        public event EventHandler<IVector>? NewPositionNotification;
        public IVector Velocity
        {
            get => new Vector(_velocity.x, _velocity.y);
            set
            {
                if (value is Vector vector)
                {
                    _velocity = vector;
                }
                else
                {
                    throw new ArgumentException("Velocity must be of type Vector");
                }
            }
        }
        public double Mass { get; }
        public double Radius { get; }
        public IVector Position
        {
            get => new Vector(_position.x, _position.y);
            set
            {
                if (value is Vector vector)
                {
                    _position = vector;
                    RaiseNewPositionChangeNotification();
                }
                else
                {
                    throw new ArgumentException("Position must be of type Vector");
                }
            }
        }
        public double TableWidth
        {
            get => _tableWidth;
        }
        public double TableHeight
        {
            get => _tableHeight;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _isRunning = false;
            _moveThread?.Join();
        }
        #endregion

        #region private
        private readonly double _tableWidth;
        private readonly double _tableHeight;
        private Vector _position;
        private Vector _velocity;
        private Thread? _moveThread;
        private volatile bool _isRunning;
        private bool _disposed = false;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, _position);
        }

        private void Move(Vector delta)
        {
            double newX = _position.x + delta.x;
            double newY = _position.y + delta.y;

            _position = new Vector(newX, newY);
            RaiseNewPositionChangeNotification();
        }

        internal void StartMoving()
        {
            _moveThread = new Thread(new ThreadStart(MoveContinuously));
            _moveThread.IsBackground = true;
            _moveThread.Start();
        }

        private void MoveContinuously()
        {
            while (_isRunning)
            {
                Move(_velocity);
                Thread.Sleep(10);
            }
        }
        #endregion
    }
}