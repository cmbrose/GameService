using System.IO;

namespace GameApplication.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _logPath;
        public FileLogger(string logPath)
            : this(logPath, true)
        {
        }

        public FileLogger(string logPath, bool resetLog)
        {
            _logPath = logPath;

            if (resetLog && File.Exists(_logPath))
            {
                File.Delete(_logPath);
            }
        }

        public void Info(string msg)
        {
            if (!File.Exists(_logPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_logPath));                
            }

            File.AppendAllText(_logPath, msg + "\n");
        }
    }
}