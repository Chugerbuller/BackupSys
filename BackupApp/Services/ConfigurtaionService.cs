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
            throw new BackupException(ErrorCode.ConfigInitProblem, "Путь к файлу конфигурации не может быть пустым.");
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
                throw new BackupException(ErrorCode.ConfigInitProblem, $"Файл конфигурации не найден по пути: {fullPath}");
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
                throw new BackupException(ErrorCode.ConfigInitProblem, "Секция 'BackupSettings' отсутствует в файле конфигурации.");
            }

            // Обязательно валидируем данные перед тем, как отдать их программе
            ValidateSettings(settings);

            return settings;
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"FormatException: {ex.Message}");
            throw ex;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"FileNotFoundException: {ex.Message}");
            throw ex;
        }
        catch (Exception ex)
        {
            // Перехватываем любые системные ошибки (JSON, IO) и бережно упаковываем в наше исключение
            throw new BackupException(ErrorCode.ConfigInitProblem, $"Критическая ошибка при чтении конфигурации: {ex.Message}");
        }
    }

    private void ValidateSettings(BackupSettings settings)
    {
        if (settings.SourceFolders == null || !settings.SourceFolders.Any())
        {
            throw new BackupException(ErrorCode.ConfigInvalidFormat, "Список исходных папок (SourceFolders) пуст или не задан.");
        }

        if (string.IsNullOrWhiteSpace(settings.TargetFolder))
        {
            throw new BackupException(ErrorCode.ConfigInvalidFormat, "Целевая папка (TargetFolder) не указана.");
        }

        // Проверяем, что исходные папки вообще существуют физически
        foreach (var source in settings.SourceFolders)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new BackupException(ErrorCode.ConfigInvalidFormat, "Обнаружен пустой путь в списке исходных папок.");
            }
        }
    }
}
