using Serilog;

namespace BookReader.Core.Services;

public static class LoggerService
{
    private static readonly ILogger _logger;

    static LoggerService()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/bookreader.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public static void LogError(string message, Exception ex = null)
    {
        _logger.Error(ex, message);
    }

    public static void LogInfo(string message)
    {
        _logger.Information(message);
    }
}
