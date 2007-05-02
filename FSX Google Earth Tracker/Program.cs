using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Fsxget
{
	static class Program
	{
        private static Config config = new Config();
        /// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			
			//Form frmMain = new Form1();
			//frmMain.Visible = false;
			Application.Run(new FsxetForm());
		}

        public static Config Config
        {
            get
            {
                return config;
            }
        }
	}
}