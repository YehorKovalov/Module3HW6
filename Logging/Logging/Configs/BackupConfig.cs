namespace Logging.Configs
{
    public class BackupConfig
    {
        public int ReportsStep { get; init; }
        public string DirectoryPath { get; init; }
        public FileInfoConfig FileInfoConfig { get; init; }
    }
}
