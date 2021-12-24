using static flier268.Win32API.User32;

namespace MapleStory_HealthKeeper.Plugin
{
    public class BackgroundSimulate
    {
        public PreciseDelay PreciseDelay { get; } = new();
        public IntPtr Hwnd { get; set; }

        public BackgroundSimulate()
        {
        }

        public BackgroundSimulate(IntPtr hwnd)
        {
            Hwnd = hwnd;
        }

        public void KeyDown(IFullKeyInfo fullKeyInfo)
        {
            KeyDown(fullKeyInfo.Key, fullKeyInfo.ScanCode, fullKeyInfo.IsExtented);
        }

        public void KeyDown(Keys keys, int scanCode, bool isExtented)
        {
            uint repeatCount = 0;
            uint extended = (uint)(isExtented ? 1 : 0);
            uint context = 0;
            uint previousState = 0;
            uint transition = 0;

            uint lParamDown = repeatCount
                | (uint)scanCode << 16
                | extended << 24
                | context << 29
                | previousState << 30
                | transition << 31;
            _ = PostMessage(Hwnd, WM_KEYDOWN, (int)keys, (int)lParamDown);
        }

        public void KeyUp(IFullKeyInfo fullKeyInfo)
        {
            KeyUp(fullKeyInfo.Key, fullKeyInfo.ScanCode, fullKeyInfo.IsExtented);
        }

        public void KeyUp(Keys keys, int scanCode, bool isExtented)
        {
            uint repeatCount = 0;
            uint extended = (uint)(isExtented ? 1 : 0);
            uint context = 0;
            uint previousState = 1;
            uint transition = 1;
            uint lParamUp = repeatCount
                | (uint)scanCode << 16
                | extended << 24
                | context << 29
                | previousState << 30
                | transition << 31;
            _ = PostMessage(Hwnd, WM_KEYUP, (int)keys, (int)lParamUp);
        }

        public void KeyPress(IFullKeyInfo fullKeyInfo, int delayAfterDown = 50, int delayAfterUp = 50)
        {
            KeyDown(fullKeyInfo.Key, fullKeyInfo.ScanCode, fullKeyInfo.IsExtented);
            PreciseDelay.Delay(delayAfterDown);
            KeyUp(fullKeyInfo.Key, fullKeyInfo.ScanCode, fullKeyInfo.IsExtented);
            PreciseDelay.Delay(delayAfterUp);
        }

        public void KeyPress(Keys keys, int scanCode, bool isExtended, int delayAfterDown = 50, int delayAfterUp = 50)
        {
            KeyDown(keys, scanCode, isExtended);
            PreciseDelay.Delay(delayAfterDown);
            KeyUp(keys, scanCode, isExtended);
            PreciseDelay.Delay(delayAfterUp);
        }
    }
}