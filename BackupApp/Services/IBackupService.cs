using BackupApp.Models;

namespace BackupApp.Services;

public interface IBackupService
{
    Task<BackupResult> BackUp();
}
