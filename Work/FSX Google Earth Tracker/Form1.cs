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


namespace FSX_Google_Earth_Tracker
{
	public partial class Form1 : Form
	{
        //Config config;

		#region Global Variables

        bool bErrorOnLoad = false;

        String szAppPath = "";
        //String szCommonPath = "";
        String szUserAppPath = "";

		String szFilePathPub = "";
		String szFilePathData = "";
		String szServerPath = "";

		IPAddress[] ipalLocal1 = null;
		IPAddress[] ipalLocal2 = null;
		System.Object lockIPAddressList = new System.Object();

		string szPathGE, szPathFSX;
		const string szRegKeyRun = "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run";


		//const int iOnlineVersionCheckRawDataLength = 64;
		//WebRequest wrOnlineVersionCheck;
		//WebResponse wrespOnlineVersionCheck;
		//private byte[] bOnlineVersionCheckRawData = new byte[iOnlineVersionCheckRawDataLength];
		//String szOnlineVersionCheckData = "";


		XmlTextReader xmlrSeetingsFile;
		XmlTextWriter xmlwSeetingsFile;
		XmlDocument xmldSettings;

		GlobalFixConfiguration gconffixCurrent;

		GlobalChangingConfiguration gconfchCurrent;
		System.Object lockChConf = new System.Object();

		bool bClose = false;
		bool bConnected = false;
		//bool bServerUp = false;

		bool bRestartRequired = false;

		const int WM_USER_SIMCONNECT = 0x0402;
		SimConnect simconnect = null;

		Icon icActive, icDisabled, icReceive;


		HttpListener listener;
		System.Object lockListenerControl = new System.Object();


		uint uiUserAircraftID = 0;
		bool bUserAircraftIDSet = false;
		System.Object lockUserAircraftID = new System.Object();

		StructBasicMovingSceneryObject suadCurrent;
		System.Object lockKmlUserAircraft = new System.Object();

		String szKmlUserAircraftPath = "";
		System.Object lockKmlUserPath = new System.Object();

		PathPosition ppPos1, ppPos2;
		String szKmlUserPrediction = "";
		List<PathPositionStored> listKmlPredictionPoints;
		System.Object lockKmlUserPrediction = new System.Object();
		System.Object lockKmlPredictionPoints = new System.Object();

		DataRequestReturn drrAIPlanes;
		System.Object lockDrrAiPlanes = new System.Object();

		DataRequestReturn drrAIHelicopters;
		System.Object lockDrrAiHelicopters = new System.Object();

		DataRequestReturn drrAIBoats;
		System.Object lockDrrAiBoats = new System.Object();

		DataRequestReturn drrAIGround;
		System.Object lockDrrAiGround = new System.Object();

		List<ObjectImage> listIconsGE;
		List<ObjectImage> listImgUnitsAir, listImgUnitsWater, listImgUnitsGround;

		//List<FlightPlan> listFlightPlans;
		//System.Object lockFlightPlanList = new System.Object();

		byte[] imgNoImage;
        byte[] imgLogo;

		#endregion


		#region Structs & Enums


		enum DEFINITIONS
		{
			StructBasicMovingSceneryObject,
		};

		enum DATA_REQUESTS
		{
			REQUEST_USER_AIRCRAFT,
			REQUEST_USER_PATH,
			REQUEST_USER_PREDICTION,
			REQUEST_AI_HELICOPTER,
			REQUEST_AI_PLANE,
			REQUEST_AI_BOAT,
			REQUEST_AI_GROUND,
		};


		enum KML_FILES
		{
			REQUEST_USER_AIRCRAFT,
			REQUEST_USER_PATH,
			REQUEST_USER_PREDICTION,
			REQUEST_AI_HELICOPTER,
			REQUEST_AI_PLANE,
			REQUEST_AI_BOAT,
			REQUEST_AI_GROUND,
			REQUEST_FLIGHT_PLANS,
		};

		enum KML_ACCESS_MODES
		{
			MODE_SERVER,
			MODE_FILE_LOCAL,
			MODE_FILE_USERDEFINED,
		};

		enum KML_IMAGE_TYPES
		{
			AIRCRAFT,
			WATER,
			GROUND,
		};

		enum KML_ICON_TYPES
		{
			USER_AIRCRAFT_POSITION,
			USER_PREDICTION_POINT,
			AI_AIRCRAFT_PREDICTION_POINT,
			AI_HELICOPTER_PREDICTION_POINT,
			AI_BOAT_PREDICTION_POINT,
			AI_GROUND_PREDICTION_POINT,
			AI_AIRCRAFT,
			AI_HELICOPTER,
			AI_BOAT,
			AI_GROUND_UNIT,
			PLAN_VOR,
			PLAN_NDB,
			PLAN_USER,
			PLAN_PORT,
			PLAN_INTER,
			UNKNOWN,
		};


		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct StructBasicMovingSceneryObject
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public String szTitle;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public String szATCType;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public String szATCModel;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public String szATCID;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
			public String szATCAirline;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public String szATCFlightNumber;
			public double dLatitude;
			public double dLongitude;
			public double dAltitude;
			//public double dSpeedX;
			//public double dSpeedY;
			//public double dSpeedZ;
			public double dTime;
            public double dHeading;
		};


		struct DataRequestReturn
		{
			public List<DataRequestReturnObject> listFirst;
			public List<DataRequestReturnObject> listSecond;
			public uint uiLastEntryNumber;
			public uint uiCurrentDataSet;
			public bool bClearOnNextRun;
		};

		struct DataRequestReturnObject
		{
			public uint uiObjectID;
			public StructBasicMovingSceneryObject bmsoObject;
			public String szCoursePrediction;
			public PathPositionStored[] ppsPredictionPoints;
		}


		struct PathPosition
		{
			public bool bInitialized;
			public double dLat;
			public double dLong;
			public double dAlt;
			public double dTime;
		}

		struct PathPositionStored
		{
			public double dLat;
			public double dLong;
			public double dAlt;
			public double dTime;
        }


		struct ObjectImage
		{
			public String szTitle;
			public String szPath;
			public byte[] bData;
		};


		struct GlobalFixConfiguration
		{
			public bool bLoadKMLFile;
			//public bool bCheckForUpdates;

			public long iServerPort;
			public uint uiServerAccessLevel;
			public String szUserdefinedPath;

			public bool bQueryUserAircraft;
			public long iTimerUserAircraft;
			public bool bQueryUserPath;
			public long iTimerUserPath;

			public bool bUserPathPrediction;
			public long iTimerUserPathPrediction;
			public double[] dPredictionTimes;

			public bool bQueryAIObjects;

			public bool bQueryAIAircrafts;
			public long iTimerAIAircrafts;
			public long iRangeAIAircrafts;
			public bool bPredictAIAircrafts;
			public bool bPredictPointsAIAircrafts;

			public bool bQueryAIHelicopters;
			public long iTimerAIHelicopters;
			public long iRangeAIHelicopters;
			public bool bPredictAIHelicopters;
			public bool bPredictPointsAIHelicopters;

			public bool bQueryAIBoats;
			public long iTimerAIBoats;
			public long iRangeAIBoats;
			public bool bPredictAIBoats;
			public bool bPredictPointsAIBoats;

			public bool bQueryAIGroundUnits;
			public long iTimerAIGroundUnits;
			public long iRangeAIGroundUnits;
			public bool bPredictAIGroundUnits;
			public bool bPredictPointsAIGroundUnits;

			public long iUpdateGEUserAircraft;
			public long iUpdateGEUserPath;
			public long iUpdateGEUserPrediction;
			public long iUpdateGEAIAircrafts;
			public long iUpdateGEAIHelicopters;
			public long iUpdateGEAIBoats;
			public long iUpdateGEAIGroundUnits;

			//public bool bLoadFlightPlans;
		};

		struct GlobalChangingConfiguration
		{
			public bool bEnabled;
			public bool bShowBalloons;
		};


		struct ListBoxPredictionTimesItem
		{
			public double dTime;
			
			public override String ToString()
			{
				if (dTime < 60)
					return "ETA " + dTime + " sec";
				else
					return "ETA " + dTime / 60.0 + " min";
			}
		}

		//struct ListViewFlightPlansItem
		//{
		//    public String szName;
		//    public int iID;

		//    public override String ToString()
		//    {
		//        return szName.ToString();
		//    }
		//}

		//struct FlightPlan
		//{
		//    public int uiID;
		//    public String szName;
		//    public XmlDocument xmldPlan;
		//}

		#endregion



		#region Form Functions


