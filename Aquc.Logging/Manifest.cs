using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Logging;

[Serializable]
public class LoggingManifest
{
    public readonly int manifestVersion = 1;
    public List<LoggingMailAccountConfig> mailAccountConfigs=new();
    public string logFilePath="";
}

[Serializable]
public class LoggingMailAccountConfig
{
    public string sendto;
    public string sendfrom;
    public string host;
    public string username;
    public string password;
    public int port;
    public string name;
    public LoggingMailAccountConfig(string host, string username, string password, int port, string name, string sendto, string sendfrom)
    {
        this.host = host;
        this.username = username;
        this.password = password;
        this.port = port;
        this.name = name;
        this.sendto = sendto;
        this.sendfrom = sendfrom;
    }
}