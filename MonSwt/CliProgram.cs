using System.Diagnostics;
using System.IO.Pipes;

namespace MonSwtCli;

class CliProgram
{
    const string PipeName = "MonSwt.Pipe";
    const string ExeName = "MonSwt.exe";
    private static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Hello");
            EnsureExeRunning();
            using (var client = new NamedPipeClientStream(".", "MyMonitorPipe", PipeDirection.InOut))
            {
                try
                {
                    // Wait up to a second for connection:
                    client.Connect(1000);

                    using (var writer = new StreamWriter(client) { AutoFlush = true })
                    using (var reader = new StreamReader(client))
                    {
                        writer.WriteLine("basic poll");
                        string response = reader.ReadLine();
                        Console.WriteLine(response);
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("Could not connect to GUI process.");
                }
            }

        }
        catch (Exception ex)
        {
            var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
            var progname = Path.GetFileNameWithoutExtension(fullname);
            Console.Error.WriteLine($"{progname} Error: {ex.Message}");
        }
    }

    private static void EnsureExeRunning()
    {
        Console.WriteLine("Ensure");
        var runningP = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ExeName));
        bool isRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ExeName)).Any();
        if (!isRunning)
        {
            try
            {
                Console.WriteLine($"Starting process...");
                var result = Process.Start(ExeName);
                Console.WriteLine($"Process should be running...{result.Id}");

                // Give the GUI a second to initialise the pipe:
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start GUI: {ex.Message}");
                Environment.Exit(1);
            }

        }
    }
}