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
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        #region ctor
        public BusinessLogicImplementation() : this(null)
        {
        }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer ?? UnderneathLayerAPI.GetDataLayer();
            //_cancellationTokenSource = new CancellationTokenSource();
            //CollisionTask = Task.Run(() => DetectCollisionsAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }
        #endregion

        #region BusinessLogicAbstractAPI
        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            Ball[] ballsToDispose;
            lock (_lock)
            {
                ballsToDispose = BallsList.ToArray();
                BallsList.Clear();
            }
            foreach (var ball in ballsToDispose)
            {
                ball.Dispose();
            }
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            layerBellow.Start(numberOfBalls, tableWidth, tableHeight, (startingPosition, databall) =>
            {
                lock (_lock)
                {
                    Ball logicBall = new Ball(databall, BallsList, _lock);
                    upperLayerHandler(new Position(startingPosition.x, startingPosition.y), logicBall);
                    BallsList.Add(logicBall);
                }
            });
        }
        #endregion

        #region private
        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;
        private readonly List<Ball> BallsList = [];
        private readonly object _lock = new();

        #endregion

        #region TestingInfrastructure
        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }
        #endregion
    }
}