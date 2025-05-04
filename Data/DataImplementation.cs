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
        #region ctor
        public DataImplementation()
        {
            MoveTask = Task.Run(MoveAsync);
        }
        #endregion

        #region DataAbstractAPI
        public override void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            double minDimension = Math.Min(tableWidth, tableHeight);
            double radius = Math.Clamp(minDimension / 20.0, 2.0, 20.0);

            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                double x = radius + random.NextDouble() * (tableWidth - 2 * radius);
                double y = radius + random.NextDouble() * (tableHeight - 2 * radius);
                Vector startingPosition = new(x, y);
                Vector velocity = new Vector((random.NextDouble() - 0.5) * 10, (random.NextDouble() - 0.5) * 10);
                Ball newBall = new(startingPosition, velocity, tableWidth, tableHeight, radius);
                upperLayerHandler(startingPosition, newBall);
                lock (_lock)
                {
                    BallsList.Add(newBall);
                }
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
                    MoveTask?.Wait();
                    BallsList.Clear();
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
        private readonly Task? MoveTask;
        private readonly List<Ball> BallsList = [];
        private readonly object _lock = new();

        private async Task MoveAsync()
        {
            while (!Disposed)
            {
                lock (_lock)
                {
                    var ballsCopy = BallsList.ToList();

                    foreach (Ball ball in ballsCopy)
                    {
                        ball.Move((Vector)ball.Velocity);
                    }
                }
                await Task.Delay(16); // ~60 FPS
            }
        }
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