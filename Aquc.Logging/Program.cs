using Aquc.Netdisk.Mail;
using Huanent.Logging.File;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.CommandLine;
using System.Diagnostics;
using System.Net.Mail;

namespace Aquc.Logging;

internal class Program
{
    static async Task Main(string[] args)
    {
        using var host = new HostBuilder()
            .ConfigureLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddFilter<ConsoleLoggerProvider>(level => level >= LogLevel.Debug);
                builder.AddFilter<FileLoggerProvider>(level => level >= LogLevel.Debug);
                builder.AddConsole();
                builder.AddFile();
            })
            .ConfigureServices(container =>
            {
                container.TryAddSingleton(services=>
                    new MailService(services.GetRequiredService<ILogger<MailService>>(),"aquamarine5@163.com", "TKFXIAGOFXKRIOSX", "smtp.163.com"));
            })
            .Build();
        var _logger = host.Services.GetRequiredService<ILogger<Program>>();
        var registerCommand = new Command("register");
        var uploadCommand = new Command("upload");
        var rootCommand = new RootCommand()
        {
            uploadCommand,
            registerCommand
        };
        registerCommand.SetHandler(async () =>
        {
            using var process2 = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "schtasks",
                    Arguments = $"/Create /F /SC daily /st 20:00 /TR \"'{Environment.ProcessPath + "' upload"}\" /TN \"Aquacore\\Aquc.Logging.UploadLogFileSchtask\"",
                    CreateNoWindow = true
                }
            };
            process2.Start();
            process2.WaitForExit();
            _logger.LogInformation("Success schedule subscriptions-update-all");
        });
        uploadCommand.SetHandler(async () =>
        {
            var mail = host.Services.GetRequiredService<MailService>();
            var logFile = Path.Combine(Directory.GetCurrentDirectory(), "logs", $"{DateTime.Now:yyyMMdd}.txt");
            var content = string.Empty;
            if (File.Exists(logFile)) {
                using var fs = new FileStream(logFile, FileMode.Open);
                using var sr = new StreamReader(fs);
                content = (await sr.ReadToEndAsync()).Replace("\n\n", "\n");
            }
            else
            {
                _logger?.LogWarning("Failed to find log file: {f}", logFile);
                content = $"Failed to find log file: {logFile}";
            }
            await mail.Send(new MailMessage("aquamarine5@163.com", "3168287806@qq.com", $"{DateTime.Now:yyMMdd}-log", content));
            _logger?.LogInformation("Send log file to {m}", "3168287806@qq.com");
        });
        if (args.Length == 0) args = new string[] { "upload" };
        await rootCommand.InvokeAsync(args);
    }
}