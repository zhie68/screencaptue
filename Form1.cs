//DLL_API FIBITMAP *DLL_CALLCONV FreeImage_Load(FREE_IMAGE_FORMAT fif, const char *filename, int flag FI_DEFAULT(0));

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Design;
using System.Timers;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Management;
using FreeImageAPI.IO;
using FreeImageAPI;
using System.Configuration;



namespace ScreenCaptureDemo
{
    public partial class Form1 : Form
    {
       

        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int nXdest, int nyDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
        [DllImport("user32.dll", CharSet = CharSet.Auto,ExactSpelling=true)]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxcount);
        [DllImport("karnel32.dll")]
        public static extern bool FreeConsole();

        [DllImport("karnel32.dll")]
        static extern IntPtr GetConsole();
        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hWnd, int nCmdShow);

        [DllImport("karnel32.dll",SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool Alloccon();

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDIBSection(IntPtr hDC, IntPtr hdr, uint colors, ref IntPtr pBits, IntPtr hFile, uint offset);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDIB(IntPtr hDC,[In] ref BITMAPINFOHEADER pbmi, uint pila,out IntPtr ppvbits,IntPtr hsecsion, uint offset);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool setForegroundWindow(IntPtr hWnd);

        const int XPerlsPerMeter = 0xb12;
        const int YPerlsPerMeter = 0xb12;
        const int GPTR = 0x40;
        const int SRCCOPY = 0x00CC0020;
        const int SW_HIDE = 0;

        public static readonly System.Drawing.Imaging.Encoder ColorDepth;
        
        //public void hide()
        //{
        //    int hwnd;
        //    Process processrun = Process.GetCurrentProcess();
        //    hwnd = processrun.MainWindowHandle.ToInt32();
        //    ShowWindow(hwnd, SW_HIDE);
        //} 
       
        public Form1()
        {
            InitializeComponent();

            string PATH = @"D:\PICT";
            try
            {
                if (!Directory.Exists(PATH))
                {
                    Directory.CreateDirectory(PATH);
                }
            }
            catch
            {
                //fail
            }
            
        }
        
        

        // Hide Form
        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            Opacity = 0;
            base.OnLoad(e);      
           
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            int str = int.Parse(ConfigurationSettings.AppSettings["date"]);
            //int a = 0;
            //Parallel.Invoke(() => CaptureMyScreen(), () => delete());

            Thread t2 = new Thread(new ThreadStart(delete));
            Thread t1 = new Thread(new ThreadStart(CaptureWindow));
            t1.IsBackground = true;
            t1.Start();
            t2.Start();
            
        }
        
        
       
     
        
        public void CaptureWindow()
        {

            Process processrun = Process.GetCurrentProcess();
            Process.GetCurrentProcess().MaxWorkingSet = new IntPtr(2097152);
            IntPtr handle = User32.GetDesktopWindow();
            //get hdc of target window
            IntPtr hdcScr = User32.GetWindowDC(handle);
            // get size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;

            //create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcScr);
           
            //byte[] arraybyte = CreateBinary(hdcDest, height, width);
            //using get device cap to get width / height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcScr, width, height);

            //select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);

            //bitblt over
            
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcScr, 0, 0,GDI32.SRCCOPY);

            //restore selection
            GDI32.SelectObject(hdcDest, hOld);
            
          
            
            
            //Image img = Image.FromHbitmap(hBitmap);
            Bitmap f =Bitmap.FromHbitmap(hBitmap);
            
            GDI32.DeleteObject(hBitmap);
            //byte[] terv=CaptureScreen.GetControlBitArray(img);

            FIBITMAP dib = FreeImage.CreateFromBitmap(f);
           
            if (dib.IsNull)
            {
                //FreeImage.Unload(dib);
                
                //return f;
                Application.Restart();
                
            }
            dib = FreeImage.ConvertColorDepth(dib, FREE_IMAGE_COLOR_DEPTH.FICD_08_BPP, true);
            

            if (FreeImage.GetBPP(dib) == 8)
            {
                // Convert the FreeImage-Bitmap into a .NET bitmap
               
                string fileName = string.Format(@"D:\PICT\Cap " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".png");
                FreeImage.Save(FREE_IMAGE_FORMAT.FIF_PNG, dib, fileName, FREE_IMAGE_SAVE_FLAGS.PNG_Z_BEST_COMPRESSION);
                
                
            }
            FreeImage.UnloadEx(ref dib);
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcScr);
            f.Dispose();
            Console.WriteLine("Capture");
            //return f;

        }
        
        //screenshot
        private void capture2()
        {


            
            Process processrun = Process.GetCurrentProcess();
            Process.GetCurrentProcess().MaxWorkingSet = new IntPtr(2097152);
            
            Rectangle bound = SystemInformation.VirtualScreen;
            using (Bitmap temp = new Bitmap(bound.Width, bound.Height, PixelFormat.Format24bppRgb))
            {

                using (Graphics g = Graphics.FromImage(temp))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingMode = CompositingMode.SourceCopy;

                    //g.DrawImageUnscaled((Image)temp, 0, 0);
                    g.CopyFromScreen(0, 0, 0, 0, temp.Size);
                    g.Save();
                    //Thread.Sleep(50);
                    g.Dispose();
                    

                }
                
                FIBITMAP dib = FreeImage.CreateFromBitmap(temp);
                if (dib.IsNull)
                {
                    Application.Restart();
                }
                dib = FreeImage.ConvertColorDepth(dib, FREE_IMAGE_COLOR_DEPTH.FICD_08_BPP, true);

                if (FreeImage.GetBPP(dib) == 8)
                {
                    // Convert the FreeImage-Bitmap into a .NET bitmap
                    //Bitmap bitmap = FreeImage.GetBitmap(dib);
                    string fileName = string.Format(@"D:\PICT\Cap " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".png");
                    FreeImage.Save(FREE_IMAGE_FORMAT.FIF_PNG, dib, fileName, FREE_IMAGE_SAVE_FLAGS.PNG_Z_BEST_COMPRESSION);
                    Console.WriteLine("Capture" + fileName);
                    //bitmap.Save(fileName, ImageFormat.Png);
                    
                    FreeImage.UnloadEx(ref dib);
                }
                temp.Dispose();
               
            }
        }

        
       
        //delete
        private static void delete()
        {
            Process processrun = Process.GetCurrentProcess();
            Process.GetCurrentProcess().MaxWorkingSet = new IntPtr(2097152);
            string rootFolder = @"D:\PICT\";
           

            DateTime adere = DateTime.Now.AddDays(-1); //satu hari delete
            DirectoryInfo di = new DirectoryInfo(rootFolder);
            
            string[] files = Directory.GetFiles(rootFolder);
           
            //foreach (string filename in Directory.EnumerateFiles(rootFolder))
            foreach (string filename in files)
            {
                string def = filename;
                
                DateTime dse = File.GetCreationTime(filename);
                
              

                if (dse < adere)
                {
                    File.Delete(def);
                    //CaptureMyScreen();
                   // Thread.Sleep(1000);
                    Console.WriteLine(def);
                    //listBox1.Items.Add("Delete" + def);
                }
                else
                {
                    break;
                }
            }
           // timer1.Enabled = false;
            Console.WriteLine("Delete");
        }
       
        private void timer1_Tick(object sender, EventArgs e)
        {
           
           //capture2();
           //CaptureMyScreen();
           CaptureWindow();
         
           DateTime sr = DateTime.Now;
           if (sr.Hour == 00 && sr.Minute == 00 && sr.Second == 00)
           {
               //timer2.Enabled = true;
               var thread = new Thread(Program.ThreadStarts);
               thread.TrySetApartmentState(ApartmentState.STA);
               thread.Start();
               
           }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {

            
            //delete();
            //DateTime sr = DateTime.Now;
            //if (sr.Hour == 00 && sr.Minute == 00 && sr.Second == 00)
            //{
            //   delete();
            //   Console.WriteLine("Delete");
            //}
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                //Hide();
            }
        }

        private class GDI32
        {
            public const int SRCCOPY = 0x00CC0020; //BITBLT DWROP PARAMETER
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXdest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);
            
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

           


           
        }
        private class User32
        {
            [StructLayoutAttribute(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
            [DllImport("user32.dll")]
            public static extern IntPtr GetDC(IntPtr hwnd);

            [DllImport("kernel32.dll")]
            public static extern IntPtr LocalAlloc(uint flags, uint cb);
            
            [DllImport("kernel32.dll")]
            public static extern IntPtr LocalFree(IntPtr hMem);
        }

        struct BITMAPFILEHEADER
        {
            public uint bfOffBits;
            public ushort bfReserved1;
            public ushort bfReserved2;
            public uint bfSize;
            public ushort bfType;
        }
        struct BITMAPINFOHEADER
        {
            public uint biSize;
            public ushort biBitCount;
            public ushort biPlanes;
            public uint biClrImportant;
            public uint biClrUsed;
            public uint biCompression;
            public int biHeight;
            
            public uint biSizeImage;
            public int biWidth;
            public int biXPelsMeter;
            public int biYPelsMeter;
        }
        
    }
}
