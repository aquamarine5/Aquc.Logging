using Serilog;
using Serilog.Core;
using System.CommandLine;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aquc.Logging;
public class LoggingProgram
{
    public static readonly Logger logger = new Func<Logger>(() =>
    {
        return new LoggerConfiguration()
            .WriteTo.File($"log/{DateTime.Now:yyyyMMdd}.log",shared:true)
            .WriteTo.Console()
            .MinimumLevel.Verbose()
            .CreateLogger();
    }).Invoke();

    public const string MANIFEST_FILENAME = "Aquc.Logging.manifest.json";

    public static async Task Main(string[] args)
    {
        ConsoleFix.BindToConsole();
        var createCommand = new Command("create");
        var registerCommand = new Command("register");
        var uploadCommand = new Command("upload");
        createCommand.SetHandler(async () =>
        {
            await File.WriteAllTextAsync(MANIFEST_FILENAME,
                JsonSerializer.Serialize(new LoggingManifest()
                {
                    mailAccountConfigs = new()
                    {
                        new("mail.host.com","username","password",45,"Foo","sendto@foo.com", "sendfrom@foo.com")
                    }
                }, new JsonSerializerOptions { IncludeFields = true, WriteIndented = true })
            );
        });
        uploadCommand.SetHandler(async () =>
        {
            var service = new LoggingService();
            logger.Information("Start send log");
            await service.UploadLog();

        });
        var rootCommand = new RootCommand()
        {
            createCommand,
            registerCommand,
            uploadCommand
        };
        await rootCommand.InvokeAsync(args);
        ConsoleFix.FreeBind();
    }
}