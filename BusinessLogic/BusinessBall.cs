//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        public Ball(Data.IBall ball, List<Ball> otherBalls, object sharedLock, double tableWidth, double tableHeight, double radius, ILogger logger)
        {
            _dataBall = ball;
            _otherBalls = otherBalls;
            _lock = sharedLock;
            _tableWidth = tableWidth;
            _tableHeight = tableHeight;
            _radius = radius;
            _logger = logger;
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
        internal void CollideWith(Ball other, double distance, double dx, double dy)
        {
            Data.IVector myVelocity = _dataBall.Velocity;
            Data.IVector otherVelocity = other._dataBall.Velocity;
            Data.IVector myPosition = _dataBall.Position;
            Data.IVector otherPosition = other._dataBall.Position;

            if (distance == 0)
                return;

            double nx = dx / distance;
            double ny = dy / distance;

            double dvx = myVelocity.x - otherVelocity.x;
            double dvy = myVelocity.y - otherVelocity.y;

            double impactSpeed = dvx * nx + dvy * ny;

            if (impactSpeed > 0)
                return;

            double m1 = Mass;
            double m2 = other.Mass;

            double impulse = -(2 * impactSpeed) / (m1 + m2);

            double newXVel = myVelocity.x + impulse * m2 * nx;
            double newYVel = myVelocity.y + impulse * m2 * ny;

            double newOtherXVel = otherVelocity.x - impulse * m1 * nx;
            double newOtherYVel = otherVelocity.y - impulse * m1 * ny;

            _dataBall.UpdateVelocity(newXVel, newYVel);
            other._dataBall.UpdateVelocity(newOtherXVel, newOtherYVel);
            _logger.Log(1, _dataBall.GetHashCode(), myPosition, newXVel, newYVel, m1,
                        other._dataBall.GetHashCode(), otherPosition, newOtherXVel, newOtherYVel, m2);
        }

        internal void CheckWallCollisions(Data.IVector position)
        {
            double borderThickness = 8.0;
            double newX = position.x;
            double newY = position.y;
            Data.IVector velocity = _dataBall.Velocity;
            double newVelocityX;
            double newVelocityY;

            if (newX - Radius <= 0 && velocity.x < 0)
            {
                newVelocityX = -velocity.x;
                newVelocityY = velocity.y;
                _dataBall.UpdateVelocity(newVelocityX, newVelocityY);
                _logger.Log(2, _dataBall.GetHashCode(), position, newVelocityX, newVelocityY, Mass);
            }
            else if (newX + Radius >= _tableWidth - borderThickness && velocity.x > 0)
            {
                newVelocityX = -velocity.x;
                newVelocityY = velocity.y;
                _dataBall.UpdateVelocity(newVelocityX, newVelocityY);
                _logger.Log(3, _dataBall.GetHashCode(), position, newVelocityX, newVelocityY, Mass);
            }
            if (newY - Radius <= 0 && velocity.y < 0)
            {
                newVelocityX = velocity.x;
                newVelocityY = -velocity.y;
                _dataBall.UpdateVelocity(newVelocityX, newVelocityY);
                _logger.Log(4, _dataBall.GetHashCode(), position, newVelocityX, newVelocityY, Mass);
            }
            else if (newY + Radius >= _tableHeight - borderThickness && velocity.y > 0)
            {
                newVelocityX = velocity.x;
                newVelocityY = -velocity.y;
                _dataBall.UpdateVelocity(newVelocityX, newVelocityY);
                _logger.Log(5, _dataBall.GetHashCode(), position, newVelocityX, newVelocityY, Mass);
            }
        }
        #endregion

        #region private
        private Data.IBall _dataBall;
        private readonly List<Ball> _otherBalls;
        private readonly object _lock;
        private readonly double _tableWidth;
        private readonly double _tableHeight;
        private readonly double _radius;
        private readonly ILogger _logger;

        private void DetectCollisions(Data.IVector myPosition)
        {
            lock (_lock)
            {
                foreach (var otherBall in _otherBalls)
                {
                    if (otherBall == this) continue;

                    Data.IVector otherPosition = otherBall._dataBall.Position;
                    double dx = myPosition.x - otherPosition.x;
                    double dy = myPosition.y - otherPosition.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    if (distance <= Radius + otherBall.Radius)
                    {
                        CollideWith(otherBall, distance, dx, dy);
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