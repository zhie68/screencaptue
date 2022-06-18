using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ScreenCaptureDemo
{
    //class GDI
    //{

    //}
    public static class GDI
    {
        [DllImport("gdi32.dll", EntryPoint =  "CreateCompatibleDC",SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC([In] IntPtr hdC);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdobj);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(uint crColor);

        [DllImport("gdi32.dll")]
        public static extern bool Fillrgn(IntPtr hdc, IntPtr hrgn, IntPtr hbr);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRectRgn(int nLeftrect, int nTopRect, int nRightrec, int nBottomRect);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hgiobj);

       
    }
}
