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
using TP.ConcurrentProgramming.Data;

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
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
            logger = layerBellow.GetLogger();
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
            logger.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            double radius = 0.04 * tableHeight;
            layerBellow.Start(numberOfBalls, tableWidth, tableHeight, (startingPosition, databall) =>
            {
                lock (_lock)
                {
                    Ball logicBall = new Ball(databall, BallsList, _lock, tableWidth, tableHeight, radius, logger);
                    upperLayerHandler(new Position(startingPosition.x, startingPosition.y), logicBall);
                    BallsList.Add(logicBall);
                }
            });
        }
        #endregion

        #region private
        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;
        private ILogger logger;
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