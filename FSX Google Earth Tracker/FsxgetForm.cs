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
        
		bool bClose = false;
		bool bConnected = false;
        bool bErrorOnLoad = false;

		Icon icEnabled, icDisabled, icConnected, icPaused;

		HttpListener listener;
		System.Object lockListenerControl = new System.Object();

		#endregion

		#region Form Functions


		public FsxgetForm()
		{
			//As this method doesn't start any other threads we don't need to lock anything here (especially not the config file xml document)

			InitializeComponent();

            Text = Program.Config.AssemblyTitle;
            fsxCon = new FsxConnection(this, false);
            kmlFactory = new KmlFactory(ref fsxCon);
            kmlFactory.CreateStartupKML(Program.Config.UserDataPath + "/pub/fsxget.kml");

            enableTrackerToolStripMenuItem.Checked = Program.Config[Config.SETTING.ENABLE_ON_STARTUP]["Enabled"].BoolValue;
            if (enableTrackerToolStripMenuItem.Checked)
            {
                fsxCon.Connect();
            }
            
//            File.WriteAllText( "c:\\test.kml", kmlFactory.GenVorKML( 8.58423605561256, 48.9929445460439, 0 ), Encoding.UTF8 );
//            fsxCon.GetSceneryObjects();

//            fsxCon.AddFlightPlan(@"D:\Eigene Dateien\Flight Simulator X-Dateien\VFR Munich to Frankfurt Main.PLN");

			timerIPAddressRefresh.Interval = 10000;

			if (!HttpListener.IsSupported)
			{
				MessageBox.Show("This program requires Windows XP SP2 or Server 2003 with the latest version of the .NET framework. Aborting!", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				bErrorOnLoad = true;
				return;
			}

			listener = new HttpListener();
			listener.Prefixes.Add("http://+:" + Program.Config[Config.SETTING.GE_SERVER_PORT]["Value"].IntValue.ToString() + "/");

            icEnabled = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Fsxget.data.gfx.icons.tbenabled.ico"));
            icDisabled = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Fsxget.data.gfx.icons.tbdisabled.ico"));
            icConnected = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Fsxget.data.gfx.icons.tbconnected.ico"));
            icPaused = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Fsxget.data.gfx.icons.tbpaused.ico"));

            notifyIconMain.Icon = icEnabled;
			notifyIconMain.Text = this.Text;
			notifyIconMain.Visible = true;

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
					listener.Start();
					listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
					//bServerUp = true;
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
				//bServerUp = false;

				listener.Stop();
				listener.Abort();

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

		#region Server
		public void ListenerCallback(IAsyncResult result)
		{
			lock (lockListenerControl)
			{
				HttpListener listener = (HttpListener)result.AsyncState;

				if (!listener.IsListening)
					return;

				HttpListenerContext context = listener.EndGetContext(result);

				HttpListenerRequest request = context.Request;
				HttpListenerResponse response = context.Response;


				// This code using the objects IsLocal property doesn't work for some reason ...

				//if (gconffixCurrent.uiServerAccessLevel == 0 && !request.IsLocal)
				//{
				//    response.Abort();
				//    return;
				//}

				// ... so I'm using my own code.

				if (!IsLocalHostIP(request.RemoteEndPoint.Address))
				{
					response.Abort();
					return;
				}


				byte[] buffer = System.Text.Encoding.UTF8.GetBytes("");
				String szHeader = "";
				bool bContentSet = false;

                String strRequest = request.Url.PathAndQuery.ToLower();

                if (strRequest.StartsWith("/gfx"))
                {
                    buffer = kmlFactory.GetImage(strRequest);
                    szHeader = "image/png";
                    bContentSet = true;
                }
                else if (strRequest == "/fsxobjs.kml")
                {
                    bContentSet = true;
                    String str = kmlFactory.GenFSXObjects();
                    szHeader = "application/vnd.google-earth.kml+xml";
                    buffer = System.Text.Encoding.UTF8.GetBytes(str);
                }
                else if (strRequest == "/fsxuu.kml")
                {
                    bContentSet = true;
                    String str = kmlFactory.GenUserPositionUpdate();
                    szHeader = "application/vnd.google-earth.kml+xml";
                    buffer = System.Text.Encoding.UTF8.GetBytes(str);
                }
                else if (strRequest == "/fsxaipu.kml")
                {
                    bContentSet = true;
                    String str = kmlFactory.GenAIAircraftUpdate();
                    szHeader = "application/vnd.google-earth.kml+xml";
                    buffer = System.Text.Encoding.UTF8.GetBytes(str);
                }
                else if (strRequest == "/fsxaihu.kml")
                {
                    bContentSet = true;
                    String str = kmlFactory.GenAIHelicpoterUpdate();
                    szHeader = "application/vnd.google-earth.kml+xml";
                    buffer = System.Text.Encoding.UTF8.GetBytes(str);
                }
                else if (strRequest == "/fsxaibu.kml")
                {
                    bContentSet = true;
                    String str = kmlFactory.GenAIBoatUpdate();
                    szHeader = "application/vnd.google-earth.kml+xml";
                    buffer = System.Text.Encoding.UTF8.GetBytes(str);
                }
                else if (strRequest == "/fsxaigu.kml")
                {
                    bContentSet = true;
                    String str = kmlFactory.GenAIGroundUnitUpdate();
                    szHeader = "application/vnd.google-earth.kml+xml";
                    buffer = System.Text.Encoding.UTF8.GetBytes(str);
                }
                else if (strRequest == "/fsxpu.kml")
                {
                    bContentSet = true;
                    String str = kmlFactory.GenUserPath();
                    szHeader = "application/vnd.google-earth.kml+xml";
                    buffer = System.Text.Encoding.UTF8.GetBytes(str);
                }
                else if (strRequest == "/fsxpreu.kml")
                {
                    bContentSet = true;
                    String str = kmlFactory.GenUserPrediction();
                    szHeader = "application/vnd.google-earth.kml+xml";
                    buffer = System.Text.Encoding.UTF8.GetBytes(str);
                }
                else if (strRequest == "/fsxfpu.kml")
                {
                    bContentSet = true;
                    String str = kmlFactory.GenFlightplanUpdate();
                    szHeader = "application/vnd.google-earth.kml+xml";
                    buffer = System.Text.Encoding.UTF8.GetBytes(str);
                }
                else
                    bContentSet = false;

				if (bContentSet)
				{
					response.AddHeader("Content-type", szHeader);
					response.ContentLength64 = buffer.Length;
					System.IO.Stream output = response.OutputStream;
					output.Write(buffer, 0, buffer.Length);
					output.Close();
				}
				else
				{
					response.StatusCode = 404;
					response.Close();
				}

				listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
			}
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

    }
}