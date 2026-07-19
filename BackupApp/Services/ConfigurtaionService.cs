using BackupApp.Errors;
using BackupApp.Models;

namespace BackupApp.Services;

using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

public class ConfigurationService : IConfigurationService
{     
    public BackupSettings GetSettings(string path) { 
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new BackupException(ErrorCode.ConfigInitProblem, "The path to the configuration file cannot be empty.");
        }

        try
        {
            // Получаем абсолютный путь к файлу относительно директории приложения, 
            // если передан относительный путь.
            var fullPath = Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

            if (!File.Exists(fullPath))
            {
                throw new BackupException(ErrorCode.ConfigInitProblem, $"Configuration file not found at path: {fullPath}");
            }

            var directory = Path.GetDirectoryName(fullPath) ?? AppDomain.CurrentDomain.BaseDirectory;
            var fileName = Path.GetFileName(fullPath);

            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(directory)
                .AddJsonFile(fileName, optional: false, reloadOnChange: true)
                .Build();

            var settings = config.GetSection("BackupSettings").Get<BackupSettings>();

            if (settings is null)
            {
                throw new BackupException(ErrorCode.ConfigInitProblem, "The 'BackupSettings' section is missing from the configuration file.");
            }

            // Обязательно валидируем данные перед тем, как отдать их программе
            ValidateSettings(settings);

            return settings;
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"FormatException: {ex.Message}");
            throw;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"FileNotFoundException: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            // Перехватываем любые системные ошибки (JSON, IO) и бережно упаковываем в наше исключение
            throw new BackupException(ErrorCode.ConfigInitProblem, $"Critical error reading configuration: {ex.Message}");
        }
    }

    private void ValidateSettings(BackupSettings settings)
    {
        if (settings.SourceFolders == null || !settings.SourceFolders.Any())
        {
            throw new BackupException(ErrorCode.ConfigInvalidFormat, "The list of source folders (SourceFolders) is empty or not specified.");
        }

        if (string.IsNullOrWhiteSpace(settings.TargetFolder))
        {
            throw new BackupException(ErrorCode.ConfigInvalidFormat, "The target folder (TargetFolder) is not specified.");
        }

        // Проверяем, что исходные папки вообще существуют физически
        foreach (var source in settings.SourceFolders)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new BackupException(ErrorCode.ConfigInvalidFormat, "An empty path was found in the list of source folders.");
            }
        }
    }
}
