using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;



namespace ScreenCaptureDemo
{
    static class Program
    {
     
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            //bool createdNew;
            //mutex = new Mutex(true, "ScreenCaptureDemo", out createdNew);
            //if (createdNew)
            //    Application.Run(new Form1());
            //else
            //    MessageBox.Show("The Application is Running.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          
            bool CreateNew = true;
            using (Mutex mutex = new Mutex(true, "ScreenCaptureDemo", out CreateNew))
            {
                if (CreateNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    //var thread = new Thread(ThreadStarts);
                    //thread.TrySetApartmentState(ApartmentState.STA);
                    //thread.Start();
                    Application.Run(new Form1());
                   
                    
                }
                else
                {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            //setForegroundWindow(process.MainWindowHandle);
                            MessageBox.Show("The Application is Running.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                        }
                    }
                }
            }}
           
            public static void ThreadStarts()
            {
               
                    //timer2.Enabled = true;
                   Application.Run(new Form2());
                               
            }

           
        }
    }

