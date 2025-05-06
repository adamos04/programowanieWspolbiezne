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
        public Ball(Data.IBall ball)
        {
            _dataBall = ball;
            _dataBall.NewPositionNotification += RaisePositionChangeEvent;
        }

        #region IBall
        public event EventHandler<IPosition>? NewPositionNotification;
        public double Radius => _dataBall.Radius;
        public double Mass => _dataBall.Mass;
        #endregion

        #region public
        public Data.IBall DataBall => _dataBall;
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

            // Aktualizacja prędkości
            _dataBall.Velocity = new Data.Vector(v1.x - factor * dx.x, v1.y - factor * dx.y);
            other._dataBall.Velocity = new Data.Vector(v2.x + factor * dx.x * m1 / m2, v2.y + factor * dx.y * m1 / m2);

            // Korekta pozycji, aby kulki się nie przenikały
            double distance = Math.Sqrt(dx.x * dx.x + dx.y * dx.y);
            double overlap = (Radius + other.Radius) - distance;
            if (overlap > 0)
            {
                // Normalizacja wektora dx
                double nx = dx.x / distance;
                double ny = dx.y / distance;
                // Przesunięcie kulek proporcjonalnie do ich mas
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

            // Korekta pozycji w przypadku przeniknięcia
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

            // Bezwarunkowa aktualizacja pozycji, jeśli zmieniono newX lub newY
            _dataBall.Position = new Data.Vector(newX, newY);
        }
        #endregion

        #region private
        private readonly Data.IBall _dataBall;

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }
        #endregion
    }
}