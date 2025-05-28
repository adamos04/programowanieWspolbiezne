//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region DataAbstractAPI
        public override void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            double minDimension = Math.Min(tableWidth, tableHeight);
            double radius = 0.04 * tableHeight;

            Random random = new Random();
            List<Ball> tempBalls = new List<Ball>();

            for (int i = 0; i < numberOfBalls; i++)
            {
                bool positionValid;
                Vector startingPosition;
                int maxAttempts = 100;
                int attempts = 0;
                do
                {
                    double x = radius + random.NextDouble() * (tableWidth - 2 * radius);
                    double y = radius + random.NextDouble() * (tableHeight - 2 * radius);
                    startingPosition = new Vector(x, y);

                    positionValid = true;
                    foreach (var existingBall in tempBalls)
                    {
                        double distance = Math.Sqrt(
                            Math.Pow(startingPosition.x - existingBall.Position.x, 2) +
                            Math.Pow(startingPosition.y - existingBall.Position.y, 2)
                        );
                        if (distance < 2 * radius)
                        {
                            positionValid = false;
                            break;
                        }
                    }

                    attempts++;
                    if (attempts >= maxAttempts)
                    {
                        throw new InvalidOperationException("Could not find a non-overlapping position for a ball after maximum attempts.");
                    }
                } while (!positionValid);

                Vector velocity = new Vector((random.NextDouble() - 0.5) * 5, (random.NextDouble() - 0.5) * 5);
                Ball newBall = new Ball(startingPosition, velocity);
                upperLayerHandler(startingPosition, newBall);
                if (newBall is Ball ballImplementation)
                {
                    ballImplementation.StartMoving();
                }
                tempBalls.Add(newBall);
            }

            lock (_lock)
            {
                BallsList.AddRange(tempBalls);
            }
        }
        #endregion

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    lock (_lock)
                    {
                        foreach (var ball in BallsList)
                        {
                            ball.Dispose();
                        }
                        BallsList.Clear();
                    }
                    DiagnosticLogger.Instance.Stop();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region private
        private bool Disposed = false;
        private readonly List<Ball> BallsList = [];
        private readonly object _lock = new();
        #endregion

        #region TestingInfrastructure
        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            lock (_lock)
            {
                returnBallsList(BallsList);
            }
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            lock (_lock)
            {
                returnNumberOfBalls(BallsList.Count);
            }
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }
        #endregion
    }
}