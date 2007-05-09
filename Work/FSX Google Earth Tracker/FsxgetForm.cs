using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Collections;
using System.Web;
using System.Xml;
using System.Reflection;
using Microsoft.Win32;

using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;


namespace Fsxget
{
	public partial class FsxgetForm : Form
	{
		#region Global Variables

        public FsxConnection fsxCon;
        public KmlFactory kmlFactory;
		private HttpServer httpServer;
        
		bool bClose = false;
		bool bConnected = false;
        bool bErrorOnLoad = false;

		Icon icEnabled, icDisabled, icConnected, icPaused;

		System.Object lockListenerControl = new System.Object();

		#endregion

		#region Form Functions


		public FsxgetForm()
		{
			//As this method doesn't start any other threads we don't need to lock 
			// anything here (especially not the config file xml document)

			InitializeComponent();

            Text = Program.Config.AssemblyTitle;

            fsxCon = new FsxConnection(this, false);
			httpServer = new HttpServer(50);

			httpServer.addPrefix("http://+:" + Program.Config[Config.SETTING.GE_SERVER_PORT]["Value"].IntValue.ToString() + "/");

            kmlFactory = new KmlFactory(ref fsxCon, ref httpServer);
            kmlFactory.CreateStartupKML(Program.Config.UserDataPath + "/pub/fsxget.kml");

            enableTrackerToolStripMenuItem.Checked = Program.Config[Config.SETTING.ENABLE_ON_STARTUP]["Enabled"].BoolValue;
            if (enableTrackerToolStripMenuItem.Checked)
            {
                fsxCon.Connect();
            }
            
//            fsxCon.GetSceneryObjects("c:\\fsxnavaids.kml");

//            fsxCon.AddFlightPlan(@"D:\Eigene Dateien\Flight Simulator X-Dateien\IFR Frankfurt Main to Stuttgart.PLN");

			timerIPAddressRefresh.Interval = 10000;

			if (!HttpListener.IsSupported)
			{
				MessageBox.Show(Program.getText("System_HTTPListenerNotSupported"), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				bErrorOnLoad = true;
				return;
			}

            icEnabled = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Fsxget.data.gfx.icons.tbenabled.ico"));
            icDisabled = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Fsxget.data.gfx.icons.tbdisabled.ico"));
            icConnected = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Fsxget.data.gfx.icons.tbconnected.ico"));
            icPaused = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Fsxget.data.gfx.icons.tbpaused.ico"));

            notifyIconMain.Icon = icEnabled;
			notifyIconMain.Text = this.Text;
			notifyIconMain.Visible = true;

			// Set GUI caption
			updateCaptions();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
            if (bErrorOnLoad)
				return;
		}

		private void Form1_Shown(object sender, EventArgs e)
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


		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!bClose)
			{
				e.Cancel = true;
				safeHideMainDialog();
			}
		}


		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (bErrorOnLoad)
				return;

			// Stop server
			lock (lockListenerControl)
			{
				httpServer.stop();
				timerIPAddressRefresh.Stop();
			}
		}


		protected override void DefWndProc(ref Message m)
		{
            if (fsxCon == null || !fsxCon.OnMessageReceive(ref m) )
				base.DefWndProc(ref m);
		}


		#endregion

		#region Helper Functions

        public void NotifyError(String strError)
        {
            MessageBox.Show(strError, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

		private void safeShowBalloonTip(int timeout, String tipTitle, String tipText, ToolTipIcon tipIcon)
		{
		}

		private void safeShowMainDialog(int iTab)
		{
		}

		private void safeHideMainDialog()
		{
		}

		private bool IsLocalHostIP(IPAddress ipaRequest)
		{
            return true;
		}

		#endregion


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

		private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			safeShowMainDialog(0);
		}

        private void runMicrosoftFlightSimulatorXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Program.Config.CanRunFSX)
            {
                try
                {
                    System.Diagnostics.Process.Start(Program.Config.FSXPath);
                }
                catch
                {
                    MessageBox.Show("An error occured while trying to start Microsoft Flight Simulator X.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void runGoogleEarthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Program.Config.CanRunGE)
            {
                try
                {
                    lock (lockListenerControl)
                    {
                        System.Diagnostics.Process.Start(Program.Config.UserDataPath + "\\pub\\fsxget.kml");
                    }
                }
                catch
                {
                    MessageBox.Show("An error occured while trying to start Google Earth.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void createGoogleEarthKMLFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialogKMLFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    kmlFactory.CreateStartupKML(saveFileDialogKMLFile.FileName);
                }
                catch
                {
                    MessageBox.Show("Could not save KML file!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void clearUserAircraftPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fsxCon.objUserAircraft.objPath != null)
                fsxCon.objUserAircraft.objPath.Clear();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        #endregion

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

		private void timerIPAddressRefresh_Tick(object sender, EventArgs e)
		{

		}

		public void updateCaptions()
		{
			exitToolStripMenuItem.Text = Program.getText("Form_Main_ContextMenu_Exit");
			aboutToolStripMenuItem.Text = Program.getText("Form_Main_ContextMenu_About");

			optionsToolStripMenuItem.Text = Program.getText("Form_Main_ContextMenu_Options");

			clearUserAircraftPathToolStripMenuItem.Text = Program.getText("Form_Main_ContextMenu_ClearPath");
			recreateGoogleEarthObjectsToolStripMenuItem.Text = Program.getText("Form_Main_ContextMenu_RecreateObjects");
			pauseToolStripMenuItem.Text = Program.getText("Form_Main_ContextMenu_Pause");

			createGoogleEarthKMLFileToolStripMenuItem.Text = Program.getText("Form_Main_ContextMenu_CreateKmlFile");

			runMicrosoftFlightSimulatorXToolStripMenuItem.Text = Program.getText("Form_Main_ContextMenu_RunFSX");
			runGoogleEarthToolStripMenuItem.Text = Program.getText("Form_Main_ContextMenu_RunGE");

			enableTrackerToolStripMenuItem.Text = Program.getText("Form_Main_ContextMenu_Enable");
		}
    }
}