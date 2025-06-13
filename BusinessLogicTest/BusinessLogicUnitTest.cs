//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TP.ConcurrentProgramming.Data;
using TP.ConcurrentProgramming.BusinessLogic;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BusinessLogicImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            using (var newInstance = new BusinessLogicImplementation(new DataLayerConstructorFixcure(), new LoggerLayerFixcure()))
            {
                bool newInstanceDisposed = true;
                newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
                Assert.IsFalse(newInstanceDisposed);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            var dataLayerFixcure = new DataLayerDisposeFixcure();
            var loggerLayerFixcure = new LoggerLayerFixcure();
            var newInstance = new BusinessLogicImplementation(dataLayerFixcure, loggerLayerFixcure);
            bool newInstanceDisposed = true;
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);
            Assert.IsFalse(dataLayerFixcure.Disposed);
            Assert.IsFalse(loggerLayerFixcure.Disposed);
            newInstance.Dispose();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);
            Assert.IsTrue(dataLayerFixcure.Disposed);
            Assert.IsTrue(loggerLayerFixcure.Disposed);
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, 200, 200, (position, ball) => { }));
        }

        [TestMethod]
        public void StartTestMethod()
        {
            var dataLayerFixcure = new DataLayerStartFixcure();
            var loggerLayerFixcure = new LoggerLayerFixcure();
            using (var newInstance = new BusinessLogicImplementation(dataLayerFixcure, loggerLayerFixcure))
            {
                int called = 0;
                int numberOfBalls2Create = 10;
                newInstance.Start(
                    numberOfBalls2Create,
                    400.0,
                    400.0,
                    (startingPosition, ball) =>
                    {
                        called++;
                        Assert.IsNotNull(startingPosition);
                        Assert.IsNotNull(ball);
                    });
                Assert.AreEqual(numberOfBalls2Create, called);
                Assert.IsTrue(dataLayerFixcure.StartCalled);
                Assert.AreEqual(numberOfBalls2Create, dataLayerFixcure.NumberOfBallsCreated);
                Assert.IsTrue(loggerLayerFixcure.GetLoggerCalled);
            }
        }

        #region Testing Instrumentation

        private class DataLayerConstructorFixcure : DataAbstractAPI
        {
            public override void Dispose()
            {
            }

            public override void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IVector, Data.IBall> upperLayerHandler, ILogger logger)
            {
                throw new NotImplementedException();
            }
        }

        private class DataLayerDisposeFixcure : DataAbstractAPI
        {
            internal bool Disposed = false;

            public override void Dispose()
            {
                Disposed = true;
            }

            public override void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IVector, Data.IBall> upperLayerHandler, ILogger logger)
            {
                throw new NotImplementedException();
            }
        }

        private class LoggerLayerFixcure : LoggerAbstractAPI
        {
            internal bool Disposed = false;
            internal bool GetLoggerCalled = false;

            public override ILogger GetLogger()
            {
                GetLoggerCalled = true;
                return new LoggerFix();
            }

            public override void Dispose()
            {
                Disposed = true;
            }

            private class LoggerFix : ILogger
            {
                public void Log(int ballId, IVector position, double velX, double velY, double mass)
                {
                }

                public void LogBallCollision(int ball1Id, IVector ball1Pos, double ball1VelX, double ball1VelY, double ball1Mass,
                                             int ball2Id, IVector ball2Pos, double ball2VelX, double ball2VelY, double ball2Mass)
                {
                }

                public void LogWallCollision(int ballId, IVector position, double velX, double velY, double mass)
                {
                }

                public void Dispose()
                {
                }
            }
        }

        private class DataLayerStartFixcure : DataAbstractAPI
        {
            internal bool StartCalled = false;
            internal int NumberOfBallsCreated = -1;

            public override void Dispose()
            {
            }

            public override void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IVector, Data.IBall> upperLayerHandler, ILogger logger)
            {
                StartCalled = true;
                NumberOfBallsCreated = numberOfBalls;

                // Symulacja tworzenia wielu piłek
                for (int i = 0; i < numberOfBalls; i++)
                {
                    upperLayerHandler(new DataVectorFixture { x = i * 10.0, y = i * 10.0 }, new DataBallFixture { Velocity = new DataVectorFixture() });
                }
            }
        }

        private record DataVectorFixture : IVector
        {
            public double x { get; init; }
            public double y { get; init; }
        }

        private class DataBallFixture : Data.IBall
        {
            public required IVector Velocity { get; set; } = new DataVectorFixture();
            public double Mass { get; } = 1.0;
            public IVector Position { get; } = new DataVectorFixture();

            public event EventHandler<IVector>? NewPositionNotification;

            public void UpdateVelocity(double x, double y)
            {
                Velocity = new DataVectorFixture { x = x, y = y };
            }

            public void SimulateNewPositionNotification(IVector newPosition)
            {
                NewPositionNotification?.Invoke(this, newPosition);
            }

            public void Dispose()
            {
            }
        }

        #endregion Testing Instrumentation
    }
}