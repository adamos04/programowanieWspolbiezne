using System;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TP.ConcurrentProgramming.Data
{
    internal class DiagnosticLogger : ILogger, IDisposable
    {
        private static readonly Lazy<DiagnosticLogger> singletonInstance = new Lazy<DiagnosticLogger>(() => new DiagnosticLogger());
        private readonly Thread _logThread;
        private volatile bool _isRunning = true;
        private string _logFilePath;
        private StreamWriter logWriter;
        private readonly DiagnosticBuffer _logBuffer;
        private readonly AutoResetEvent _bufferEvent = new AutoResetEvent(false);
        private bool _disposed = false;

        private DiagnosticLogger()
        {
            _logBuffer = new DiagnosticBuffer(1000, _bufferEvent);
            _logThread = new Thread(LogToFile);
            _logThread.Start();
        }

        internal static DiagnosticLogger LoggerInstance
        {
            get
            {
                return singletonInstance.Value;
            }
        }

        private void InitializeFile()
        {
            if (logWriter != null)
                return;

            string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
            string logsDirectory = Path.Combine(projectDirectory, "Logs");
            Directory.CreateDirectory(logsDirectory);
            string dateTime = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm");
            _logFilePath = Path.Combine(logsDirectory, $"diagnostics_{dateTime}.json");

            if (File.Exists(_logFilePath))
            {
                try
                {
                    File.Delete(_logFilePath);
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Nie udało się usunąć pliku logu: {ex.Message}");
                }
            }

            logWriter = new StreamWriter(_logFilePath, append: true, Encoding.UTF8) { AutoFlush = true };
        }
        public void Log(DateTime timestamp, int ballId, IVector position, double velX, double velY, double mass)
        {
            if (_isRunning && !_disposed)
            {
                _logBuffer.TryAdd(new LogEntry(
                                    timestamp: timestamp,
                                    ballId: ballId,
                                    posX: Math.Round(position.x, 2),
                                    posY: Math.Round(position.y, 2),
                                    velX: Math.Round(velX, 2),
                                    velY: Math.Round(velY, 2),
                                    mass: Math.Round(mass, 2)));
            }
        }

        public void LogBallCollision(DateTime timestamp, int ball1Id, IVector ball1Pos, double ball1VelX, double ball1VelY, double ball1Mass,
                                     int ball2Id, IVector ball2Pos, double ball2VelX, double ball2VelY, double ball2Mass)
        {
            if (_isRunning && !_disposed)
            {
                _logBuffer.TryAdd(new BallCollisionLogEntry(
                                    timestamp: timestamp,
                                    ball1Id: ball1Id,
                                    posX: Math.Round(ball1Pos.x, 2),
                                    posY: Math.Round(ball1Pos.y, 2),
                                    velX: Math.Round(ball1VelX, 2),
                                    velY: Math.Round(ball1VelY, 2),
                                    mass: Math.Round(ball1Mass, 2),
                                    ball2Id: ball2Id,
                                    ball2PosX: Math.Round(ball2Pos.x, 2),
                                    ball2PosY: Math.Round(ball2Pos.y, 2),
                                    ball2VelX: Math.Round(ball2VelX, 2),
                                    ball2VelY: Math.Round(ball2VelY, 2),
                                    ball2Mass: Math.Round(ball2Mass, 2)));
            }
        }

        public void LogWallCollision(DateTime timestamp, int ballId, IVector position, double velX, double velY, double mass)
        {
            if (_isRunning && !_disposed)
            {
                _logBuffer.TryAdd(new WallCollisionEntry(
                    timestamp: timestamp,
                    ballId: ballId,
                    posX: Math.Round(position.x, 2),
                    posY: Math.Round(position.y, 2),
                    velX: Math.Round(velX, 2),
                    velY: Math.Round(velY, 2),
                    mass: Math.Round(mass, 2)));
            }
        }

        private void LogToFile()
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                Converters = { new JsonStringEnumConverter() }
            };

            while (_isRunning)
            {
                if (_logBuffer.TryTake(out var message))
                {
                    InitializeFile();
                    try
                    {
                        string jsonMessage = JsonSerializer.Serialize(message, options);
                        logWriter.WriteLine(jsonMessage);
                    }
                    catch (IOException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Błąd zapisu do logu: {ex.Message}");
                    }
                }
                else
                {
                    _bufferEvent.WaitOne();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _isRunning = false;
                    _bufferEvent.Set();
                    while (_logBuffer.Count > 0)
                    {
                        _bufferEvent.WaitOne(100);
                    }
                    try
                    {
                        logWriter?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Błąd podczas zamykania pliku logu: {ex.Message}");
                    }
                }
                _disposed = true;
            }
        }
    }

    internal class DiagnosticBuffer
    {
        private readonly object?[] _buffer;
        private int _head;
        private int _tail;
        private int _count;
        private readonly int _capacity;
        private readonly object _bufferLock = new object();

        private readonly AutoResetEvent _bufferEvent;
        internal int Count => _count;

        public DiagnosticBuffer(int capacity, AutoResetEvent bufferEvent)
        {
            _capacity = capacity;
            _buffer = new object?[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
            _bufferEvent = bufferEvent;
        }

        public bool TryAdd(object item)
        {
            lock (_bufferLock)
            {
                if (_count == _capacity)
                {
                    return false;
                }

                _buffer[_tail] = item;
                _tail = (_tail + 1) % _capacity;
                _count++;
                _bufferEvent.Set();
                return true;
            }
        }

        public bool TryTake(out object? item)
        {
            lock (_bufferLock)
            {
                if (_count == 0)
                {
                    item = null;
                    return false;
                }

                item = _buffer[_head];
                _buffer[_head] = null;
                _head = (_head + 1) % _capacity;
                _count--;
                if (_count == 0)
                {
                    _bufferEvent.Set();
                }
                return true;
            }
        }
    }

    internal enum LogType
    {
        BallMovement,
        BallToBallCollision,
        WallCollision
    }

    internal interface ILogEntry
    {
        DateTime Timestamp { get; init; }
        LogType Type { get; init; }
        int BallId { get; init; }
        public double PosX { get; init; }
        public double PosY { get; init; }
        double VelX { get; init; }
        double VelY { get; init; }
        double Mass { get; init; }
    }

    internal class LogEntry : ILogEntry
    {
        internal LogEntry(DateTime timestamp, int ballId, double posX, double posY, double velX, double velY, double mass)
        {
            Timestamp = timestamp;
            Type = LogType.BallMovement;
            BallId = ballId;
            PosX = posX;
            PosY = posY;
            VelX = velX;
            VelY = velY;
            Mass = mass;
        }

        public DateTime Timestamp { get; init; }
        public LogType Type { get; init; }
        public int BallId { get; init; }
        public double PosX { get; init; }
        public double PosY { get; init; }
        public double VelX { get; init; }
        public double VelY { get; init; }
        public double Mass { get; init; }
    }

    internal class BallCollisionLogEntry : ILogEntry
    {
        internal BallCollisionLogEntry(DateTime timestamp,
            int ball1Id, double posX, double posY, double velX, double velY, double mass,
            int ball2Id, double ball2PosX, double ball2PosY, double ball2VelX, double ball2VelY, double ball2Mass)
        {
            Timestamp = timestamp;
            Type = LogType.BallToBallCollision;
            BallId = ball1Id;
            PosX = posX;
            PosY = posY;
            VelX = velX;
            VelY = velY;
            Mass = mass;
            Ball2Id = ball2Id;
            Ball2PosX = ball2PosX;
            Ball2PosY = ball2PosY;
            Ball2VelX = ball2VelX;
            Ball2VelY = ball2VelY;
            Ball2Mass = ball2Mass;
        }

        public DateTime Timestamp { get; init; }
        public LogType Type { get; init; }
        public int BallId { get; init; }
        public double PosX { get; init; }
        public double PosY { get; init; }
        public double VelX { get; init; }
        public double VelY { get; init; }
        public double Mass { get; init; }
        public int Ball2Id { get; init; }
        public double Ball2PosX { get; init; }
        public double Ball2PosY { get; init; }
        public double Ball2VelX { get; init; }
        public double Ball2VelY { get; init; }
        public double Ball2Mass { get; init; }
    }

    internal class WallCollisionEntry : ILogEntry
    {
        internal WallCollisionEntry(DateTime timestamp, int ballId, double posX, double posY, double velX, double velY, double mass)
        {
            Timestamp = timestamp;
            Type = LogType.WallCollision;
            BallId = ballId;
            PosX = posX;
            PosY = posY;
            VelX = velX;
            VelY = velY;
            Mass = mass;
        }

        public DateTime Timestamp { get; init; }
        public LogType Type { get; init; }
        public int BallId { get; init; }
        public double PosX { get; init; }
        public double PosY { get; init; }
        public double VelX { get; init; }
        public double VelY { get; init; }
        public double Mass { get; init; }
    }
}