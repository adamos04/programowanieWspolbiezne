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
            //_cancellationTokenSource = new CancellationTokenSource();
            //MoveTask = Task.Run(() => MoveAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
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
            double radius = 0.04 * tableHeight;

            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                double x = radius + random.NextDouble() * (tableWidth - 2 * radius);
                double y = radius + random.NextDouble() * (tableHeight - 2 * radius);
                Vector startingPosition = new(x, y);
                Vector velocity = new Vector((random.NextDouble() - 0.5) * 5, (random.NextDouble() - 0.5) * 5);
                Ball newBall = new(startingPosition, velocity, tableWidth, tableHeight, radius);
                upperLayerHandler(startingPosition, newBall);
                if (newBall is Ball ballImplementation)
                {
                    ballImplementation.StartMoving();
                }
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
                    lock (_lock)
                    {
                        foreach (var ball in BallsList)
                        {
                            ball.Dispose();
                        }
                        BallsList.Clear();
                    }
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