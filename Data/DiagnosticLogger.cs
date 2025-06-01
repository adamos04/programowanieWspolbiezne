using System.Collections.Concurrent;
using System.IO;

namespace TP.ConcurrentProgramming.Data
{
    internal class DiagnosticLogger : ILogger, IDisposable
    {
        private static readonly DiagnosticLogger _instance = new DiagnosticLogger();
        public static DiagnosticLogger Instance => _instance;

        private readonly Thread _logThread;
        private volatile bool _isRunning = true;
        private readonly string _logFilePath;
        private readonly object _fileLock = new object();
        private readonly DiagnosticBuffer _logBuffer;
        private bool _disposed = false;

        private DiagnosticLogger()
        {
            string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string logsDirectory = Path.Combine(projectDirectory, "Logs");
            Directory.CreateDirectory(logsDirectory);
            _logFilePath = Path.Combine(logsDirectory, "diagnostics.log");

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

            _logBuffer = new DiagnosticBuffer(1000);
            _logThread = new Thread(LogToFile) { IsBackground = true };
            _logThread.Start();
        }

        public void Log(string message)
        {
            if (!_logBuffer.TryAdd($"{DateTime.Now:O}: {message}"))
            {
                System.Diagnostics.Debug.WriteLine("Bufor logów pełny, odrzucono wiadomość.");
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
                            File.AppendAllText(_logFilePath, message + Environment.NewLine);
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
                }
                _disposed = true;
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _logThread.Join();
        }
    }


    internal class DiagnosticBuffer
    {
        private readonly string?[] _buffer;
        private int _head;
        private int _tail;
        private int _count;
        private readonly int _capacity;
        private readonly object _bufferLock = new object();

        public DiagnosticBuffer(int capacity)
        {
            _capacity = capacity;
            _buffer = new string?[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        public bool TryAdd(string item)
        {
            lock (_bufferLock)
            {
                if (_count == _capacity)
                {
                    return false; // Bufor pełny
                }

                _buffer[_tail] = item;
                _tail = (_tail + 1) % _capacity;
                _count++;
                return true;
            }
        }

        public bool TryTake(out string? item)
        {
            lock (_bufferLock)
            {
                if (_count == 0)
                {
                    item = null;
                    return false; // Bufor pusty
                }

                item = _buffer[_head];
                _buffer[_head] = null; // Czyszczenie slotu
                _head = (_head + 1) % _capacity;
                _count--;
                return true;
            }
        }
    }
}
