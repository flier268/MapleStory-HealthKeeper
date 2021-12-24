using System.Drawing;
using System.Drawing.Imaging;
using Point = System.Drawing.Point;

namespace Flier.SuperTools.Screen
{
    /// <summary>
    /// 找圖找色
    /// </summary>
    public class FindPIC
    {
        /// <summary>
        /// 在大圖裡找小圖
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="S_bmp">大圖(顏色格式只支援24位bmp)</param>
        /// <param name="P_bmp">小圖(顏色格式只支援24位bmp)</param>
        /// <param name="similar">容錯值 取值0~255，數值越高效率越低，不建議超過50</param>
        /// <returns></returns>
        public static List<Point> FindPic(int left, int top, int width, int height, Bitmap S_bmp, Bitmap P_bmp, int similar)
        {
            if (S_bmp.PixelFormat != PixelFormat.Format24bppRgb) { throw new Exception("颜色格式只支援24位bmp"); }
            if (P_bmp.PixelFormat != PixelFormat.Format24bppRgb) { throw new Exception("颜色格式只支援24位bmp"); }
            int S_Width = S_bmp.Width;
            int S_Height = S_bmp.Height;
            int P_Width = P_bmp.Width;
            int P_Height = P_bmp.Height;
            //取出4个角的颜色
            int px1 = P_bmp.GetPixel(0, 0).ToArgb(); //左上角
            int px2 = P_bmp.GetPixel(P_Width - 1, 0).ToArgb(); //右上角
            int px3 = P_bmp.GetPixel(0, P_Height - 1).ToArgb(); //左下角
            int px4 = P_bmp.GetPixel(P_Width - 1, P_Height - 1).ToArgb(); //右下角
            Color BackColor = P_bmp.GetPixel(0, 0); //背景色
            BitmapData S_Data = S_bmp.LockBits(new Rectangle(0, 0, S_Width, S_Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData P_Data = P_bmp.LockBits(new Rectangle(0, 0, P_Width, P_Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            List<Point> List;
            if (px1 == px2 && px1 == px3 && px1 == px4) //如果4个角的颜色相同
            {
                //透明找图
                List = _FindPic(left, top, width, height, S_Data, P_Data, GetPixelData(P_Data, BackColor), similar);
            }
            else if (similar > 0)
            {
                //相似找图
                List = _FindPic(left, top, width, height, S_Data, P_Data, similar);
            }
            else
            {
                //全匹配找图效率最高
                List = _FindPic(left, top, width, height, S_Data, P_Data);
            }
            S_bmp.UnlockBits(S_Data);
            P_bmp.UnlockBits(P_Data);
            return List;
        }

        //全匹配找圖
        private static unsafe List<Point> _FindPic(int left, int top, int width, int height, BitmapData S_Data, BitmapData P_Data)
        {
            List<Point> List = new List<Point>();
            int S_stride = S_Data.Stride;
            int P_stride = P_Data.Stride;
            IntPtr S_Iptr = S_Data.Scan0;
            IntPtr P_Iptr = P_Data.Scan0;
            byte* S_ptr;
            byte* P_ptr;
            bool IsOk = false;
            int _BreakW = width - P_Data.Width + 1;
            int _BreakH = height - P_Data.Height + 1;
            for (int h = top; h < _BreakH; h++)
            {
                for (int w = left; w < _BreakW; w++)
                {
                    P_ptr = (byte*)(P_Iptr);
                    for (int y = 0; y < P_Data.Height; y++)
                    {
                        for (int x = 0; x < P_Data.Width; x++)
                        {
                            S_ptr = (byte*)((Int64)S_Iptr + S_stride * (h + y) + (w + x) * 3);
                            P_ptr = (byte*)((Int64)P_Iptr + P_stride * y + x * 3);
                            if (S_ptr[0] == P_ptr[0] && S_ptr[1] == P_ptr[1] && S_ptr[2] == P_ptr[2])
                            {
                                IsOk = true;
                            }
                            else
                            {
                                IsOk = false; break;
                            }
                        }
                        if (IsOk == false) { break; }
                    }
                    if (IsOk) { List.Add(new Point(w, h)); }
                    IsOk = false;
                }
            }
            return List;
        }

        //相似找圖
        private static unsafe List<Point> _FindPic(int left, int top, int width, int height, BitmapData S_Data, BitmapData P_Data, int similar)
        {
            List<Point> List = new List<Point>();
            int S_stride = S_Data.Stride;
            int P_stride = P_Data.Stride;
            IntPtr S_Iptr = S_Data.Scan0;
            IntPtr P_Iptr = P_Data.Scan0;
            byte* S_ptr;
            byte* P_ptr;
            bool IsOk = false;
            int _BreakW = width - P_Data.Width + 1;
            int _BreakH = height - P_Data.Height + 1;
            for (int h = top; h < _BreakH; h++)
            {
                for (int w = left; w < _BreakW; w++)
                {
                    P_ptr = (byte*)(P_Iptr);
                    for (int y = 0; y < P_Data.Height; y++)
                    {
                        for (int x = 0; x < P_Data.Width; x++)
                        {
                            S_ptr = (byte*)((Int64)S_Iptr + S_stride * (h + y) + (w + x) * 3);
                            P_ptr = (byte*)((Int64)P_Iptr + P_stride * y + x * 3);
                            if (ScanColor(S_ptr[0], S_ptr[1], S_ptr[2], P_ptr[0], P_ptr[1], P_ptr[2], similar))  //比较颜色
                            {
                                IsOk = true;
                            }
                            else
                            {
                                IsOk = false; break;
                            }
                        }
                        if (IsOk == false) { break; }
                    }
                    if (IsOk) { List.Add(new Point(w, h)); }
                    IsOk = false;
                }
            }
            return List;
        }

        //透明找圖
        private static unsafe List<Point> _FindPic(int left, int top, int width, int height, BitmapData S_Data, BitmapData P_Data, int[,] PixelData, int similar)
        {
            List<Point> List = new List<Point>();
            int Len = PixelData.GetLength(0);
            int S_stride = S_Data.Stride;
            int P_stride = P_Data.Stride;
            IntPtr S_Iptr = S_Data.Scan0;
            IntPtr P_Iptr = P_Data.Scan0;
            byte* S_ptr;
            byte* P_ptr;
            bool IsOk = false;
            int _BreakW = width - P_Data.Width + 1;
            int _BreakH = height - P_Data.Height + 1;
            for (int h = top; h < _BreakH; h++)
            {
                for (int w = left; w < _BreakW; w++)
                {
                    for (int i = 0; i < Len; i++)
                    {
                        S_ptr = (byte*)((Int64)S_Iptr + S_stride * (h + PixelData[i, 1]) + (w + PixelData[i, 0]) * 3);
                        P_ptr = (byte*)((Int64)P_Iptr + P_stride * PixelData[i, 1] + PixelData[i, 0] * 3);
                        if (ScanColor(S_ptr[0], S_ptr[1], S_ptr[2], P_ptr[0], P_ptr[1], P_ptr[2], similar))  //比较颜色
                        {
                            IsOk = true;
                        }
                        else
                        {
                            IsOk = false; break;
                        }
                    }
                    if (IsOk) { List.Add(new Point(w, h)); }
                    IsOk = false;
                }
            }
            return List;
        }

        #region FindColor

        /// <summary>
        /// 找色
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="S_bmp">大圖(顏色格式只支援24位bmp)</param>
        /// <param name="clr">要找的顏色</param>
        /// <param name="similar">容錯值 取值0~255，數值越高效率越低，不建議超過50</param>
        /// <returns></returns>
        public static unsafe List<Point> FindColor(int left, int top, int width, int height, Bitmap S_bmp, Color clr, int similar)
        {
            if (S_bmp.PixelFormat != PixelFormat.Format24bppRgb) { throw new Exception("顏色格式只支援24位bmp"); }
            BitmapData S_Data = S_bmp.LockBits(new Rectangle(0, 0, S_bmp.Width, S_bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            IntPtr _Iptr = S_Data.Scan0;
            byte* _ptr;
            List<Point> List = new List<Point>();
            for (int y = top; y < height; y++)
            {
                for (int x = left; x < width; x++)
                {
                    _ptr = (byte*)((Int64)_Iptr + S_Data.Stride * (y) + (x) * 3);
                    if (ScanColor(_ptr[0], _ptr[1], _ptr[2], clr.B, clr.G, clr.R, similar))
                    {
                        List.Add(new Point(x, y));
                    }
                }
            }
            S_bmp.UnlockBits(S_Data);
            return List;
        }

        #endregion FindColor

        #region GetPixelColor

        /// <summary>
        /// 取得圖片中某像素的顏色
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="S_bmp">圖</param>
        /// <returns></returns>
        public static unsafe Color GetPixelColor(int x, int y, Bitmap S_bmp)
        {
            if (S_bmp.PixelFormat != PixelFormat.Format24bppRgb) { throw new Exception("顏色格式只支援24位bmp"); }
            BitmapData S_Data = S_bmp.LockBits(new Rectangle(0, 0, S_bmp.Width, S_bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            IntPtr _Iptr = S_Data.Scan0;
            byte* _ptr;
            _ptr = (byte*)((Int64)_Iptr + S_Data.Stride * (y) + (x) * 3);

            S_bmp.UnlockBits(S_Data);
            return Color.FromArgb(_ptr[0], _ptr[1], _ptr[2]);
        }

        #endregion GetPixelColor

        #region IsColor

        /// <summary>
        /// 比較兩個 Color 是否相似
        /// </summary>
        /// <param name="clr1">顏色一</param>
        /// <param name="clr2">顏色二</param>
        /// <param name="similar">容錯值 取值0~255，數值越高效率越低，不建議超過50</param>
        /// <returns></returns>
        public static bool IsColor(Color clr1, Color clr2, int similar = 0)
        {
            if (ScanColor(clr1.B, clr1.G, clr1.R, clr2.B, clr2.G, clr2.R, similar))
            {
                return true;
            }
            return false;
        }

        #endregion IsColor

        #region CopyScreen

        /// <summary>
        /// 螢幕截圖
        /// </summary>
        /// <param name="rect">截圖範圍</param>
        /// <returns></returns>
        public static Bitmap CopyScreen(Rectangle rect)
        {
            Bitmap bitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size);
                g.Dispose();
            }
            System.GC.Collect();
            return bitmap;
        }

        /// <summary>
        /// 螢幕截圖
        /// </summary>
        /// <param name="rect">截圖範圍</param>
        /// <returns></returns>
        public static Bitmap CopyScreen()
        {
            return CopyScreen(System.Windows.Forms.Screen.PrimaryScreen.Bounds);
        }

        /// <summary>
        /// 從螢幕的某個範圍內找圖
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="P_bmp">要找的圖</param>
        /// <param name="similar">容錯值 取值0~255，數值越高效率越低，不建議超過50</param>
        /// <returns></returns>
        public static List<Point> FindPic_FromScreen(int left, int top, int width, int height, Bitmap P_bmp, int similar)
        {
            return FindPic(0, 0, width, height, CopyScreen(new Rectangle(left, top, width, height)), P_bmp, similar);
        }

        /// <summary>
        /// 從整個螢幕內找圖
        /// </summary>
        /// <param name="P_bmp">要找的圖</param>
        /// <param name="similar">容錯值 取值0~255，數值越高效率越低，不建議超過50</param>
        /// <returns></returns>
        public static List<Point> FindPic_FromScreen(Bitmap P_bmp, int similar)
        {
            return FindPic(0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, CopyScreen(System.Windows.Forms.Screen.PrimaryScreen.Bounds), P_bmp, similar);
        }

        /// <summary>
        /// 從螢幕的某個範圍內找色
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="clr">要找的顏色</param>
        /// <param name="similar">容錯值 取值0~255，數值越高效率越低，不建議超過50</param>
        /// <returns></returns>
        public static List<Point> FindColor_FromScreen(int left, int top, int width, int height, Color clr, int similar)
        {
            return FindColor(0, 0, width, height, CopyScreen(new Rectangle(left, top, width, height)), clr, similar);
        }

        /// <summary>
        /// 從整個螢幕內找色
        /// </summary>
        /// <param name="clr">要找的顏色</param>
        /// <param name="similar">容錯值 取值0~255，數值越高效率越低，不建議超過50</param>
        /// <returns></returns>
        public static List<Point> FindColor_FromScreen(Color clr, int similar)
        {
            return FindColor(0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, CopyScreen(System.Windows.Forms.Screen.PrimaryScreen.Bounds), clr, similar);
        }

        #endregion CopyScreen

        #region 私有方法

        private static unsafe int[,] GetPixelData(BitmapData P_Data, Color BackColor)
        {
            byte B = BackColor.B, G = BackColor.G, R = BackColor.R;
            int Width = P_Data.Width, Height = P_Data.Height;
            int P_stride = P_Data.Stride;
            IntPtr P_Iptr = P_Data.Scan0;
            byte* P_ptr;
            int[,] PixelData = new int[Width * Height, 2];
            int i = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    P_ptr = (byte*)((Int64)P_Iptr + P_stride * y + x * 3);
                    if (B == P_ptr[0] & G == P_ptr[1] & R == P_ptr[2])
                    {
                    }
                    else
                    {
                        PixelData[i, 0] = x;
                        PixelData[i, 1] = y;
                        i++;
                    }
                }
            }
            int[,] PixelData2 = new int[i, 2];
            Array.Copy(PixelData, PixelData2, i * 2);
            return PixelData2;
        }

        //找圖BGR比較
        private static unsafe bool ScanColor(byte b1, byte g1, byte r1, byte b2, byte g2, byte r2, int similar)
        {
            if ((Math.Abs(b1 - b2)) > similar) { return false; } //B
            if ((Math.Abs(g1 - g2)) > similar) { return false; } //G
            if ((Math.Abs(r1 - r2)) > similar) { return false; } //R
            return true;
        }

        #endregion 私有方法
    }
}