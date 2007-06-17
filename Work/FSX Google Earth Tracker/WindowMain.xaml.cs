using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Net;
using System.ComponentModel;
using System.Reflection;

namespace Fsxget
{
	/// <summary>
	/// Interaction logic for WindowMain.xaml
	/// </summary>

	public partial class WindowMain : System.Windows.Window
	{
		NotifyIcon notifyIconMain = new NotifyIcon();
		ToolStripMenuItem pauseToolStripMenuItem = new ToolStripMenuItem();
		ToolStripMenuItem enableTrackerToolStripMenuItem = new ToolStripMenuItem();


		#region Global Variables

		public FsxConnection fsxCon;
		public KmlFactory kmlFactory;
		private HttpServer httpServer;

		bool bClose = false;
		bool bConnected = false;
		bool bErrorOnLoad = false;

		System.Drawing.Icon icEnabled, icDisabled, icConnected, icPaused;

		System.Object lockListenerControl = new System.Object();

		#endregion


		#region Helper Functions

		public void NotifyError(String strError)
		{
			System.Windows.MessageBox.Show(strError, Title, MessageBoxButton.OK, MessageBoxImage.Error);
		}


		//private bool IsLocalHostIP(IPAddress ipaRequest)
		//{
		//    return true;
		//}

		#endregion


		public WindowMain()
		{
			InitializeComponent();


			#region Context Menu Definition

			enableTrackerToolStripMenuItem.Checked = true;
			enableTrackerToolStripMenuItem.CheckOnClick = true;
			enableTrackerToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			enableTrackerToolStripMenuItem.Name = "enableTrackerToolStripMenuItem";
			enableTrackerToolStripMenuItem.Text = "&Enable Tracker";
			enableTrackerToolStripMenuItem.Click += new System.EventHandler(this.enableTrackerToolStripMenuItem_Click);

			ToolStripSeparator test2ToolStripMenuItem = new ToolStripSeparator();
			test2ToolStripMenuItem.Name = "test2ToolStripMenuItem";

			ToolStripMenuItem runMicrosoftFlightSimulatorXToolStripMenuItem = new ToolStripMenuItem();
			runMicrosoftFlightSimulatorXToolStripMenuItem.Name = "runMicrosoftFlightSimulatorXToolStripMenuItem";
			runMicrosoftFlightSimulatorXToolStripMenuItem.Text = "Run Microsoft Flight Simulator X";
			runMicrosoftFlightSimulatorXToolStripMenuItem.Click += new System.EventHandler(this.runMicrosoftFlightSimulatorXToolStripMenuItem_Click);

			ToolStripMenuItem runGoogleEarthToolStripMenuItem = new ToolStripMenuItem();
			runGoogleEarthToolStripMenuItem.Name = "runGoogleEarthToolStripMenuItem";
			runGoogleEarthToolStripMenuItem.Text = "Run Google Earth 4";
			runGoogleEarthToolStripMenuItem.Click += new System.EventHandler(this.runGoogleEarthToolStripMenuItem_Click);

			ToolStripSeparator toolStripMenuItem4 = new ToolStripSeparator();
			toolStripMenuItem4.Name = "toolStripMenuItem4";

			ToolStripMenuItem createGoogleEarthKMLFileToolStripMenuItem = new ToolStripMenuItem();
			createGoogleEarthKMLFileToolStripMenuItem.Name = "createGoogleEarthKMLFileToolStripMenuItem";
			createGoogleEarthKMLFileToolStripMenuItem.Text = "Create Google Earth KML File";
			createGoogleEarthKMLFileToolStripMenuItem.Click += new System.EventHandler(this.createGoogleEarthKMLFileToolStripMenuItem_Click);

			ToolStripSeparator toolStripMenuItem3 = new ToolStripSeparator();
			toolStripMenuItem3.Name = "toolStripMenuItem3";

			pauseToolStripMenuItem.CheckOnClick = true;
			pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
			pauseToolStripMenuItem.Text = "Pause";
			pauseToolStripMenuItem.Click += new System.EventHandler(this.pauseToolStripMenuItem_Click);

			ToolStripMenuItem recreateGoogleEarthObjectsToolStripMenuItem = new ToolStripMenuItem();
			recreateGoogleEarthObjectsToolStripMenuItem.Name = "recreateGoogleEarthObjectsToolStripMenuItem";
			recreateGoogleEarthObjectsToolStripMenuItem.Text = "Recreate Google Earth Objects";
			recreateGoogleEarthObjectsToolStripMenuItem.Click += new System.EventHandler(this.recreateGoogleEarthObjectsToolStripMenuItem_Click);

			ToolStripMenuItem clearUserAircraftPathToolStripMenuItem = new ToolStripMenuItem();
			clearUserAircraftPathToolStripMenuItem.Name = "clearUserAircraftPathToolStripMenuItem";
			clearUserAircraftPathToolStripMenuItem.Text = "&Clear User Aircraft Path";
			clearUserAircraftPathToolStripMenuItem.Click += new System.EventHandler(this.clearUserAircraftPathToolStripMenuItem_Click);

			ToolStripSeparator toolStripMenuItem1 = new ToolStripSeparator();
			toolStripMenuItem1.Name = "toolStripMenuItem1";

			ToolStripMenuItem exitToolStripMenuItem = new ToolStripMenuItem();
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.Text = "E&xit";
			exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);


