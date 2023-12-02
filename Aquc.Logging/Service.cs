using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Serilog.Core;

namespace Aquc.Logging;

public class LoggingService
{
    public const string REGEX_PATTERN = "%";
    public const string DATETIME_PATTERN = "DATETIME:";
    const string REGEX_DATETIME_PATTERN = $"{REGEX_PATTERN}{DATETIME_PATTERN}(\\w*){REGEX_PATTERN}";

    private static Logger Logger => LoggingProgram.logger;

    public LoggingManifest manifest;

    public LoggingService() 
    {
        manifest = JsonSerializer.Deserialize<LoggingManifest>(File.ReadAllText(LoggingProgram.MANIFEST_FILENAME),new JsonSerializerOptions { IncludeFields=true})!;
    }

    public async Task UploadLog()
    {

        Logger.Information($"Matched {manifest.mailAccountConfigs.Count} mail account.");
        for (int i = 0; i < manifest.mailAccountConfigs.Count; i++)
        {
            var account = manifest.mailAccountConfigs[i];
            Logger.Information($"Try {i + 1}/{manifest.mailAccountConfigs.Count}: {account.name} ({account.sendfrom}) to post mail.");
            if (await UploadLogByAccount(account,i))
            {
                return;
            }
        }
    }

    async Task<bool> UploadLogByAccount(LoggingMailAccountConfig account,int i)
    {
        try
        {
            using var smtpServer = new SmtpClient(account.host, account.port)
            {
                UseDefaultCredentials=false,
                Credentials = new NetworkCredential(account.username, account.password),
                EnableSsl=true, 
            };
            await smtpServer.SendMailAsync(await CreateMailMessage(account));
            Logger.Information($"{i+1}/{manifest.mailAccountConfigs.Count} {account.name}: send mail to {account.sendto} successfully.");
        }
        catch (Exception ex)
        {
            Logger.Error($"{i+1}/{manifest.mailAccountConfigs.Count} {account.name}: raised exception {ex.GetType()} {ex.Message}");
            Logger.Error(ex.StackTrace??"");
            return false;
        }
        return true;
    }

    async Task<MailMessage> CreateMailMessage(LoggingMailAccountConfig accountConfig)
    {
        var mailMessage = new MailMessage(accountConfig.sendfrom, accountConfig.sendto)
        {
            Subject = $"{Environment.MachineName}: {DateTime.Now:yyyyMMdd}-log",
            Body = await GetLogContent()
        };
        return mailMessage;
    }
    string GetLogFilePath()
    {
        var filePath = manifest.logFilePath;
        var datetimeRegexMatch = Regex.Match(filePath, REGEX_DATETIME_PATTERN);

        Console.WriteLine(datetimeRegexMatch.Groups[1].Value);
        if (datetimeRegexMatch.Success)
        {
            filePath = Regex.Replace(filePath, REGEX_DATETIME_PATTERN, DateTime.Now.ToString(datetimeRegexMatch.Groups[1].Value));
        }
        return filePath;
    }
    async Task<string> GetLogContent()
    {
        var filePath=GetLogFilePath();
        using var fs=new FileStream(filePath, FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
        using var sr = new StreamReader(fs);
        return await sr.ReadToEndAsync();
    }
}
