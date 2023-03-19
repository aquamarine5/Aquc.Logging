using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aquc.Logging.File
{
    public static class FileLoggerFactoryExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder,Action<FileLoggerOptions>? option=null)
        {
            builder.Services.AddOptions<FileLoggerOptions>();
            if(option != null) builder.Services.Configure(option);
            return builder;
        }
    }
    public class FileLoggerOptions
    {
        public string Path { get; set; } = "logs";
        public string DebugLogFileFormat { get; set; } = "yyyMMdd";
        public string DebugLogFileSuffix { get; set; } = "-debug";
        public string CriticalLogFileFormat { get; set; } = "yyyMMdd";
        public string CriticalLogFileSuffix { get; set; } = "-critical";
        public string LogFileType { get; set; } = ".log";
    }
    public class FileLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            throw new NotImplementedException();

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}