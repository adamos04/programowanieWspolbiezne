using System;
using System.Text;
using System.Text.Json;

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
            while (_isRunning)
            {
                if (_logBuffer.TryTake(out var message))
                {
                    lock (_fileLock)
                    {
                        try
                        {
                            string jsonMessage = message switch
                            {
                                BallPositionLog ballPosition => JsonSerializer.Serialize(ballPosition),
                                BallCollisionLog ballCollision => JsonSerializer.Serialize(ballCollision),
                                WallCollisionLog wallCollision => JsonSerializer.Serialize(wallCollision),
                                _ => throw new InvalidOperationException($"Unknown LogMessage type: {message?.GetType().Name}")
                            };
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

    public abstract class LogMessage
    {
        public DateTime Timestamp { get; set; }
        public abstract string MessageType { get; }
    }

    public class BallPositionLog : LogMessage
    {
        public int BallId { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double Mass { get; set; }
        public double DeltaTime { get; set; }
        public override string MessageType => "BallPosition";
    }

    public class BallCollisionLog : LogMessage
    {
        public int Ball1Id { get; set; }
        public double Ball1PosX { get; set; }
        public double Ball1PosY { get; set; }
        public double Ball1VelX { get; set; }
        public double Ball1VelY { get; set; }
        public double Ball1Mass { get; set; }
        public int Ball2Id { get; set; }
        public double Ball2PosX { get; set; }
        public double Ball2PosY { get; set; }
        public double Ball2VelX { get; set; }
        public double Ball2VelY { get; set; }
        public double Ball2Mass { get; set; }
        public override string MessageType => "BallCollision";
    }

    public class WallCollisionLog : LogMessage
    {
        public int BallId { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double Mass { get; set; }
        public string Wall { get; set; } = string.Empty;
        public override string MessageType => "WallCollision";
    }
}