			ContextMenuStrip contextMenuStripNotifyIcon = new ContextMenuStrip();
			contextMenuStripNotifyIcon.Items.AddRange(
				new System.Windows.Forms.ToolStripItem[] {
					enableTrackerToolStripMenuItem,
					test2ToolStripMenuItem,
					runMicrosoftFlightSimulatorXToolStripMenuItem,
					runGoogleEarthToolStripMenuItem,
					toolStripMenuItem4,
					createGoogleEarthKMLFileToolStripMenuItem,
					toolStripMenuItem3,
					pauseToolStripMenuItem,
					recreateGoogleEarthObjectsToolStripMenuItem,
					clearUserAircraftPathToolStripMenuItem,
					toolStripMenuItem1,
					exitToolStripMenuItem
				});
			contextMenuStripNotifyIcon.Name = "contextMenuStripNotifyIcon";

			#endregion




			//System.Drawing.Icon icEnabled = new System.Drawing.Icon("../../data/gfx/icons/tbenabled.ico");

			notifyIconMain.ContextMenuStrip = contextMenuStripNotifyIcon;
			notifyIconMain.Text = Title;
			

			//As this method doesn't start any other threads we don't need to lock 
			// anything here (especially not the config file xml document)

			Title = App.Config.AssemblyTitle;

			fsxCon = new FsxConnection(this, false);
			httpServer = new HttpServer(50);

			httpServer.addPrefix("http://+:" + App.Config[Config.SETTING.GE_SERVER_PORT]["Value"].IntValue.ToString() + "/");

			kmlFactory = new KmlFactory(ref fsxCon, ref httpServer);
			kmlFactory.CreateStartupKML(App.Config.UserDataPath + "/pub/fsxget.kml");

			enableTrackerToolStripMenuItem.Checked = App.Config[Config.SETTING.ENABLE_ON_STARTUP]["Enabled"].BoolValue;
			if (enableTrackerToolStripMenuItem.Checked)
			{
				fsxCon.Connect();
			}

			//            fsxCon.GetSceneryObjects("c:\\fsxnavaids.kml");

			//            fsxCon.AddFlightPlan(@"D:\Eigene Dateien\Flight Simulator X-Dateien\IFR Frankfurt Main to Stuttgart.PLN");

