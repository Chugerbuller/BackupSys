using BackupApp.Errors;
using BackupApp.Models;
using BackupApp.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("App starting...");

var cfgService = new ConfigurationService();
BackupSettings settings;

try
{
    Log.Information("Read config...");
    settings = cfgService.GetSettings("appsettings.json");

    if (!Enum.TryParse<LogEventLevel>(settings.LogLevel, true, out var serilogLevel))
    {
        serilogLevel = LogEventLevel.Information;
    }
    var logFileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.log";
    var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settings.LogDirectory, logFileName);

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Is(serilogLevel)
        .WriteTo.Console()
        .WriteTo.File(logPath)
        .CreateLogger();
    Log.Information("Logs path: {LogPath}", logPath);
    var backUpService = new BackupService(settings);
    Log.Information("Start of copying...");
    var backupRes = await backUpService.BackUp();
    Log.Information("Results: {Result}", backupRes.ToString());

    if (backupRes.Errors.Count > 0)
    {
        Log.Warning("Errors result ({Count}): {Errors}",
            backupRes.Errors.Count,
            backupRes.ErrorsToString());
    }
    else
    {
        Log.Information("End!");
    }
}
catch (BackupException ex)
{
    Log.Error("Config error [{Code}]: {Message}", ex.Code, ex.Message);
    Environment.ExitCode = 1;
    return;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Critical error");

    Environment.ExitCode = 2;
    return;
}
finally
{
    Log.CloseAndFlush();
}



