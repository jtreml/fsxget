using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace Starter_App
{
    static class Program
    {
		static bool bFirstInstance = false;
		static Mutex mtxSingleInstance = new Mutex(true, "FSXGET Single Instance Mutex", out bFirstInstance);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			if (!bFirstInstance)
				return;
			else
				mtxSingleInstance.Close();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
#if DEBUG
            startInfo.WorkingDirectory = Application.StartupPath + "\\..\\..\\..\\FSX Google Earth Tracker\\bin\\Debug";
#else
            startInfo.WorkingDirectory = Application.StartupPath;
#endif
            startInfo.FileName = @"fsxget.exe";

            try
            {
                Process p = Process.Start(startInfo);
            }
            catch { }
        }
    }
}
