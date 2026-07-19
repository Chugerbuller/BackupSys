using Xunit;
using BackupApp.Services;
using BackupApp.Models;

namespace BackupApp.Tests;

public class BackupServiceIntegrationTests : IDisposable
{
    private readonly string _testRoot;
    private readonly string _sourceDir;
    private readonly string _targetDir;

    public BackupServiceIntegrationTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _sourceDir = Path.Combine(_testRoot, "Source");
        _targetDir = Path.Combine(_testRoot, "Target");

        Directory.CreateDirectory(_sourceDir);
        Directory.CreateDirectory(_targetDir);
    }

    [Fact]
    public async Task BackUp_ShouldCopyFilesSuccessfully()
    {
        string file1 = Path.Combine(_sourceDir, "text1.txt");
        string file2 = Path.Combine(_sourceDir, "text2.txt");
        await File.WriteAllTextAsync(file1, "Hello World"); // 11 байт
        await File.WriteAllTextAsync(file2, "Line 2");      // 6 байт

        var settings = new BackupSettings
        {
            SourceFolders = new List<string> { _sourceDir },
            TargetFolder = _targetDir
        };

        var service = new BackupService(settings);

        BackupResult result = await service.BackUp();

        Assert.Equal(2, result.Files);
        Assert.Equal(17, result.Memory);
        Assert.Empty(result.Errors);

        var createdBackupDir = Directory.GetDirectories(_targetDir).FirstOrDefault();
        Assert.NotNull(createdBackupDir);
        Assert.True(File.Exists(Path.Combine(createdBackupDir, "text1.txt")));
        Assert.True(File.Exists(Path.Combine(createdBackupDir, "text2.txt")));
    }

    [Fact]
    public async Task BackUp_ShouldHandleEmptyOrMissingSourceFolders()
    {
        var settings = new BackupSettings
        {
            SourceFolders = new List<string> { Path.Combine(_testRoot, "NonExistentFolder") },
            TargetFolder = _targetDir
        };
        var service = new BackupService(settings);

        BackupResult result = await service.BackUp();

        Assert.Equal(0, result.Files);
        Assert.Equal(0, result.Memory);
        Assert.Empty(result.Errors);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, true);
        }
    }
}
