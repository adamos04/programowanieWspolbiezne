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
        public Ball(Data.IBall ball, List<Ball> otherBalls, object sharedLock, double tableWidth, double tableHeight, double radius)
        {
            _dataBall = ball;
            _otherBalls = otherBalls;
            _lock = sharedLock;
            _tableWidth = tableWidth;
            _tableHeight = tableHeight;
            _radius = radius;
            _dataBall.NewPositionNotification += RaisePositionChangeEvent;
        }

        #region IBall
        public event EventHandler<IPosition>? NewPositionNotification;
        public double Radius => _radius;
        public double Mass => _dataBall.Mass;
        #endregion

        #region public
        public Data.IBall DataBall => _dataBall;

        public void Dispose()
        {
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

            Data.DiagnosticLogger.Instance.Log(
                $"BallCollision: Ball1 (ID: {GetHashCode()}, Pos: {x1.x:F2}, {x1.y:F2}, Vel: {v1.x:F2}, {v1.y:F2}, Mass: {m1:F2}) " +
                $"with Ball2 (ID: {other.GetHashCode()}, Pos: {x2.x:F2}, {x2.y:F2}, Vel: {v2.x:F2}, {v2.y:F2}, Mass: {m2:F2})"
            );
        }

        internal void CheckWallCollisions(Data.IVector position)
        {
            double borderThickness = 8.0;
            double newX = position.x;
            double newY = position.y;
            Data.Vector velocity = (Data.Vector)_dataBall.Velocity;
            bool velocityChanged = false;

            if (newX - Radius <= 0 && velocity.x < 0)
            {
                velocity = new Data.Vector(-velocity.x, velocity.y);
                velocityChanged = true;
                TP.ConcurrentProgramming.Data.DiagnosticLogger.Instance.Log(
                    $"WallCollision: Ball (ID: {GetHashCode()}, Pos: {newX:F2}, {newY:F2}, Vel: {velocity.x:F2}, {velocity.y:F2}, Mass: {Mass:F2}, Wall: Left)"
                );
            }
            else if (newX + Radius >= _tableWidth - borderThickness && velocity.x > 0)
            {
                velocity = new Data.Vector(-velocity.x, velocity.y);
                velocityChanged = true;
                TP.ConcurrentProgramming.Data.DiagnosticLogger.Instance.Log(
                    $"WallCollision: Ball (ID: {GetHashCode()}, Pos: {newX:F2}, {newY:F2}, Vel: {velocity.x:F2}, {velocity.y:F2}, Mass: {Mass:F2}, Wall: Right)"
                );
            }
            if (newY - Radius <= 0 && velocity.y < 0)
            {
                velocity = new Data.Vector(velocity.x, -velocity.y);
                velocityChanged = true;
                TP.ConcurrentProgramming.Data.DiagnosticLogger.Instance.Log(
                    $"WallCollision: Ball (ID: {GetHashCode()}, Pos: {newX:F2}, {newY:F2}, Vel: {velocity.x:F2}, {velocity.y:F2}, Mass: {Mass:F2}, Wall: Top)"
                );
            }
            else if (newY + Radius >= _tableHeight - borderThickness && velocity.y > 0)
            {
                velocity = new Data.Vector(velocity.x, -velocity.y);
                velocityChanged = true;
                TP.ConcurrentProgramming.Data.DiagnosticLogger.Instance.Log(
                    $"WallCollision: Ball (ID: {GetHashCode()}, Pos: {newX:F2}, {newY:F2}, Vel: {velocity.x:F2}, {velocity.y:F2}, Mass: {Mass:F2}, Wall: Bottom)"
                );
            }

            if (velocityChanged)
            {
                _dataBall.Velocity = velocity;
            }
        }
        #endregion

        #region private
        private readonly Data.IBall _dataBall;
        private readonly List<Ball> _otherBalls;
        private readonly object _lock;
        private readonly double _tableWidth;
        private readonly double _tableHeight;
        private readonly double _radius;

        private void DetectCollisions(Data.IVector myPosition)
        {
            lock (_lock)
            {
                foreach (var otherBall in _otherBalls)
                {
                    if (otherBall == this) continue;

                    Data.IVector otherPostion = otherBall._dataBall.Position;
                    double dx = myPosition.x - otherPostion.x;
                    double dy = myPosition.y - otherPostion.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    if (distance <= Radius + otherBall.Radius)
                    {
                        CollideWith(otherBall);
                    }
                }
            }
        }

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            Data.IVector newPosition = e;
            DetectCollisions(newPosition);
            CheckWallCollisions(newPosition);
            NewPositionNotification?.Invoke(this, new Position(newPosition.x, newPosition.y));
        }
        #endregion
    }
}