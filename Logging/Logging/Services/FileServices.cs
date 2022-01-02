using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Logging.Configs;
using Logging.Services.Abstractions;

namespace Logging.Services
{
    public class FileServices : IFileServices
    {
        public IAsyncDisposable GetStreamWriterInstance(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
            {
                var newFile = fileInfo.Create();
                newFile.Close();
            }

            return new StreamWriter(filePath, true);
        }

        public async Task<bool> WriteLine(IAsyncDisposable streamWriter, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var writer = streamWriter as StreamWriter;
            if (writer == null)
            {
                return false;
            }

            await writer.WriteLineAsync(text);
            await writer.FlushAsync();
            return true;
        }

        public async Task<bool> WriteAllText(string path, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            await File.WriteAllTextAsync(path, text);
            return true;
        }

        public async Task<string> ReadAllTextOrNull(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (!File.Exists(path))
            {
                return null;
            }

            return await File.ReadAllTextAsync(path);
        }

        public async Task ConfigureLoggerDirectory(LoggerConfig loggerConfig)
        {
            var dirPath = loggerConfig.DirectoryPath;
            await ConfigureDirectory(dirPath);
        }

        public async Task ConfigureBackupDirectory(BackupConfig backupConfig)
        {
            var dirPath = backupConfig.DirectoryPath;
            await ConfigureDirectory(dirPath);
        }

        public async Task DisposeAsync(IAsyncDisposable streamWriter)
        {
            var stream = streamWriter as StreamWriter;
            if (stream != null)
            {
                await stream.DisposeAsync();
            }
        }

        private async Task ConfigureDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            else
            {
                await ClearDirectory(dirPath);
            }
        }

        private async Task ClearDirectory(string dirPath)
        {
            await Task.Run(async () =>
            {
                await DeleteAllFiles(dirPath);
                var subDirs = Directory.GetDirectories(dirPath);
                DirectoryInfo subDirInfo;
                foreach (var dir in subDirs)
                {
                    subDirInfo = new DirectoryInfo(dir);
                    var allFilesInSubDir = subDirInfo.GetFiles();
                    await DeleteAllFiles(subDirInfo.FullName);
                    subDirInfo.Delete();
                }
            });
        }

        private async Task DeleteAllFiles(string dirPath)
        {
            await Task.Run(() =>
            {
                var files = Directory.GetFiles(dirPath);
                FileInfo fileInfo;
                foreach (var file in files)
                {
                    fileInfo = new FileInfo(file);
                    fileInfo.Delete();
                }
            });
        }
    }
}