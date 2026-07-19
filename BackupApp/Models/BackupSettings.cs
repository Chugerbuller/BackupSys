namespace BackupApp.Models
{

    public record BackupSettings
    {
        public List<string> SourceFolders { get; init; } = new();
        public string TargetFolder { get; init; } = string.Empty;
        public string LogLevel { get; init; } = "Info";
        public string LogDirectory { get; init; } = "logs";
        public BackupSettings() { }
    }
}