			if (!HttpListener.IsSupported)
			{
				System.Windows.MessageBox.Show("This program requires Windows XP SP2 or Windows Server 2003 with the latest version of the .NET framework installed! The application will exit now.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
				bErrorOnLoad = true;
				return;
			}


			icEnabled = new System.Drawing.Icon(Assembly.GetCallingAssembly().GetManifestResourceStream("FSXGET.data.gfx.icons.tbenabled.ico"));
			icDisabled = new System.Drawing.Icon(Assembly.GetCallingAssembly().GetManifestResourceStream("FSXGET.data.gfx.icons.tbdisabled.ico"));
			icConnected = new System.Drawing.Icon(Assembly.GetCallingAssembly().GetManifestResourceStream("FSXGET.data.gfx.icons.tbconnected.ico"));
			icPaused = new System.Drawing.Icon(Assembly.GetCallingAssembly().GetManifestResourceStream("FSXGET.data.gfx.icons.tbpaused.ico"));


			notifyIconMain.Icon = icEnabled;
			notifyIconMain.Text = Title;
			notifyIconMain.Visible = true;

		}


		private void WindowMain_FormLoaded(object sender, EventArgs e)
		{
			if (bErrorOnLoad)
			{
				bClose = true;
				Close();
			}
			else
			{
				Hide();

				lock (lockListenerControl)
				{
					httpServer.start();
				}
			}
		}


		void WindowMain_FormClosing(object sender, CancelEventArgs e)
		{
			if (!bClose)
			{
				e.Cancel = true;
			}
		}


		private void WindowMain_FormClosed(object sender, EventArgs e)
		{
			if (bErrorOnLoad)
				return;

			// Stop server
			lock (lockListenerControl)
			{
				httpServer.stop();
			}
		}



		public bool Connected
		{
			get
			{
				return bConnected;
			}
			set
			{
				bConnected = value;
				if (bConnected)
					notifyIconMain.Icon = icConnected;
				else
					notifyIconMain.Icon = icEnabled;
			}
		}


		#region User Interface Handlers

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			bClose = true;
			Close();
		}

		private void runMicrosoftFlightSimulatorXToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (App.Config.CanRunFSX)
			{
				try
				{
					System.Diagnostics.Process.Start(App.Config.FSXPath);
				}
				catch
				{
					System.Windows.MessageBox.Show("An error occured while trying to start Microsoft Flight Simulator X.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
		}

		private void runGoogleEarthToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (App.Config.CanRunGE)
			{
				try
				{
					lock (lockListenerControl)
					{
						System.Diagnostics.Process.Start(App.Config.UserDataPath + "\\pub\\fsxget.kml");
					}
				}
				catch
				{
					System.Windows.MessageBox.Show("An error occured while trying to start Google Earth.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
		}

		private void createGoogleEarthKMLFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog saveFileDialogKMLFile = new SaveFileDialog();
			saveFileDialogKMLFile.Filter = "Google Earth Files|*.kml";
			saveFileDialogKMLFile.DefaultExt = "kml";

			if (saveFileDialogKMLFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				try
				{
					kmlFactory.CreateStartupKML(saveFileDialogKMLFile.FileName);
				}
				catch
				{
					System.Windows.MessageBox.Show("Could not save KML file!", Title, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void clearUserAircraftPathToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (fsxCon.objUserAircraft.objPath != null)
				fsxCon.objUserAircraft.objPath.Clear();
		}

		private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (bConnected)
			{
				notifyIconMain.Icon = pauseToolStripMenuItem.Checked ? icPaused : icConnected;
				fsxCon.EnableTimers(!pauseToolStripMenuItem.Checked);
			}
		}

		private void recreateGoogleEarthObjectsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			fsxCon.DeleteAllObjects();
		}

		private void enableTrackerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (enableTrackerToolStripMenuItem.Checked)
			{
				notifyIconMain.Icon = icEnabled;
				fsxCon.Connect();
			}
			else
			{
				fsxCon.Disconnect();
				notifyIconMain.Icon = icDisabled;
			}
		}

		#endregion


	}
}