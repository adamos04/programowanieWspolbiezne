//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        public Ball(Data.IBall ball, List<Ball> otherBalls, object sharedLock)
        {
            _dataBall = ball;
            _otherBalls = otherBalls;
            _lock = sharedLock;
            _dataBall.NewPositionNotification += RaisePositionChangeEvent;
            _collisionCts = new CancellationTokenSource();
            _collisionTask = Task.Run(() => DetectCollisionsAsync(_collisionCts.Token), _collisionCts.Token);
        }

        #region IBall
        public event EventHandler<IPosition>? NewPositionNotification;
        public double Radius => _dataBall.Radius;
        public double Mass => _dataBall.Mass;
        #endregion

        #region public
        public Data.IBall DataBall => _dataBall;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _collisionCts.Cancel();
            try
            {
                _collisionTask.Wait();
            }
            catch (AggregateException)
            {
                
            }
            _collisionCts.Dispose();
            _dataBall.Dispose();
        }
        #endregion

        #region internal
        internal void CollideWith(Ball other)
        {
            Data.IVector v1 = _dataBall.Velocity;
            Data.IVector v2 = other._dataBall.Velocity;
            Data.IVector x1 = _dataBall.Position;
            Data.IVector x2 = other._dataBall.Position;

            Data.Vector dx = new Data.Vector(x1.x - x2.x, x1.y - x2.y);
            Data.Vector dv = new Data.Vector(v1.x - v2.x, v1.y - v2.y);

            double dot = dx.x * dv.x + dx.y * dv.y;
            if (dot >= 0)
                return;

            double m1 = Mass;
            double m2 = other.Mass;
            double factor = 2 * m2 / (m1 + m2) * dot / (dx.x * dx.x + dx.y * dx.y);

            _dataBall.Velocity = new Data.Vector(v1.x - factor * dx.x, v1.y - factor * dx.y);
            other._dataBall.Velocity = new Data.Vector(v2.x + factor * dx.x * m1 / m2, v2.y + factor * dx.y * m1 / m2);

            double distance = Math.Sqrt(dx.x * dx.x + dx.y * dx.y);
            double overlap = (Radius + other.Radius) - distance;
            if (overlap > 0)
            {
                double nx = dx.x / distance;
                double ny = dx.y / distance;

                double correctionFactor = overlap / (m1 + m2);
                _dataBall.Position = new Data.Vector(x1.x + nx * correctionFactor * m2, x1.y + ny * correctionFactor * m2);
                other._dataBall.Position = new Data.Vector(x2.x - nx * correctionFactor * m1, x2.y - ny * correctionFactor * m1);
            }
        }

        internal void CheckWallCollisions()
        {
            double borderThickness = 8.0;
            double newX = _dataBall.Position.x;
            double newY = _dataBall.Position.y;
            double tableWidth = _dataBall.TableWidth;
            double tableHeight = _dataBall.TableHeight;

            if (newX - Radius < 0)
            {
                newX = Radius;
                _dataBall.Velocity = new Data.Vector(-_dataBall.Velocity.x, _dataBall.Velocity.y);
            }
            else if (newX + Radius > tableWidth - borderThickness)
            {
                newX = tableWidth - borderThickness - Radius;
                _dataBall.Velocity = new Data.Vector(-_dataBall.Velocity.x, _dataBall.Velocity.y);
            }
            if (newY - Radius < 0)
            {
                newY = Radius;
                _dataBall.Velocity = new Data.Vector(_dataBall.Velocity.x, -_dataBall.Velocity.y);
            }
            else if (newY + Radius > tableHeight - borderThickness)
            {
                newY = tableHeight - borderThickness - Radius;
                _dataBall.Velocity = new Data.Vector(_dataBall.Velocity.x, -_dataBall.Velocity.y);
            }

            _dataBall.Position = new Data.Vector(newX, newY);
        }
        #endregion

        #region private
        private readonly Data.IBall _dataBall;
        private readonly List<Ball> _otherBalls;
        private readonly object _lock;
        private readonly Task _collisionTask;
        private readonly CancellationTokenSource _collisionCts;
        private bool _disposed = false;

        private async Task DetectCollisionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                CheckWallCollisions();

                lock (_lock)
                {
                    foreach (var otherBall in _otherBalls)
                    {
                        if (otherBall == this) continue;
                        double dx = _dataBall.Position.x - otherBall.DataBall.Position.x;
                        double dy = _dataBall.Position.y - otherBall.DataBall.Position.y;
                        double distance = Math.Sqrt(dx * dx + dy * dy);
                        if (distance < Radius + otherBall.Radius)
                        {
                            CollideWith(otherBall);
                        }
                    }
                }
                if (cancellationToken.IsCancellationRequested)
                    break;
                await Task.Delay(10);
                
            }
        }

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }
        #endregion
    }
}