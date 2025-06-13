using System;
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
        private readonly string _logFilePath;
        private readonly StreamWriter logWriter;
        private readonly DiagnosticBuffer _logBuffer;
        private readonly AutoResetEvent _bufferEvent = new AutoResetEvent(false);
        private bool _disposed = false;

        private DiagnosticLogger()
        {
            string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
            string logsDirectory = Path.Combine(projectDirectory, "Logs");
            Directory.CreateDirectory(logsDirectory);
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
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
        public void Log(int messageType, int ball1Id, IVector ball1Pos, double ball1VelX, double ball1VelY, double ball1Mass,
                int? ball2Id = null, IVector? ball2Pos = null, double? ball2VelX = null, double? ball2VelY = null, double? ball2Mass = null)
        {
            if (_isRunning && !_disposed)
            {
                if (!Enum.IsDefined(typeof(LogMessageType), messageType))
                {
                    System.Diagnostics.Debug.WriteLine($"Nieprawidłowy messageType: {messageType}");
                    return;
                }

                var logMessage = new LogMessage
                {
                    Timestamp = DateTime.Now,
                    MessageType = (LogMessageType)messageType,
                    Ball1 = new BallData
                    {
                        BallId = ball1Id,
                        PosX = Math.Round(ball1Pos.x, 2),
                        PosY = Math.Round(ball1Pos.y, 2),
                        VelX = Math.Round(ball1VelX, 2),
                        VelY = Math.Round(ball1VelY, 2),
                        Mass = Math.Round(ball1Mass, 2)
                    },
                    Ball2 = ball2Id.HasValue ? new BallData
                    {
                        BallId = ball2Id.Value,
                        PosX = Math.Round(ball2Pos?.x ?? 0, 2),
                        PosY = Math.Round(ball2Pos?.y ?? 0, 2),
                        VelX = Math.Round(ball2VelX ?? 0, 2),
                        VelY = Math.Round(ball2VelY ?? 0, 2),
                        Mass = Math.Round(ball2Mass ?? 0, 2)
                    } : null
                };
                _logBuffer.TryAdd(logMessage);
            }
        }

        private void LogToFile()
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() }
            };

            while (_isRunning)
            {
                if (_logBuffer.TryTake(out var message))
                {
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
        private readonly LogMessage?[] _buffer;
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
            _buffer = new LogMessage?[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
            _bufferEvent = bufferEvent;
        }

        public bool TryAdd(LogMessage item)
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

        public bool TryTake(out LogMessage? item)
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

    public class LogMessage
    {
        public DateTime Timestamp { get; set; }
        public LogMessageType MessageType { get; set; }
        public BallData? Ball1 { get; set; }
        public BallData? Ball2 { get; set; }
    }

    public class BallData
    {
        public int BallId { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double Mass { get; set; }
        public double? DeltaTime { get; set; }
    }

    public enum LogMessageType
    {
        BallMovement,
        BallToBallCollision,
        WallCollisionTop,
        WallCollisionBottom,
        WallCollisionLeft,
        WallCollisionRight
    }
}