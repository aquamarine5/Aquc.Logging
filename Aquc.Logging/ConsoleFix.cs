using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Logging;

public class ConsoleFix
{
    private const string DLLNAME="kernel32.dll";
    [DllImport(DLLNAME)]
    private static extern bool AllocConsole();
    [DllImport(DLLNAME)]
    private static extern bool AttachConsole(int dwordProcessId);
    [DllImport(DLLNAME)]
    private static extern bool FreeConsole();
    [DllImport(DLLNAME)]
    private static extern IntPtr GetConsoleWindow();
    [DllImport(DLLNAME)]
    private static extern int GetConsoleOutputCP();
    public static void BindToConsole()
    {
        AttachConsole(-1);
        Console.WriteLine("\n");
    }
    public static void FreeBind()
    {
        FreeConsole();
    }
}
