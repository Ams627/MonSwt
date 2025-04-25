using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using static MonSwtExe.NativeWindows;

namespace MonSwtExe;

class ExeProgram
{
    const string PipeName = "MonSwt.Pipe";
    private static uint ddcCode = 0;
    private static readonly DateTime _startTime = DateTime.Now;
    private static readonly ManualResetEvent eventToWaitFor = new ManualResetEvent(false);
    private static void Main(string[] args)
    {
        try
        {
            if (SingleInstanceChecker.IsAlreadyRunning("3541F58A-C130-41BE-A1E0-94759B66E40C"))
            {
                Environment.Exit(0);
            }
            var dir = @"c:\temp\logging";
            Directory.CreateDirectory(dir);
            //var logFile = Path.Combine(dir, "MonSwt.txt");
            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Debug()
            //    .WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
            //    .CreateLogger();
            //Log.Information("Log started");
            Task.Run(() => Pipe());

            // Register hotkey
            NativeWindows.RegisterHotKey(IntPtr.Zero, NativeWindows.HOTKEY_ID, 0, NativeWindows.VK_F16);
            Console.WriteLine("Hotkey F16 registered.");

            IntPtr[] handles = { eventToWaitFor.SafeWaitHandle.DangerousGetHandle() };

            // Windows Message loop
            while (true)
            {
                uint result = MsgWaitForMultipleObjects(
                    1,                  // one handle
                    handles,
                    false,              // wait for any
                    5000,               // timeout (optional)
                    QS_ALLINPUT         // wake on input/message
                );

                if (result == WAIT_OBJECT_0)
                {
                    break;
                }
                else if (result == WAIT_OBJECT_0 + 1)
                {
                    while (PeekMessage(out MSG msg, IntPtr.Zero, 0, 0, PM_REMOVE))
                    {
                        if (msg.message == NativeWindows.WM_HOTKEY && (int)msg.wParam == NativeWindows.HOTKEY_ID)
                        {
                            SwitchMonitors();
                        }
                        TranslateMessage(ref msg);
                        DispatchMessage(ref msg);
                    }
                }
                else if (result == WAIT_FAILED)
                {
                    Console.WriteLine("Wait failed: " + Marshal.GetLastWin32Error());
                    break;
                }
                else
                {

                }
            }
            Console.WriteLine("exiting");
        }
        catch (Exception ex)
        {
            var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
            var progname = Path.GetFileNameWithoutExtension(fullname);
            Console.Error.WriteLine($"{progname} Error: {ex.Message}");
        }
    }

    private static void SwitchMonitors()
    {
        // 0x1e
        if (ddcCode != 0)
            DDCController.SetInputSource(ddcCode);
    }

    private static void Pipe()
    {
        var pid = Process.GetCurrentProcess().Id;
        //   Log.Information("Starting Pipe");
        while (true)
        {
            using (var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut))
            {
                server.WaitForConnection();
                using (var reader = new StreamReader(server))
                using (var writer = new StreamWriter(server) { AutoFlush = true })
                {
                    string line = reader.ReadLine();
                    if (line == "BasicPoll")
                    {
                        var uptime = (int)(DateTime.Now - _startTime).TotalSeconds;
                        writer.WriteLine($"MonSwt has been running for {uptime} seconds with PID {pid}");
                    }
                    else if (line is "exit" or "kill")
                    {
                        var uptime = (int)(DateTime.Now - _startTime).TotalSeconds;

                        writer.WriteLine($"Monswt PID {pid} exiting");
                        eventToWaitFor.Set();
                    }
                    else if (line == "hdmi")
                    {
                        ddcCode = 0x11;
                        writer.WriteLine($"Monswt PID {pid} will switch to HDMI");
                    }
                    else if (line == "thunder")
                    {
                        ddcCode = 0x1e;
                        writer.WriteLine($"Monswt PID {pid} will switch to thunderbolt");
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
