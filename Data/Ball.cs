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
            Radius = radius; // Promień przekazywany jako parametr
        }
        #endregion

        #region IBall
        public event EventHandler<IVector>? NewPositionNotification;
        public IVector Velocity { get; set; }
        public double Mass { get; }
        public double Radius { get; }
        #endregion

        #region public
        public IVector Position => _position;
        #endregion

        #region private
        private readonly double _tableWidth;
        private readonly double _tableHeight;
        private Vector _position;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, _position);
        }

        internal void Move(Vector delta)
        {
            double newX = _position.x + delta.x;
            double newY = _position.y + delta.y;

            // Boundary constraints
            if (newX - Radius < 0)
            {
                newX = Radius;
                Velocity = new Vector(-Velocity.x, Velocity.y);
            }
            else if (newX + Radius > _tableWidth)
            {
                newX = _tableWidth - Radius;
                Velocity = new Vector(-Velocity.x, Velocity.y);
            }
            if (newY - Radius < 0)
            {
                newY = Radius;
                Velocity = new Vector(Velocity.x, -Velocity.y);
            }
            else if (newY + Radius > _tableHeight)
            {
                newY = _tableHeight - Radius;
                Velocity = new Vector(Velocity.x, -Velocity.y);
            }

            _position = new Vector(newX, newY);
            RaiseNewPositionChangeNotification();
        }

        internal void CollideWith(Ball other)
        {
            Vector v1 = (Vector)Velocity;
            Vector v2 = (Vector)other.Velocity;
            Vector x1 = _position;
            Vector x2 = other._position;

            Vector dx = new Vector(x1.x - x2.x, x1.y - x2.y);
            Vector dv = new Vector(v1.x - v2.x, v1.y - v2.y);

            double dot = dx.x * dv.x + dx.y * dv.y;
            if (dot >= 0)
                return;

            double m1 = Mass;
            double m2 = other.Mass;
            double factor = 2 * m2 / (m1 + m2) * dot / (dx.x * dx.x + dx.y * dx.y);

            Velocity = new Vector(v1.x - factor * dx.x, v1.y - factor * dx.y);
            other.Velocity = new Vector(v2.x + factor * dx.x * m1 / m2, v2.y + factor * dx.y * m1 / m2);
        }
        #endregion
    }
}