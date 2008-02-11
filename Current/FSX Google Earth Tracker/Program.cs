using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Management;
using System.Reflection;
using System.Net;

namespace FSX_Google_Earth_Tracker
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
			// If OS is before Windows XP, don't run at all
			if (System.Environment.OSVersion.Version.Major < 5)
			{
				MessageBox.Show("This application requires Windows XP or later. Aborting!", AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			// If OS is Windows XP, Server 2003, etc.
			else if (System.Environment.OSVersion.Version.Major == 5)
			{
				// If it's Windows 2000
				if (System.Environment.OSVersion.Version.Minor == 0)
				{
					MessageBox.Show("This application requires Windows XP or later. Aborting!", AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				// If it's Windows XP
				else if (System.Environment.OSVersion.Version.Minor == 1)
				{
					SelectQuery query = new SelectQuery("Win32_OperatingSystem");
					ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
					foreach (ManagementObject mo in searcher.Get())
					{
						// If Service Pack 2 is not installed
						if (int.Parse(mo["ServicePackMajorVersion"].ToString()) < 2)
						{
							MessageBox.Show("On Windows XP this application requires Service Pack 2 (or higher) to be installed. Aborting!", AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
							return;
						}
					}
				}
				// If it's anything else (not XP or 2000) check if HttpListener is supported
				else if (!HttpListener.IsSupported)
				{
					MessageBox.Show("Your system cannot run this application. It's designed for Windows XP or later. You may try to install the latest version of the .NET Framework and run the application again. If it still fails to start, switch to Windows XP or later please. Aborting!", AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}


			try
			{
				if (!bFirstInstance)
					return;

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				//Form frmMain = new Form1();
				//frmMain.Visible = false;

				Application.Run(new Form1());
			}
			finally
			{
				mtxSingleInstance.Close();
			}
		}

		public static string AssemblyTitle
		{
			get
			{
				// Get all Title attributes on this assembly
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				// If there is at least one Title attribute
				if (attributes.Length > 0)
				{
					// Select the first one
					AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
					// If it is not an empty string, return it
					if (titleAttribute.Title != "")
						return titleAttribute.Title;
				}
				// If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
				return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

	}
}