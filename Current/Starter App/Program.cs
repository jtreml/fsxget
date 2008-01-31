using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace Starter_App
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
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
