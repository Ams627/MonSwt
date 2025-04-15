using Serilog;
using System.Diagnostics;
using System.IO.Pipes;

namespace MonSwtExe;

class ExeProgram
{
    const string PipeName = "MonSwt.Pipe";
    private static readonly DateTime _startTime = DateTime.Now;

    private static void Main(string[] args)
    {
        try
        {
            var dir = @"c:\temp\logging";
            Directory.CreateDirectory(dir);
            var logFile = Path.Combine(dir, "MonSwt.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
                .CreateLogger();
            Log.Information("Log started");
            Pipe();
        }
        catch (Exception ex)
        {
            var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
            var progname = Path.GetFileNameWithoutExtension(fullname);
            Console.Error.WriteLine($"{progname} Error: {ex.Message}");
        }
    }

    private static void Pipe()
    {
        Log.Information("Starting Pipe");
        while (true)
        {
            using (var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut))
            {
                server.WaitForConnection();
                Console.WriteLine("Connection");

                using (var reader = new StreamReader(server))
                using (var writer = new StreamWriter(server) { AutoFlush = true })
                {
                    string line = reader.ReadLine();
                    if (line == "basic poll")
                    {
                        var uptime = (int)(DateTime.Now - _startTime).TotalSeconds;
                        var pid = Process.GetCurrentProcess().Id;
                        writer.WriteLine($"Running for {uptime} seconds with PID {pid}");
                    }
                    else
                    {
                        writer.WriteLine("Unknown command");
                    }
                }
            }
        }

    }
}
