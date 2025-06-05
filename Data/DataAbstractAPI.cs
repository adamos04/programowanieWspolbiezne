//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
    public abstract class DataAbstractAPI : IDisposable
    {
        public static DataAbstractAPI GetDataLayer()
        {
            return modelInstance.Value;
        }

        public static ILogger GetLogger()
        {
            return DiagnosticLogger.Instance;
        }

        public abstract void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IVector, IBall> upperLayerHandler);

        public abstract void Dispose();

        private static Lazy<DataAbstractAPI> modelInstance = new Lazy<DataAbstractAPI>(() => new DataImplementation());
    }

    public interface IVector
    {
        double x { get; init; }
        double y { get; init; }
    }

    public interface IBall :IDisposable
    {
        event EventHandler<IVector> NewPositionNotification;
        IVector Velocity { get; }
        double Mass { get; }
        IVector Position { get; }
        void UpdateVelocity(double x, double y);
    }
    public interface ILogger : IDisposable
    {
        public void Log(int messageType, int ball1Id, IVector ball1Pos, double ball1VelX, double ball1VelY, double ball1Mass, 
                int? ball2Id = null, IVector? ball2Pos = null, double? ball2VelX = null, double? ball2VelY = null, double? ball2Mass = null);
    }
}
