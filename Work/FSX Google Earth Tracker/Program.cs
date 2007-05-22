using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Resources;
using HardwareInfo;
using System.Threading;
using System.Reflection;

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
			// Send usage statistics on program start (in separate thread)
#if !DEBUG
			Thread thrStatisticsStart = new Thread(new ThreadStart(sendStatisticsOnStart));
			thrStatisticsStart.Priority = ThreadPriority.BelowNormal;
			thrStatisticsStart.Start();

			try
			{
#endif

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

#if !DEBUG
			}
			catch
			{
				// TODO: Inform user that an error has occured and information will be sent
				sendErrorData();
			}


			// Make sure the statistic have been sent
			thrStatisticsStart.Join();

			// Send statistics on program exit
			sendStatisticsOnExit();
#endif
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


		#region Statistic & Error Collecting

		static void sendStatisticsOnStart()
		{
			try
			{
				List<WebLoader.PostEntry> listSendData = getPostDataUniqueId();
				listSendData.Add(new WebLoader.PostEntry("FsxgetMessageType", "FsxgetStart"));

				sendData(listSendData);
			}
			catch
			{
				// TODO: Should make some kind of entry in a log file and then just exit the function
#if DEBUG
				throw new Exception("Send Startup Statistics Eception");
#endif
			}
		}

		static void sendStatisticsOnExit()
		{
			try
			{
				List<WebLoader.PostEntry> listSendData = getPostDataUniqueId();
				listSendData.Add(new WebLoader.PostEntry("FsxgetMessageType", "FsxgetExit"));

				sendData(listSendData);
			}
			catch
			{
				// TODO: Should make some kind of entry in a log file and then just exit the function
#if DEBUG
				throw new Exception("Send Exit Statistics Eception");
#endif
			}
		}

		static List<WebLoader.PostEntry> getPostDataUniqueId()
		{
			List<WebLoader.PostEntry> listPostData = new List<WebLoader.PostEntry>(5);

			// Send CPU IDs
			List<String> listCpuIds = HardwareInfo.HardwareInfo.CpuIds;
			int iCount = 0;
			foreach (String szLoop in listCpuIds)
			{
				iCount++;
				listPostData.Add(new WebLoader.PostEntry("CpuId" + iCount, Md5Worker.getMd5Hash(szLoop)));
			}

			// Send HDD ID's
			List<HardwareInfo.HardwareInfo.Hdd> listHdds = HardwareInfo.HardwareInfo.HddList;
			int iCount1 = 0;

			foreach (HardwareInfo.HardwareInfo.Hdd hddLoop in listHdds)
			{
				iCount1++;

				listPostData.Add(new WebLoader.PostEntry("Hdd" + iCount1, Md5Worker.getMd5Hash(hddLoop.Model)));

				int iCount2 = 0;
				foreach (HardwareInfo.HardwareInfo.Hdd.Drive drvLoop in hddLoop.Drives)
				{
					iCount2++;

					listPostData.Add(new WebLoader.PostEntry("Hdd" + iCount1 + "drive" + iCount2, Md5Worker.getMd5Hash(drvLoop.Serial)));
				}
			}

			return listPostData;
		}

		static void sendData(List<WebLoader.PostEntry> listSendData)
		{
			// Send data (please do not use POST but GET for the moment)
			WebLoader.WebLoader wlThis = new WebLoader.WebLoader();
			wlThis.getRawData("http://www.fsxget.com/incoming/log.php", listSendData, WebLoader.WebLoader.Method.GET);
		}

		static void sendErrorData()
		{
			try
			{
				List<WebLoader.PostEntry> listSendData = getPostDataUniqueId();
				listSendData.Add(new WebLoader.PostEntry("FsxgetMessageType", "FsxgetError"));
				listSendData.Add(new WebLoader.PostEntry("FsxgetStackTrace", Environment.StackTrace));

				sendData(listSendData);
			}
			catch
			{
				// TODO: Should make some kind of entry in a log file and then just exit the function
#if DEBUG
				throw new Exception("Send StackTrace On Error Eception");
#endif
			}

		}

		#endregion
	}
}