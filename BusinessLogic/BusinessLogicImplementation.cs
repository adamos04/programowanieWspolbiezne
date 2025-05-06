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
            _cancellationTokenSource = new CancellationTokenSource();
            CollisionTask = Task.Run(() => DetectCollisionsAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }
        #endregion

        #region BusinessLogicAbstractAPI
        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            _cancellationTokenSource.Cancel();
            layerBellow.Dispose();
            BallsList.Clear();
            _cancellationTokenSource.Dispose();
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
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<Ball> BallsList = [];
        private readonly object _lock = new();

        private async Task DetectCollisionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                lock (_lock)
                {
                    var ballsCopy = BallsList.ToList();
                    bool collisionDetected;
                    do
                    {
                        collisionDetected = false;
                        // Kolizje między kulkami
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
                                    collisionDetected = true;
                                }
                            }
                        }
                        // Kolizje ze ściankami
                        foreach (var ball in ballsCopy)
                        {
                            ball.CheckWallCollisions();
                        }
                    } while (collisionDetected); // Powtarzaj, aż nie będzie kolizji
                }
                if (cancellationToken.IsCancellationRequested)
                    break;
                await Task.Delay(10);
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