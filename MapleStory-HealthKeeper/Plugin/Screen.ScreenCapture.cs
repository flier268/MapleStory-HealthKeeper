using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Flier.SuperTools.Screen
{
    /// <summary>
    /// 螢幕截圖
    /// </summary>
    public class ScreenCapture
    {
        #region 抓取螢幕

        /// <summary>
        /// 抓取螢幕(層疊的窗口)
        /// </summary>
        /// <param name="x">左上角的橫坐標</param>
        /// <param name="y">左上角的縱坐標</param>
        /// <param name="width">抓取寬度</param>
        /// <param name="height">抓取高度</param>
        /// <returns></returns>
        public static Bitmap captureScreen(int x, int y, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(new Point(x, y), new Point(0, 0), bmp.Size);
                g.Dispose();
            }
            //bit.Save(@"capture2.png");
            return bmp;
        }

        /// <summary>
        ///  抓取整個螢幕
        /// </summary>
        /// <returns></returns>
        public static Bitmap captureScreen()
        {
            Size screenSize = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size;
            return captureScreen(0, 0, screenSize.Width, screenSize.Height);
        }

        #endregion 抓取螢幕

        #region 使用BitBlt方法抓取控件，無論控件是否被遮擋

        /// <summary>
        /// 控件(窗口)的截圖，控件被其他窗口(而非本窗口內控件)遮擋時也可以正確截圖，使用BitBlt方法
        /// </summary>
        /// <param name="control">需要被截圖的控件</param>
        /// <returns>該控件的截圖，控件被遮擋時也可以正確截圖</returns>
        public static Bitmap captureControl(Control control)
        {
            //調用API截屏
            IntPtr hSrce = GetWindowDC(control.Handle);
            IntPtr hDest = CreateCompatibleDC(hSrce);
            IntPtr hBmp = CreateCompatibleBitmap(hSrce, control.Width, control.Height);
            IntPtr hOldBmp = SelectObject(hDest, hBmp);
            if (BitBlt(hDest, 0, 0, control.Width, control.Height, hSrce, 0, 0, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt))
            {
                Bitmap bmp = Image.FromHbitmap(hBmp);
                SelectObject(hDest, hOldBmp);
                DeleteObject(hBmp);
                DeleteDC(hDest);
                ReleaseDC(control.Handle, hSrce);
                // bmp.Save(@"a.png");
                // bmp.Dispose();
                return bmp;
            }
            return null;
        }

        //         /// <summary>
        //         /// 有問題！！！！！用戶區域坐標不對啊
        //         /// 控件(窗口)的用戶區域截圖，控件被其他窗口(而非本窗口內控件)遮擋時也可以正確截圖，使用BitBlt方法
        //         /// </summary>
        //         /// <param name="control">需要被截圖的控件</param>
        //         /// <returns>控件(窗口)的用戶區域截圖</returns>
        //         public static Bitmap captureClientArea(Control control)
        //         {
        //
        //             Size sz = control.Size;
        //             Rectangle rect = control.ClientRectangle;
        //
        //
        //             //調用API截屏
        //             IntPtr hSrce = GetWindowDC(control.Handle);
        //             IntPtr hDest = CreateCompatibleDC(hSrce);
        //             IntPtr hBmp = CreateCompatibleBitmap(hSrce, rect.Width, rect.Height);
        //             IntPtr hOldBmp = SelectObject(hDest, hBmp);
        //             if (BitBlt(hDest, 0, 0, rect.Width, rect.Height, hSrce, rect.X, rect.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt))
        //             {
        //                 Bitmap bmp = Image.FromHbitmap(hBmp);
        //                 SelectObject(hDest, hOldBmp);
        //                 DeleteObject(hBmp);
        //                 DeleteDC(hDest);
        //                 ReleaseDC(control.Handle, hSrce);
        //                 // bmp.Save(@"a.png");
        //                 // bmp.Dispose();
        //                 return bmp;
        //             }
        //             return null;
        //
        //         }

        #endregion 使用BitBlt方法抓取控件，無論控件是否被遮擋

        #region 使用PrintWindow方法抓取窗口，無論控件是否被遮擋

        /// <summary>
        /// 窗口的截圖，窗口被遮擋時也可以正確截圖，使用PrintWindow方法
        /// </summary>
        /// <param name="form">需要被截圖的窗口</param>
        /// <returns>窗口的截圖，控件被遮擋時也可以正確截圖</returns>
        public static Bitmap captureWindowUsingPrintWindow(Form form)
        {
            return GetWindow(form.Handle);
        }

        private static Bitmap GetWindow(IntPtr hWnd)
        {
            IntPtr hscrdc = GetWindowDC(hWnd);
            Control control = Control.FromHandle(hWnd);
            IntPtr hbitmap = CreateCompatibleBitmap(hscrdc, control.Width, control.Height);
            IntPtr hmemdc = CreateCompatibleDC(hscrdc);
            SelectObject(hmemdc, hbitmap);
            PrintWindow(hWnd, hmemdc, 0);
            Bitmap bmp = Bitmap.FromHbitmap(hbitmap);
            DeleteDC(hscrdc);//刪除用過的對象
            DeleteDC(hmemdc);//刪除用過的對象
            return bmp;
        }

        public static Color GetPixelColor(IntPtr hwnd, int x, int y)
        {
            IntPtr hdc = (IntPtr)User32.GetDC(hwnd);
            int pixel = GDI32.GetPixel(hdc, x, y);
            ReleaseDC(hwnd, hdc);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                            (int)(pixel & 0x0000FF00) >> 8,
                            (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }

        #endregion 使用PrintWindow方法抓取窗口，無論控件是否被遮擋

        #region DLL calls

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int
        wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);

        [DllImport("gdi32.dll")]
        private static extern IntPtr DeleteDC(IntPtr hDc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr DeleteObject(IntPtr hDc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, UInt32 nFlags);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

        #endregion DLL calls
    }
}