using System.Runtime.InteropServices;

namespace MapleStory_HealthKeeper.Plugin
{
    public class User32 : flier268.Win32API.User32
    {
        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int GetDpiForWindow(IntPtr hwnd);
    }
}