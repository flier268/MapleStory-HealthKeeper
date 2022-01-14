using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using Flier.SuperTools.Screen;
using MapleStory_HealthKeeper.Plugin;

namespace MapleStory_HealthKeeper
{
    public class HealthKeeperService
    {
        private MainWindowViewModel ViewModel { get; set; }

        public HealthKeeperService(MainWindowViewModel mainWindowViewModel)
        {
            backgroundWorker = new();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            ViewModel = mainWindowViewModel;
        }

        public void Start()
        {
            if (backgroundWorker.IsBusy == false)
                backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            double Y = 585 / 600d;
            double InGameCheckPoint = 75 / 800d;
            double HpMin = 220 / 800d;
            double HpMax = 324 / 800d;
            double MpMin = 329 / 800d;
            double MpMax = 432 / 800d;
            BackgroundSimulate backgroundSimulate = new();
            while (true)
            {
                var ps = Process.GetProcessesByName(ViewModel.MapleStoryProcessName);
                if (ps.Length == 0)
                {
                    ViewModel.Status = MainWindowViewModel.MapleStoryNotFound;
                    PreciseDelay.Delay(3000);
                    continue;
                }
                ViewModel.Status = $"Found {ps.Length} process";
                foreach (var process in ps)
                {
                    RECT rect = new RECT();
                    if (User32.GetClientRect(process.MainWindowHandle, ref rect) != 0)
                    {
                        int Width = rect.Right - rect.Left;
                        int Height = rect.Bottom - rect.Top;
                        //修正DPI問題
                        var Window1607 = new Version(10, 0, 0, 9200);
                        if (Environment.OSVersion.Version >= Window1607)
                        {
                            if (Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero)
                            {
                                int windowDPI = User32.GetDpiForWindow(process.MainWindowHandle);
                                int thisWindowDPI = User32.GetDpiForWindow(Process.GetCurrentProcess().MainWindowHandle);
                                Width = Width * windowDPI / thisWindowDPI;
                                Height = Height * windowDPI / thisWindowDPI;
                            }
                        }
                        int YPoint = (int)(Y * Height);
                        //Debug
                        //Color hpmin = ScreenCapture.GetPixelColor(process.MainWindowHandle, (int)(HpMin * Width), YPoint);
                        //Color hpmax = ScreenCapture.GetPixelColor(process.MainWindowHandle, (int)(HpMax * Width), YPoint);
                        //Color mpmin = ScreenCapture.GetPixelColor(process.MainWindowHandle, (int)(MpMin * Width), YPoint);
                        //Color mpmax = ScreenCapture.GetPixelColor(process.MainWindowHandle, (int)(MpMax - MpMin), YPoint);
                        //var picture = ScreenCapture.Capture(process.MainWindowHandle, Width, Height);
                        //var g = Graphics.FromImage(picture);
                        //g.DrawRectangle(Pens.Red, (int)(HpMin * Width), YPoint - 5, (int)(HpMax * Width) - (int)(HpMin * Width), 10);
                        //g.DrawRectangle(Pens.Blue, (int)(MpMin * Width), YPoint - 5, (int)(MpMax * Width) - (int)(MpMin * Width), 10);
                        //picture.Save("123.bmp");

                        // Check in game
                        Color InGameCheckPointColor = ScreenCapture.GetPixelColor(process.MainWindowHandle, (int)(InGameCheckPoint * Width), YPoint);
                        if (FindPIC.IsColor(CheckPointColor, InGameCheckPointColor, 10) == false)
                            continue;

                        Color HP = ScreenCapture.GetPixelColor(process.MainWindowHandle, (int)(((HpMax - HpMin) * (ViewModel.KeepHpOverThen / 100d) + HpMin) * Width), YPoint);
                        Color MP = ScreenCapture.GetPixelColor(process.MainWindowHandle, (int)(((MpMax - MpMin) * (ViewModel.KeepMpOverThen / 100d) + MpMin) * Width), YPoint);
                        backgroundSimulate.Hwnd = process.MainWindowHandle;
                        if (FindPIC.IsColor(Gray, HP, 30))
                        {
                            backgroundSimulate.KeyPress(ViewModel.HpKey);
                        }
                        if (FindPIC.IsColor(Gray, MP, 30))
                        {
                            backgroundSimulate.KeyPress(ViewModel.MpKey);
                        }
                    }
                }
                PreciseDelay.Delay(ViewModel.Delay);
            }
        }

        private readonly Color Gray = Color.FromArgb(190, 190, 190);
        private readonly Color CheckPointColor = Color.FromArgb(17, 17, 17);
        private PreciseDelay PreciseDelay { get; } = new();
        private BackgroundWorker backgroundWorker { get; }
    }
}