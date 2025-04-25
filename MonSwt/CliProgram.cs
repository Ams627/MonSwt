using System.Diagnostics;
using System.IO.Pipes;

namespace MonSwtCli;

class CliProgram
{
    const string PipeName = "MonSwt.Pipe";
    private static void Main(string[] args)
    {
        try
        {
            EnsureExeRunning();
            using (var client = new NamedPipeClientStream(".", "MonSwt.Pipe", PipeDirection.InOut))
            {
                try
                {
                    var command = args.Length > 0 ? args[0] : "BasicPoll";

                    // Wait up to a second for connection:
                    client.Connect(1000);

                    using (var reader = new StreamReader(client))
                    using (var writer = new StreamWriter(client) { AutoFlush = true })
                    {
                        {
                            writer.WriteLine(command);
                            string response = reader.ReadLine();
                            Console.WriteLine(response);
                        }
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
        string pathOfThisProcess = Process.GetCurrentProcess().MainModule.FileName;
        string exeName = Path.ChangeExtension(pathOfThisProcess, ".exe");

        bool isRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeName)).Any();
        Console.WriteLine(isRunning);
        if (!isRunning)
        {
            try
            {
                Console.WriteLine($"Starting monswt hotkey monitoring process...");
                var result = Process.Start(exeName);
                Console.WriteLine($"Process is running...{result.Id}");

                // Give the GUI a second to initialise the pipe:
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start GUI: {ex}");
                Environment.Exit(1);
            }

        }
    }
}