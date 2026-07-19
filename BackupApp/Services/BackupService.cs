using BackupApp.Models;
using Serilog;
using System.Collections.Concurrent;

namespace BackupApp.Services;

public class BackupService : IBackupService
{
    private BackupSettings _settings;
    public BackupService(BackupSettings settings)
    {
        _settings = settings;
    }
    public async Task<BackupResult> BackUp()
    {
        long totalFiles = 0;
        long totalBytes = 0;
        var concurrentErrors = new ConcurrentDictionary<string, string>();

        string timestamp = $"{DateTime.Now:yyyyMMdd_HHmmss}";
        string currentBackupTarget = Path.Combine(_settings.TargetFolder, $"backup_{timestamp}");

        try
        {
            Directory.CreateDirectory(currentBackupTarget);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Can`t create backup folder");
            throw;
        }
        var allFiles = _settings.SourceFolders
            .Where(Directory.Exists)
            .SelectMany(folder => Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories))
            .ToList();

        Log.Information("Files for copy: {Count}", allFiles.Count);
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 4
        };

        await Parallel.ForEachAsync(allFiles, parallelOptions, async (filePath, cancellationToken) =>
        {
            try
            {
                var fileInfo = new FileInfo(filePath);

                string matchingSource = _settings.SourceFolders
                    .First(f => filePath.StartsWith(f, StringComparison.OrdinalIgnoreCase));

                string relativePath = Path.GetRelativePath(matchingSource, filePath);
                string targetFilePath = Path.Combine(currentBackupTarget, relativePath);

                string? targetDirectory = Path.GetDirectoryName(targetFilePath);
                if (targetDirectory != null)
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                using (var sourceStream = File.OpenRead(filePath))
                using (var destinationStream = File.Create(targetFilePath))
                {
                    await sourceStream.CopyToAsync(destinationStream, cancellationToken);
                }
                Interlocked.Increment(ref totalFiles);
                Interlocked.Add(ref totalBytes, fileInfo.Length);

                Log.Debug("Succes copyed: {Path}", fileInfo.Name);
            }
            catch (Exception ex)
            {
                Log.Error("Copy error {Path}: {Message}", filePath, ex.Message);
                concurrentErrors.TryAdd(filePath, ex.Message);
            }
        });

        var finalErrors = concurrentErrors.ToDictionary(k => k.Key, v => v.Value);

        return new BackupResult
        {
            Files = totalFiles,
            Memory = totalBytes,
            Errors = finalErrors
        };
    }
}
