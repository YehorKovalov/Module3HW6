using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Logging.Configs;
using Logging.Services.Abstractions;

namespace Logging.Services
{
    public class BackupServices : IBackupServices
    {
        private readonly IFileServices _fileServices;
        private readonly BackupConfig _backupConfig;
        private readonly StringBuilder _reports;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private int _reportsStepForBackup;
        private int _currentReportsAmount;
        private string _previousBackupFilePath;
        private int _tempCounterForCollisions;
        public BackupServices(
            IFileServices fileServices,
            IConfigServices configServices)
        {
            _reports = new StringBuilder();
            _fileServices = fileServices;
            _backupConfig = configServices.BackupConfig;
            Init().GetAwaiter().GetResult();
        }

        public async Task Create(string report)
        {
            await _semaphore.WaitAsync();
            AddNewReport(report);
            if (BackupIsRequired())
            {
                var filePath = GetNewFilePath();
                _reports.AppendLine(string.Empty);
                await _fileServices.WriteAllText(filePath, _reports.ToString());
            }

            _semaphore.Release();
        }

        private bool BackupIsRequired() => _currentReportsAmount % _reportsStepForBackup == 0;

        private void AddNewReport(string report)
        {
            _currentReportsAmount++;
            _reports.AppendLine(report);
        }

        private string GetNewFilePath()
        {
            var fileInfoConfig = _backupConfig.FileInfoConfig;
            var dirPath = _backupConfig.DirectoryPath;
            var extension = fileInfoConfig.Extension;
            var nameFormat = fileInfoConfig.NameFormat;
            var fileTitle = DateTime.UtcNow.ToString(nameFormat);
            return CheckForCollisionAndGetFilePath(dirPath, fileTitle, extension);
        }

        private string CheckForCollisionAndGetFilePath(string dirPath, string fileTitle, string extension)
        {
            var filePath = $"{dirPath}/{fileTitle}{extension}";
            var collisionIsHappen = string.Compare(filePath, _previousBackupFilePath) == 0;
            _previousBackupFilePath = filePath;
            if (collisionIsHappen)
            {
                filePath = $"{dirPath}/{fileTitle}({_tempCounterForCollisions++}){extension}";
            }
            else
            {
                _tempCounterForCollisions = 0;
            }

            return filePath;
        }

        private async Task Init()
        {
            await _fileServices.ConfigureBackupDirectory(_backupConfig);
            _reportsStepForBackup = _backupConfig.ReportsStep;
        }
    }
}
