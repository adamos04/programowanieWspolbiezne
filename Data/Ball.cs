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
        internal void Move(Vector delta, double radius)
        {
            // Calculate new center position
            double newX = Position.x + delta.x;
            double newY = Position.y + delta.y;

            // Check horizontal bounds (left and right)
            if (newX < 0)
            {
                newX = 0;
                Velocity = new Vector(-Velocity.x, Velocity.y);
            }
            else if (newX + radius > _tableWidth)
            {
                newX = _tableWidth - radius;
                Velocity = new Vector(-Velocity.x, Velocity.y);
            }

            // Check vertical bounds (top and bottom)
            if (newY < 0)
            {
                newY = 0;
                Velocity = new Vector(Velocity.x, -Velocity.y);
            }
            else if (newY + radius > _tableHeight)
            {
                newY = _tableHeight - radius;
                Velocity = new Vector(Velocity.x, -Velocity.y);
            }

            // Update position (center)
            Position = new Vector(newX, newY);
            RaiseNewPositionChangeNotification();
        }

        #endregion private
    }
}