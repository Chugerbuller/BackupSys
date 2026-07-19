using BackupApp.Models;

namespace BackupApp.Services;

public interface IConfigurationService
{
    BackupSettings GetSettings(string path);
}
