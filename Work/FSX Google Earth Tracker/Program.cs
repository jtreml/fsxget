using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Resources;

namespace Fsxget
{
	static class Program
	{
		private static Config config;
		private static ResourceManager resLang;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// Initialize the config object here because if initialized above with its 
			// declaration, in case of an exception, Visual Studio is unable to show where 
			// the exception occured.
			config = new Config();

			resLang = new ResourceManager("Fsxget.lang." + config.Language, System.Reflection.Assembly.GetExecutingAssembly());

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//Form frmMain = new Form1();
			//frmMain.Visible = false;
			Application.Run(new FsxgetForm());
		}

		public static Config Config
		{
			get
			{
				return config;
			}
		}

		public static String getText(String id)
		{
			return resLang.GetString(id);
		}
	}
}