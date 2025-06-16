//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor
        internal Ball(Vector initialPosition, Vector initialVelocity, ILogger logger)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            Mass = new Random().NextDouble() * 3 + 3;
            _isRunning = true;
            _moveThread = null!;
            this.logger = logger;
        }
        #endregion

        #region IBall
        public event EventHandler<IVector>? NewPositionNotification;
        public IVector Velocity
        {
            get => _velocity;
        }
        public double Mass { get; }

        public void UpdateVelocity(double x, double y)
        {
            _velocity = new Vector(x, y);
        }

        public IVector Position => _position;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _isRunning = false;
        }
        #endregion

        #region private
        private Vector _position;
        private Vector _velocity;
        private Thread? _moveThread;
        private volatile bool _isRunning;
        private bool _disposed = false;
        private readonly ILogger logger;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, _position);
        }

        private void Move(double deltaTime)
        {
            Vector velocity = (Vector)Velocity;
            _position = new Vector(_position.x + velocity.x * deltaTime, _position.y + velocity.y * deltaTime);
            logger.Log(DateTime.UtcNow, GetHashCode(), _position, velocity.x, velocity.y, Mass);
            RaiseNewPositionChangeNotification();
        }

        internal void StartMoving()
        {
            _moveThread = new Thread(new ThreadStart(MoveContinuously));
            _moveThread.Start();
        }

        private void MoveContinuously()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            double lastUpdateTime = 0.0;

            while (_isRunning)
            {
                double currentTime = stopwatch.Elapsed.TotalSeconds;
                double deltaTime = currentTime - lastUpdateTime;

                if (deltaTime > 0.0)
                {
                    Move(deltaTime);
                    lastUpdateTime = currentTime;
                }

                double speed = Math.Sqrt(_velocity.x * _velocity.x + _velocity.y * _velocity.y);
                int delay = (int)Math.Clamp(1000.0 / (speed * 40.0 + 0.1), 10, 30);
                Thread.Sleep(delay);
            }
        }
        #endregion
    }
}