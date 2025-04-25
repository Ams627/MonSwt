using System.Runtime.InteropServices;

namespace MonSwtExe;

static class NativeWindows
{
    public const int WM_HOTKEY = 0x0312;
    public const int HOTKEY_ID = 1;
    public const int MOD_ALT = 0x0001;
    public const int VK_F10 = 0x79;
    public const int VK_F16 = 0x7F;
    public const uint QS_ALLINPUT = 0x04FF;
    public const uint WAIT_OBJECT_0 = 0x00000000;
    public const uint WAIT_FAILED = 0xFFFFFFFF;
    public const uint PM_REMOVE = 1;

    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll")]
    public static extern int GetMessage(out MSG msg, IntPtr hWnd, uint min, uint max);


    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint MsgWaitForMultipleObjects(uint nCount, IntPtr[] pHandles, bool bWaitAll, uint dwMilliseconds, uint dwWakeMask);

    [DllImport("user32.dll")]
    public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [DllImport("user32.dll")]
    public static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    public static extern IntPtr DispatchMessage(ref MSG lpMsg);

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT { public int x, y; }
}
