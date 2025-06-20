﻿//____________________________________________________________________________________________________________________________________
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
    public class BallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            LoggerFix logger = new LoggerFix();
            Vector testinVector = new(0.0, 0.0);
            Ball newInstance = new(testinVector, testinVector, logger);
        }

        [TestMethod]
        public void MoveTestMethod()
        {
            LoggerFix logger = new LoggerFix();
            Vector initialPosition = new(10.0, 10.0);
            Ball newInstance = new(initialPosition, new Vector(0.0, 0.0), logger);
            IVector curentPosition = new Vector(0.0, 0.0);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) =>
            {
                Assert.IsNotNull(sender);
                curentPosition = position;
                numberOfCallBackCalled++;
            };
            newInstance.StartMoving();
            Thread.Sleep(50);
            newInstance.Dispose();
        }

        private class LoggerFix : ILogger
        {
            public void Log(DateTime timestamp, int ballId, IVector position, double velX, double velY, double mass)
            {
            }

            public void LogBallCollision(DateTime timestamp, int ball1Id, IVector ball1Pos, double ball1VelX, double ball1VelY, double ball1Mass,
                                         int ball2Id, IVector ball2Pos, double ball2VelX, double ball2VelY, double ball2Mass)
            {
            }

            public void LogWallCollision(DateTime timestamp, int ballId, IVector position, double velX, double velY, double mass)
            {
            }

            public void Dispose()
            {
            }
        }
    }
}