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

        private readonly double _tableWidth;
        private readonly double _tableHeight;

        internal Ball(Vector initialPosition, Vector initialVelocity, double tableWidth, double tableHeight)
    {
      Position = initialPosition;
      Velocity = initialVelocity;
      _tableWidth = tableWidth;
      _tableHeight = tableHeight;
        }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;

    public IVector Velocity { get; set; }

    #endregion IBall

    #region private

    private Vector Position;

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

        //internal void Move(Vector delta)
        //{
        //  Position = new Vector(Position.x + delta.x, Position.y + delta.y);
        //  RaiseNewPositionChangeNotification();
        //}

        // Add boundary constraints to ensure the ball does not move outside the table dimensions.
        internal void Move(Vector delta, double tableWidth, double tableHeight, double radius)
        {
            // Calculate the new position
            double newX = Position.x + delta.x;
            double newY = Position.y + delta.y;
            double diameter = radius * 2;

            // Check horizontal bounds
            if (newX < 0 || newX + diameter > _tableWidth)
            {
                newX = Math.Clamp(newX, 0, _tableWidth - diameter);
                Velocity = new Vector(-Velocity.x, Velocity.y); // Reverse X velocity
            }

            // Check vertical bounds
            if (newY < 0 || newY + diameter > _tableHeight)
            {
                newY = Math.Clamp(newY, 0, _tableHeight - diameter);
                Velocity = new Vector(Velocity.x, -Velocity.y); // Reverse Y velocity
            }

            // Update position
            Position = new Vector(newX, newY);
            RaiseNewPositionChangeNotification();
        }

        #endregion private
    }
}