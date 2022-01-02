using Logging.Configs;

namespace Logging.Services.Abstractions
{
    public interface IConfigServices
{
        LoggerConfig LoggerConfig { get; }
        BackupConfig BackupConfig { get; }
    }
}
