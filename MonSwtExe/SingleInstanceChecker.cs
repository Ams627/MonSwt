namespace MonSwtExe;

public static class SingleInstanceChecker
{
    private static Mutex mutex;

    public static bool IsAlreadyRunning(string mutexNameBase)
    {
        string[] scopes = { "Global", "Local" };

        foreach (var scope in scopes)
        {
            string mutexName = $@"{scope}\{mutexNameBase}";


            try
            {
                mutex = new Mutex(true, mutexName, out bool createdNew);
                return !createdNew; // true if already running
            }
            catch (UnauthorizedAccessException)
            {
                // No permission for global — try local
            }
            catch (Exception ex)
            {
                // Unexpected error — fail safe
                Console.Error.WriteLine($"Unexpected mutex error: {ex.Message}");
                return true;
            }
        }

        // We got through the loop and created a mutex
        return false;
    }
}
