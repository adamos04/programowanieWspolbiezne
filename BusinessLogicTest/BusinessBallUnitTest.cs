﻿//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
  [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void MoveTestMethod()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            List<Ball> otherBalls = new List<Ball>();
            object sharedLock = new object();
            Ball newInstance = new(dataBallFixture, otherBalls, sharedLock, 100.0, 50.0, 5.0);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) =>
            {
                Assert.IsNotNull(sender);
                Assert.IsNotNull(position);
                numberOfCallBackCalled++;
            };
            dataBallFixture.Move();
            Thread.Sleep(20);
            Assert.AreEqual<int>(1, numberOfCallBackCalled);
            newInstance.Dispose();
        }

        #region testing instrumentation

        private class DataBallFixture : Data.IBall
        {
            public Data.IVector Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public double Radius { get => throw new NotImplementedException(); }
            public double Mass { get => throw new NotImplementedException(); }
            public Data.IVector Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public double TableWidth { get => throw new NotImplementedException(); }
            public double TableHeight { get => throw new NotImplementedException(); }

            public event EventHandler<Data.IVector>? NewPositionNotification;
            public void Dispose()
            {
            }

            internal void Move()
            {
                NewPositionNotification?.Invoke(this, new VectorFixture(0.0, 0.0));
            }
        }

        private class VectorFixture : Data.IVector
        {
            internal VectorFixture(double X, double Y)
            {
                x = X; y = Y;
            }

            public double x { get; init; }
            public double y { get; init; }
        }

        #endregion testing instrumentation
    }
}