		public Form1()
		{
			//As this method doesn't start any other threads we don't need to lock anything here (especially not the config file xml document)

			InitializeComponent();

			
			Text = AssemblyTitle;


			// Set data for the about page
			this.labelProductName.Text = AssemblyProduct;
			this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
			this.labelCopyright.Text = AssemblyCopyright;
			this.labelCompanyName.Text = AssemblyCompany;


            // Set file path
#if DEBUG
            szAppPath = Application.StartupPath + "\\..\\..";
            //szCommonPath = szAppPath + "\\Common Files Folder";
            szUserAppPath = szAppPath + "\\User's Application Data Folder";
#else
            szAppPath = Application.StartupPath;
			//szAppPath = Application.StartupPath + "\\..\\..";
            //szCommonPath = Application.CommonAppDataPath;
            szUserAppPath = Application.UserAppDataPath;
#endif
            //config = new Config();

			szFilePathPub = szAppPath + "\\pub";
			szFilePathData = szAppPath + "\\data";

            // Check if config file for current user exists
			if (!File.Exists(szUserAppPath + "\\settings.cfg"))
			{
				if (!Directory.Exists(szUserAppPath))
					Directory.CreateDirectory(szUserAppPath);

				File.Copy(szAppPath + "\\data\\settings.default", szUserAppPath + "\\settings.cfg");
			}

			// Load config file into memory
            xmlrSeetingsFile = new XmlTextReader(szUserAppPath + "\\settings.cfg");
			xmldSettings = new XmlDocument();
			xmldSettings.Load(xmlrSeetingsFile);
			xmlrSeetingsFile.Close();
			xmlrSeetingsFile = null;

			// Make sure we have a config file for the right version
			// (future version should contain better checks and update from old config files to new version)
			String szConfigVersion = "";
			bool bUpdate = false;
			try
			{
				szConfigVersion = xmldSettings["fsxget"]["settings"].Attributes["version"].Value.ToLower();
			}
			catch
			{
				bUpdate = true;
			}

			xmlrSeetingsFile = new XmlTextReader(szAppPath + "\\data\\settings.default");
			XmlDocument xmldSettingsDefault = new XmlDocument();
			xmldSettingsDefault.Load(xmlrSeetingsFile);
			xmlrSeetingsFile.Close();
			xmlrSeetingsFile = null;

			String szConfigDefaultVersion = xmldSettingsDefault["fsxget"]["settings"].Attributes["version"].Value.ToLower();

			if (bUpdate || !szConfigVersion.Equals(szConfigDefaultVersion))
			{
				try
				{
					File.Delete(szUserAppPath + "\\settings.cfg");
					File.Copy(szAppPath + "\\data\\settings.default", szUserAppPath + "\\settings.cfg");

					xmlrSeetingsFile = new XmlTextReader(szUserAppPath + "\\settings.cfg");
					xmldSettings = new XmlDocument();
					xmldSettings.Load(xmlrSeetingsFile);
					xmlrSeetingsFile.Close();
					xmlrSeetingsFile = null;
				}
				catch
				{
					MessageBox.Show("The config file for this program cannot be updated. Aborting!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
					bErrorOnLoad = true;
					return;
				}
			}



			// Mirror values we need from config file in memory to variables
			try
			{
				ConfigMirrorToVariables();
			}
			catch
			{
				MessageBox.Show("The config file for this program contains errors. Aborting!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				bErrorOnLoad = true;
				return;
			}


			// Update the notification icon context menu
			lock (lockChConf)
			{
				enableTrackerToolStripMenuItem.Checked = gconfchCurrent.bEnabled;
				showBalloonTipsToolStripMenuItem.Checked = gconfchCurrent.bShowBalloons;
			}


			// Set timer intervals
			timerFSXConnect.Interval = 3000;

			timerQueryUserAircraft.Interval = (int)gconffixCurrent.iTimerUserAircraft;
			timerQueryUserPath.Interval = (int)gconffixCurrent.iTimerUserPath;
			timerUserPrediction.Interval = (int)gconffixCurrent.iTimerUserPathPrediction;

			timerQueryAIAircrafts.Interval = (int)gconffixCurrent.iTimerAIAircrafts;
			timerQueryAIHelicopters.Interval = (int)gconffixCurrent.iTimerAIHelicopters;
			timerQueryAIBoats.Interval = (int)gconffixCurrent.iTimerAIBoats;
			timerQueryAIGroundUnits.Interval = (int)gconffixCurrent.iTimerAIGroundUnits;

			timerIPAddressRefresh.Interval = 10000;

			// Set server settings
			szServerPath = "http://+:" + gconffixCurrent.iServerPort.ToString();

			if (!HttpListener.IsSupported)
			{
				MessageBox.Show("This program requires Windows XP SP2 or Server 2003 with the latest version of the .NET framework. Aborting!", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				bErrorOnLoad = true;
				return;
			}

			listener = new HttpListener();
			listener.Prefixes.Add(szServerPath + "/");


			// Lookup (in the config file) and load program icons, google earth pins and object images
			try
			{
				// notification icons
				for (XmlNode xmlnTemp = xmldSettings["fsxget"]["gfx"]["program"]["icons"].FirstChild; xmlnTemp != null; xmlnTemp = xmlnTemp.NextSibling)
				{
					if (xmlnTemp.Attributes["Name"].Value == "Taskbar - Enabled")
						icActive = new Icon(szFilePathData + xmlnTemp.Attributes["Img"].Value);
					else if (xmlnTemp.Attributes["Name"].Value == "Taskbar - Disabled")
						icDisabled = new Icon(szFilePathData + xmlnTemp.Attributes["Img"].Value);
					else if (xmlnTemp.Attributes["Name"].Value == "Taskbar - Connected")
						icReceive = new Icon(szFilePathData + xmlnTemp.Attributes["Img"].Value);
				}

				notifyIconMain.Icon = icDisabled;
				notifyIconMain.Text = this.Text;
				notifyIconMain.Visible = true;


				// google earth icons
				listIconsGE = new List<ObjectImage>(xmldSettings["fsxget"]["gfx"]["ge"]["icons"].ChildNodes.Count);
				for (XmlNode xmlnTemp = xmldSettings["fsxget"]["gfx"]["ge"]["icons"].FirstChild; xmlnTemp != null; xmlnTemp = xmlnTemp.NextSibling)
				{
					ObjectImage imgTemp = new ObjectImage();
					imgTemp.szTitle = xmlnTemp.Attributes["Name"].Value;
					imgTemp.bData = File.ReadAllBytes(szFilePathPub + xmlnTemp.Attributes["Img"].Value);
					listIconsGE.Add(imgTemp);
				}


				// no-image image
				imgNoImage = File.ReadAllBytes(szFilePathPub + xmldSettings["fsxget"]["gfx"]["scenery"]["noimage"].Attributes["Img"].Value);
                imgLogo = File.ReadAllBytes(szFilePathPub + xmldSettings["fsxget"]["gfx"]["scenery"]["logo"].Attributes["Img"].Value);

				// object images
				listImgUnitsAir = new List<ObjectImage>(xmldSettings["fsxget"]["gfx"]["scenery"]["air"].ChildNodes.Count);
				for (XmlNode xmlnTemp = xmldSettings["fsxget"]["gfx"]["scenery"]["air"].FirstChild; xmlnTemp != null; xmlnTemp = xmlnTemp.NextSibling)
				{
					ObjectImage imgTemp = new ObjectImage();
					imgTemp.szTitle = xmlnTemp.Attributes["Name"].Value;
					imgTemp.szPath = xmlnTemp.Attributes["Img"].Value;
					imgTemp.bData = File.ReadAllBytes(szFilePathPub + xmlnTemp.Attributes["Img"].Value);
					listImgUnitsAir.Add(imgTemp);
				}

				listImgUnitsWater = new List<ObjectImage>(xmldSettings["fsxget"]["gfx"]["scenery"]["water"].ChildNodes.Count);
				for (XmlNode xmlnTemp = xmldSettings["fsxget"]["gfx"]["scenery"]["water"].FirstChild; xmlnTemp != null; xmlnTemp = xmlnTemp.NextSibling)
				{
					ObjectImage imgTemp = new ObjectImage();
					imgTemp.szTitle = xmlnTemp.Attributes["Name"].Value;
					imgTemp.szPath = xmlnTemp.Attributes["Img"].Value;
					imgTemp.bData = File.ReadAllBytes(szFilePathPub + xmlnTemp.Attributes["Img"].Value);
					listImgUnitsWater.Add(imgTemp);
				}

				listImgUnitsGround = new List<ObjectImage>(xmldSettings["fsxget"]["gfx"]["scenery"]["ground"].ChildNodes.Count);
				for (XmlNode xmlnTemp = xmldSettings["fsxget"]["gfx"]["scenery"]["ground"].FirstChild; xmlnTemp != null; xmlnTemp = xmlnTemp.NextSibling)
				{
					ObjectImage imgTemp = new ObjectImage();
					imgTemp.szTitle = xmlnTemp.Attributes["Name"].Value;
					imgTemp.szPath = xmlnTemp.Attributes["Img"].Value;
					imgTemp.bData = File.ReadAllBytes(szFilePathPub + xmlnTemp.Attributes["Img"].Value);
					listImgUnitsWater.Add(imgTemp);
				}
			}
			catch
			{
				MessageBox.Show("Could not load all graphics files probably due to errors in the config file. Aborting!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				bErrorOnLoad = true;
				return;
			}


			// Initialize some variables
			clearDrrStructure(ref drrAIPlanes);
			clearDrrStructure(ref drrAIHelicopters);
			clearDrrStructure(ref drrAIBoats);
			clearDrrStructure(ref drrAIGround);

			clearPPStructure(ref ppPos1);
			clearPPStructure(ref ppPos2);

			//listFlightPlans = new List<FlightPlan>();
			listKmlPredictionPoints = new List<PathPositionStored>(gconffixCurrent.dPredictionTimes.GetLength(0));


			// Test drive the following function which loads data from the config file, as it will be used regularly later on
			try
			{
				ConfigMirrorToForm();
			}
			catch
			{
				MessageBox.Show("The config file for this program contains errors. Aborting!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				bErrorOnLoad = true;
				return;
			}

			// Load FSX and Google Earth path from registry
			const string szRegKeyFSX = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Microsoft Games\\flight simulator\\10.0";
			const string szRegKeyGE = "HKEY_CLASSES_ROOT\\.kml";
//			const string szRegKeyGE2 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Google\\Google Earth Pro";

			szPathGE = (string)Registry.GetValue(szRegKeyGE, "", "");
			szPathFSX = (string)Registry.GetValue(szRegKeyFSX, "SetupPath", "");

			if (szPathGE != "")
			{
//				szPathGE += "\\googleearth.exe";
//				if (File.Exists(szPathGE))
				runGoogleEarthToolStripMenuItem.Enabled = true;
			}
			else
				runGoogleEarthToolStripMenuItem.Enabled = false;

			if (szPathFSX != "")
			{
				szPathFSX += "fsx.exe";
				if (File.Exists(szPathFSX))
					runMicrosoftFlightSimulatorXToolStripMenuItem.Enabled = true;
				else
					runMicrosoftFlightSimulatorXToolStripMenuItem.Enabled = false;
			}
			else
				runMicrosoftFlightSimulatorXToolStripMenuItem.Enabled = false;


			// Write Google Earth startup KML file
			String szTempKMLFile = "";
			String szTempKMLFileStatic = "";
			if (CompileKMLStartUpFileDynamic("localhost", ref szTempKMLFile) && CompileKMLStartUpFileStatic("localhost", ref szTempKMLFileStatic))
			{
				try
				{
					if (!Directory.Exists(szUserAppPath + "\\pub"))
						Directory.CreateDirectory(szUserAppPath + "\\pub");

					File.WriteAllText(szUserAppPath + "\\pub\\fsxgetd.kml", szTempKMLFile);
					File.WriteAllText(szUserAppPath + "\\pub\\fsxgets.kml", szTempKMLFileStatic);
				}
				catch
				{
					MessageBox.Show("Could not write KML file for google earth. Aborting!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
					bErrorOnLoad = true;
					return;
				}
			}
			else
			{
				MessageBox.Show("Could not write KML file for google earth. Aborting!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				bErrorOnLoad = true;
				return;
			}

			// Init some never-changing form fields
			textBoxLocalPubPath.Text = szFilePathPub;


			// Load flight plans
			//if (gconffixCurrent.bLoadFlightPlans)
			//    LoadFlightPlans();


			// Online Update Check
			//if (gconffixCurrent.bCheckForUpdates)
			//    checkForProgramUpdate();
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

				globalConnect();
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

			// Save xml document in memory to config file on disc
			xmlwSeetingsFile = new XmlTextWriter(szUserAppPath + "\\settings.cfg", null);
			xmldSettings.Save(xmlwSeetingsFile);
			xmlwSeetingsFile.Flush();
			xmlwSeetingsFile.Close();
			xmldSettings = null;

			// Disconnect with FSX
			globalDisconnect();

			// Stop server
			lock (lockListenerControl)
			{
				//bServerUp = false;

				listener.Stop();
				listener.Abort();

				timerIPAddressRefresh.Stop();
			}

			// Delete temporary KML file
			try
			{
				File.Delete(szFilePathPub + "\\fsxget.kml");
			}
			catch { }
		}


		protected override void DefWndProc(ref Message m)
		{
			if (m.Msg == WM_USER_SIMCONNECT)
			{
				if (simconnect != null)
				{
					try
					{
						simconnect.ReceiveMessage();
					}
					catch
					{
#if DEBUG
						safeShowBalloonTip(3, "Error", "Error receiving data from FSX!", ToolTipIcon.Error);
#endif
					}
				}
			}
			else
				base.DefWndProc(ref m);
		}


		#endregion


		#region FSX Connection


		private bool openConnection()
		{
			if (simconnect == null)
			{
				try
				{
					simconnect = new SimConnect(Text, this.Handle, WM_USER_SIMCONNECT, null, 0);
					if (initDataRequest())
						return true;
					else
						return false;
				}
				catch
				{
					return false;
				}
			}
			else
				return false;
		}

		private void closeConnection()
		{
			if (simconnect != null)
			{
				simconnect.Dispose();
				simconnect = null;
			}
		}

		private bool initDataRequest()
		{
			try
			{
				// listen to connect and quit msgs
				simconnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(simconnect_OnRecvOpen);
				simconnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(simconnect_OnRecvQuit);

				// listen to exceptions
				simconnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(simconnect_OnRecvException);

				// define a data structure
				simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "Title", null, SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
				simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "ATC Type", null, SIMCONNECT_DATATYPE.STRING32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
				simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "ATC Model", null, SIMCONNECT_DATATYPE.STRING32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
				simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "ATC ID", null, SIMCONNECT_DATATYPE.STRING32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
				simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "ATC Airline", null, SIMCONNECT_DATATYPE.STRING64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
				simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "ATC Flight Number", null, SIMCONNECT_DATATYPE.STRING32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
				simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "Plane Latitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
				simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "Plane Longitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
				simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "Plane Altitude", "meters", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
				//simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "Velocity World X", "meter per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
				//simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "Velocity World Y", "meter per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
				//simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "Velocity World Z", "meter per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
				simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "Absolute Time", "seconds", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "PLANE HEADING DEGREES TRUE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);

				// IMPORTANT: register it with the simconnect managed wrapper marshaller
				// if you skip this step, you will only receive a uint in the .dwData field.
				simconnect.RegisterDataDefineStruct<StructBasicMovingSceneryObject>(DEFINITIONS.StructBasicMovingSceneryObject);

				// catch a simobject data request
				simconnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(simconnect_OnRecvSimobjectDataBytype);

				return true;
			}
			catch (COMException ex)
			{
				safeShowBalloonTip(3000, Text, "FSX Exception!\n\n" + ex.Message, ToolTipIcon.Error);
				return false;
			}
		}


		void simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
		{
			lock (lockUserAircraftID)
			{
				bUserAircraftIDSet = false;
			}

			if (gconffixCurrent.bQueryUserAircraft)
				timerQueryUserAircraft.Start();

			if (gconffixCurrent.bQueryUserPath)
				timerQueryUserPath.Start();

			if (gconffixCurrent.bUserPathPrediction)
				timerUserPrediction.Start();

			if (gconffixCurrent.bQueryAIObjects)
			{
				if (gconffixCurrent.bQueryAIAircrafts)
					timerQueryAIAircrafts.Start();

				if (gconffixCurrent.bQueryAIHelicopters)
					timerQueryAIHelicopters.Start();

				if (gconffixCurrent.bQueryAIBoats)
					timerQueryAIBoats.Start();

				if (gconffixCurrent.bQueryAIGroundUnits)
					timerQueryAIGroundUnits.Start();
			}

			timerIPAddressRefresh_Tick(null, null);
			timerIPAddressRefresh.Start();
		}

		void simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
		{
			globalDisconnect();
		}

		void simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
		{
			safeShowBalloonTip(3000, Text, "FSX Exception!", ToolTipIcon.Error);
		}

		void simconnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
		{
			if (data.dwentrynumber == 0 && data.dwoutof == 0)
				return;

			DataRequestReturnObject drroTemp;
			drroTemp.bmsoObject = (StructBasicMovingSceneryObject)data.dwData[0];
			drroTemp.uiObjectID = data.dwObjectID;
			drroTemp.szCoursePrediction = "";
			drroTemp.ppsPredictionPoints = null;

			switch ((DATA_REQUESTS)data.dwRequestID)
			{
				case DATA_REQUESTS.REQUEST_USER_AIRCRAFT:
				case DATA_REQUESTS.REQUEST_USER_PATH:
				case DATA_REQUESTS.REQUEST_USER_PREDICTION:
					lock (lockUserAircraftID)
					{
						bUserAircraftIDSet = true;
						uiUserAircraftID = drroTemp.uiObjectID;
					}

					switch ((DATA_REQUESTS)data.dwRequestID)
					{
						case DATA_REQUESTS.REQUEST_USER_AIRCRAFT:
							lock (lockKmlUserAircraft)
							{
								suadCurrent = drroTemp.bmsoObject;
							}
							break;

						case DATA_REQUESTS.REQUEST_USER_PATH:
							lock (lockKmlUserPath)
							{
								szKmlUserAircraftPath += drroTemp.bmsoObject.dLongitude.ToString().Replace(",", ".") + "," + drroTemp.bmsoObject.dLatitude.ToString().Replace(",", ".") + "," + drroTemp.bmsoObject.dAltitude.ToString().Replace(",", ".") + "\n";
							}
							break;

						case DATA_REQUESTS.REQUEST_USER_PREDICTION:
							lock (lockKmlPredictionPoints)
							{

								if (!ppPos1.bInitialized)
								{
									ppPos1.dLong = drroTemp.bmsoObject.dLongitude;
									ppPos1.dLat = drroTemp.bmsoObject.dLatitude;
									ppPos1.dAlt = drroTemp.bmsoObject.dAltitude;
									ppPos1.dTime = drroTemp.bmsoObject.dTime;
                                    ppPos1.bInitialized = true;
									return;
								}
								else
								{
									if (!ppPos2.bInitialized)
									{
										ppPos2.dLong = drroTemp.bmsoObject.dLongitude;
										ppPos2.dLat = drroTemp.bmsoObject.dLatitude;
										ppPos2.dAlt = drroTemp.bmsoObject.dAltitude;
                                        ppPos2.dTime = drroTemp.bmsoObject.dTime;
                                        ppPos2.bInitialized = true;
									}
									else
									{
										ppPos1 = ppPos2;

										ppPos2.dLong = drroTemp.bmsoObject.dLongitude;
										ppPos2.dLat = drroTemp.bmsoObject.dLatitude;
										ppPos2.dAlt = drroTemp.bmsoObject.dAltitude;
                                        ppPos2.dTime = drroTemp.bmsoObject.dTime;
                                        //ppPos2.bInitialized = true;
									}
								}

								if (ppPos1.dTime != ppPos2.dTime && ppPos1.bInitialized && ppPos2.bInitialized)
								{
									lock (lockKmlUserPrediction)
									{
										szKmlUserPrediction = drroTemp.bmsoObject.dLongitude.ToString().Replace(",", ".") + "," + drroTemp.bmsoObject.dLatitude.ToString().Replace(",", ".") + "," + drroTemp.bmsoObject.dAltitude.ToString().Replace(",", ".") + "\n";
										listKmlPredictionPoints.Clear();

										for (uint n = 0; n < gconffixCurrent.dPredictionTimes.GetLength(0); n++)
										{
											double dLongNew = 0.0, dLatNew = 0.0, dAltNew = 0.0;
											calcPositionByTime(ref ppPos1, ref ppPos2, gconffixCurrent.dPredictionTimes[n], ref dLatNew, ref dLongNew, ref dAltNew);

											PathPositionStored ppsTemp;
											ppsTemp.dLat = dLatNew;
											ppsTemp.dLong = dLongNew;
											ppsTemp.dAlt = dAltNew;
											ppsTemp.dTime = gconffixCurrent.dPredictionTimes[n];
                                            listKmlPredictionPoints.Add(ppsTemp);

											szKmlUserPrediction += dLongNew.ToString().Replace(",", ".") + "," + dLatNew.ToString().Replace(",", ".") + "," + dAltNew.ToString().Replace(",", ".") + "\n";
										}
									}
								}
							}
							break;
					}
					break;

				case DATA_REQUESTS.REQUEST_AI_PLANE:
				case DATA_REQUESTS.REQUEST_AI_HELICOPTER:
				case DATA_REQUESTS.REQUEST_AI_BOAT:
				case DATA_REQUESTS.REQUEST_AI_GROUND:
					lock (lockUserAircraftID)
					{
						if (bUserAircraftIDSet && (drroTemp.uiObjectID == uiUserAircraftID))
							return;
					}

					switch ((DATA_REQUESTS)data.dwRequestID)
					{
						case DATA_REQUESTS.REQUEST_AI_PLANE:
							processDataRequestResultAlternatingly(data.dwentrynumber, data.dwoutof, ref drrAIPlanes, ref lockDrrAiPlanes, ref drroTemp, gconffixCurrent.bPredictAIAircrafts, gconffixCurrent.bPredictPointsAIAircrafts);
							break;

						case DATA_REQUESTS.REQUEST_AI_HELICOPTER:
							processDataRequestResultAlternatingly(data.dwentrynumber, data.dwoutof, ref drrAIHelicopters, ref lockDrrAiHelicopters, ref drroTemp, gconffixCurrent.bPredictAIHelicopters, gconffixCurrent.bPredictPointsAIHelicopters);
							break;

						case DATA_REQUESTS.REQUEST_AI_BOAT:
							processDataRequestResultAlternatingly(data.dwentrynumber, data.dwoutof, ref drrAIBoats, ref lockDrrAiBoats, ref drroTemp, gconffixCurrent.bPredictAIBoats, gconffixCurrent.bPredictPointsAIBoats);
							break;

						case DATA_REQUESTS.REQUEST_AI_GROUND:
							processDataRequestResultAlternatingly(data.dwentrynumber, data.dwoutof, ref drrAIGround, ref lockDrrAiGround, ref drroTemp, gconffixCurrent.bPredictAIGroundUnits, gconffixCurrent.bPredictPointsAIGroundUnits);
							break;
					}
					break;

				default:
#if DEBUG
					safeShowBalloonTip(3000, Text, "Received unknown data from FSX!", ToolTipIcon.Warning);
#endif
					break;
			}
		}


		private void globalConnect()
		{
			lock (lockChConf)
			{
				if (!gconfchCurrent.bEnabled)
					return;
			}
			notifyIconMain.Icon = icActive;
			notifyIconMain.Text = Text + "(Waiting for connection...)";

			if (bConnected)
				return;

			lock (lockKmlUserAircraft)
			{
				szKmlUserAircraftPath = "";
				uiUserAircraftID = 0;
			}

			if (!timerFSXConnect.Enabled)
				timerFSXConnect.Start();
		}

		private void globalDisconnect()
		{
			if (bConnected)
			{
				bConnected = false;

				// Stop all query timers
				timerQueryUserAircraft.Stop();
				timerQueryUserPath.Stop();
				timerUserPrediction.Stop();

				timerQueryAIAircrafts.Stop();
				timerQueryAIHelicopters.Stop();
				timerQueryAIBoats.Stop();
				timerQueryAIGroundUnits.Stop();

				closeConnection();

				lock (lockChConf)
				{
					if (gconfchCurrent.bEnabled)
						safeShowBalloonTip(1000, Text, "Disconnected from FSX!", ToolTipIcon.Info);
				}
			}

			lock (lockChConf)
			{
				if (gconfchCurrent.bEnabled && !timerFSXConnect.Enabled)
				{
					timerFSXConnect.Start();
					notifyIconMain.Icon = icActive;
					notifyIconMain.Text = Text + "(Waiting for connection...)";
				}
				else
				{
					notifyIconMain.Icon = icDisabled;
					notifyIconMain.Text = Text + "(Disabled)";
				}
			}
		}


		#endregion


		#region Helper Functions


		#region Old Version

		//void processDataRequestResultAlternatingly(uint entryNumber, uint entriesCount, ref DataRequestReturn currentRequestStructure, ref object relatedLock, ref StructBasicMovingSceneryObject receivedData, ref String processedData)
		//{
		//    lock (relatedLock)
		//    {
		//        if (entryNumber <= currentRequestStructure.uiLastEntryNumber)
		//        {
		//            if (currentRequestStructure.uiCurrentDataSet == 1)
		//            {
		//                currentRequestStructure.szData2 = "";
		//                currentRequestStructure.uiCurrentDataSet = 2;
		//            }
		//            else
		//            {
		//                currentRequestStructure.szData1 = "";
		//                currentRequestStructure.uiCurrentDataSet = 1;
		//            }
		//        }

		//        currentRequestStructure.uiLastEntryNumber = entryNumber;

		//        if (currentRequestStructure.uiCurrentDataSet == 1)
		//            currentRequestStructure.szData1 += processedData;
		//        else
		//            currentRequestStructure.szData2 += processedData;

		//        if (entryNumber == entriesCount)
		//        {
		//            if (currentRequestStructure.uiCurrentDataSet == 1)
		//            {
		//                currentRequestStructure.szData2 = "";
		//                currentRequestStructure.uiCurrentDataSet = 2;
		//            }
		//            else
		//            {
		//                currentRequestStructure.szData1 = "";
		//                currentRequestStructure.uiCurrentDataSet = 1;
		//            }

		//            currentRequestStructure.uiLastEntryNumber = 0;
		//        }
		//    }
		//}

		#endregion

		void processDataRequestResultAlternatingly(uint entryNumber, uint entriesCount, ref DataRequestReturn currentRequestStructure, ref object relatedLock, ref DataRequestReturnObject receivedData, bool bCoursePrediction, bool bPredictionPoints)
		{
			lock (relatedLock)
			{
				// In case last data request return aborted unnormally and we're dealing with a new result, switch lists
				if (entryNumber <= currentRequestStructure.uiLastEntryNumber)
				{
					if (currentRequestStructure.uiCurrentDataSet == 1)
						currentRequestStructure.uiCurrentDataSet = 2;
					else
						currentRequestStructure.uiCurrentDataSet = 1;
				}

				List<DataRequestReturnObject> listCurrent = currentRequestStructure.uiCurrentDataSet == 1 ? currentRequestStructure.listFirst : currentRequestStructure.listSecond;
				List<DataRequestReturnObject> listOld = currentRequestStructure.uiCurrentDataSet == 1 ? currentRequestStructure.listSecond : currentRequestStructure.listFirst;


				// In case we have switched lists, clear new list and resize if necessary
				if (currentRequestStructure.bClearOnNextRun)
				{
					currentRequestStructure.bClearOnNextRun = false;
					listCurrent.Clear();
					if (listCurrent.Capacity < entriesCount)
						listCurrent.Capacity = (int)((double)entriesCount * 1.1);
				}


				// Calculate course prediction
				if (bCoursePrediction)
				{
					foreach (DataRequestReturnObject drroTemp in listOld)
					{
						if (drroTemp.uiObjectID == receivedData.uiObjectID)
						{
							if (drroTemp.bmsoObject.dTime != receivedData.bmsoObject.dTime)
							{
								if (bPredictionPoints)
									receivedData.ppsPredictionPoints = new PathPositionStored[gconffixCurrent.dPredictionTimes.GetLength(0)];

								receivedData.szCoursePrediction = receivedData.bmsoObject.dLongitude.ToString().Replace(",", ".") + "," + receivedData.bmsoObject.dLatitude.ToString().Replace(",", ".") + "," + receivedData.bmsoObject.dAltitude.ToString().Replace(",", ".") + "\n";

								for (uint n = 0; n < gconffixCurrent.dPredictionTimes.GetLength(0); n++)
								{
									PathPosition ppOld, ppCurrent;
									double dLongNew = 0.0, dLatNew = 0.0, dAltNew = 0.0;

									ppOld.bInitialized = true;
									ppOld.dLat = drroTemp.bmsoObject.dLatitude;
									ppOld.dLong = drroTemp.bmsoObject.dLongitude;
									ppOld.dAlt = drroTemp.bmsoObject.dAltitude;
									ppOld.dTime = drroTemp.bmsoObject.dTime;

									ppCurrent.bInitialized = true;
									ppCurrent.dLat = receivedData.bmsoObject.dLatitude;
									ppCurrent.dLong = receivedData.bmsoObject.dLongitude;
									ppCurrent.dAlt = receivedData.bmsoObject.dAltitude;
									ppCurrent.dTime = receivedData.bmsoObject.dTime;

									calcPositionByTime(ref ppOld, ref ppCurrent, gconffixCurrent.dPredictionTimes[n], ref dLatNew, ref dLongNew, ref dAltNew);

									receivedData.szCoursePrediction += dLongNew.ToString().Replace(",", ".") + "," + dLatNew.ToString().Replace(",", ".") + "," + dAltNew.ToString().Replace(",", ".") + "\n";

									if (bPredictionPoints)
									{
										PathPositionStored ppsTemp;
										ppsTemp.dLat = dLatNew;
										ppsTemp.dLong = dLongNew;
										ppsTemp.dAlt = dAltNew;
										ppsTemp.dTime = gconffixCurrent.dPredictionTimes[n];
										receivedData.ppsPredictionPoints[n] = ppsTemp;
									}
								}
							}
							else
							{
								receivedData.szCoursePrediction = drroTemp.szCoursePrediction;
								receivedData.ppsPredictionPoints = drroTemp.ppsPredictionPoints;
							}

							break;
						}
					}
				}


				// Set current entry number
				currentRequestStructure.uiLastEntryNumber = entryNumber;

				// Insert new data into the list
				if (currentRequestStructure.uiCurrentDataSet == 1)
					currentRequestStructure.listFirst.Add(receivedData);
				else
					currentRequestStructure.listSecond.Add(receivedData);


				// If this is the last entry from the current return, switch lists, so that http server can work with the just completed list
				if (entryNumber == entriesCount)
				{
					if (currentRequestStructure.uiCurrentDataSet == 1)
						currentRequestStructure.uiCurrentDataSet = 2;
					else
						currentRequestStructure.uiCurrentDataSet = 1;

					currentRequestStructure.uiLastEntryNumber = 0;
					currentRequestStructure.bClearOnNextRun = true;
				}
			}
		}

		private List<DataRequestReturnObject> GetCurrentList(ref DataRequestReturn drrnCurrent)
		{
			if (drrnCurrent.uiCurrentDataSet == 1)
				return drrnCurrent.listSecond;
			else
				return drrnCurrent.listFirst;
		}


		private void safeShowBalloonTip(int timeout, String tipTitle, String tipText, ToolTipIcon tipIcon)
		{
			lock (lockChConf)
			{
				if (!gconfchCurrent.bShowBalloons)
					return;
			}

			notifyIconMain.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
		}

		private void safeShowMainDialog(int iTab)
		{
			notifyIconMain.ContextMenuStrip = null;

			ConfigMirrorToForm();

			// Check if autostart is set
			string szRun = (string)Registry.GetValue(szRegKeyRun, AssemblyTitle, "");
			if (szRun != Application.ExecutablePath)
				checkBoxAutostart.Checked = false;

			bRestartRequired = false;

			tabControl1.SelectedIndex = iTab;
			Show();
		}

		private void safeHideMainDialog()
		{
			notifyIconMain.ContextMenuStrip = contextMenuStripNotifyIcon;
			Hide();

		}


		private void clearDrrStructure(ref DataRequestReturn drrToClear)
		{
			if (drrToClear.listFirst == null)
				drrToClear.listFirst = new List<DataRequestReturnObject>();

			if (drrToClear.listSecond == null)
				drrToClear.listSecond = new List<DataRequestReturnObject>();

			drrToClear.listFirst.Clear();
			drrToClear.listSecond.Clear();
			drrToClear.uiLastEntryNumber = 0;
			drrToClear.uiCurrentDataSet = 1;
			drrToClear.bClearOnNextRun = true;
		}

		private void clearPPStructure(ref PathPosition ppToClear)
		{
			ppToClear.bInitialized = false;
			ppToClear.dAlt = ppToClear.dLong = ppToClear.dAlt = 0.0;
		}


		private bool IsLocalHostIP(IPAddress ipaRequest)
		{
			lock (lockIPAddressList)
			{
				if (ipalLocal1 != null)
				{
					foreach (IPAddress ipaTemp in ipalLocal1)
					{
						if (ipaTemp.Equals(ipaRequest))
							return true;
					}
				}

				if (ipalLocal2 != null)
				{
					foreach (IPAddress ipaTemp in ipalLocal2)
					{
						if (ipaTemp.Equals(ipaRequest))
							return true;
					}
				}
			}

			return false;
		}


		private bool CompileKMLStartUpFileDynamic(String szIPAddress, ref String szResult)
		{
			try
			{
				string szTempKMLFile = File.ReadAllText(szFilePathData + "\\fsxget.template");

				szTempKMLFile = szTempKMLFile.Replace("%FSXU%", gconffixCurrent.bQueryUserAircraft ? File.ReadAllText(szFilePathData + "\\fsxget-fsxu.part") : "");
				szTempKMLFile = szTempKMLFile.Replace("%FSXP%", gconffixCurrent.bQueryUserPath ? File.ReadAllText(szFilePathData + "\\fsxget-fsxp.part") : "");
				szTempKMLFile = szTempKMLFile.Replace("%FSXPRE%", gconffixCurrent.bUserPathPrediction ? File.ReadAllText(szFilePathData + "\\fsxget-fsxpre.part") : "");

				szTempKMLFile = szTempKMLFile.Replace("%FSXAIP%", gconffixCurrent.bQueryAIAircrafts ? File.ReadAllText(szFilePathData + "\\fsxget-fsxaip.part") : "");
				szTempKMLFile = szTempKMLFile.Replace("%FSXAIH%", gconffixCurrent.bQueryAIHelicopters ? File.ReadAllText(szFilePathData + "\\fsxget-fsxaih.part") : "");
				szTempKMLFile = szTempKMLFile.Replace("%FSXAIB%", gconffixCurrent.bQueryAIBoats ? File.ReadAllText(szFilePathData + "\\fsxget-fsxaib.part") : "");
				szTempKMLFile = szTempKMLFile.Replace("%FSXAIG%", gconffixCurrent.bQueryAIGroundUnits ? File.ReadAllText(szFilePathData + "\\fsxget-fsxaig.part") : "");

				//szTempKMLFile = szTempKMLFile.Replace("%FSXFLIGHTPLAN%", gconffixCurrent.bLoadFlightPlans ? File.ReadAllText(szFilePathData + "\\fsxget-fsxflightplan.part") : "");

				szTempKMLFile = szTempKMLFile.Replace("%PATH%", "http://" + szIPAddress + ":" + gconffixCurrent.iServerPort.ToString());

				szResult = szTempKMLFile;
				return true;
			}
			catch
			{
				return false;
			}
		}

		private bool CompileKMLStartUpFileStatic(String szIPAddress, ref String szResult)
		{
			try
			{
				string szTempKMLFile = File.ReadAllText(szFilePathData + "\\fsxget.template");

				szTempKMLFile = szTempKMLFile.Replace("%FSXU%", "");
				szTempKMLFile = szTempKMLFile.Replace("%FSXP%", "");
				szTempKMLFile = szTempKMLFile.Replace("%FSXPRE%", "");

				szTempKMLFile = szTempKMLFile.Replace("%FSXAIP%", "");
				szTempKMLFile = szTempKMLFile.Replace("%FSXAIH%", "");
				szTempKMLFile = szTempKMLFile.Replace("%FSXAIB%", "");
				szTempKMLFile = szTempKMLFile.Replace("%FSXAIG%", "");

				//szTempKMLFile = szTempKMLFile.Replace("%FSXFLIGHTPLAN%", gconffixCurrent.bLoadFlightPlans ? File.ReadAllText(szFilePathData + "\\fsxget-fsxflightplan.part") : "");

				szTempKMLFile = szTempKMLFile.Replace("%PATH%", "http://" + szIPAddress + ":" + gconffixCurrent.iServerPort.ToString());

				szResult = szTempKMLFile;
				return true;
			}
			catch
			{
				return false;
			}
		}


		private void UpdateCheckBoxStates()
		{
		}


		private double ConvertDegToDouble(String szDeg)
		{

			String szTemp = szDeg;

			szTemp = szTemp.Replace("N", "+");
			szTemp = szTemp.Replace("S", "-");
			szTemp = szTemp.Replace("E", "+");
			szTemp = szTemp.Replace("W", "-");

			szTemp = szTemp.Replace(" ", "");

			szTemp = szTemp.Replace("\"", "");
			szTemp = szTemp.Replace("'", "/");
			szTemp = szTemp.Replace("°", "/");

			char[] szSeperator = { '/' };
			String[] szParts = szTemp.Split(szSeperator);

			if (szParts.GetLength(0) != 3)
			{
				throw new System.Exception("Wrong coordinate format!");
			}

			
			double d1 = System.Double.Parse(szParts[0], System.Globalization.NumberFormatInfo.InvariantInfo);
			int iSign = Math.Sign(d1);
			d1 = Math.Abs(d1);
			double d2 = System.Double.Parse(szParts[1], System.Globalization.NumberFormatInfo.InvariantInfo);
			double d3 = System.Double.Parse(szParts[2], System.Globalization.NumberFormatInfo.InvariantInfo);

			return iSign * (d1 + (d2 * 60.0 + d3) / 3600.0);
		}


		#endregion


		#region Calucaltion


		private void calcPositionByTime(ref PathPosition ppOld, ref PathPosition ppNew, double dSeconds, ref double dResultLat, ref double dResultLong, ref double dResultAlt)
		{
			double dTimeElapsed = ppNew.dTime - ppOld.dTime;
			double dScale = dSeconds / dTimeElapsed;

			dResultLat = ppNew.dLat + dScale * (ppNew.dLat - ppOld.dLat);
			dResultLong = ppNew.dLong + dScale * (ppNew.dLong - ppOld.dLong);
			dResultAlt = ppNew.dAlt + dScale * (ppNew.dAlt - ppOld.dAlt);
		}

		#region Old Calculation

		//private void calcPositionByTime(double dLong, double dLat, double dAlt, double dSpeedX, double dSpeedY, double dSpeedZ, double dSeconds, ref double dResultLat, ref double dResultLong, ref double dResultAlt)
		//{
		//    const double dRadEarth = 6371000.8;


		//    dResultLat = dLat + dSpeedZ * dSeconds / 1852.0;

		//    dResultAlt = dAlt + dSpeedY * dSeconds;

		//    double dLatMiddle = dLat + (dSpeedZ * dSeconds / 1852.0 / 2.0);
		//    dResultLong = dLong + (dSpeedX * dSeconds / (2.0 * Math.PI * dRadEarth / 360.0 * Math.Cos(dLatMiddle * Math.PI / 180.0)));





		//    //double dNewPosX = dSpeedX * dSeconds;
		//    //double dNewPosY = dSpeedY * dSeconds;
		//    //double dNewPosZ = dSpeedZ * dSeconds;


		//    //double dCosAngle = (dNewPosY * 1.0 + dNewPosZ * 0.0) / (Math.Sqrt(Math.Pow(dNewPosY, 2.0) + Math.Pow(dNewPosZ, 2.0)) * Math.Sqrt(Math.Pow(0.0, 2.0) + Math.Pow(1.0, 2.0)));
		//    //dResultLat = dLat + Math.Acos(dCosAngle / 180.0 * Math.PI);

		//    //// East-West-Position
		//    //dCosAngle = (dNewPosX * 1.0 + dNewPosY * 0.0) / (Math.Sqrt(Math.Pow(dNewPosX, 2.0) + Math.Pow(dNewPosY, 2.0)) * Math.Sqrt(Math.Pow(1.0, 2.0) + Math.Pow(0.0, 2.0)));
		//    //dResultLong = dLong + Math.Acos(dCosAngle / 180.0 * Math.PI);

		//    //// Altitude
		//    //dResultAlt = dAlt + Math.Sqrt(Math.Pow(dNewPosX, 2.0) + Math.Pow(dNewPosY, 2.0) + Math.Pow(dNewPosZ, 2.0));



		//    //const double dRadEarth = 6371000.8;


		//    ////x' = cos(theta)*x - sin(theta)*y 
		//    ////y' = sin(theta)*x + cos(theta)*y


		//    //// Calculate North-South-Position
		//    //double dTempX = dAlt + dRadEarth;
		//    //double dTempY = 0.0;

		//    //double dPosY = Math.Cos(dLat) * dTempX - Math.Sin(dLat) * dTempY;
		//    //double dPosZ = Math.Sin(dLat) * dTempX - Math.Cos(dLat) * dTempY;

		//    //// Calculate East-West-Position
		//    //dTempX = dAlt + dRadEarth;
		//    //dTempY = 0;

		//    //double dPosX = Math.Cos(dLong) * dTempX - Math.Sin(dLong) * dTempY;
		//    ////dPosZ = Math.Sin(dLat) * dTempX - Math.Cos(dLat) * dTempY;


		//    //// Normalize
		//    //double dLength = Math.Sqrt(Math.Pow(dPosX, 2.0) + Math.Pow(dPosY, 2.0) + Math.Pow(dPosZ, 2.0));
		//    //dPosX = dPosX / dLength * (dAlt + dRadEarth);
		//    //dPosY = dPosY / dLength * (dAlt + dRadEarth);
		//    //dPosZ = dPosZ / dLength * (dAlt + dRadEarth);

		//    //double dTest = Math.Sqrt(Math.Pow(dPosX, 2.0) + Math.Pow(dPosY, 2.0) + Math.Pow(dPosZ, 2.0)) - dRadEarth;


		//    //// Calculate position after given time
		//    //double dNewPosX = dPosX + dSpeedX * dSeconds;
		//    //double dNewPosY = dPosY + dSpeedY * dSeconds;
		//    //double dNewPosZ = dPosZ + dSpeedZ * dSeconds;


		//    //// Now again translate into lat-long-coordinates

		//    //// North-South-Position
		//    //double dCosAngle = (dNewPosY * dPosY + dNewPosZ * dPosZ) / (Math.Sqrt(Math.Pow(dNewPosY, 2.0) + Math.Pow(dNewPosZ, 2.0)) * Math.Sqrt(Math.Pow(dPosY, 2.0) + Math.Pow(dPosZ, 2.0)));
		//    //dResultLat = dLat + Math.Acos(dCosAngle / 180.0 * Math.PI);

		//    //// East-West-Position
		//    //dCosAngle = (dNewPosX * dPosX + dNewPosY * dPosY) / (Math.Sqrt(Math.Pow(dNewPosX, 2.0) + Math.Pow(dNewPosY, 2.0)) * Math.Sqrt(Math.Pow(dPosX, 2.0) + Math.Pow(dPosY, 2.0)));
		//    //dResultLong = dLong + Math.Acos(dCosAngle / 180.0 * Math.PI);

		//    //// Altitude
		//    //dResultAlt = Math.Sqrt(Math.Pow(dNewPosX, 2.0) + Math.Pow(dNewPosY, 2.0) + Math.Pow(dNewPosZ, 2.0)) - dRadEarth;
		//}

		#endregion


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

				if (gconffixCurrent.uiServerAccessLevel == 0 && !IsLocalHostIP(request.RemoteEndPoint.Address))
				{
					response.Abort();
					return;
				}


				byte[] buffer = System.Text.Encoding.UTF8.GetBytes("");
				String szHeader = "";
				bool bContentSet = false;


                if (request.Url.PathAndQuery.ToLower().StartsWith("/gfx/scenery/"))
                {
                    String strTmp = request.Url.PathAndQuery.Substring(13);
                    if (strTmp.StartsWith("air/"))
                    {
                        String szTemp = request.Url.PathAndQuery.Substring(17);

                        if (szTemp.Length >= 4)
                        {
                            // Cut the .png suffix from the url
                            szTemp = szTemp.Substring(0, szTemp.Length - 4);

                            foreach (ObjectImage aimgCurrent in listImgUnitsAir)
                            {
                                String szTemp2 = HttpUtility.UrlDecode(szTemp);
                                if (aimgCurrent.szTitle.ToLower() == HttpUtility.UrlDecode(szTemp).ToLower())
                                {
                                    buffer = aimgCurrent.bData;
                                    szHeader = "image/png";
                                    bContentSet = true;
                                    break;
                                }
                            }

                            if (!bContentSet)
                            {
                                buffer = imgNoImage;
                                szHeader = "image/png";
                                bContentSet = true;
                            }
                        }
                    }
                    else if (strTmp.StartsWith("water/"))
                    {
                        String szTemp = request.Url.PathAndQuery.Substring(19);

                        if (szTemp.Length >= 4)
                        {
                            // Cut the .png suffix from the url
                            szTemp = szTemp.Substring(0, szTemp.Length - 4);

                            foreach (ObjectImage aimgCurrent in listImgUnitsWater)
                            {
                                String szTemp2 = HttpUtility.UrlDecode(szTemp);
                                if (aimgCurrent.szTitle.ToLower() == HttpUtility.UrlDecode(szTemp).ToLower())
                                {
                                    buffer = aimgCurrent.bData;
                                    szHeader = "image/png";
                                    bContentSet = true;
                                    break;
                                }
                            }

                            if (!bContentSet)
                            {
                                buffer = imgNoImage;
                                szHeader = "image/png";
                                bContentSet = true;
                            }
                        }
                    }
                    else if (strTmp.StartsWith("ground/"))
                    {
                        String szTemp = request.Url.PathAndQuery.Substring(20);

                        if (szTemp.Length >= 4)
                        {
                            // Cut the .png suffix from the url
                            szTemp = szTemp.Substring(0, szTemp.Length - 4);

                            foreach (ObjectImage aimgCurrent in listImgUnitsGround)
                            {
                                String szTemp2 = HttpUtility.UrlDecode(szTemp);
                                if (aimgCurrent.szTitle.ToLower() == HttpUtility.UrlDecode(szTemp).ToLower())
                                {
                                    buffer = aimgCurrent.bData;
                                    szHeader = "image/png";
                                    bContentSet = true;
                                    break;
                                }
                            }

                            if (!bContentSet)
                            {
                                buffer = imgNoImage;
                                szHeader = "image/png";
                                bContentSet = true;
                            }
                        }
                    }
                    else if (strTmp.Equals("logo.png"))
                    {
                        buffer = imgLogo;
                        szHeader = "image/png";
                        bContentSet = true;
                    }
                }
				else if (request.Url.PathAndQuery.ToLower().StartsWith("/gfx/ge/icons/"))
				{
					String szTemp = request.Url.PathAndQuery.Substring(14);

					if (szTemp.Length >= 4)
					{
						// Cut the .png suffix from the url
						szTemp = szTemp.Substring(0, szTemp.Length - 4);

						buffer = null;
						foreach (ObjectImage oimgTemp in listIconsGE)
						{
							if (oimgTemp.szTitle.ToLower() == szTemp.ToLower())
							{
								szHeader = "image/png";
								buffer = oimgTemp.bData;

								bContentSet = true;

								break;
							}
						}

						if (!bContentSet)
						{
							buffer = imgNoImage;
							szHeader = "image/png";
							bContentSet = true;
						}
					}
				}
				else if (request.Url.PathAndQuery.ToLower() == "/fsxu.kml")
				{
					bContentSet = true;
                    String str = KmlGenFile(KML_FILES.REQUEST_USER_AIRCRAFT, KML_ACCESS_MODES.MODE_SERVER, true, (uint)gconffixCurrent.iUpdateGEUserAircraft, request.UserHostName);
                    szHeader = "application/vnd.google-earth.kml+xml";
					buffer = System.Text.Encoding.UTF8.GetBytes(str);
                    System.Diagnostics.Trace.WriteLine(str);
                }
				else if (request.Url.PathAndQuery.ToLower() == "/fsxp.kml")
				{
					bContentSet = true;
					szHeader = "application/vnd.google-earth.kml+xml";
                    buffer = System.Text.Encoding.UTF8.GetBytes(KmlGenFile(KML_FILES.REQUEST_USER_PATH, KML_ACCESS_MODES.MODE_SERVER, true, (uint)gconffixCurrent.iUpdateGEUserPath, request.UserHostName));
				}
				else if (request.Url.PathAndQuery.ToLower() == "/fsxpre.kml")
				{
					bContentSet = true;
                    szHeader = "application/vnd.google-earth.kml+xml";
                    buffer = System.Text.Encoding.UTF8.GetBytes(KmlGenFile(KML_FILES.REQUEST_USER_PREDICTION, KML_ACCESS_MODES.MODE_SERVER, true, (uint)gconffixCurrent.iUpdateGEUserPrediction, request.UserHostName));
				}
				else if (request.Url.PathAndQuery.ToLower() == "/fsxaip.kml")
				{
					bContentSet = true;
					szHeader = "application/vnd.google-earth.kml+xml";
					buffer = System.Text.Encoding.UTF8.GetBytes(KmlGenFile(KML_FILES.REQUEST_AI_PLANE, KML_ACCESS_MODES.MODE_SERVER, true, (uint)gconffixCurrent.iUpdateGEAIAircrafts, request.UserHostName));
				}
				else if (request.Url.PathAndQuery.ToLower() == "/fsxaih.kml")
				{
					bContentSet = true;
					szHeader = "application/vnd.google-earth.kml+xml";
					buffer = System.Text.Encoding.UTF8.GetBytes(KmlGenFile(KML_FILES.REQUEST_AI_HELICOPTER, KML_ACCESS_MODES.MODE_SERVER, true, (uint)gconffixCurrent.iUpdateGEAIHelicopters, request.UserHostName));
				}
				else if (request.Url.PathAndQuery.ToLower() == "/fsxaib.kml")
				{
					bContentSet = true;
					szHeader = "application/vnd.google-earth.kml+xml";
					buffer = System.Text.Encoding.UTF8.GetBytes(KmlGenFile(KML_FILES.REQUEST_AI_BOAT, KML_ACCESS_MODES.MODE_SERVER, true, (uint)gconffixCurrent.iUpdateGEAIBoats, request.UserHostName));
				}
				else if (request.Url.PathAndQuery.ToLower() == "/fsxaig.kml")
				{
					bContentSet = true;
					szHeader = "application/vnd.google-earth.kml+xml";
					buffer = System.Text.Encoding.UTF8.GetBytes(KmlGenFile(KML_FILES.REQUEST_AI_GROUND, KML_ACCESS_MODES.MODE_SERVER, true, (uint)gconffixCurrent.iUpdateGEAIGroundUnits, request.UserHostName));
				}
				else if (request.Url.PathAndQuery.ToLower() == "/fsxflightplans.kml")
				{
					bContentSet = true;
					szHeader = "application/vnd.google-earth.kml+xml";
					buffer = System.Text.Encoding.UTF8.GetBytes(KmlGenFile(KML_FILES.REQUEST_FLIGHT_PLANS, KML_ACCESS_MODES.MODE_SERVER, false, 0, request.UserHostName));
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


		private String KmlGenFile(KML_FILES kmlfWanted, KML_ACCESS_MODES AccessMode, bool bExpires, uint uiSeconds, String szSever)
		{
			String szTemp = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><kml xmlns=\"http://earth.google.com/kml/2.1\">" +
				KmlGetExpireString(bExpires, uiSeconds) +
				"<Document>";

			switch (kmlfWanted)
			{
				case KML_FILES.REQUEST_USER_AIRCRAFT:
					szTemp += KmlGenUserPosition(AccessMode, szSever);
					break;

				case KML_FILES.REQUEST_USER_PATH:
					szTemp += KmlGenUserPath(AccessMode, szSever);
					break;

				case KML_FILES.REQUEST_USER_PREDICTION:
					szTemp += KmlGenUserPrediction(AccessMode, szSever);
					break;

				case KML_FILES.REQUEST_AI_PLANE:
					szTemp += KmlGenAIAircraft(AccessMode, szSever);
					break;

				case KML_FILES.REQUEST_AI_HELICOPTER:
					szTemp += KmlGenAIHelicopter(AccessMode, szSever);
					break;

				case KML_FILES.REQUEST_AI_BOAT:
					szTemp += KmlGenAIBoat(AccessMode, szSever);
					break;

				case KML_FILES.REQUEST_AI_GROUND:
					szTemp += KmlGenAIGroundUnit(AccessMode, szSever);
					break;

				//case KML_FILES.REQUEST_FLIGHT_PLANS:
				//    szTemp += KmlGenFlightPlans(AccessMode, szSever);
				//    break;

				default:
					break;
			}

			szTemp += "</Document></kml>";

			return szTemp;
		}


		private String KmlGetExpireString(bool bExpires, uint uiSeconds)
		{
			if (!bExpires)
				return "";

			DateTime date = DateTime.Now;
			date = date.AddSeconds(uiSeconds);
			date = date.ToUniversalTime();

			return "<NetworkLinkControl><expires>" + date.ToString("yyyy-MM-ddTHH:mm:ssZ") + "</expires></NetworkLinkControl>";
		}

		private String KmlGetImageLink(KML_ACCESS_MODES AccessMode, KML_IMAGE_TYPES ImageType, String szTitle, String szServer)
		{
			if (AccessMode == KML_ACCESS_MODES.MODE_SERVER)
			{
				String szPrefix = "";
				switch (ImageType)
				{
					case KML_IMAGE_TYPES.AIRCRAFT:
						szPrefix = "/gfx/scenery/air/";
						break;

					case KML_IMAGE_TYPES.WATER:
						szPrefix = "/gfx/scenery/water/";
						break;

					case KML_IMAGE_TYPES.GROUND:
						szPrefix = "/gfx/scenery/ground/";
						break;
				}
				return "http://" + szServer + szPrefix + szTitle + ".png";
			}
			else
			{
				List<ObjectImage> listTemp;
				switch (ImageType)
				{
					case KML_IMAGE_TYPES.AIRCRAFT:
						listTemp = listImgUnitsAir;
						break;

					case KML_IMAGE_TYPES.WATER:
						listTemp = listImgUnitsWater;
						break;

					case KML_IMAGE_TYPES.GROUND:
						listTemp = listImgUnitsGround;
						break;

					default:
						return "";
				}

				foreach (ObjectImage oimgTemp in listTemp)
				{
					if (oimgTemp.szTitle.ToLower() == szTitle.ToLower())
					{
						if (AccessMode == KML_ACCESS_MODES.MODE_FILE_LOCAL)
							return szFilePathPub + oimgTemp.szPath;
						else if (AccessMode == KML_ACCESS_MODES.MODE_FILE_USERDEFINED)
							return gconffixCurrent.szUserdefinedPath + oimgTemp.szPath;
					}
				}

				return "";
			}
		}

		private String KmlGetIconLink(KML_ACCESS_MODES AccessMode, KML_ICON_TYPES IconType, String szServer)
		{
			String szIcon = "";
			switch (IconType)
			{
				case KML_ICON_TYPES.USER_AIRCRAFT_POSITION:
					szIcon = "fsxu";
					break;

				case KML_ICON_TYPES.USER_PREDICTION_POINT:
					szIcon = "fsxpm";
					break;

				case KML_ICON_TYPES.AI_AIRCRAFT:
					szIcon = "fsxaip";
					break;

				case KML_ICON_TYPES.AI_HELICOPTER:
					szIcon = "fsxaih";
					break;

				case KML_ICON_TYPES.AI_BOAT:
					szIcon = "fsxaib";
					break;

				case KML_ICON_TYPES.AI_GROUND_UNIT:
					szIcon = "fsxaig";
					break;

				case KML_ICON_TYPES.AI_AIRCRAFT_PREDICTION_POINT:
					szIcon = "fsxaippp";
					break;

				case KML_ICON_TYPES.AI_HELICOPTER_PREDICTION_POINT:
					szIcon = "fsxaihpp";
					break;

				case KML_ICON_TYPES.AI_BOAT_PREDICTION_POINT:
					szIcon = "fsxaibpp";
					break;

				case KML_ICON_TYPES.AI_GROUND_PREDICTION_POINT:
					szIcon = "fsxaigpp";
					break;

				case KML_ICON_TYPES.PLAN_INTER:
					szIcon = "plan-inter";
					break;

				case KML_ICON_TYPES.PLAN_NDB:
					szIcon = "plan-ndb";
					break;

				case KML_ICON_TYPES.PLAN_PORT:
					szIcon = "plan-port";
					break;

				case KML_ICON_TYPES.PLAN_USER:
					szIcon = "plan-user";
					break;

				case KML_ICON_TYPES.PLAN_VOR:
					szIcon = "plan-vor";
					break;
			}

			if (AccessMode == KML_ACCESS_MODES.MODE_SERVER)
			{
				return "http://" + szServer + "/gfx/ge/icons/" + szIcon + ".png";
			}
			else
			{
				foreach (ObjectImage oimgTemp in listIconsGE)
				{
					if (oimgTemp.szTitle.ToLower() == szIcon.ToLower())
					{
						if (AccessMode == KML_ACCESS_MODES.MODE_FILE_LOCAL)
							return szFilePathPub + oimgTemp.szPath;
						else if (AccessMode == KML_ACCESS_MODES.MODE_FILE_USERDEFINED)
							return gconffixCurrent.szUserdefinedPath + oimgTemp.szPath;
					}
				}

				return "";
			}
		}

		private String KmlGenETAPoints(ref PathPositionStored[] ppsCurrent, bool bGenerate, KML_ACCESS_MODES AccessMode, KML_ICON_TYPES Icon, String szServer)
		{
			if (ppsCurrent == null)
				return "";

			if (bGenerate)
			{
				String szTemp = "<Folder><name>ETA Points</name>";

				for (uint n = 0; n < ppsCurrent.GetLength(0); n++)
					szTemp += "<Placemark>" +
						"<name>ETA " + ((ppsCurrent[n].dTime < 60.0) ? (((int)ppsCurrent[n].dTime).ToString() + " sec") : (ppsCurrent[n].dTime / 60.0 + " min")) + "</name><visibility>1</visibility><open>0</open>" +
						"<description><![CDATA[Esitmated Position]]></description>" +
						"<Style>" +
						"<IconStyle><Icon><href>" + KmlGetIconLink(AccessMode, Icon, szServer) + "</href></Icon><scale>0.2</scale></IconStyle>" +
						"<LabelStyle><scale>0.4</scale></LabelStyle>" +
						"</Style>" +
						"<Point><altitudeMode>absolute</altitudeMode><coordinates>" + ppsCurrent[n].dLong.ToString().Replace(",", ".") + "," + ppsCurrent[n].dLat.ToString().Replace(",", ".") + "," + ppsCurrent[n].dAlt.ToString().Replace(",", ".") + "</coordinates><extrude>1</extrude></Point></Placemark>";

				return szTemp + "</Folder>";
			}
			else
				return "";
		}


		private String KmlGenUserPosition(KML_ACCESS_MODES AccessMode, String szServer)
		{
			lock (lockKmlUserAircraft)
			{
				return "<Placemark>" +
					"<name>User Aircraft Position</name><visibility>1</visibility><open>0</open>" +
					"<description><![CDATA[Microsoft Flight Simulator X - User Aircraft<br>&nbsp;<br>" +
					"<b>Title:</b> " + suadCurrent.szTitle + "<br>&nbsp;<br>" +
					"<b>Type:</b> " + suadCurrent.szATCType + "<br>" +
					"<b>Model:</b> " + suadCurrent.szATCModel + "<br>&nbsp;<br>" +
					"<b>Identification:</b> " + suadCurrent.szATCID + "<br>&nbsp;<br>" +
					"<b>Flight Number:</b> " + suadCurrent.szATCFlightNumber + "<br>" +
					"<b>Airline:</b> " + suadCurrent.szATCAirline + "<br>&nbsp;<br>" +
					"<b>Altitude:</b> " + ((int)suadCurrent.dAltitude).ToString().Replace(",", ".") + " m<br>&nbsp;<br>" +
					"<center><img src=\"" + KmlGetImageLink(AccessMode, KML_IMAGE_TYPES.AIRCRAFT, suadCurrent.szTitle, szServer) + "\"></center>]]></description>" +
					"<Snippet>" + suadCurrent.szATCType + " " + suadCurrent.szATCModel + " (" + suadCurrent.szTitle + "), " + suadCurrent.szATCID + "\nAltitude: " + ((int)suadCurrent.dAltitude).ToString().Replace(",", ".") + " m</Snippet>" +
					"<Style>" +
					"<IconStyle><Icon><href>" + KmlGetIconLink(AccessMode, KML_ICON_TYPES.USER_AIRCRAFT_POSITION, szServer) + "</href></Icon><scale>0.8</scale></IconStyle>" +
					"<LabelStyle><scale>1.0</scale></LabelStyle>" +
					"</Style>" +
                    "<Point><altitudeMode>absolute</altitudeMode><coordinates>" + suadCurrent.dLongitude.ToString().Replace(",", ".") + "," + suadCurrent.dLatitude.ToString().Replace(",", ".") + "," + suadCurrent.dAltitude.ToString().Replace(",", ".") + "</coordinates><extrude>1</extrude></Point></Placemark>" +
                    "<LookAt><longitude>" + XmlConvert.ToString(suadCurrent.dLongitude) + "</longitude><latitude>" + XmlConvert.ToString(suadCurrent.dLatitude) + "</latitude><heading>" + XmlConvert.ToString(suadCurrent.dHeading) + "</heading></LookAt>";
			}
		}

		private String KmlGenUserPath(KML_ACCESS_MODES AccessMode, String szServer)
		{
			lock (lockKmlUserPath)
			{
				return "<Placemark><name>User Aircraft Path</name><description>Path of the user aircraft since tracking started.</description><visibility>1</visibility><open>0</open><Style><LineStyle><color>9fffffff</color><width>2</width></LineStyle></Style><LineString><altitudeMode>absolute</altitudeMode><coordinates>" + szKmlUserAircraftPath + "</coordinates></LineString></Placemark>";
			}
		}

		private String KmlGenUserPrediction(KML_ACCESS_MODES AccessMode, String szServer)
		{
			String szTemp = "";

			lock (lockKmlUserPrediction)
			{
				szTemp = "<Placemark><name>User Aircraft Path Prediction</name><description>Path prediction of the user aircraft.</description><visibility>1</visibility><open>0</open><Style><LineStyle><color>9f00ffff</color><width>2</width></LineStyle></Style><LineString><altitudeMode>absolute</altitudeMode><coordinates>" + szKmlUserPrediction + "</coordinates></LineString></Placemark>" +
					"<Folder><name>ETA Points</name>";

				foreach (PathPositionStored ppsTemp in listKmlPredictionPoints)
				{
					szTemp += "<Placemark>" +
						"<name>ETA " + ((ppsTemp.dTime < 60.0) ? (((int)ppsTemp.dTime).ToString() + " sec") : (ppsTemp.dTime / 60.0 + " min")) + "</name><visibility>1</visibility><open>0</open>" +
						"<description><![CDATA[Esitmated Position]]></description>" +
						"<Style>" +
						"<IconStyle><Icon><href>" + KmlGetIconLink(AccessMode, KML_ICON_TYPES.USER_PREDICTION_POINT, szServer) + "</href></Icon><scale>0.2</scale></IconStyle>" +
						"<LabelStyle><scale>0.4</scale></LabelStyle>" +
						"</Style>" +
						"<Point><altitudeMode>absolute</altitudeMode><coordinates>" + ppsTemp.dLong.ToString().Replace(",", ".") + "," + ppsTemp.dLat.ToString().Replace(",", ".") + "," + ppsTemp.dAlt.ToString().Replace(",", ".") + "</coordinates><extrude>1</extrude></Point></Placemark>";
				}
			}

			return szTemp + "</Folder>";
		}


		private String KmlGenAIAircraft(KML_ACCESS_MODES AccessMode, String szServer)
		{
			String szTemp = "<Folder><name>Aircraft Positions</name>";

			lock (lockDrrAiPlanes)
			{
				List<DataRequestReturnObject> listTemp = GetCurrentList(ref drrAIPlanes);

				foreach (DataRequestReturnObject bmsoTemp in listTemp)
				{
					szTemp += "<Placemark>" +
						"<name>" + bmsoTemp.bmsoObject.szATCType + " " + bmsoTemp.bmsoObject.szATCModel + " (" + bmsoTemp.bmsoObject.szATCID + ")</name><visibility>1</visibility><open>0</open>" +
						"<description><![CDATA[Microsoft Flight Simulator X - AI Plane<br>&nbsp;<br>" +
						"<b>Title:</b> " + bmsoTemp.bmsoObject.szTitle + "<br>&nbsp;<br>" +
						"<b>Type:</b> " + bmsoTemp.bmsoObject.szATCType + "<br>" +
						"<b>Model:</b> " + bmsoTemp.bmsoObject.szATCModel + "<br>&nbsp;<br>" +
						"<b>Identification:</b> " + bmsoTemp.bmsoObject.szATCID + "<br>&nbsp;<br>" +
						"<b>Flight Number:</b> " + bmsoTemp.bmsoObject.szATCFlightNumber + "<br>" +
						"<b>Airline:</b> " + bmsoTemp.bmsoObject.szATCAirline + "<br>&nbsp;<br>" +
						"<b>Altitude:</b> " + ((int)bmsoTemp.bmsoObject.dAltitude).ToString().Replace(",", ".") + " m<br>&nbsp;<br>" +
						"<center><img src=\"" + KmlGetImageLink(AccessMode, KML_IMAGE_TYPES.AIRCRAFT, bmsoTemp.bmsoObject.szTitle, szServer) + "\"></center>]]></description>" +
						"<Snippet>" + bmsoTemp.bmsoObject.szTitle + "\nAltitude: " + ((int)bmsoTemp.bmsoObject.dAltitude).ToString().Replace(",", ".") + " m</Snippet>" +
						"<Style>" +
						"<IconStyle><Icon><href>" + KmlGetIconLink(AccessMode, KML_ICON_TYPES.AI_AIRCRAFT, szServer) + "</href></Icon><scale>0.6</scale></IconStyle>" +
						"<LabelStyle><scale>0.6</scale></LabelStyle>" +
						"</Style>" +
						"<Point><altitudeMode>absolute</altitudeMode><coordinates>" + bmsoTemp.bmsoObject.dLongitude.ToString().Replace(",", ".") + "," + bmsoTemp.bmsoObject.dLatitude.ToString().Replace(",", ".") + "," + bmsoTemp.bmsoObject.dAltitude.ToString().Replace(",", ".") + "</coordinates><extrude>1</extrude></Point></Placemark>";
				}

				szTemp += "</Folder>";

				if (gconffixCurrent.bPredictAIAircrafts)
				{
					szTemp += "<Folder><name>Aircraft Courses</name>";

					foreach (DataRequestReturnObject bmsoTemp in listTemp)
					{
						if (bmsoTemp.szCoursePrediction == "")
							continue;

						szTemp += "<Placemark><name>" + bmsoTemp.bmsoObject.szATCType + " " + bmsoTemp.bmsoObject.szATCModel + " (" + bmsoTemp.bmsoObject.szATCID + ")</name><description>Course prediction of the aircraft.</description><visibility>1</visibility><open>0</open><Style><LineStyle><color>9fd20091</color><width>2</width></LineStyle></Style><LineString><altitudeMode>absolute</altitudeMode><coordinates>" + bmsoTemp.szCoursePrediction + "</coordinates></LineString></Placemark>";

						PathPositionStored[] ppsTemp = bmsoTemp.ppsPredictionPoints;
						szTemp += KmlGenETAPoints(ref ppsTemp, gconffixCurrent.bPredictPointsAIAircrafts, AccessMode, KML_ICON_TYPES.AI_AIRCRAFT_PREDICTION_POINT, szServer);
					}

					szTemp += "</Folder>";
				}
			}

			return szTemp;
		}

		private String KmlGenAIHelicopter(KML_ACCESS_MODES AccessMode, String szServer)
		{
			String szTemp = "<Folder><name>Helicopter Positions</name>";

			lock (lockDrrAiHelicopters)
			{
				List<DataRequestReturnObject> listTemp = GetCurrentList(ref drrAIHelicopters);

				foreach (DataRequestReturnObject bmsoTemp in listTemp)
				{
					szTemp += "<Placemark>" +
						"<name>" + bmsoTemp.bmsoObject.szATCType + " " + bmsoTemp.bmsoObject.szATCModel + " (" + bmsoTemp.bmsoObject.szATCID + ")</name><visibility>1</visibility><open>0</open>" +
						"<description><![CDATA[Microsoft Flight Simulator X - AI Helicopter<br>&nbsp;<br>" +
						"<b>Title:</b> " + bmsoTemp.bmsoObject.szTitle + "<br>&nbsp;<br>" +
						"<b>Type:</b> " + bmsoTemp.bmsoObject.szATCType + "<br>" +
						"<b>Model:</b> " + bmsoTemp.bmsoObject.szATCModel + "<br>&nbsp;<br>" +
						"<b>Identification:</b> " + bmsoTemp.bmsoObject.szATCID + "<br>&nbsp;<br>" +
						"<b>Flight Number:</b> " + bmsoTemp.bmsoObject.szATCFlightNumber + "<br>" +
						"<b>Airline:</b> " + bmsoTemp.bmsoObject.szATCAirline + "<br>&nbsp;<br>" +
						"<b>Altitude:</b> " + ((int)bmsoTemp.bmsoObject.dAltitude).ToString().Replace(",", ".") + " m<br>&nbsp;<br>" +
						"<center><img src=\"" + KmlGetImageLink(AccessMode, KML_IMAGE_TYPES.AIRCRAFT, bmsoTemp.bmsoObject.szTitle, szServer) + "\"></center>]]></description>" +
						"<Snippet>" + bmsoTemp.bmsoObject.szTitle + "\nAltitude: " + ((int)bmsoTemp.bmsoObject.dAltitude).ToString().Replace(",", ".") + " m</Snippet>" +
						"<Style>" +
						"<IconStyle><Icon><href>" + KmlGetIconLink(AccessMode, KML_ICON_TYPES.AI_HELICOPTER, szServer) + "</href></Icon><scale>0.6</scale></IconStyle>" +
						"<LabelStyle><scale>0.6</scale></LabelStyle>" +
						"</Style>" +
						"<Point><altitudeMode>absolute</altitudeMode><coordinates>" + bmsoTemp.bmsoObject.dLongitude.ToString().Replace(",", ".") + "," + bmsoTemp.bmsoObject.dLatitude.ToString().Replace(",", ".") + "," + bmsoTemp.bmsoObject.dAltitude.ToString().Replace(",", ".") + "</coordinates><extrude>1</extrude></Point></Placemark>";
				}

				szTemp += "</Folder>";

				if (gconffixCurrent.bPredictAIHelicopters)
				{
					szTemp += "<Folder><name>Helicopter Courses</name>";

					foreach (DataRequestReturnObject bmsoTemp in listTemp)
					{
						if (bmsoTemp.szCoursePrediction == "")
							continue;

						szTemp += "<Placemark><name>" + bmsoTemp.bmsoObject.szATCType + " " + bmsoTemp.bmsoObject.szATCModel + " (" + bmsoTemp.bmsoObject.szATCID + ")</name><description>Course prediction of the helicopter.</description><visibility>1</visibility><open>0</open><Style><LineStyle><color>9fd20091</color><width>2</width></LineStyle></Style><LineString><altitudeMode>absolute</altitudeMode><coordinates>" + bmsoTemp.szCoursePrediction + "</coordinates></LineString></Placemark>";

						PathPositionStored[] ppsTemp = bmsoTemp.ppsPredictionPoints;
						szTemp += KmlGenETAPoints(ref ppsTemp, gconffixCurrent.bPredictPointsAIHelicopters, AccessMode, KML_ICON_TYPES.AI_HELICOPTER_PREDICTION_POINT, szServer);
					}

					szTemp += "</Folder>";
				}
			}

			return szTemp;
		}

		private String KmlGenAIBoat(KML_ACCESS_MODES AccessMode, String szServer)
		{
			String szTemp = "<Folder><name>Boat Positions</name>";

			lock (lockDrrAiBoats)
			{
				List<DataRequestReturnObject> listTemp = GetCurrentList(ref drrAIBoats);

				foreach (DataRequestReturnObject bmsoTemp in listTemp)
				{
					szTemp += "<Placemark>" +
						"<name>" + bmsoTemp.bmsoObject.szTitle + "</name><visibility>1</visibility><open>0</open>" +
						"<description><![CDATA[Microsoft Flight Simulator X - AI Boat<br>&nbsp;<br>" +
						"<b>Title:</b> " + bmsoTemp.bmsoObject.szTitle + "<br>&nbsp;<br>" +
						"<b>Altitude:</b> " + ((int)bmsoTemp.bmsoObject.dAltitude).ToString().Replace(",", ".") + " m<br>&nbsp;<br>" +
						"<center><img src=\"" + KmlGetImageLink(AccessMode, KML_IMAGE_TYPES.WATER, bmsoTemp.bmsoObject.szTitle, szServer) + "\"></center>]]></description>" +
						"<Snippet>Altitude: " + ((int)bmsoTemp.bmsoObject.dAltitude).ToString().Replace(",", ".") + " m</Snippet>" +
						"<Style>" +
						"<IconStyle><Icon><href>" + KmlGetIconLink(AccessMode, KML_ICON_TYPES.AI_BOAT, szServer) + "</href></Icon><scale>0.6</scale></IconStyle>" +
						"<LabelStyle><scale>0.6</scale></LabelStyle>" +
						"</Style>" +
						"<Point><altitudeMode>absolute</altitudeMode><coordinates>" + bmsoTemp.bmsoObject.dLongitude.ToString().Replace(",", ".") + "," + bmsoTemp.bmsoObject.dLatitude.ToString().Replace(",", ".") + "," + bmsoTemp.bmsoObject.dAltitude.ToString().Replace(",", ".") + "</coordinates><extrude>1</extrude></Point></Placemark>";
				}

				szTemp += "</Folder>";

				if (gconffixCurrent.bPredictAIBoats)
				{
					szTemp += "<Folder><name>Boat Courses</name>";

					foreach (DataRequestReturnObject bmsoTemp in listTemp)
					{
						if (bmsoTemp.szCoursePrediction == "")
							continue;

						szTemp += "<Placemark><name>" + bmsoTemp.bmsoObject.szTitle + "</name><description>Course prediction of the boat.</description><visibility>1</visibility><open>0</open><Style><LineStyle><color>9f00b545</color><width>2</width></LineStyle></Style><LineString><altitudeMode>absolute</altitudeMode><coordinates>" + bmsoTemp.szCoursePrediction + "</coordinates></LineString></Placemark>";

						PathPositionStored[] ppsTemp = bmsoTemp.ppsPredictionPoints;
						szTemp += KmlGenETAPoints(ref ppsTemp, gconffixCurrent.bPredictPointsAIBoats, AccessMode, KML_ICON_TYPES.AI_BOAT_PREDICTION_POINT, szServer);
					}

					szTemp += "</Folder>";
				}
			}

			return szTemp;
		}

		private String KmlGenAIGroundUnit(KML_ACCESS_MODES AccessMode, String szServer)
		{
			String szTemp = "<Folder><name>Ground Vehicle Positions</name>";

			lock (lockDrrAiGround)
			{
				List<DataRequestReturnObject> listTemp = GetCurrentList(ref drrAIGround);
				foreach (DataRequestReturnObject bmsoTemp in listTemp)
				{
					szTemp += "<Placemark>" +
						"<name>" + bmsoTemp.bmsoObject.szTitle + "</name><visibility>1</visibility><open>0</open>" +
						"<description><![CDATA[Microsoft Flight Simulator X - AI Vehicle<br>&nbsp;<br>" +
						"<b>Title:</b> " + bmsoTemp.bmsoObject.szTitle + "<br>&nbsp;<br>" +
						"<b>Altitude:</b> " + ((int)bmsoTemp.bmsoObject.dAltitude).ToString().Replace(",", ".") + " m<br>&nbsp;<br>" +
						"<center><img src=\"" + KmlGetImageLink(AccessMode, KML_IMAGE_TYPES.GROUND, bmsoTemp.bmsoObject.szTitle, szServer) + "\"></center>]]></description>" +
						"<Snippet>Altitude: " + ((int)bmsoTemp.bmsoObject.dAltitude).ToString().Replace(",", ".") + " m</Snippet>" +
						"<Style>" +
						"<IconStyle><Icon><href>" + KmlGetIconLink(AccessMode, KML_ICON_TYPES.AI_GROUND_UNIT, szServer) + "</href></Icon><scale>0.6</scale></IconStyle>" +
						"<LabelStyle><scale>0.6</scale></LabelStyle>" +
						"</Style>" +
						"<Point><altitudeMode>absolute</altitudeMode><coordinates>" + bmsoTemp.bmsoObject.dLongitude.ToString().Replace(",", ".") + "," + bmsoTemp.bmsoObject.dLatitude.ToString().Replace(",", ".") + "," + bmsoTemp.bmsoObject.dAltitude.ToString().Replace(",", ".") + "</coordinates><extrude>1</extrude></Point></Placemark>";
				}

				szTemp += "</Folder>";

				if (gconffixCurrent.bPredictAIGroundUnits)
				{
					szTemp += "<Folder><name>Ground Vehicle Courses</name>";

					foreach (DataRequestReturnObject bmsoTemp in listTemp)
					{
						if (bmsoTemp.szCoursePrediction == "")
							continue;

						szTemp += "<Placemark><name>" + bmsoTemp.bmsoObject.szTitle + "</name><description>Course prediction of the ground vehicle.</description><visibility>1</visibility><open>0</open><Style><LineStyle><color>9f00b545</color><width>2</width></LineStyle></Style><LineString><altitudeMode>absolute</altitudeMode><coordinates>" + bmsoTemp.szCoursePrediction + "</coordinates></LineString></Placemark>";

						PathPositionStored[] ppsTemp = bmsoTemp.ppsPredictionPoints;
						szTemp += KmlGenETAPoints(ref ppsTemp, gconffixCurrent.bPredictPointsAIGroundUnits, AccessMode, KML_ICON_TYPES.AI_GROUND_PREDICTION_POINT, szServer);
					}

					szTemp += "</Folder>";
				}
			}

			return szTemp;
		}


		//private String KmlGenFlightPlans(KML_ACCESS_MODES AccessMode, String szServer)
		//{
		//    String szTemp = "";

		//    lock (lockFlightPlanList)
		//    {
		//        foreach (FlightPlan fpTemp in listFlightPlans)
		//        {
		//            XmlDocument xmldTemp = fpTemp.xmldPlan;
		//            String szTempInner = "";

		//            String szTempWaypoints = "";
		//            String szPath = "";

		//            try
		//            {
		//                for (XmlNode xmlnTemp = xmldTemp["SimBase.Document"]["FlightPlan.FlightPlan"].FirstChild; xmlnTemp != null; xmlnTemp = xmlnTemp.NextSibling)
		//                {
		//                    if (xmlnTemp.Name.ToLower() != "atcwaypoint")
		//                        continue;

		//                    KML_ICON_TYPES iconType;
		//                    String szType = xmlnTemp["ATCWaypointType"].InnerText.ToLower();
		//                    if (szType == "intersection")
		//                        iconType = KML_ICON_TYPES.PLAN_INTER;
		//                    else if (szType == "ndb")
		//                        iconType = KML_ICON_TYPES.PLAN_NDB;
		//                    else if (szType == "vor")
		//                        iconType = KML_ICON_TYPES.PLAN_VOR;
		//                    else if (szType == "user")
		//                        iconType = KML_ICON_TYPES.PLAN_USER;
		//                    else if (szType == "airport")
		//                        iconType = KML_ICON_TYPES.PLAN_PORT;
		//                    else
		//                        iconType = KML_ICON_TYPES.UNKNOWN;

		//                    char[] szSeperator = { ',' };
		//                    String[] szCoordinates = xmlnTemp["WorldPosition"].InnerText.Split(szSeperator);

		//                    if (szCoordinates.GetLength(0) != 3)
		//                        throw new System.Exception("Invalid position value");

		//                    String szAirway = "", szICAOIdent = "", szICAORegion = "";
		//                    if (xmlnTemp["ATCAirway"] != null)
		//                        szAirway = xmlnTemp["ATCAirway"].InnerText;
		//                    if (xmlnTemp["ICAO"] != null)
		//                    {
		//                        if (xmlnTemp["ICAO"]["ICAOIdent"] != null)
		//                            szICAOIdent = xmlnTemp["ICAO"]["ICAOIdent"].InnerText;
		//                        if (xmlnTemp["ICAO"]["ICAORegion"] != null)
		//                            szICAORegion = xmlnTemp["ICAO"]["ICAORegion"].InnerText;
		//                    }

		//                    double dCurrentLong = ConvertDegToDouble(szCoordinates[1]);
		//                    double dCurrentLat = ConvertDegToDouble(szCoordinates[0]);
		//                    double dCurrentAlt = System.Double.Parse(szCoordinates[2]);

		//                    szTempWaypoints += "<Placemark>" +
		//                        "<name>" + xmlnTemp["ATCWaypointType"].InnerText + " (" + xmlnTemp.Attributes["id"].Value + ")</name><visibility>1</visibility><open>0</open>" +
		//                        "<description><![CDATA[Flight Plane Element<br>&nbsp;<br>" +
		//                        "<b>Waypoint Type:</b> " + xmlnTemp["ATCWaypointType"].InnerText + "<br>&nbsp;<br>" +
		//                        (szAirway != "" ? "<b>ATC Airway:</b> " + szAirway + "<br>&nbsp;<br>" : "") +
		//                        (szICAOIdent != "" ? "<b>ICAO Identification:</b> " + szICAOIdent + "<br>" : "") +
		//                        (szICAORegion != "" ? "<b>ICAO Region:</b> " + szICAORegion : "") +
		//                        "]]></description>" +
		//                        "<Snippet>Waypoint Type: " + xmlnTemp["ATCWaypointType"].InnerText + (szAirway != "" ? "\nAirway: " + szAirway : "") + "</Snippet>" +
		//                        "<Style>" +
		//                        "<IconStyle><Icon><href>" + KmlGetIconLink(AccessMode, iconType, szServer) + "</href></Icon><scale>1.0</scale></IconStyle>" +
		//                        "<LabelStyle><scale>0.6</scale></LabelStyle>" +
		//                        "</Style>" +
		//                        "<Point><altitudeMode>clampToGround</altitudeMode><coordinates>" + dCurrentLong.ToString().Replace(",", ".") + "," + dCurrentLat.ToString().Replace(",", ".") + "," + dCurrentAlt.ToString().Replace(",", ".") + "</coordinates><extrude>1</extrude></Point></Placemark>";

		//                    szPath += dCurrentLong.ToString().Replace(",", ".") + "," + dCurrentLat.ToString().Replace(",", ".") + "," + dCurrentAlt.ToString().Replace(",", ".") + "\n";
		//                }

		//                szTempInner = "<Folder><open>0</open>" +
		//                    "<name>" + (xmldTemp["SimBase.Document"]["FlightPlan.FlightPlan"]["Title"] != null ? xmldTemp["SimBase.Document"]["FlightPlan.FlightPlan"]["Title"].InnerText : "n/a") + "</name>" +
		//                    "<description><![CDATA[" +
		//                    "Type: " + (xmldTemp["SimBase.Document"]["FlightPlan.FlightPlan"]["FPType"] != null ? xmldTemp["SimBase.Document"]["FlightPlan.FlightPlan"]["FPType"].InnerText : "n/a") + " (" + (xmldTemp["SimBase.Document"]["FlightPlan.FlightPlan"]["RouteType"] != null ? xmldTemp["SimBase.Document"]["FlightPlan.FlightPlan"]["RouteType"].InnerText : "n/a") + ")<br>" +
		//                    "Flight from " + (xmldTemp["SimBase.Document"]["FlightPlan.FlightPlan"]["DepartureName"] != null ? xmldTemp["SimBase.Document"]["FlightPlan.FlightPlan"]["DepartureName"].InnerText : "n/a") + " to " + (xmldTemp["SimBase.Document"]["FlightPlan.FlightPlan"]["DestinationName"] != null ? xmldTemp["SimBase.Document"]["FlightPlan.FlightPlan"]["DestinationName"].InnerText : "n/a") + ".<br>&nbsp;<br>" +
		//                    "Altitude: " + (xmldTemp["SimBase.Document"]["FlightPlan.FlightPlan"]["CruisingAlt"] != null ? xmldTemp["SimBase.Document"]["FlightPlan.FlightPlan"]["CruisingAlt"].InnerText : "n/a") +
		//                    "]]></description>" +
		//                    "<Placemark><name>Path</name><Style><LineStyle><color>9f1ab6ff</color><width>2</width></LineStyle></Style><LineString><tessellate>1</tessellate><altitudeMode>clampToGround</altitudeMode><coordinates>" + szPath + "</coordinates></LineString></Placemark>" +
		//                    "<Folder><open>0</open><name>Waypoints</name>" + szTempWaypoints + "</Folder>" +
		//                    "</Folder>";
		//            }
		//            catch
		//            {
		//                szTemp += "<Folder><name>Invalid Flight Plan</name><snippet>Error loading flight plan.</snippet></Folder>";
		//                continue;
		//            }

		//            szTemp += szTempInner;
		//        }
		//    }

		//    return szTemp;
		//}


		#endregion


		#region Update Check

//        private void checkForProgramUpdate()
//        {
//            try
//            {
//                szOnlineVersionCheckData = "";

//                wrOnlineVersionCheck = WebRequest.Create("http://juergentreml.online.de/fsxget/provide/version.txt");
//                wrOnlineVersionCheck.BeginGetResponse(new AsyncCallback(RespCallback), wrOnlineVersionCheck);
//            }
//            catch
//            {
//#if DEBUG
//                notifyIconMain.ShowBalloonTip(5, Text, "Couldn't check for program update online!", ToolTipIcon.Warning);
//#endif
//            }
//        }

//        private void RespCallback(IAsyncResult asynchronousResult)
//        {
//            try
//            {
//                WebRequest myWebRequest = (WebRequest)asynchronousResult.AsyncState;
//                wrespOnlineVersionCheck = myWebRequest.EndGetResponse(asynchronousResult);
//                Stream responseStream = wrespOnlineVersionCheck.GetResponseStream();

//                responseStream.BeginRead(bOnlineVersionCheckRawData, 0, iOnlineVersionCheckRawDataLength, new AsyncCallback(ReadCallBack), responseStream);
//            }
//            catch
//            {
//#if DEBUG
//                notifyIconMain.ShowBalloonTip(5, Text, "Couldn't check for program update online!", ToolTipIcon.Warning);
//#endif
//            }
//        }

//        private void ReadCallBack(IAsyncResult asyncResult)
//        {
//            try
//            {
//                Stream responseStream = (Stream)asyncResult.AsyncState;
//                int iRead = responseStream.EndRead(asyncResult);
//                if (iRead > 0)
//                {
//                    szOnlineVersionCheckData += Encoding.ASCII.GetString(bOnlineVersionCheckRawData, 0, iRead);
//                    responseStream.BeginRead(bOnlineVersionCheckRawData, 0, iOnlineVersionCheckRawDataLength, new AsyncCallback(ReadCallBack), responseStream);
//                }
//                else
//                {
//                    responseStream.Close();
//                    wrespOnlineVersionCheck.Close();

//                    char[] szSeperator = { '.' };
//                    String[] szVersionLocal = Application.ProductVersion.Split(szSeperator);
//                    String[] szVersionOnline = szOnlineVersionCheckData.Split(szSeperator);
//                    for (int i = 0; i < Math.Min(szVersionLocal.GetLength(0), szVersionOnline.GetLength(0)); i++)
//                    {
//                        if (Int64.Parse(szVersionOnline[i]) > Int64.Parse(szVersionLocal[i]))
//                        {
//                            notifyIconMain.ShowBalloonTip(30, Text, "A new program version is available!\n\nLatest Version:\t" + szOnlineVersionCheckData + "\nYour Version:\t" + Application.ProductVersion, ToolTipIcon.Info);
//                            break;
//                        }
//                        else if (Int64.Parse(szVersionOnline[i]) < Int64.Parse(szVersionLocal[i]))
//                            break;
//                    }
//                }
//            }
//            catch
//            {
//#if DEBUG
//                notifyIconMain.ShowBalloonTip(5, Text, "Couldn't check for program update online!", ToolTipIcon.Warning);
//#endif
//            }
//        }


		#endregion


		#region Timers


		private void timerFSXConnect_Tick(object sender, EventArgs e)
		{
			if (openConnection())
			{
				timerFSXConnect.Stop();
				bConnected = true;
				notifyIconMain.Icon = icReceive;
				notifyIconMain.Text = Text + "(Waiting for connection...)";
				safeShowBalloonTip(1000, Text, "Connected to FSX!", ToolTipIcon.Info);
			}
		}

		private void timerIPAddressRefresh_Tick(object sender, EventArgs e)
		{
			IPHostEntry ipheLocalhost1 = Dns.GetHostEntry(Dns.GetHostName());
			IPHostEntry ipheLocalhost2 = Dns.GetHostEntry("localhost");

			lock (lockIPAddressList)
			{
				ipalLocal1 = ipheLocalhost1.AddressList;
				ipalLocal2 = ipheLocalhost2.AddressList;
			}
		}


		private void timerQueryUserAircraft_Tick(object sender, EventArgs e)
		{
			simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_USER_AIRCRAFT, DEFINITIONS.StructBasicMovingSceneryObject, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
		}

		private void timerQueryUserPath_Tick(object sender, EventArgs e)
		{
			simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_USER_PATH, DEFINITIONS.StructBasicMovingSceneryObject, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
		}

		private void timerUserPrediction_Tick(object sender, EventArgs e)
		{
			simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_USER_PREDICTION, DEFINITIONS.StructBasicMovingSceneryObject, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
		}


		private void timerQueryAIAircrafts_Tick(object sender, EventArgs e)
		{
			simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_AI_PLANE, DEFINITIONS.StructBasicMovingSceneryObject, (uint)gconffixCurrent.iRangeAIAircrafts, SIMCONNECT_SIMOBJECT_TYPE.AIRCRAFT);
		}

		private void timerQueryAIHelicopters_Tick(object sender, EventArgs e)
		{
			simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_AI_HELICOPTER, DEFINITIONS.StructBasicMovingSceneryObject, (uint)gconffixCurrent.iRangeAIHelicopters, SIMCONNECT_SIMOBJECT_TYPE.HELICOPTER);
		}

		private void timerQueryAIBoats_Tick(object sender, EventArgs e)
		{
			simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_AI_BOAT, DEFINITIONS.StructBasicMovingSceneryObject, (uint)gconffixCurrent.iRangeAIBoats, SIMCONNECT_SIMOBJECT_TYPE.BOAT);
		}

		private void timerQueryAIGroundUnits_Tick(object sender, EventArgs e)
		{
			simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_AI_GROUND, DEFINITIONS.StructBasicMovingSceneryObject, (uint)gconffixCurrent.iRangeAIGroundUnits, SIMCONNECT_SIMOBJECT_TYPE.GROUND);
		}


		#endregion


		#region Config File Read & Write

		private void ConfigMirrorToVariables()
		{
			lock (lockChConf)
			{
				gconfchCurrent.bEnabled = (xmldSettings["fsxget"]["settings"]["options"]["general"]["enable-on-startup"].Attributes["Enabled"].Value == "1");
				gconfchCurrent.bShowBalloons = (xmldSettings["fsxget"]["settings"]["options"]["general"]["show-balloon-tips"].Attributes["Enabled"].Value == "1");
			}
			gconffixCurrent.bLoadKMLFile = (xmldSettings["fsxget"]["settings"]["options"]["general"]["load-kml-file"].Attributes["Enabled"].Value == "1");
			//gconffixCurrent.bCheckForUpdates = (xmldSettings["fsxget"]["settings"]["options"]["general"]["update-check"].Attributes["Enabled"].Value == "1");


			gconffixCurrent.iTimerUserAircraft = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-user-aircraft"].Attributes["Interval"].Value);
			gconffixCurrent.bQueryUserAircraft = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-user-aircraft"].Attributes["Enabled"].Value == "1");


			gconffixCurrent.iTimerUserPath = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-user-path"].Attributes["Interval"].Value);
			gconffixCurrent.bQueryUserPath = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-user-path"].Attributes["Enabled"].Value == "1");

			gconffixCurrent.iTimerUserPathPrediction = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["user-path-prediction"].Attributes["Interval"].Value);
			gconffixCurrent.bUserPathPrediction = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["user-path-prediction"].Attributes["Enabled"].Value == "1");

			int iCount = 0;
			for (XmlNode xmlnTemp = xmldSettings["fsxget"]["settings"]["options"]["fsx"]["user-path-prediction"].FirstChild; xmlnTemp != null; xmlnTemp = xmlnTemp.NextSibling)
			{
				if (xmlnTemp.Name == "prediction-point")
					iCount++;
			}

			gconffixCurrent.dPredictionTimes = new double[iCount];
			iCount = 0;
			for (XmlNode xmlnTemp = xmldSettings["fsxget"]["settings"]["options"]["fsx"]["user-path-prediction"].FirstChild; xmlnTemp != null; xmlnTemp = xmlnTemp.NextSibling)
			{
				if (xmlnTemp.Name == "prediction-point")
				{
					gconffixCurrent.dPredictionTimes[iCount] = System.Int64.Parse(xmlnTemp.Attributes["Time"].Value);
					iCount++;
				}
			}


			gconffixCurrent.iTimerAIAircrafts = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["Interval"].Value);
			gconffixCurrent.iRangeAIAircrafts = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["Range"].Value);
			gconffixCurrent.bQueryAIAircrafts = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["Enabled"].Value == "1");
			gconffixCurrent.bPredictAIAircrafts = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["Prediction"].Value == "1");
			gconffixCurrent.bPredictPointsAIAircrafts = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["PredictionPoints"].Value == "1");

			gconffixCurrent.iTimerAIHelicopters = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["Interval"].Value);
			gconffixCurrent.iRangeAIHelicopters = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["Range"].Value);
			gconffixCurrent.bQueryAIHelicopters = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["Enabled"].Value == "1");
			gconffixCurrent.bPredictAIHelicopters = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["Prediction"].Value == "1");
			gconffixCurrent.bPredictPointsAIHelicopters = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["PredictionPoints"].Value == "1");

			gconffixCurrent.iTimerAIBoats = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["Interval"].Value);
			gconffixCurrent.iRangeAIBoats = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["Range"].Value);
			gconffixCurrent.bQueryAIBoats = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["Enabled"].Value == "1");
			gconffixCurrent.bPredictAIBoats = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["Prediction"].Value == "1");
			gconffixCurrent.bPredictPointsAIBoats = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["PredictionPoints"].Value == "1");

			gconffixCurrent.iTimerAIGroundUnits = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["Interval"].Value);
			gconffixCurrent.iRangeAIGroundUnits = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["Range"].Value);
			gconffixCurrent.bQueryAIGroundUnits = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["Enabled"].Value == "1");
			gconffixCurrent.bPredictAIGroundUnits = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["Prediction"].Value == "1");
			gconffixCurrent.bPredictPointsAIGroundUnits = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["PredictionPoints"].Value == "1");

			gconffixCurrent.bQueryAIObjects = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"].Attributes["Enabled"].Value == "1");


			gconffixCurrent.iUpdateGEUserAircraft = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["user-aircraft"].Attributes["Interval"].Value);
			gconffixCurrent.iUpdateGEUserPath = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["user-path"].Attributes["Interval"].Value);
			gconffixCurrent.iUpdateGEUserPrediction = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["user-path-prediction"].Attributes["Interval"].Value);
			gconffixCurrent.iUpdateGEAIAircrafts = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["ai-aircrafts"].Attributes["Interval"].Value);
			gconffixCurrent.iUpdateGEAIHelicopters = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["ai-helicopters"].Attributes["Interval"].Value);
			gconffixCurrent.iUpdateGEAIBoats = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["ai-boats"].Attributes["Interval"].Value);
			gconffixCurrent.iUpdateGEAIGroundUnits = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["ai-ground-units"].Attributes["Interval"].Value);

			gconffixCurrent.iServerPort = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["server-settings"]["port"].Attributes["Value"].Value);
			gconffixCurrent.uiServerAccessLevel = (uint)System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["server-settings"]["access-level"].Attributes["Value"].Value);


			//gconffixCurrent.bLoadFlightPlans = (xmldSettings["fsxget"]["settings"]["options"]["flightplans"].Attributes["Enabled"].Value == "1");

			gconffixCurrent.szUserdefinedPath = "";
		}


		private void ConfigMirrorToForm()
		{
			checkEnableOnStartup.Checked = (xmldSettings["fsxget"]["settings"]["options"]["general"]["enable-on-startup"].Attributes["Enabled"].Value == "1");
			checkShowInfoBalloons.Checked = (xmldSettings["fsxget"]["settings"]["options"]["general"]["show-balloon-tips"].Attributes["Enabled"].Value == "1");
			checkBoxLoadKMLFile.Checked = (xmldSettings["fsxget"]["settings"]["options"]["general"]["load-kml-file"].Attributes["Enabled"].Value == "1");
			//checkBoxUpdateCheck.Checked = (xmldSettings["fsxget"]["settings"]["options"]["general"]["update-check"].Attributes["Enabled"].Value == "1");

			numericUpDownQueryUserAircraft.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-user-aircraft"].Attributes["Interval"].Value);
			checkQueryUserAircraft.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-user-aircraft"].Attributes["Enabled"].Value == "1");

			numericUpDownQueryUserPath.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-user-path"].Attributes["Interval"].Value);
			checkQueryUserPath.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-user-path"].Attributes["Enabled"].Value == "1");

			numericUpDownUserPathPrediction.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["user-path-prediction"].Attributes["Interval"].Value);
			checkBoxUserPathPrediction.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["user-path-prediction"].Attributes["Enabled"].Value == "1");

			listBoxPathPrediction.Items.Clear();
			for (XmlNode xmlnTemp = xmldSettings["fsxget"]["settings"]["options"]["fsx"]["user-path-prediction"].FirstChild; xmlnTemp != null; xmlnTemp = xmlnTemp.NextSibling)
			{
				if (xmlnTemp.Name == "prediction-point")
				{
					ListBoxPredictionTimesItem lbptiTemp = new ListBoxPredictionTimesItem();
					lbptiTemp.dTime = System.Int64.Parse(xmlnTemp.Attributes["Time"].Value);
					bool bInserted = false;
					for (int n = 0; n < listBoxPathPrediction.Items.Count; n++)
					{
						if (((ListBoxPredictionTimesItem)listBoxPathPrediction.Items[n]).dTime > lbptiTemp.dTime)
						{
							listBoxPathPrediction.Items.Insert(n, lbptiTemp);
							bInserted = true;
							break;
						}
					}
					if (!bInserted)
						listBoxPathPrediction.Items.Add(lbptiTemp);
				}
			}


			numericUpDownQueryAIAircraftsInterval.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["Interval"].Value);
			numericUpDownQueryAIAircraftsRadius.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["Range"].Value);
			checkBoxAIAircraftsPredictPoints.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["PredictionPoints"].Value == "1");
			checkBoxAIAircraftsPredict.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["Prediction"].Value == "1");
			checkBoxAIAircraftsPredict_CheckedChanged(null, null);
			checkBoxQueryAIAircrafts.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["Enabled"].Value == "1");
			checkBoxQueryAIAircrafts_CheckedChanged(null, null);

			numericUpDownQueryAIHelicoptersInterval.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["Interval"].Value);
			numericUpDownQueryAIHelicoptersRadius.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["Range"].Value);
			checkBoxAIHelicoptersPredictPoints.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["PredictionPoints"].Value == "1");
			checkBoxAIHelicoptersPredict.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["Prediction"].Value == "1");
			checkBoxAIHelicoptersPredict_CheckedChanged(null, null);
			checkBoxQueryAIHelicopters.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["Enabled"].Value == "1");
			checkBoxQueryAIHelicopters_CheckedChanged(null, null);

			numericUpDownQueryAIBoatsInterval.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["Interval"].Value);
			numericUpDownQueryAIBoatsRadius.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["Range"].Value);
			checkBoxAIBoatsPredictPoints.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["PredictionPoints"].Value == "1");
			checkBoxAIBoatsPredict.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["Prediction"].Value == "1");
			checkBoxAIBoatsPredict_CheckedChanged(null, null);
			checkBoxQueryAIBoats.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["Enabled"].Value == "1");
			checkBoxQueryAIBoats_CheckedChanged(null, null);

			numericUpDownQueryAIGroudUnitsInterval.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["Interval"].Value);
			numericUpDownQueryAIGroudUnitsRadius.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["Range"].Value);
			checkBoxAIGroundPredictPoints.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["PredictionPoints"].Value == "1");
			checkBoxAIGroundPredict.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["Prediction"].Value == "1");
			checkBoxAIGroundPredict_CheckedChanged(null, null);
			checkBoxQueryAIGroudUnits.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["Enabled"].Value == "1");
			checkBoxQueryAIGroudUnits_CheckedChanged(null, null);

			checkBoxQueryAIObjects.Checked = (xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"].Attributes["Enabled"].Value == "1");
			checkBoxQueryAIObjects_CheckedChanged(null, null);


			numericUpDownRefreshUserAircraft.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["user-aircraft"].Attributes["Interval"].Value);
			numericUpDownRefreshUserPath.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["user-path"].Attributes["Interval"].Value);
			numericUpDownRefreshUserPrediction.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["user-path-prediction"].Attributes["Interval"].Value);
			numericUpDownRefreshAIAircrafts.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["ai-aircrafts"].Attributes["Interval"].Value);
			numericUpDownRefreshAIHelicopter.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["ai-helicopters"].Attributes["Interval"].Value);
			numericUpDownRefreshAIBoats.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["ai-boats"].Attributes["Interval"].Value);
			numericUpDownRefreshAIGroundUnits.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["ai-ground-units"].Attributes["Interval"].Value);

			numericUpDownServerPort.Value = System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["server-settings"]["port"].Attributes["Value"].Value);

			if (System.Int64.Parse(xmldSettings["fsxget"]["settings"]["options"]["ge"]["server-settings"]["access-level"].Attributes["Value"].Value) == 1)
				radioButtonAccessRemote.Checked = true;
			else
				radioButtonAccessLocalOnly.Checked = true;


			//checkBoxLoadFlightPlans.Checked = (xmldSettings["fsxget"]["settings"]["options"]["flightplans"].Attributes["Enabled"].Value == "1");

			//listViewFlightPlans.Items.Clear();
			//int iCount = 0;
			//for (XmlNode xmlnTemp = xmldSettings["fsxget"]["settings"]["options"]["flightplans"].FirstChild; xmlnTemp != null; xmlnTemp = xmlnTemp.NextSibling)
			//{
			//    ListViewItem lviTemp = listViewFlightPlans.Items.Insert(iCount, xmlnTemp.Attributes["Name"].Value);
			//    lviTemp.Checked = (xmlnTemp.Attributes["Show"].Value == "1" ? true : false);
			//    lviTemp.SubItems.Add(xmlnTemp.Attributes["File"].Value);
				
			//    iCount++;
			//}


			UpdateCheckBoxStates();
		}

		private void ConfigRetrieveFromForm()
		{
			xmldSettings["fsxget"]["settings"]["options"]["general"]["enable-on-startup"].Attributes["Enabled"].Value = checkEnableOnStartup.Checked ? "1" : "0";
			xmldSettings["fsxget"]["settings"]["options"]["general"]["show-balloon-tips"].Attributes["Enabled"].Value = checkShowInfoBalloons.Checked ? "1" : "0";
			xmldSettings["fsxget"]["settings"]["options"]["general"]["load-kml-file"].Attributes["Enabled"].Value = checkBoxLoadKMLFile.Checked ? "1" : "0";
			//xmldSettings["fsxget"]["settings"]["options"]["general"]["update-check"].Attributes["Enabled"].Value = checkBoxUpdateCheck.Checked ? "1" : "0";


			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-user-aircraft"].Attributes["Interval"].Value = numericUpDownQueryUserAircraft.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-user-aircraft"].Attributes["Enabled"].Value = checkQueryUserAircraft.Checked ? "1" : "0";

			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-user-path"].Attributes["Interval"].Value = numericUpDownQueryUserPath.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-user-path"].Attributes["Enabled"].Value = checkQueryUserPath.Checked ? "1" : "0";


			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["user-path-prediction"].Attributes["Interval"].Value = numericUpDownUserPathPrediction.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["user-path-prediction"].Attributes["Enabled"].Value = checkBoxUserPathPrediction.Checked ? "1" : "0";

			XmlNode xmlnTempLoop = xmldSettings["fsxget"]["settings"]["options"]["fsx"]["user-path-prediction"].FirstChild;
			while (xmlnTempLoop != null)
			{
				XmlNode xmlnDelete = xmlnTempLoop;
				xmlnTempLoop = xmlnTempLoop.NextSibling;

				xmldSettings["fsxget"]["settings"]["options"]["fsx"]["user-path-prediction"].RemoveChild(xmlnDelete);
			}

			for (int n = 0; n < listBoxPathPrediction.Items.Count; n++)
			{

				XmlNode xmlnTemp = xmldSettings.CreateElement("prediction-point");
				XmlAttribute xmlaTemp = xmldSettings.CreateAttribute("Time");

				xmlaTemp.Value = ((ListBoxPredictionTimesItem)listBoxPathPrediction.Items[n]).dTime.ToString();

				xmlnTemp.Attributes.Append(xmlaTemp);
				xmldSettings["fsxget"]["settings"]["options"]["fsx"]["user-path-prediction"].AppendChild(xmlnTemp);
			}


			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["Interval"].Value = numericUpDownQueryAIAircraftsInterval.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["Range"].Value = numericUpDownQueryAIAircraftsRadius.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["Enabled"].Value = checkBoxQueryAIAircrafts.Checked ? "1" : "0";
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["Prediction"].Value = checkBoxAIAircraftsPredict.Checked ? "1" : "0";
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-aircrafts"].Attributes["PredictionPoints"].Value = checkBoxAIAircraftsPredictPoints.Checked ? "1" : "0";

			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["Interval"].Value = numericUpDownQueryAIHelicoptersInterval.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["Range"].Value = numericUpDownQueryAIHelicoptersRadius.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["Enabled"].Value = checkBoxQueryAIHelicopters.Checked ? "1" : "0";
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["Prediction"].Value = checkBoxAIHelicoptersPredict.Checked ? "1" : "0";
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-helicopters"].Attributes["PredictionPoints"].Value = checkBoxAIHelicoptersPredictPoints.Checked ? "1" : "0";

			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["Interval"].Value = numericUpDownQueryAIBoatsInterval.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["Range"].Value = numericUpDownQueryAIBoatsRadius.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["Enabled"].Value = checkBoxQueryAIBoats.Checked ? "1" : "0";
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["Prediction"].Value = checkBoxAIBoatsPredict.Checked ? "1" : "0";
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-boats"].Attributes["PredictionPoints"].Value = checkBoxAIBoatsPredictPoints.Checked ? "1" : "0";

			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["Interval"].Value = numericUpDownQueryAIGroudUnitsInterval.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["Range"].Value = numericUpDownQueryAIGroudUnitsRadius.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["Enabled"].Value = checkBoxQueryAIGroudUnits.Checked ? "1" : "0";
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["Prediction"].Value = checkBoxAIGroundPredict.Checked ? "1" : "0";
			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"]["query-ai-ground-units"].Attributes["PredictionPoints"].Value = checkBoxAIGroundPredictPoints.Checked ? "1" : "0";

			xmldSettings["fsxget"]["settings"]["options"]["fsx"]["query-ai-objects"].Attributes["Enabled"].Value = checkBoxQueryAIObjects.Checked ? "1" : "0";


			xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["user-aircraft"].Attributes["Interval"].Value = numericUpDownRefreshUserAircraft.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["user-path"].Attributes["Interval"].Value = numericUpDownRefreshUserPath.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["user-path-prediction"].Attributes["Interval"].Value = numericUpDownRefreshUserPrediction.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["ai-aircrafts"].Attributes["Interval"].Value = numericUpDownRefreshAIAircrafts.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["ai-helicopters"].Attributes["Interval"].Value = numericUpDownRefreshAIHelicopter.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["ai-boats"].Attributes["Interval"].Value = numericUpDownRefreshAIBoats.Value.ToString();
			xmldSettings["fsxget"]["settings"]["options"]["ge"]["refresh-rates"]["ai-ground-units"].Attributes["Interval"].Value = numericUpDownRefreshAIGroundUnits.Value.ToString();

			xmldSettings["fsxget"]["settings"]["options"]["ge"]["server-settings"]["port"].Attributes["Value"].Value = numericUpDownServerPort.Value.ToString();

			if (radioButtonAccessRemote.Checked)
				xmldSettings["fsxget"]["settings"]["options"]["ge"]["server-settings"]["access-level"].Attributes["Value"].Value = "1";
			else
				xmldSettings["fsxget"]["settings"]["options"]["ge"]["server-settings"]["access-level"].Attributes["Value"].Value = "0";


			//xmldSettings["fsxget"]["settings"]["options"]["flightplans"].Attributes["Enabled"].Value = checkBoxLoadFlightPlans.Checked ? "1" : "0";
		}


		//private void LoadFlightPlans()
		//{
		//    bool bError = false;

		//    FlightPlan fpTemp;
		//    XmlDocument xmldTemp = new XmlDocument();

		//    try
		//    {
		//        int iCount = 0;
				
		//        fpTemp.szName = "";
		//        fpTemp.uiID = 0;
		//        fpTemp.xmldPlan = null;

		//        for (XmlNode xmlnTemp = xmldSettings["fsxget"]["settings"]["options"]["flightplans"].FirstChild; xmlnTemp != null; xmlnTemp = xmlnTemp.NextSibling)
		//        {
		//            try
		//            {
		//                if (xmlnTemp.Attributes["Show"].Value == "0")
		//                    continue;

		//                XmlReader xmlrTemp = new XmlTextReader(xmlnTemp.Attributes["File"].Value);

		//                fpTemp.uiID = iCount;
		//                fpTemp.xmldPlan = new XmlDocument();
		//                fpTemp.xmldPlan.Load(xmlrTemp);

		//                xmlrTemp.Close();
		//                xmlrTemp = null;
		//            }
		//            catch
		//            {
		//                bError = true;
		//                continue;
		//            }

		//            lock (lockFlightPlanList)
		//            {
		//                listFlightPlans.Add(fpTemp);
		//            }
		//            iCount++;
		//        }
		//    }
		//    catch
		//    {
		//        MessageBox.Show("Could not read flight plan list from settings file! No flight plans will be loaded.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		//    }

		//    if (bError)
		//        MessageBox.Show("There were errors loading some of the flight plans! These flight plans will not be shown.\n\nThis problem might be due to incorrect or no longer existing flight plan files.\nPlease remove them from the flight plan list in the options dialog.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

		//}


		#endregion


		#region Assembly Attribute Accessors

		public string AssemblyTitle
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

		public string AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		public string AssemblyDescription
		{
			get
			{
				// Get all Description attributes on this assembly
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				// If there aren't any Description attributes, return an empty string
				if (attributes.Length == 0)
					return "";
				// If there is a Description attribute, return its value
				return ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

		public string AssemblyProduct
		{
			get
			{
				// Get all Product attributes on this assembly
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				// If there aren't any Product attributes, return an empty string
				if (attributes.Length == 0)
					return "";
				// If there is a Product attribute, return its value
				return ((AssemblyProductAttribute)attributes[0]).Product;
			}
		}

		public string AssemblyCopyright
		{
			get
			{
				// Get all Copyright attributes on this assembly
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				// If there aren't any Copyright attributes, return an empty string
				if (attributes.Length == 0)
					return "";
				// If there is a Copyright attribute, return its value
				return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		public string AssemblyCompany
		{
			get
			{
				// Get all Company attributes on this assembly
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				// If there aren't any Company attributes, return an empty string
				if (attributes.Length == 0)
					return "";
				// If there is a Company attribute, return its value
				return ((AssemblyCompanyAttribute)attributes[0]).Company;
			}
		}

		#endregion


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

		private void enableTrackerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			bool bTemp = enableTrackerToolStripMenuItem.Checked;

			lock (lockChConf)
			{
				if (bTemp != gconfchCurrent.bEnabled)
				{
					gconfchCurrent.bEnabled = bTemp;
					if (gconfchCurrent.bEnabled)
						globalConnect();
					else
						globalDisconnect();
				}
			}
		}

		private void showBalloonTipsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			lock (lockChConf)
			{
				gconfchCurrent.bShowBalloons = showBalloonTipsToolStripMenuItem.Checked;
			}

			// This call is safe as the existence of this key has been checked by calling configMirrorToForm at startup.
			xmldSettings["fsxget"]["settings"]["options"]["general"]["show-balloon-tips"].Attributes["Enabled"].Value = showBalloonTipsToolStripMenuItem.Checked ? "1" : "0";
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			safeShowMainDialog(5);
		}

		private void clearUserAircraftPathToolStripMenuItem_Click(object sender, EventArgs e)
		{
			lock (lockKmlUserPath)
			{
				szKmlUserAircraftPath = "";
			}

			lock (lockKmlUserPrediction)
			{
				szKmlUserPrediction = "";
				listKmlPredictionPoints.Clear();
			}

			lock (lockKmlPredictionPoints)
			{
				clearPPStructure(ref ppPos1);
				clearPPStructure(ref ppPos2);
			}
		}

		private void runMicrosoftFlightSimulatorXToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start(szPathFSX);
			}
			catch
			{
				MessageBox.Show("An error occured while trying to start Microsoft Flight Simulator X.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void runGoogleEarthToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				lock (lockListenerControl)
				{
					if (gconffixCurrent.bLoadKMLFile && bConnected)
						System.Diagnostics.Process.Start(szUserAppPath + "\\pub\\fsxgetd.kml");
					else
						System.Diagnostics.Process.Start(szUserAppPath + "\\pub\\fsxgets.kml");
				}
			}
			catch
			{
				MessageBox.Show("An error occured while trying to start Google Earth.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}


		private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
		{
			if (notifyIconMain.ContextMenuStrip == null)
				this.Activate();
		}


		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				linkLabel1.LinkVisited = true;
				System.Diagnostics.Process.Start("http://www.juergentreml.de/fsxget/");
			}
			catch
			{
				MessageBox.Show("Unable to open http://www.juergentreml.de/fsxget/!");
			}
		}


		private void checkBoxQueryAIObjects_CheckedChanged(object sender, EventArgs e)
		{
			checkBoxQueryAIAircrafts.Enabled = checkBoxQueryAIBoats.Enabled = checkBoxQueryAIGroudUnits.Enabled = checkBoxQueryAIHelicopters.Enabled = checkBoxQueryAIObjects.Checked;
			bRestartRequired = true;
		}

		private void checkBoxQueryAIAircrafts_CheckedChanged(object sender, EventArgs e)
		{
			checkBoxQueryAIAircrafts_EnabledChanged(null, null);
			bRestartRequired = true;
		}

		private void checkBoxQueryAIAircrafts_EnabledChanged(object sender, EventArgs e)
		{
			checkBoxAIAircraftsPredict.Enabled = numericUpDownQueryAIAircraftsInterval.Enabled = numericUpDownQueryAIAircraftsRadius.Enabled = (checkBoxQueryAIAircrafts.Enabled & checkBoxQueryAIAircrafts.Checked);
		}

		private void checkBoxQueryAIHelicopters_CheckedChanged(object sender, EventArgs e)
		{
			checkBoxQueryAIHelicopters_EnabledChanged(null, null);
			bRestartRequired = true;
		}

		private void checkBoxQueryAIHelicopters_EnabledChanged(object sender, EventArgs e)
		{
			checkBoxAIHelicoptersPredict.Enabled = numericUpDownQueryAIHelicoptersInterval.Enabled = numericUpDownQueryAIHelicoptersRadius.Enabled = (checkBoxQueryAIHelicopters.Enabled & checkBoxQueryAIHelicopters.Checked);
		}

		private void checkBoxQueryAIBoats_CheckedChanged(object sender, EventArgs e)
		{
			checkBoxQueryAIBoats_EnabledChanged(null, null);
			bRestartRequired = true;
		}

		private void checkBoxQueryAIBoats_EnabledChanged(object sender, EventArgs e)
		{
			checkBoxAIBoatsPredict.Enabled = numericUpDownQueryAIBoatsInterval.Enabled = numericUpDownQueryAIBoatsRadius.Enabled = (checkBoxQueryAIBoats.Enabled & checkBoxQueryAIBoats.Checked);
		}

		private void checkBoxQueryAIGroudUnits_CheckedChanged(object sender, EventArgs e)
		{
			checkBoxQueryAIGroudUnits_EnabledChanged(null, null);
			bRestartRequired = true;
		}

		private void checkBoxQueryAIGroudUnits_EnabledChanged(object sender, EventArgs e)
		{
			checkBoxAIGroundPredict.Enabled = numericUpDownQueryAIGroudUnitsInterval.Enabled = numericUpDownQueryAIGroudUnitsRadius.Enabled = (checkBoxQueryAIGroudUnits.Enabled & checkBoxQueryAIGroudUnits.Checked);
		}


		private void checkQueryUserAircraft_CheckedChanged(object sender, EventArgs e)
		{
			numericUpDownQueryUserAircraft.Enabled = checkQueryUserAircraft.Checked;
			bRestartRequired = true;
		}

		private void checkQueryUserPath_CheckedChanged(object sender, EventArgs e)
		{
			numericUpDownQueryUserPath.Enabled = checkQueryUserPath.Checked;
			bRestartRequired = true;
		}


		private void buttonOK_Click(object sender, EventArgs e)
		{
			// Set autostart if necessary
			string szRun = (string)Registry.GetValue(szRegKeyRun, AssemblyTitle, "");

			try
			{
				if (szRun != Application.ExecutablePath && checkBoxAutostart.Checked)
					Registry.SetValue(szRegKeyRun, AssemblyTitle, Application.ExecutablePath);
				else if (szRun == Application.ExecutablePath && !checkBoxAutostart.Checked)
				{
					RegistryKey regkTemp = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
					regkTemp.DeleteValue(AssemblyTitle);
				}
			}
			catch
			{
				MessageBox.Show("Couldn't change autorun value in registry!", AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

			ConfigRetrieveFromForm();

			if (bRestartRequired)
				MessageBox.Show("Some of the changes you made require a restart. Please restart " + Text + " for those changes to take effect.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

			showBalloonTipsToolStripMenuItem.Checked = checkShowInfoBalloons.Checked;

			notifyIconMain.ContextMenuStrip = contextMenuStripNotifyIcon;
			Hide();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			safeHideMainDialog();
		}


		private void numericUpDownQueryUserAircraft_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownQueryUserPath_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownQueryAIAircraftsInterval_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownQueryAIHelicoptersInterval_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownQueryAIBoatsInterval_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownQueryAIGroudUnitsInterval_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownQueryAIAircraftsRadius_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownQueryAIHelicoptersRadius_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownQueryAIBoatsRadius_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownQueryAIGroudUnitsRadius_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownRefreshUserAircraft_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownRefreshUserPath_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownRefreshAIAircrafts_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownRefreshAIHelicopter_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownRefreshAIBoats_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownRefreshAIGroundUnits_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownServerPort_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void checkBoxLoadKMLFile_CheckedChanged(object sender, EventArgs e)
		{
			gconffixCurrent.bLoadKMLFile = checkBoxLoadKMLFile.Checked;
		}

		private void checkBoxUserPathPrediction_CheckedChanged(object sender, EventArgs e)
		{
			numericUpDownUserPathPrediction.Enabled = checkBoxUserPathPrediction.Checked;
			bRestartRequired = true;
		}

		private void numericUpDownUserPathPrediction_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void numericUpDownRefreshUserPrediction_ValueChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void checkBoxSaveLog_CheckedChanged(object sender, EventArgs e)
		{
			checkBoxSubFoldersForLog.Enabled = (checkBoxSaveLog.Checked);
		}

		private void radioButtonAccessLocalOnly_CheckedChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void radioButtonAccessRemote_CheckedChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void checkBoxAIAircraftsPredict_CheckedChanged(object sender, EventArgs e)
		{
			checkBoxAIAircraftsPredict_EnabledChanged(null, null);
			bRestartRequired = true;
		}

		private void checkBoxAIAircraftsPredict_EnabledChanged(object sender, EventArgs e)
		{
			checkBoxAIAircraftsPredictPoints.Enabled = (checkBoxAIAircraftsPredict.Enabled & checkBoxAIAircraftsPredict.Checked);
		}

		private void checkBoxAIHelicoptersPredict_CheckedChanged(object sender, EventArgs e)
		{
			checkBoxAIHelicoptersPredict_EnabledChanged(null, null);
			bRestartRequired = true;
		}

		private void checkBoxAIHelicoptersPredict_EnabledChanged(object sender, EventArgs e)
		{
			checkBoxAIHelicoptersPredictPoints.Enabled = (checkBoxAIHelicoptersPredict.Enabled & checkBoxAIHelicoptersPredict.Checked);
		}

		private void checkBoxAIBoatsPredict_CheckedChanged(object sender, EventArgs e)
		{
			checkBoxAIBoatsPredict_EnabledChanged(null, null);
			bRestartRequired = true;
		}

		private void checkBoxAIBoatsPredict_EnabledChanged(object sender, EventArgs e)
		{
			checkBoxAIBoatsPredictPoints.Enabled = (checkBoxAIBoatsPredict.Enabled & checkBoxAIBoatsPredict.Checked);
		}

		private void checkBoxAIGroundPredict_CheckedChanged(object sender, EventArgs e)
		{
			checkBoxAIGroundPredict_EnabledChanged(null, null);
			bRestartRequired = true;
		}

		private void checkBoxAIGroundPredict_EnabledChanged(object sender, EventArgs e)
		{
			checkBoxAIGroundPredictPoints.Enabled = (checkBoxAIGroundPredict.Enabled & checkBoxAIGroundPredict.Checked);
		}


		private void checkBoxAIAircraftsPredictPoints_CheckedChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void checkBoxAIHelicoptersPredictPoints_CheckedChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void checkBoxAIBoatsPredictPoints_CheckedChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}

		private void checkBoxAIGroundPredictPoints_CheckedChanged(object sender, EventArgs e)
		{
			bRestartRequired = true;
		}


		private void createGoogleEarthKMLFileToolStripMenuItem_DropDownIPClick(object sender, EventArgs e)
		{
			String szTemp = sender.ToString();

			if (szTemp.Length < 7)
				return;

			szTemp = szTemp.Substring(7);

			String szKMLFile = "";
			if (CompileKMLStartUpFileDynamic(szTemp, ref szKMLFile))
			{
				safeShowMainDialog(0);

				if (saveFileDialogKMLFile.ShowDialog() == DialogResult.OK)
				{
					try
					{
						File.WriteAllText(saveFileDialogKMLFile.FileName, szKMLFile);
					}
					catch
					{
						MessageBox.Show("Could not save KML file!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}

				safeHideMainDialog();
			}
		}

		private void contextMenuStripNotifyIcon_Opening(object sender, CancelEventArgs e)
		{
			createGoogleEarthKMLFileToolStripMenuItem.DropDown.Items.Clear();
			lock (lockIPAddressList)
			{
				bool bAddressFound = false;

				if (ipalLocal1 != null)
				{
					foreach (IPAddress ipaTemp in ipalLocal1)
					{
						bAddressFound = true;
						createGoogleEarthKMLFileToolStripMenuItem.DropDown.Items.Add("For IP " + ipaTemp.ToString(), null, createGoogleEarthKMLFileToolStripMenuItem_DropDownIPClick);
					}
				}

				if (ipalLocal2 != null)
				{
					foreach (IPAddress ipaTemp in ipalLocal2)
					{
						bAddressFound = true;
						createGoogleEarthKMLFileToolStripMenuItem.DropDown.Items.Add("For IP " + ipaTemp.ToString(), null, createGoogleEarthKMLFileToolStripMenuItem_DropDownIPClick);
					}
				}

				if (!bAddressFound)
					createGoogleEarthKMLFileToolStripMenuItem.Enabled = false;
				else
					createGoogleEarthKMLFileToolStripMenuItem.Enabled = true;
			}
		}


		#endregion

		private void button3_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to remove the selected items?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				//foreach (ListViewItem lviTemp in listViewFlightPlans.SelectedItems)
				//{
				//    listViewFlightPlans.Items.Remove(lviTemp);
				//}
			}
		}
	}
}