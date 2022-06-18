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
using System.Runtime.Serialization;
using System.Management;

namespace ScreenCaptureDemo
{
    class CaptureScreen
    {
        #region Fields;
        private const int GPTR = 0x40;
        private const int PelsPerMeter = 0xb12;
        private const int SRCCOPY = 0x00cc0020;

        #endregion Fields

        #region Methods;
        public static void SaveScreenToFile(string filename)
        {
            //Control g = new Control();
            //GetControlBitArray(g);
            byte[] bitmapdata = GetScreenBitmapArray();
            FileStream fs = new FileStream(filename, FileMode.Create);
            fs.Write(bitmapdata, 0, bitmapdata.Length);
            fs.Flush();
            fs.Close();
        }

        [DllImport("gdi32.dll")]
        private static extern int BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSRC, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDIBSection(IntPtr hDC, IntPtr hdr, uint colors, ref IntPtr pBits, IntPtr hFile, uint offset);

        [DllImport("gdi32.dll")]
        private static extern void DeleteDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern void DeleteObject(IntPtr hObj);

        [DllImport("user32.dll")]
        private static extern IntPtr GetCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        public static extern IntPtr LocalAlloc(uint flags, int cb);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("user32.dll")]
        private static extern void ReleaseDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObj);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

      

        public static byte[] GetControlBitArray(IntPtr handle)
        {
            //control.Capture = true;
            IntPtr hWnd = GetCapture();
            IntPtr hdcScr = GetWindowDC(handle);
            //control.Capture = false;
            RECT windowRect = new RECT();
            GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            IntPtr hBitmapx= CreateCompatibleBitmap(hdcScr, width, height);
            IntPtr hDC = GetDC(hWnd);
            IntPtr hMemoryDC = CreateCompatibleDC(hdcScr);
            IntPtr hPreviousBitmapg = SelectObject(hMemoryDC, hBitmapx);
            Image img = Image.FromHbitmap(hPreviousBitmapg);
            BITMAPINFOHEADER bih = new BITMAPINFOHEADER();
            bih.biSize = Marshal.SizeOf(bih);
            bih.biBitCount = 24;
            bih.biClrUsed = 0;
            bih.biClrImportant = 0;
            bih.biHeight = height;
            bih.biWidth = width;
            bih.biPlanes = 1;

            int cb = (int)(bih.biHeight * bih.biWidth * bih.biBitCount / 800);
            bih.biSizeImage = cb;
            bih.biXPelsMeter = PelsPerMeter;
            bih.biYPelsMeter = PelsPerMeter;

            IntPtr pBits = IntPtr.Zero;
            IntPtr pBIH = LocalAlloc(GPTR, bih.biSize);
            Marshal.StructureToPtr(bih, pBIH, false);
            IntPtr hBitmap = CreateDIBSection(hdcScr, pBIH, 0, ref pBits, IntPtr.Zero, 0);

            BITMAPINFOHEADER bihMem = (BITMAPINFOHEADER)Marshal.PtrToStructure(pBIH, typeof(BITMAPINFOHEADER));
            IntPtr hPreviousBitmap = SelectObject(hMemoryDC, hBitmap);
            //Image imgv = Image.FromHbitmap(hPreviousBitmap);
            BitBlt(hMemoryDC, 0, 0, bih.biWidth, bih.biHeight, hdcScr, 0, 0, SRCCOPY);

            byte[] bits = new byte[cb];
            IntPtr sf = Marshal.AllocHGlobal(cb);
            
            Marshal.Copy(sf, bits, 0, bits.Length);

            BITMAPFILEHEADER bfh = new BITMAPFILEHEADER();
            bfh.bfSize = (uint)cb + 0x36;
            bfh.bfType = 0x4d42;
            bfh.bfOffBits = 0x36;
            int headerSize = 14;
            byte[] header = new byte[headerSize];
            BitConverter.GetBytes(bfh.bfType).CopyTo(header, 0);
            BitConverter.GetBytes(bfh.bfSize).CopyTo(header, 2);
            BitConverter.GetBytes(bfh.bfOffBits).CopyTo(header, 10);
            byte[] data = new byte[cb + bfh.bfOffBits];
            header.CopyTo(data, 0);
            header = new byte[Marshal.SizeOf(bih)];
            IntPtr pHeader = LocalAlloc(GPTR, Marshal.SizeOf(bih));
            Marshal.StructureToPtr(bihMem, pHeader, false);
            Marshal.Copy(pHeader, header, 0, Marshal.SizeOf(bih));
            LocalFree(pHeader);
            header.CopyTo(data, headerSize);
            bits.CopyTo(data, (int)bfh.bfOffBits);

            MemoryStream ms = new MemoryStream(data);
            //Image mf = Image.FromStream(ms);
            DeleteObject(SelectObject(hMemoryDC, hPreviousBitmap));
            DeleteDC(hMemoryDC);
            //ReleaseDC(hDC);

            return data;
        }

        private static byte[] GetScreenBitmapArray()
        {
            IntPtr hDC = GetDC(IntPtr.Zero);
            IntPtr hMemoryDC = CreateCompatibleDC(hDC);


            BITMAPINFOHEADER bih = new BITMAPINFOHEADER();
            bih.biSize = Marshal.SizeOf(bih);
            bih.biBitCount = 8;
            bih.biClrUsed = 0;
            bih.biClrImportant = 0;
            bih.biCompression = 0;
            bih.biHeight = Screen.PrimaryScreen.Bounds.Height;
            bih.biWidth = Screen.PrimaryScreen.Bounds.Width;
            bih.biPlanes = 1;
            int cb = (int)(bih.biHeight * bih.biWidth * bih.biBitCount / 24);
            bih.biSizeImage = cb;
            bih.biXPelsMeter = PelsPerMeter;
            bih.biYPelsMeter = PelsPerMeter;

            IntPtr pBits = IntPtr.Zero;
            IntPtr[] array = new IntPtr[]
            {
                Marshal.AllocHGlobal(cb),
            Marshal.AllocHGlobal(2)
            };
            IntPtr pBIH = LocalAlloc(GPTR, bih.biSize);
            Marshal.StructureToPtr(bih, pBIH, false);
            IntPtr hBitmap = CreateDIBSection(hDC, pBIH, 0, ref pBits, IntPtr.Zero, 0);

            BITMAPINFOHEADER bihMem = (BITMAPINFOHEADER)Marshal.PtrToStructure(pBIH, typeof(BITMAPINFOHEADER));
            IntPtr hPreviousBitmap = SelectObject(hMemoryDC, hBitmap);
            BitBlt(hMemoryDC, 0, 0, bih.biWidth, bih.biHeight, hDC, 0, 0, SRCCOPY);
            //Image img = Image.FromHbitmap(hBitmap);
            byte[] bits = new byte[cb];

            Marshal.Copy(array[0], bits, 0, bits.Length);
            

            BITMAPFILEHEADER bfh = new BITMAPFILEHEADER();
            bfh.bfSize = (uint)cb + 0x36;
            bfh.bfType = 0x4d42;
            bfh.bfOffBits = 0x36;
            int headerSize = 14;
            byte[] header = new byte[headerSize];
            BitConverter.GetBytes(bfh.bfType).CopyTo(header, 0);
            BitConverter.GetBytes(bfh.bfSize).CopyTo(header, 2);
            BitConverter.GetBytes(bfh.bfOffBits).CopyTo(header, 10);
            byte[] data = new byte[cb + bfh.bfOffBits];
            header.CopyTo(data, 0);
            header = new byte[Marshal.SizeOf(bih)];
            IntPtr pHeader = LocalAlloc(GPTR, Marshal.SizeOf(bih));
            Marshal.StructureToPtr(bihMem, pHeader, false);
            Marshal.Copy(pHeader, header, 0, Marshal.SizeOf(bih));
            LocalFree(pHeader);
            header.CopyTo(data, headerSize);
            bits.CopyTo(data, (int)bfh.bfOffBits);

            DeleteObject(SelectObject(hMemoryDC, hPreviousBitmap));
            DeleteDC(hMemoryDC);
            //ReleaseDC(hDC);

            return data;
        }

       
        #endregion Methods

        #region Nested Types
        private struct BITMAP
        {
            #region Fields

            public byte[] bmBits;
            public ushort bmBitPixel;
            public int bmHeight;
            public ushort bmPlanes;
            public int bmType;
            public int bmWidth;
            public int bmWidthBytes;

            #endregion Fields

        }
        private struct BITMAPFILEHEADER
        {
            #region Fields
            
            public uint bfOffBits;
            public ushort bfReserved1;
            public ushort bfReserved2;
            public uint bfSize;
            public ushort bfType;

            #endregion Fields
        }
        private struct BITMAPINFO
        {
            #region Fields
            
            public RGBQUAD[] bmiColors;
            public BITMAPINFOHEADER bmiHeader;

            #endregion Fields
        }

        private struct BITMAPINFOHEADER
        {
            #region Fields
            public ushort biBitCount;
            public uint biClrImportant;
            public uint biClrUsed;
            public int biCompression;
            public int biHeight;
            public ushort biPlanes;
            public int biSize;
            public int biSizeImage;
            public int biWidth;
            public int biXPelsMeter;
            public int biYPelsMeter;
            #endregion Fields
        }
        private struct RGBQUAD
        {
            #region Fields
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
            #endregion Fields
        }
        #endregion Nested Types

        private static Hashtable m_knowColor = new Hashtable((int)Math.Pow(2, 20), 1.0f);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Bitmap ConvertTo8bit(Bitmap bmpSource)
        {
            int imageWidth = bmpSource.Width;
            int imageHeight = bmpSource.Height;

            Bitmap bmpDest = null;
            BitmapData bmpDataDest = null;
            BitmapData bmpDataSource = null;

            try
            {
                bmpDest = new Bitmap(imageWidth, imageHeight, PixelFormat.Format8bppIndexed);

                bmpDataDest = bmpDest.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.ReadWrite, bmpDest.PixelFormat);

                bmpDataSource = bmpSource.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.ReadOnly, bmpSource.PixelFormat);

                int pixelSize = GetPixelInfoSize(bmpDataSource.PixelFormat);
                byte[] buffer = new byte[imageWidth * imageHeight * pixelSize];
                byte[] desbuffer = new byte[imageWidth * imageHeight];

                ReadBmpData(bmpDataSource, buffer, pixelSize, imageWidth, imageHeight);

                MathColor(buffer, desbuffer, pixelSize, bmpDest.Palette);

                WriteBmpData(bmpDataDest, desbuffer, imageWidth, imageHeight);

                return bmpDest;
                
            }
            finally
            {
                if (bmpDest != null) bmpDest.UnlockBits(bmpDataDest);
                if (bmpSource != null) bmpSource.UnlockBits(bmpDataSource);
 
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="desbuffer"></param>
        /// <param name="pixelSize"></param>
        /// <param name="pallete"></param>
        public static void MathColor(byte[] buffer,byte[] desbuffer,int pixelSize, ColorPalette pallete)
        {
            int length = desbuffer.Length;
            byte[] temp = new byte[pixelSize];

            int palleteSize = pallete.Entries.Length;

            int mult_1 = 256;
            int mult_2 = 256 * 256;

            int currentKey = 0;

            for (int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * pixelSize, temp, 0, pixelSize);

                currentKey = temp[0] + temp[1] * mult_1 + temp[2] * mult_2;

                if (!m_knowColor.ContainsKey(currentKey))
                {
                    desbuffer[i] = GetSimilarColor(pallete, temp, palleteSize);
                    m_knowColor.Add(currentKey, desbuffer[i]);

                }
                else
                {
                    desbuffer[i] = (byte)m_knowColor[currentKey];
                }
            }
        }

        public static int GetPixelInfoSize(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Format24bppRgb:
                    {
                        return 3;
                    }
                default:
                    {
                        throw new ApplicationException("only 24bit color supported now");
                    }

            }
        }

        private static void ReadBmpData(BitmapData bmpDataSource, byte[] buffer, int pixelSize, int width, int height)
        {
            int addrStart = bmpDataSource.Scan0.ToInt32();

            for (int i = 0; i < height; i++)
            {
                IntPtr realbyteaddr = new IntPtr(addrStart + System.Convert.ToInt32(i * bmpDataSource.Stride));

                Marshal.Copy(realbyteaddr, buffer, (int)(i * width * pixelSize), (int)(width * pixelSize));
            }
        }
        private static void WriteBmpData(BitmapData bmpDataDest, byte[] destbuffer, int imageWidth, int imageHeight)
        {
            int addStart = bmpDataDest.Scan0.ToInt32();

            for (int i = 0; i < imageHeight; i++)
            {
                IntPtr realByteAddr = new IntPtr(addStart + System.Convert.ToInt32(i * bmpDataDest.Stride));

                Marshal.Copy(destbuffer, i * imageWidth, realByteAddr, imageHeight);
            }
        }

        private static byte GetSimilarColor(ColorPalette palette, byte[] color, int paletteSize)
        {
            byte minDiff = byte.MaxValue;
            byte index = 0;

            if (color.Length == 3)
            {
                for (int i = 0; i < paletteSize - 1; i++)
                {
                    byte currentDiff = GetMaxDiff(color, palette.Entries[i]);

                    if (currentDiff < minDiff)
                    {
                        minDiff = currentDiff;
                        index = (byte)i;
                    }
                }
            }
            else
            {
                throw new ApplicationException("only 24 bit color supported now");
            }

            return index;
        }

        private static byte GetMaxDiff(byte[] a, Color b)
        {
            byte bDiff = a[0] > b.B ? (byte)(a[0] - b.B) : (byte)(b.B - a[0]);

            byte gDiff = a[1] > b.G ? (byte)(a[1] - b.G) : (byte)(b.G - a[1]);

            byte rDiff = a[2] > b.R ? (byte)(a[2] - b.R) : (byte)(b.R - a[2]);

            byte max = bDiff > gDiff ? bDiff : gDiff;

            max = max > rDiff ? max : rDiff;

            return max;
        }
    }
}
