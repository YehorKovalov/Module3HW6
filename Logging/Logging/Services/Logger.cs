using System;
using System.Threading;
using System.Threading.Tasks;
using Logging.Configs;
using Logging.Enums;
using Logging.Services.Abstractions;

namespace Logging.Services
{
    public class Logger : ILogger, IAsyncDisposable
    {
        private readonly IFileServices _fileServices;
        private readonly LoggerConfig _loggerConfig;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private IAsyncDisposable _streamWriter;

        public Logger(
            IFileServices fileServices,
            IConfigServices configServices)
        {
            _fileServices = fileServices;
            _loggerConfig = configServices.LoggerConfig;
            Init().GetAwaiter().GetResult();
        }

        public event Func<string, Task> OnBackedUp;
        public async Task LogInfo<T>(T message) => await Log(LogType.Info, message?.ToString());
        public async Task LogError<T>(T message) => await Log(LogType.Error, message?.ToString());
        public async ValueTask DisposeAsync() => await _fileServices.DisposeAsync(_streamWriter);

        private async Task Init()
        {
            await _fileServices.ConfigureLoggerDirectory(_loggerConfig);
            var loggerFilePath = GetFilePath();
            _streamWriter = _fileServices.GetStreamWriterInstance(loggerFilePath);
        }

        private async Task Log(LogType logType, string message)
        {
            await _semaphore.WaitAsync();
            var report = FormReport(logType, message);
            await _fileServices.WriteLine(_streamWriter, report);
            await OnBackedUp?.Invoke(report);
            _semaphore.Release();
        }

        private string FormReport(LogType logType, string message)
        {
            var utcNow = DateTime.UtcNow
                .ToString(_loggerConfig.TimeFormat);
            var report = $"{utcNow} {logType}: {message}";
            return report;
        }

        private string GetFilePath()
        {
            var fileInfoConfig = _loggerConfig.FileInfoConfig;
            var fileTitle = DateTime.UtcNow.ToString(fileInfoConfig.NameFormat);
            var extension = fileInfoConfig.Extension;
            var dirPath = _loggerConfig.DirectoryPath;
            return $"{dirPath}/{fileTitle}{extension}";
        }
    }
}
