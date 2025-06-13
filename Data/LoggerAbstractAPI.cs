using System;
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    public abstract class LoggerAbstractAPI : IDisposable
    {
        public static LoggerAbstractAPI GetLoggerLayer()
        {
            return loggerInstance.Value;
        }

        public abstract ILogger GetLogger();

        public abstract void Dispose();

        private static readonly Lazy<LoggerAbstractAPI> loggerInstance = new Lazy<LoggerAbstractAPI>(() => new LoggerImplementation());

    }

    public interface ILogger : IDisposable
    {
        public void Log(int messageType, int ball1Id, IVector ball1Pos, double ball1VelX, double ball1VelY, double ball1Mass,
                int? ball2Id = null, IVector? ball2Pos = null, double? ball2VelX = null, double? ball2VelY = null, double? ball2Mass = null);
    }

    internal class LoggerImplementation : LoggerAbstractAPI
    {
        private readonly ILogger _logger;
        private bool _disposed = false;

        public LoggerImplementation()
        {
            _logger = DiagnosticLogger.LoggerInstance;
        }

        public override ILogger GetLogger()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(LoggerImplementation));
            return _logger;
        }

        public override void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(LoggerImplementation));
            _logger.Dispose();
            _disposed = true;
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(_disposed);
        }
    }
}