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
        }
        #endregion

        #region IBall
        public event EventHandler<IVector>? NewPositionNotification;
        public IVector Velocity { get; set; }
        public double Mass { get; }
        public double Radius { get; }
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
        #endregion
    }
}