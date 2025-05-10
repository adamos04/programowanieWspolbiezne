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
            Velocity = initialVelocity;
            _tableWidth = tableWidth;
            _tableHeight = tableHeight;
            Mass = new Random().NextDouble() * 5 + double.Epsilon;
            Radius = radius;
            _cts = new CancellationTokenSource();
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
            if (_disposed) return; // Zabezpieczenie przed wielokrotnym wywołaniem Dispose
            _disposed = true;

            _cts.Cancel(); // Anulujemy zadanie
            if (_moveTask != null)
            {
                try
                {
                    _moveTask.Wait(); // Czekamy na zakończenie zadania
                }
                catch (AggregateException)
                {
                    // Ignorujemy wyjątki, jeśli zadanie zostało anulowane
                }
            }
            _cts.Dispose(); // Teraz bezpiecznie utylizujemy _cts
        }
        #endregion

        #region private
        private readonly double _tableWidth;
        private readonly double _tableHeight;
        private Vector _position;
        private Vector _velocity;
        private readonly CancellationTokenSource _cts;
        private Task _moveTask;
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
            _moveTask = Task.Run(() => RunAsync(_cts.Token), _cts.Token);
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Move(_velocity);
                    await Task.Delay(10, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
        #endregion
    }
}