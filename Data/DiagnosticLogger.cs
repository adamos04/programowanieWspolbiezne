using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TP.ConcurrentProgramming.Data
{
    internal class DiagnosticLogger : ILogger, IDisposable
    {
        private static readonly DiagnosticLogger _instance = new DiagnosticLogger();
        public static DiagnosticLogger Instance => _instance;

        private readonly Thread _logThread;
        private volatile bool _isRunning = true;
        private readonly string _logFilePath;
        private readonly StreamWriter logWriter;
        private readonly object _fileLock = new object();
        private readonly DiagnosticBuffer _logBuffer;
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
            _logBuffer = new DiagnosticBuffer(1000);
            _logThread = new Thread(LogToFile);
            _logThread.Start();
        }

        public void Log(LogMessage message)
        {
            if (_isRunning && !_disposed)
            {
                message.Timestamp = DateTime.Now;
                if (!_logBuffer.TryAdd(message))
                {
                    System.Diagnostics.Debug.WriteLine("Bufor logów pełny, odrzucono wiadomość.");
                }
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
                    lock (_fileLock)
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
                }
                else
                {
                    Thread.Sleep(10);
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
                    _logThread.Join();
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

        public DiagnosticBuffer(int capacity)
        {
            _capacity = capacity;
            _buffer = new LogMessage?[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
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