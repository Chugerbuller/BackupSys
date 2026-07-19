using System.Text;

namespace BackupApp.Models;

public class BackupResult
{
    public long Files { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
    public long Memory { get; set; }

    public BackupResult() { }

    public BackupResult(long files, Dictionary<string, string> errors, long memory)
    {
        Files = files;
        Errors = errors;
        Memory = memory;
    }
    public override string ToString()
    {
        double sizeInMb = Memory / 1024.0 / 1024.0;
        return $"Успешно скопировано файлов: {Files} | Общий объем: {sizeInMb:F2} МБ";
    }
    public string ErrorsToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(); // Начнем с новой строки для красоты в логах
        foreach (var item in Errors)
        {
            sb.AppendLine($"  - {item.Key} -> Ошибка: {item.Value}");
        }
        return sb.ToString();
    }
}
