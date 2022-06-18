using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            Opacity = 0;
            base.OnLoad(e);

            delete();
            this.Close();
        }

        private void delete()
        {
            Process processrun = Process.GetCurrentProcess();
            Process.GetCurrentProcess().MaxWorkingSet = new IntPtr(2097152);
            string rootFolder = @"D:\PICT\";

            int str = int.Parse(ConfigurationSettings.AppSettings["date"]);
            DateTime adere = DateTime.Now.AddDays(-str); //satu hari delete
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
                    
                    Console.WriteLine(def);
                }
                else
                {
                    break;
                }
            }
            //timer1.Enabled = false;
            Console.WriteLine("Delete");
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //delete();
            //this.Close();
            Console.WriteLine("12");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("12");
        }
    }
}
