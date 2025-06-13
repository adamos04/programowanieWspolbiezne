//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class DataImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            using DataImplementation newInstance = new();
            IEnumerable<IBall>? ballsList = null;
            newInstance.CheckBallsList(x => ballsList = x);
            Assert.IsNotNull(ballsList);
            int numberOfBalls = 0;
            newInstance.CheckNumberOfBalls(x => numberOfBalls = x);
            Assert.AreEqual<int>(0, numberOfBalls);
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataImplementation newInstance = new();
            bool newInstanceDisposed = false;
            LoggerFix logger = new LoggerFix();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);
            newInstance.Dispose();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);
            IEnumerable<IBall>? ballsList = null;
            newInstance.CheckBallsList(x => ballsList = x);
            Assert.IsNotNull(ballsList);
            newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(0, x));
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, 200, 200, (position, ball) => { }, logger));
        }

        [TestMethod]
        public void StartTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                int numberOfCallbackInvoked = 0;
                int numberOfBalls2Create = 10;
                LoggerFix logger = new LoggerFix();
                newInstance.Start(
                  numberOfBalls2Create,
                    400.0,
                    400.0,
                  (startingPosition, ball) =>
                  {
                      numberOfCallbackInvoked++;
                      Assert.IsTrue(startingPosition.x >= 0);
                      Assert.IsTrue(startingPosition.y >= 0);
                      Assert.IsNotNull(ball);
                  }, logger);
                Assert.AreEqual<int>(numberOfBalls2Create, numberOfCallbackInvoked);
                newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(10, x));
            }
        }

        private class LoggerFix : ILogger
        {
            public void Log(int messageType, int ball1Id, IVector ball1Pos, double ball1VelX, double ball1VelY, double ball1Mass,
                int? ball2Id = null, IVector? ball2Pos = null, double? ball2VelX = null, double? ball2VelY = null, double? ball2Mass = null)
            {

            }
            public void Dispose()
            {

            }
        }
    }
}