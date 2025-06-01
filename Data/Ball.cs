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
        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            Mass = new Random().NextDouble() * 3 + 3;
            _isRunning = true;
            _moveThread = null!;
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
            _velocity = new Vector(x,y);
        }

        public IVector Position => _position;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _isRunning = false;
            _moveThread?.Join();
        }
        #endregion

        #region private
        private Vector _position;
        private Vector _velocity;
        private Thread? _moveThread;
        private volatile bool _isRunning;
        private bool _disposed = false;
        private readonly DiagnosticLogger _logger = DiagnosticLogger.Instance;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, _position);
        }

        private void Move()
        {
            Vector velocity = (Vector)Velocity;
            _position = new Vector(_position.x + velocity.x, _position.y + velocity.y);
            _logger.Log($"ID: {GetHashCode()}, Ball Position: ({_position.x:F2}, {_position.y:F2}), Velocity: ({velocity.x:F2}, {velocity.y:F2}), Mass: {Mass:F2}");
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
                Move();
                double speed = Math.Sqrt(_velocity.x * _velocity.x + _velocity.y * _velocity.y);
                int delay = (int)Math.Clamp(1000 / (speed * 40), 10, 30);
                Thread.Sleep(delay);
            }
        }
        #endregion
    }
}