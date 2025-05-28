using System.Collections.Concurrent;
using System.IO;

namespace TP.ConcurrentProgramming.Data
{
    internal class DiagnosticLogger
    {
        private static readonly DiagnosticLogger _instance = new DiagnosticLogger();
        public static DiagnosticLogger Instance => _instance;

        private readonly ConcurrentQueue<string> _logBuffer = new ConcurrentQueue<string>();
        private readonly Thread _logThread;
        private volatile bool _isRunning = true;
        private readonly string _logFilePath;
        private readonly object _fileLock = new object();

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

            _logThread = new Thread(LogToFile) { IsBackground = true };
            _logThread.Start();
        }

        public void Log(string message)
        {
            if (_logBuffer.Count < 1000)
                _logBuffer.Enqueue($"{DateTime.Now:O}: {message}");
        }

        private void LogToFile()
        {
            while (_isRunning)
            {
                if (_logBuffer.TryDequeue(out var message))
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

        public void Stop()
        {
            _isRunning = false;
            _logThread.Join();
        }
    }
}