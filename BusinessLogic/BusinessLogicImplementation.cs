//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
            CollisionTask = Task.Run(DetectCollisionsAsync);
        }
        #endregion

        #region BusinessLogicAbstractAPI
        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            CollisionTask?.Wait();
            layerBellow.Dispose();
            BallsList.Clear();
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
                Ball logicBall = new Ball(databall);
                upperLayerHandler(new Position(startingPosition.x, startingPosition.y), logicBall);
                lock (_lock)
                {
                    BallsList.Add(logicBall);
                }
            });
        }
        #endregion

        #region private
        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;
        private readonly Task? CollisionTask;
        private readonly List<Ball> BallsList = [];
        private readonly object _lock = new();

        private async Task DetectCollisionsAsync()
        {
            while (!Disposed)
            {
                lock (_lock)
                {
                    var ballsCopy = BallsList.ToList();
                    for (int i = 0; i < ballsCopy.Count; i++)
                    {
                        for (int j = i + 1; j < ballsCopy.Count; j++)
                        {
                            Ball ball1 = ballsCopy[i];
                            Ball ball2 = ballsCopy[j];
                            double dx = ball1.DataBall.Position.x - ball2.DataBall.Position.x;
                            double dy = ball1.DataBall.Position.y - ball2.DataBall.Position.y;
                            double distance = Math.Sqrt(dx * dx + dy * dy);
                            if (distance < ball1.Radius + ball2.Radius)
                            {
                                ball1.CollideWith(ball2);
                            }
                        }
                    }
                }
                await Task.Delay(16); // ~60 FPS
            }
        }
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