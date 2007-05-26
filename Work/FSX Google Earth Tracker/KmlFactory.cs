using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Web;
using System.Data.OleDb;
using System.Drawing;

namespace Fsxget
{
	public class KmlFactory
	{
		#region Structs and Enums
		public enum KML_ICON_TYPES
		{
			USER_AIRCRAFT_POSITION = 0,
			USER_PREDICTION_POINT,
			AI_AIRCRAFT_PREDICTION_POINT,
			AI_HELICOPTER_PREDICTION_POINT,
			AI_BOAT_PREDICTION_POINT,
			AI_GROUND_PREDICTION_POINT,
			AI_AIRCRAFT,
			AI_HELICOPTER,
			AI_BOAT,
			AI_GROUND_UNIT,
			VOR,
			VORDME,
			VOR_OVERLAY,
			DME,
			NDB,
			AIRPORT,
			PLAN_USER,
			PLAN_INTER,
			NONE,
		};
		#endregion

		#region Variables
		static String[] strIconNames = new String[] 
            {
                "fsxu.png",
                "fsxpm.png",
                "fsxaippp.png",
                "fsxaihpp.png",
                "fsxaibpp.png",
                "fsxaigpp.png",
                "fsxaip.png",
                "fsxaih.png",
                "fsxaib.png",
                "fsxaig.png",
                "fsxvor.png",
                "fsxvordme.png",
                "fsxvorov.png",
                "fsxdme.png",
                "fsxndb.png",
                "fsxairport.png",
                "fsx-p-user.png",
                "fsx-p-inter.png",
                ""
            };
        protected static String[] strRunwayDirections = new String[] {
            "N",
            "NE",
            "SE",
            "S",
            "SW",
            "W",
            "NW"
        };

        protected static float[][] fSpeedAlt;

		protected FsxConnection fsxCon;
		protected HttpServer httpServer;

		protected String strUpdateKMLHeader;
		protected String strUpdateKMLFooter;
		protected Hashtable htKMLParts;
		protected Hashtable htFlightPlans;
		protected OleDbConnection dbCon;
		protected static double dPI180 = Math.PI / 180;
		protected static double d180PI = 180 / Math.PI;

		#endregion

		public KmlFactory(ref FsxConnection fsxCon, ref HttpServer httpServer)
		{
			fSpeedAlt = new float[9][];

            fSpeedAlt[0] = new float[3];
            fSpeedAlt[0][0] = 0;
            fSpeedAlt[0][1] = 100;
            
            fSpeedAlt[1] = new float[3];
            fSpeedAlt[1][0] = 0;
            fSpeedAlt[1][1] = 100;

            fSpeedAlt[2] = new float[3];
            fSpeedAlt[2][0] = 20;
            fSpeedAlt[2][1] = 200;

            fSpeedAlt[3] = new float[3];
            fSpeedAlt[3][0] = 60;
            fSpeedAlt[3][1] = 5000;

            fSpeedAlt[4] = new float[3];
            fSpeedAlt[4][0] = 80;
            fSpeedAlt[4][1] = 40000;

            fSpeedAlt[5] = new float[3];
            fSpeedAlt[5][0] = 120;
            fSpeedAlt[5][1] = 150000;

            fSpeedAlt[6] = new float[3];
            fSpeedAlt[6][0] = 200;
            fSpeedAlt[6][1] = 200000;

            fSpeedAlt[7] = new float[3];
            fSpeedAlt[7][0] = 300;
            fSpeedAlt[7][1] = 300000;

            fSpeedAlt[8] = new float[3];
            fSpeedAlt[8][0] = 300;
            fSpeedAlt[8][1] = 300000;
            
            for (int i = 1; i < fSpeedAlt.Length; i++)
            {
                fSpeedAlt[i][2] = (fSpeedAlt[i][1] - fSpeedAlt[i - 1][1]) / Math.Max( 1, (fSpeedAlt[i][0] - fSpeedAlt[i - 1][0]));
            }

            this.fsxCon = fsxCon;
			this.httpServer = httpServer;

			dbCon = new OleDbConnection(Program.Config.ConnectionString);
			dbCon.Open();

			htKMLParts = new Hashtable();

			// TODO: Hardcoded paths or parts of it should be avoided. Maybe we 
			// should put the GE icons in the resource file

			String[] strFiles = Directory.GetFiles(Program.Config.AppPath + "\\pub\\gfx\\ge\\icons");
			int nIdx = Program.Config.AppPath.Length + 4;
			foreach (String strFile in strFiles)
				httpServer.registerFile(strFile.Substring(nIdx).Replace('\\', '/'), new ServerFileDisc("image/png", strFile));

			SettingsList lstImg = (SettingsList)Program.Config[Config.SETTING.AIR_IMG_LIST];
			foreach (SettingsObject img in lstImg.listSettings)
				httpServer.registerFile("/gfx/" + img["Name"].StringValue + ".png", new ServerFileDisc("image/png", Program.Config.FilePathPub + img["Img"].StringValue));

			lstImg = (SettingsList)Program.Config[Config.SETTING.WATER_IMG_LIST];
			foreach (SettingsObject img in lstImg.listSettings)
				httpServer.registerFile("/gfx/" + img["Name"].StringValue + ".png", new ServerFileDisc("image/png", Program.Config.FilePathPub + img["Img"].StringValue));

			lstImg = (SettingsList)Program.Config[Config.SETTING.GROUND_IMG_LIST];
			foreach (SettingsObject img in lstImg.listSettings)
				httpServer.registerFile("/gfx/" + img["Name"].StringValue + ".png", new ServerFileDisc("image/png", Program.Config.FilePathPub + img["Img"].StringValue));

			Stream s = Assembly.GetCallingAssembly().GetManifestResourceStream("Fsxget.pub.gfx.logo.png");
			byte[] bTemp = new byte[s.Length];
			s.Read(bTemp, 0, (int)s.Length);
			httpServer.registerFile("/gfx/logo.png", new ServerFileCached("image/png", bTemp));

			// TODO: The no image functionality is still missing due to migrating to the 
			// new HTTP server class. We should consider to drop it anyway and instead, include 
			// object images in the KML files only if they really exist.

			s = Assembly.GetCallingAssembly().GetManifestResourceStream("Fsxget.pub.gfx.noimage.png");
			bTemp = new byte[s.Length];
			s.Read(bTemp, 0, (int)s.Length);
			httpServer.registerFile("/gfx/noimage.png", new ServerFileCached("image/png", bTemp));

			String[] strPartFiles = Directory.GetFiles(Program.Config.AppPath + "\\data", "*.part");
			foreach (String strPartFile in strPartFiles)
			{
				String strPart = File.ReadAllText(strPartFile);
				htKMLParts.Add(Path.GetFileNameWithoutExtension(strPartFile), strPart);
			}

			strUpdateKMLHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><kml xmlns=\"http://earth.google.com/kml/2.1\"><NetworkLinkControl>";
			strUpdateKMLFooter = "</NetworkLinkControl></kml>";

			// Register KML documents with the HTTP server
			const String szContentTypeKml = "application/vnd.google-earth.kml+xml";
			httpServer.registerFile("/fsxobjs.kml", new ServerFileDynamic(szContentTypeKml, GenFSXObjects));
			httpServer.registerFile("/fsxuu.kml", new ServerFileDynamic(szContentTypeKml, GenUserPositionUpdate));
			httpServer.registerFile("/fsxaipu.kml", new ServerFileDynamic(szContentTypeKml, GenAIAircraftUpdate));
			httpServer.registerFile("/fsxaihu.kml", new ServerFileDynamic(szContentTypeKml, GenAIHelicpoterUpdate));
			httpServer.registerFile("/fsxaibu.kml", new ServerFileDynamic(szContentTypeKml, GenAIBoatUpdate));
			httpServer.registerFile("/fsxaigu.kml", new ServerFileDynamic(szContentTypeKml, GenAIGroundUnitUpdate));
			httpServer.registerFile("/fsxpu.kml", new ServerFileDynamic(szContentTypeKml, GenUserPath));
			httpServer.registerFile("/fsxpreu.kml", new ServerFileDynamic(szContentTypeKml, GenUserPrediction));
			httpServer.registerFile("/fsxfpu.kml", new ServerFileDynamic(szContentTypeKml, GenFlightplanUpdate));
			httpServer.registerFile("/fsxnau.kml", new ServerFileDynamic(szContentTypeKml, GenNavAdisUpdate));
			httpServer.registerFile("/fsxapu.kml", new ServerFileDynamic(szContentTypeKml, GenAirportUpdate));
			httpServer.registerFile("/fsxsapi", new ServerFileDynamic("image/png", GetSimpleAirportIcon));
            httpServer.registerFile("/fsxcapi", new ServerFileDynamic("image/png", GetComplexAirportIcon));
            httpServer.registerFile("/fsxts", new ServerFileDynamic("image/png", GetTaxiSign));
            httpServer.registerFile("/fsxtp", new ServerFileDynamic("image/png", GetParkingSign));

			// Register other documents with the HTTP server
			httpServer.registerFile("/setfreq.html", new ServerFileDynamic("text/html", GenSetFreqHtml));
            httpServer.registerFile("/goto.html", new ServerFileDynamic("text/html", GenGotoHtml));

        }

		~KmlFactory()
		{
			while (dbCon.State != System.Data.ConnectionState.Closed && dbCon.State != System.Data.ConnectionState.Open)
			{
				if (dbCon.State == System.Data.ConnectionState.Closed)
					return;

				dbCon.Close();
			}
		}

		public void CreateStartupKML(String strFile)
		{
			String strKML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><kml xmlns=\"http://earth.google.com/kml/2.1\"><NetworkLink>";
			strKML += "<name>" + Program.Config.AssemblyTitle + "</name>";
			strKML += "<Link><href>" + Program.Config.Server + "/fsxobjs.kml</href></Link></NetworkLink></kml>";
			File.WriteAllText(strFile, strKML, Encoding.UTF8);
		}

		public byte[] GenFSXObjects(System.Collections.Specialized.NameValueCollection values)
		{
			return encodeDefault(((String)htKMLParts["fsxobjs"]).Replace("%SERVER%", Program.Config.Server));
		}

		public byte[] GenUserPositionUpdate(System.Collections.Specialized.NameValueCollection values)
		{
			lock (fsxCon.lockUserAircraft)
			{
                int i;
				String strKMLPart = GetExpireString((uint)Program.Config[Config.SETTING.QUERY_USER_AIRCRAFT]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
				if (fsxCon.objUserAircraft != null)
				{
					switch (fsxCon.objUserAircraft.State)
					{
						case FsxConnection.SceneryMovingObject.STATE.NEW:
							strKMLPart += "<Create><Folder targetId=\"uacpos\">";
							strKMLPart += (String)htKMLParts["fsxuc"];
							strKMLPart = strKMLPart.Replace("%ID%", "id=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "\"");
							strKMLPart = strKMLPart.Replace("%ICON%", GetIconLink(KML_ICON_TYPES.USER_AIRCRAFT_POSITION));
							strKMLPart += "</Folder></Create></Update>";
							strKMLPart += (String)htKMLParts["fsxview"];
							fsxCon.objUserAircraft.ReplaceObjectInfos(ref strKMLPart);
							break;
						case FsxConnection.SceneryMovingObject.STATE.MODIFIED:
							strKMLPart += "<Change>";
							if (fsxCon.objUserAircraft.HasChanged)
							{
								strKMLPart += (String)htKMLParts["fsxuc"];
							}
							else if (fsxCon.objUserAircraft.HasMoved)
							{
								strKMLPart += (String)htKMLParts["fsxum"];
							}
							strKMLPart = strKMLPart.Replace("%ID%", "targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "\"");
							strKMLPart = strKMLPart.Replace("%ICON%", GetIconLink(KML_ICON_TYPES.USER_AIRCRAFT_POSITION));
							strKMLPart += "</Change></Update>";
							strKMLPart += (String)htKMLParts["fsxview"];
							fsxCon.objUserAircraft.ReplaceObjectInfos(ref strKMLPart);
                            int nAlt = 0;
                            for (i = 1; i < fSpeedAlt.Length; i++)
                            {
                                if (fSpeedAlt[i][0] > fsxCon.objUserAircraft.GroundSpeed)
                                    break;
                            }
                            nAlt = (int)(fSpeedAlt[i - 1][1] + fSpeedAlt[i][2] * (fsxCon.objUserAircraft.GroundSpeed - fSpeedAlt[i - 1][0]));
                            strKMLPart = strKMLPart.Replace("%RANGE%", nAlt.ToString());
							break;
						case FsxConnection.SceneryObject.STATE.DELETED:
							strKMLPart += "<Delete><Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "\"/></Delete>";
							strKMLPart += "<Delete><Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "p\"/></Delete>";
							strKMLPart += "<Delete><Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "pp\"/></Delete>";
							if (fsxCon.objUserAircraft.pathPrediction.HasPoints)
							{
								for (i = 1; i < fsxCon.objUserAircraft.pathPrediction.Positions.Length; i++)
								{
									strKMLPart += "<Delete><Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "pp" + i.ToString() + "\"/></Delete>";
								}
							}
							strKMLPart += "</Update>";
							fsxCon.objUserAircraft = null;
							break;
						default:
							strKMLPart += "</Update>";
							break;
					}
					if (fsxCon.objUserAircraft != null)
						fsxCon.objUserAircraft.State = FsxConnection.SceneryObject.STATE.DATAREAD;
				}
				else
					strKMLPart += "</Update>";

				return encodeDefault(strUpdateKMLHeader + strKMLPart + strUpdateKMLFooter);
			}
		}

		public byte[] GenUserPath(System.Collections.Specialized.NameValueCollection values)
		{
			lock (fsxCon.lockUserAircraft)
			{
				String strKMLPart = GetExpireString((uint)Program.Config[Config.SETTING.QUERY_USER_PATH]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
				if (fsxCon.objUserAircraft != null)
				{
					switch (fsxCon.objUserAircraft.objPath.State)
					{
						case FsxConnection.SceneryObject.STATE.NEW:
							strKMLPart += "<Create><Folder targetId=\"uacpath\"><Placemark id=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "p\">";
							strKMLPart += "<name>User Aircraft Path</name><description>Path of the user aircraft since tracking started.</description>";
							strKMLPart += "<visibility>1</visibility><open>0</open><Style><LineStyle><color>9fffffff</color><width>2</width></LineStyle>";
							strKMLPart += "</Style><LineString><altitudeMode>absolute</altitudeMode><coordinates>";
							strKMLPart += fsxCon.objUserAircraft.objPath.Coordinates + "</coordinates></LineString></Placemark></Folder></Create>";
							break;
						case FsxConnection.SceneryObject.STATE.MODIFIED:
							strKMLPart += "<Change><Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "p\"><LineString><coordinates>" + fsxCon.objUserAircraft.objPath.Coordinates + "</coordinates></LineString></Placemark></Change>";
							break;
					}
					fsxCon.objUserAircraft.objPath.State = FsxConnection.SceneryObject.STATE.DATAREAD;
				}
				return encodeDefault(strUpdateKMLHeader + strKMLPart + "</Update>" + strUpdateKMLFooter);
			}
		}

		public byte[] GenUserPrediction(System.Collections.Specialized.NameValueCollection values)
		{
			lock (fsxCon.lockUserAircraft)
			{
				String strKMLPart = GetExpireString((uint)Program.Config[Config.SETTING.USER_PATH_PREDICTION]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
				if (fsxCon.objUserAircraft != null)
				{
					switch (fsxCon.objUserAircraft.pathPrediction.State)
					{
						case FsxConnection.SceneryObject.STATE.NEW:
							strKMLPart += "<Create><Folder targetId=\"uacpre\"><Placemark id=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "pp\">";
							strKMLPart += "<name>User Aircraft Path Prediction</name><description>Path prediction of the user aircraft.</description>";
							strKMLPart += "<visibility>1</visibility><open>0</open><Style><LineStyle><color>9f00ffff</color><width>2</width>";
							strKMLPart += "</LineStyle></Style><LineString><altitudeMode>absolute</altitudeMode><coordinates>";
							strKMLPart += fsxCon.objUserAircraft.pathPrediction.Positions[0].Coordinate + " " + fsxCon.objUserAircraft.pathPrediction.Positions[fsxCon.objUserAircraft.pathPrediction.Positions.Length - 1].Coordinate;
							strKMLPart += "</coordinates></LineString></Placemark></Folder></Create>";
							strKMLPart += GenPredictionPoints(fsxCon.objUserAircraft, KML_ICON_TYPES.USER_PREDICTION_POINT, "uacprepts");
							break;
						case FsxConnection.SceneryObject.STATE.MODIFIED:
							strKMLPart += "<Change><Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "pp\"><LineString><coordinates>";
							strKMLPart += fsxCon.objUserAircraft.pathPrediction.Positions[0].Coordinate + " " + fsxCon.objUserAircraft.pathPrediction.Positions[fsxCon.objUserAircraft.pathPrediction.Positions.Length - 1].Coordinate;
							strKMLPart += "</coordinates></LineString></Placemark></Change>";
							strKMLPart += GenPredictionPoints(fsxCon.objUserAircraft);
							break;
					}
					fsxCon.objUserAircraft.pathPrediction.State = FsxConnection.SceneryObject.STATE.DATAREAD;
				}
				return encodeDefault(strUpdateKMLHeader + strKMLPart + "</Update>" + strUpdateKMLFooter);
			}
		}

		public byte[] GenAIAircraftUpdate(System.Collections.Specialized.NameValueCollection values)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_PLANE].lockObject)
			{
				strKML += GetAIObjectUpdate(fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_PLANE].htObjects, "aia", "fsxau", KML_ICON_TYPES.AI_AIRCRAFT, KML_ICON_TYPES.AI_AIRCRAFT_PREDICTION_POINT, "9fd20091");
			}
			strKML += "</Update>" + strUpdateKMLFooter;
			return encodeDefault(strKML);
		}

		public byte[] GenAIHelicpoterUpdate(System.Collections.Specialized.NameValueCollection values)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_HELICOPTER].lockObject)
			{
				strKML += GetAIObjectUpdate(fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_HELICOPTER].htObjects, "aih", "fsxhu", KML_ICON_TYPES.AI_HELICOPTER, KML_ICON_TYPES.AI_HELICOPTER_PREDICTION_POINT, "9fd20091");
			}
			strKML += "</Update>" + strUpdateKMLFooter;
			return encodeDefault(strKML);
		}

		public byte[] GenAIBoatUpdate(System.Collections.Specialized.NameValueCollection values)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_BOATS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_BOAT].lockObject)
			{
				strKML += GetAIObjectUpdate(fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_BOAT].htObjects, "aib", "fsxbu", KML_ICON_TYPES.AI_BOAT, KML_ICON_TYPES.AI_BOAT_PREDICTION_POINT, "9fd20091");
			}
			strKML += "</Update>" + strUpdateKMLFooter;
			//            File.WriteAllText(String.Format("C:\\temp\\boatupd{0}.kml", nFileNr++), strKML, Encoding.UTF8);
			return encodeDefault(strKML);
		}

		public byte[] GenAIGroundUnitUpdate(System.Collections.Specialized.NameValueCollection values)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_GROUND].lockObject)
			{
				strKML += GetAIObjectUpdate(fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_GROUND].htObjects, "aig", "fsxgu", KML_ICON_TYPES.AI_GROUND_UNIT, KML_ICON_TYPES.AI_GROUND_PREDICTION_POINT, "9fd20091");
			}
			strKML += "</Update>" + strUpdateKMLFooter;
			return encodeDefault(strKML);
		}

		public byte[] GenFlightplanUpdate(System.Collections.Specialized.NameValueCollection values)
		{
			String strKML = strUpdateKMLHeader + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			foreach (DictionaryEntry entry in fsxCon.htFlightPlans)
			{
				FsxConnection.FlightPlan obj = (FsxConnection.FlightPlan)entry.Value;
				switch (obj.State)
				{
					case FsxConnection.SceneryObject.STATE.NEW:
						String str;
						strKML += "<Create><Folder targetId=\"fsxfp\"><Folder id=\"fp" + obj.ObjectID.ToString() + "\"><name>" + obj.Name + "</name>";
						String strCoords = "";
						foreach (FsxConnection.FlightPlan.Waypoint wp in obj.Waypoints)
						{
							strCoords += XmlConvert.ToString(wp.Longitude) + "," + XmlConvert.ToString(wp.Latitude) + " ";
							str = (String)htKMLParts["fsxfpwp"];
							str = str.Replace("%NAME%", wp.Name);
							str = str.Replace("%ICON%", GetIconLink(wp.IconType));
							str = str.Replace("%LONGITUDE%", XmlConvert.ToString(wp.Longitude));
							str = str.Replace("%LATITUDE%", XmlConvert.ToString(wp.Latitude));
							strKML += str;
						}
						str = (String)htKMLParts["fsxfppath"];
						str = str.Replace("%COORDINATES%", strCoords);
						strKML += str;
						strKML += "</Folder></Folder></Create>";
						obj.State = FsxConnection.SceneryObject.STATE.DATAREAD;
						break;
					case FsxConnection.SceneryObject.STATE.DELETED:
						strKML += "<Delete><Placemark targetId=\"fp" + obj.ObjectID.ToString() + "\"/></Delete>";
						break;
				}
			}
			fsxCon.CleanupHashtable(ref fsxCon.htFlightPlans);
			strKML += "</Update>" + strUpdateKMLFooter;
			return encodeDefault(strKML);
		}

		public byte[] GenNavAdisUpdate(System.Collections.Specialized.NameValueCollection values)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_NAVAIDS]["Interval"].IntValue) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objects[(int)FsxConnection.OBJCONTAINER.NAVAIDS].lockObject)
			{
				foreach (DictionaryEntry entry in fsxCon.objects[(int)FsxConnection.OBJCONTAINER.NAVAIDS].htObjects)
				{
					FsxConnection.SceneryStaticObject navaid = (FsxConnection.SceneryStaticObject)entry.Value;
                    FsxConnection.SceneryNavaidObjectData navaidData = (FsxConnection.SceneryNavaidObjectData)navaid.Data;
                    switch (navaid.State)
					{
						case FsxConnection.SceneryObject.STATE.NEW:
							strKML += GenNavAidKml(ref navaidData);
							navaid.State = FsxConnection.SceneryObject.STATE.DATAREAD;
							break;
						case FsxConnection.SceneryObject.STATE.DELETED:
							strKML += "<Delete><Placemark targetId=\"na" + navaid.ObjectID.ToString() + "\"/></Delete>";
							strKML += "<Delete><GroundOverlay targetId=\"na" + navaid.ObjectID.ToString() + "ov\"/></Delete>";
							break;
					}
				}
				fsxCon.CleanupHashtable(ref fsxCon.objects[(int)FsxConnection.OBJCONTAINER.NAVAIDS].htObjects);
			}
			return encodeDefault(strKML + "</Update>" + strUpdateKMLFooter);
		}

		public byte[] GenSetFreqHtml(System.Collections.Specialized.NameValueCollection values)
		{
            bool bError = true;
            try
            {
                bError = !fsxCon.SetFrequency(values["type"], float.Parse(values["freq"], System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            catch
            {
            }

			if (bError)
				return encodeDefault("<html><body>" + System.Web.HttpUtility.HtmlEncode(Program.getText("Kml_SetFreq_Error")) + "</body></html>");
			else
				return encodeDefault("<html><body>" + System.Web.HttpUtility.HtmlEncode(Program.getText("Kml_SetFreq_Ok")) + "</body></html>");
		}

        public byte[] GenGotoHtml(System.Collections.Specialized.NameValueCollection values)
        {
            bool bError = true;
            try
            {
                bError = !fsxCon.Goto(float.Parse(values["lon"], System.Globalization.NumberFormatInfo.InvariantInfo),
                                      float.Parse(values["lat"], System.Globalization.NumberFormatInfo.InvariantInfo),
                                      float.Parse(values["alt"], System.Globalization.NumberFormatInfo.InvariantInfo),
                                      float.Parse(values["head"], System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            catch
            {
                bError = true;
            }
            if (bError)
                return encodeDefault("<html><body>" + System.Web.HttpUtility.HtmlEncode("Position not set") + "</body></html>");
            else
                return encodeDefault("<html><body>" + System.Web.HttpUtility.HtmlEncode("Position set") + "</body></html>");
        }

		public byte[] GenAirportUpdate(System.Collections.Specialized.NameValueCollection values)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_NAVAIDS]["Interval"].IntValue) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AIRPORTS].lockObject)
			{
				foreach (DictionaryEntry entry in fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AIRPORTS].htObjects)
				{
                    FsxConnection.SceneryAirportObject airport = (FsxConnection.SceneryAirportObject)entry.Value;
                    FsxConnection.SceneryAirportObjectData airportData = airport.AirportData;
                    switch (airport.State)
					{
						case FsxConnection.SceneryObject.STATE.NEW:
                            strKML += GenAirportUpdate(ref airportData);
                            airport.State = FsxConnection.SceneryObject.STATE.DATAREAD;
							break;
						case FsxConnection.SceneryObject.STATE.DELETED:
                            strKML += "<Delete><Folder targetId=\"ap" + airport.ObjectID.ToString() + "\"/></Delete>";
							break;
					}
                    if (airport.TaxiSignData == null && airport.TaxiSignsState == FsxConnection.SceneryObject.STATE.DELETED)
                    {
                        strKML += "<Delete><Folder targetId=\"apts" + airport.ObjectID.ToString() + "\"/></Delete>";
                        strKML += "<Delete><Folder targetId=\"aptp" + airport.ObjectID.ToString() + "\"/></Delete>";
                        airport.TaxiSignsState = FsxConnection.SceneryObject.STATE.DATAREAD;
                    }
                    else if (airport.TaxiSignData != null && airport.TaxiSignsState == FsxConnection.SceneryObject.STATE.NEW)
                    {
                        strKML += GenTaxiSigns(ref airport);
                        strKML += GenParkingSigns(ref airport);
                        airport.TaxiSignsState = FsxConnection.SceneryObject.STATE.DATAREAD;
                    }
				}
				fsxCon.CleanupHashtable(ref fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AIRPORTS].htObjects);
			}
			return encodeDefault(strKML + "</Update>" + strUpdateKMLFooter);
		}

		private String GetAIObjectUpdate(Hashtable ht, String strFolderPrefix, String strPartFile, KML_ICON_TYPES icoObject, KML_ICON_TYPES icoPredictionPoint, String strPredictionColor)
		{
			String strKMLPart = "";
			foreach (DictionaryEntry entry in ht)
			{
				FsxConnection.SceneryMovingObject obj = (FsxConnection.SceneryMovingObject)entry.Value;
				switch (obj.State)
				{
					case FsxConnection.SceneryMovingObject.STATE.NEW:
						strKMLPart += "<Create><Folder targetId=\"" + strFolderPrefix + "p\">";
						strKMLPart += (String)htKMLParts[strPartFile + "c"];
						strKMLPart = strKMLPart.Replace("%ID%", "id=\"" + obj.ObjectID.ToString() + "\"");
						strKMLPart = strKMLPart.Replace("%ICON%", GetIconLink(icoObject));
						obj.ReplaceObjectInfos(ref strKMLPart);
						strKMLPart += "</Folder></Create>";
						if (obj.pathPrediction != null)
						{
							strKMLPart += "<Create><Folder targetId=\"" + strFolderPrefix + "c\"><Placemark id=\"" + obj.ObjectID.ToString() + "pp\">";
							strKMLPart += "<name>Path Prediction</name><description>Path prediction</description>";
							strKMLPart += "<visibility>1</visibility><open>0</open><Style><LineStyle><color>" + strPredictionColor + "</color><width>2</width>";
							strKMLPart += "</LineStyle></Style><LineString><altitudeMode>absolute</altitudeMode><coordinates>";
							strKMLPart += obj.pathPrediction.Positions[0].Coordinate + " " + obj.pathPrediction.Positions[obj.pathPrediction.Positions.Length - 1].Coordinate;
							strKMLPart += "</coordinates></LineString></Placemark></Folder></Create>";
							strKMLPart += GenPredictionPoints(obj, icoPredictionPoint, strFolderPrefix + "c");
							obj.pathPrediction.State = FsxConnection.SceneryObject.STATE.DATAREAD;
						}
						obj.State = FsxConnection.SceneryObject.STATE.DATAREAD;
						break;
					case FsxConnection.SceneryMovingObject.STATE.MODIFIED:
						strKMLPart += "<Change>";
						if (obj.HasChanged)
						{
							strKMLPart += (String)htKMLParts[strPartFile + "c"];
						}
						if (obj.HasMoved)
						{
							strKMLPart += (String)htKMLParts[strPartFile + "m"];
						}
						strKMLPart = strKMLPart.Replace("%ID%", "targetId=\"" + obj.ObjectID.ToString() + "\"");
						strKMLPart = strKMLPart.Replace("%ICON%", GetIconLink(icoObject));
						strKMLPart += "</Change>";
						if (obj.pathPrediction != null && obj.HasMoved)
						{
							strKMLPart += "<Change><Placemark targetId=\"" + obj.ObjectID.ToString() + "pp\"><LineString><coordinates>";
							strKMLPart += obj.pathPrediction.Positions[0].Coordinate + " " + obj.pathPrediction.Positions[obj.pathPrediction.Positions.Length - 1].Coordinate;
							strKMLPart += "</coordinates></LineString></Placemark></Change>";
							strKMLPart += GenPredictionPoints(obj);
							obj.pathPrediction.State = FsxConnection.SceneryObject.STATE.DATAREAD;
						}
						obj.ReplaceObjectInfos(ref strKMLPart);
						obj.State = FsxConnection.SceneryObject.STATE.DATAREAD;
						break;
					case FsxConnection.SceneryMovingObject.STATE.DELETED:
						strKMLPart += "<Delete>";
						strKMLPart += "<Placemark targetId=\"" + obj.ObjectID.ToString() + "\"/>";
						strKMLPart += "</Delete>";
						if (obj.objPath != null)
							strKMLPart += "<Delete><Placemark targetId=\"" + obj.ObjectID.ToString() + "p\"/></Delete>";
						if (obj.pathPrediction != null)
						{
							strKMLPart += "<Delete><Placemark targetId=\"" + obj.ObjectID.ToString() + "pp\"/></Delete>";
							if (obj.pathPrediction.HasPoints)
							{
								for (int i = 1; i < obj.pathPrediction.Positions.Length; i++)
								{
									strKMLPart += "<Delete><Placemark targetId=\"" + obj.ObjectID.ToString() + "pp" + i.ToString() + "\"/></Delete>";
								}
							}
						}
						break;
				}
			}
			fsxCon.CleanupHashtable(ref ht);
			return strKMLPart;
		}

		private String GetExpireString(uint uiSeconds)
		{
			DateTime date = DateTime.Now;
			date = date.AddSeconds(uiSeconds);
			date = date.ToUniversalTime();

			return "<expires>" + date.ToString("yyyy-MM-ddTHH:mm:ssZ") + "</expires>";
		}

		private String GenPredictionPoints(FsxConnection.SceneryMovingObject obj)
		{
			return GenPredictionPoints(obj, KML_ICON_TYPES.NONE, null);
		}

		private String GenPredictionPoints(FsxConnection.SceneryMovingObject obj, KML_ICON_TYPES icon, String strFolder)
		{
			String strKMLPart = "";
			if (obj.pathPrediction.HasPoints)
			{
				if (strFolder == null)
				{
					for (int i = 1; i < obj.pathPrediction.Positions.Length; i++)
					{
						strKMLPart += "<Change><Placemark targetId=\"" + obj.ObjectID.ToString() + "pp" + i.ToString() + "\">";
						strKMLPart += "<Point><coordinates>" + obj.pathPrediction.Positions[i].Coordinate + "</coordinates>";
						strKMLPart += "</Point></Placemark></Change>";
					}
				}
				else
				{
					for (int i = 1; i < obj.pathPrediction.Positions.Length; i++)
					{
						strKMLPart += "<Create><Folder targetId=\"" + strFolder + "\"><Placemark id=\"" + obj.ObjectID.ToString() + "pp" + i.ToString() + "\">";
						strKMLPart += "<name>ETA " + ((obj.pathPrediction.Positions[i].Time < 60.0) ? (((int)obj.pathPrediction.Positions[i].Time).ToString() + " sec") : (obj.pathPrediction.Positions[i].Time / 60.0 + " min")) + "</name>";
						strKMLPart += "<visibility>1</visibility><open>0</open><description>Esitmated Position</description>";
						strKMLPart += "<Style><IconStyle><Icon><href>" + GetIconLink(icon) + "</href></Icon>";
						strKMLPart += "<scale>0.3</scale></IconStyle><LabelStyle><scale>0.6</scale></LabelStyle></Style>";
						strKMLPart += "<Point><altitudeMode>absolute</altitudeMode><coordinates>" + obj.pathPrediction.Positions[i].Coordinate + "</coordinates>";
						strKMLPart += "<extrude>1</extrude></Point></Placemark></Folder></Create>";
					}
				}
			}
			return strKMLPart;
		}

		public String GetIconLink(KML_ICON_TYPES icon)
		{
			if (httpServer.fileExists("/gfx/ge/icons/" + strIconNames[(int)icon]))
				return Program.Config.Server + "/gfx/ge/icons/" + strIconNames[(int)icon];
			else
				return Program.Config.Server + "/gfx/noimage.png";
		}

		public String GetTemplate(String strName)
		{
			return (String)htKMLParts[strName];
		}

		private String GenVorKML(uint unID, float dLongitude, float dLatitude, float dMagVar)
		{
			String strKMLPart = (String)htKMLParts["fsxvoroverlay"];

			float dLatResult = 0;
			float dLonResult = 0;
			float dRadius = 5000;

			MovePoint(dLongitude, dLatitude, 0, dRadius, ref dLonResult, ref dLatResult);
			strKMLPart = strKMLPart.Replace("%NORTH%", XmlConvert.ToString(dLatResult));
			MovePoint(dLongitude, dLatitude, 90, dRadius, ref dLonResult, ref dLatResult);
			strKMLPart = strKMLPart.Replace("%EAST%", XmlConvert.ToString(dLonResult));
			MovePoint(dLongitude, dLatitude, 180, dRadius, ref dLonResult, ref dLatResult);
			strKMLPart = strKMLPart.Replace("%SOUTH%", XmlConvert.ToString(dLatResult));
			MovePoint(dLongitude, dLatitude, 270, dRadius, ref dLonResult, ref dLatResult);
			strKMLPart = strKMLPart.Replace("%WEST%", XmlConvert.ToString(dLonResult));
			strKMLPart = strKMLPart.Replace("%MAGVAR%", XmlConvert.ToString(dMagVar));
			strKMLPart = strKMLPart.Replace("%ICON%", GetIconLink(KML_ICON_TYPES.VOR_OVERLAY));
			strKMLPart = strKMLPart.Replace("%ID%", "id=\"na" + unID.ToString() + "ov\"");
			return strKMLPart;
		}

		private String GenNavAidKml(ref FsxConnection.SceneryNavaidObjectData navaidData)
		{
			String strKMLPart = "<Create><Folder targetId=\"";
			KML_ICON_TYPES tIconType = KML_ICON_TYPES.NONE;
            switch (navaidData.Type)
			{
				case FsxConnection.SceneryNavaidObjectData.TYPE.DME:
					strKMLPart += "fsxnavor\">";
					strKMLPart += (String)htKMLParts["fsxvor"];
					tIconType = KML_ICON_TYPES.DME;
					break;
                case FsxConnection.SceneryNavaidObjectData.TYPE.VOR:
					strKMLPart += "fsxnavor\">";
					strKMLPart += (String)htKMLParts["fsxvor"];
					strKMLPart += GenVorKML(navaidData.ObjectID, navaidData.Longitude, navaidData.Latitude, navaidData.MagVar);
					tIconType = KML_ICON_TYPES.VOR;
					break;
                case FsxConnection.SceneryNavaidObjectData.TYPE.VORDME:
					strKMLPart += "fsxnavor\">";
					strKMLPart += (String)htKMLParts["fsxvor"];
                    strKMLPart += GenVorKML(navaidData.ObjectID, navaidData.Longitude, navaidData.Latitude, navaidData.MagVar);
                    tIconType = KML_ICON_TYPES.VORDME;
					break;
                case FsxConnection.SceneryNavaidObjectData.TYPE.NDB:
					strKMLPart += "fsxnandb\">";
					strKMLPart += (String)htKMLParts["fsxndb"];
					tIconType = KML_ICON_TYPES.NDB;
					break;
				default:
					return "";
			}
            strKMLPart = strKMLPart.Replace("%TYPE%", navaidData.TypeName);
            strKMLPart = strKMLPart.Replace("%ICON%", GetIconLink(tIconType));
			strKMLPart = strKMLPart.Replace("%ID%", "id=\"na" + navaidData.ObjectID.ToString() + "\"");
			strKMLPart = strKMLPart.Replace("%NAME%", navaidData.Name);
			strKMLPart = strKMLPart.Replace("%MAGVAR%", XmlConvert.ToString(navaidData.MagVar));
			strKMLPart = strKMLPart.Replace("%IDENT%", navaidData.Ident);
			strKMLPart = strKMLPart.Replace("%MORSE%", FsxConnection.GetMorseCode(navaidData.Ident));
			strKMLPart = strKMLPart.Replace("%FREQUENCY_UF%", String.Format("{0:F2}", navaidData.Frequency));
			strKMLPart = strKMLPart.Replace("%FREQUENCY%", XmlConvert.ToString(navaidData.Frequency));
			strKMLPart = strKMLPart.Replace("%ALTITUDE%", XmlConvert.ToString(navaidData.Altitude));
			strKMLPart = strKMLPart.Replace("%ALTITUDE_UF%", String.Format("{0:F2}ft", navaidData.Altitude * 3.28095));
			strKMLPart = strKMLPart.Replace("%LONGITUDE%", XmlConvert.ToString(navaidData.Longitude));
			strKMLPart = strKMLPart.Replace("%LATITUDE%", XmlConvert.ToString(navaidData.Latitude));
			strKMLPart = strKMLPart.Replace("%SERVER%", Program.Config.Server);
			return strKMLPart + "</Folder></Create>";
		}

        private String GenAirportUpdate(ref FsxConnection.SceneryAirportObjectData airportData)
		{
            String strKMLPart = "<Create><Folder targetId=\"fsxap\">";

            strKMLPart += htKMLParts["fsxapu"];
            strKMLPart = strKMLPart.Replace("%IDENT%", airportData.Ident);
            strKMLPart = strKMLPart.Replace("%NAME%", airportData.Name);
            strKMLPart = strKMLPart.Replace("%LONGITUDE%", XmlConvert.ToString(airportData.Longitude));
            strKMLPart = strKMLPart.Replace("%LATITUDE%", XmlConvert.ToString(airportData.Latitude));
            strKMLPart = strKMLPart.Replace("%ALTITUDE_UF%", String.Format("{0:F2}ft", airportData.Altitude * 3.28095));
            strKMLPart = strKMLPart.Replace("%MAGVAR%", XmlConvert.ToString(airportData.MagVar));
            strKMLPart = strKMLPart.Replace("%ID%", "id=\"ap" + airportData.ObjectID.ToString() + "\"");

            FsxConnection.SceneryAirportObjectData.ComFrequency.COMTYPE tType = 0;
            String strComs = "";
            String strCom = "";
            String strFreq = ((String)htKMLParts["fsxapfreq"]).Replace("%SERVER%", Program.Config.Server);
            String strFreqs = "";
            foreach (FsxConnection.SceneryAirportObjectData.ComFrequency com in airportData.ComFrequencies)
            {
                if( com.ComType != tType )
                {
                    strComs += strCom.Replace("%FREQ%", strFreqs);
                    strFreqs = "";
                    strCom = (String)htKMLParts["fsxapcom"];
                    strCom = strCom.Replace("%TYPE%", com.Name);
                    tType = com.ComType;
                }
                String strTmp = strFreq;
                strTmp = strTmp.Replace("%FREQ_UF%", com.Frequency.ToString());
                strTmp = strTmp.Replace("%FREQ%", XmlConvert.ToString(com.Frequency));
                strFreqs += strTmp;
            }
            strComs += strCom.Replace("%FREQ%", strFreqs);
            strKMLPart = strKMLPart.Replace("%COMS%", ((String)htKMLParts["fsxapcoms"]).Replace("%COMS%", strComs));

            // Runways
            String strRunways = "";
            String strIlsTmpl = (String)htKMLParts["fsxils"];
            String strPattern = "";
            strIlsTmpl = strIlsTmpl.Replace("%SERVER%", Program.Config.Server);
            String strRunwayTmpl = (String)htKMLParts["fsxaprw"];
            strRunwayTmpl = strRunwayTmpl.Replace("%SERVER%", Program.Config.Server);
            foreach (FsxConnection.SceneryAirportObjectData.Runway runway in airportData.Runways)
            {
                String strRunway = strRunwayTmpl;

                strRunway = strRunway.Replace("%LONGITUDE%", XmlConvert.ToString(runway.Longitude));
                strRunway = strRunway.Replace("%LATITUDE%", XmlConvert.ToString(runway.Latitude));
                strRunway = strRunway.Replace("%HEADING%", XmlConvert.ToString(runway.Heading));
                strRunway = strRunway.Replace("%ALTITUDE%", XmlConvert.ToString(runway.Altitude));
                
                strPattern = (String)htKMLParts["fsxpat"];
                strPattern = strPattern.Replace("%ALTITUDE_UF%", String.Format("{0:F2}ft", (runway.PatternAltitude * 3.28095)));
                strPattern = strPattern.Replace("%TRAFFIC%", runway.PatternTraffic == FsxConnection.SceneryAirportObjectData.Runway.PATTERTRAFFIC.LEFT ? "LEFT" : "RIGHT");
                
                String strIls = strIlsTmpl;
                if (runway.ILSData != null)
                {
                    strIls = strIls.Replace("%IDENT%", runway.ILSData.Ident);
                    strIls = strIls.Replace("%NAME%", runway.ILSData.Name);
                    strIls = strIls.Replace("%FREQ_UF%", runway.ILSData.Frequency.ToString());
                    strIls = strIls.Replace("%FREQ%", XmlConvert.ToString(runway.ILSData.Frequency));
                    strIls = strIls.Replace("%HEADING%", runway.ILSData.Heading.ToString());
                }
                else
                {
                    strIls = strIls.Replace("%IDENT%", "");
                    strIls = strIls.Replace("%NAME%", "");
                    strIls = strIls.Replace("%FREQ_UF%", "");
                    strIls = strIls.Replace("%FREQ%", "");
                    strIls = strIls.Replace("%HEADING%", "");
                }
                strRunway = strRunway.Replace("%RUNWAY%", runway.Name);
                strRunway = strRunway.Replace("%LENGTH%", runway.Length.ToString());
                strRunway = strRunway.Replace("%SURFACE%", runway.SurfaceName );
                strRunway = strRunway.Replace("%PATTERN%", strPattern);
                strRunway = strRunway.Replace("%ILS%", strIls);

                strRunways += strRunway;
            }
            strKMLPart = strKMLPart.Replace("%RUNWAYS%", ((String)htKMLParts["fsxaprws"]).Replace("%RUNWAYS%", strRunways));
            if( airportData.ComplexIcon )
                strKMLPart = strKMLPart.Replace("%ICON%", String.Format( "<![CDATA[{0}/fsxcapi?{1}]]>", Program.Config.Server, airportData.IconParams ));
            else
                strKMLPart = strKMLPart.Replace("%ICON%", String.Format("<![CDATA[{0}/fsxsapi?{1}]]>", Program.Config.Server, airportData.IconParams ));
            
            // Boundary-Fences
            String strBoundary = "";
            foreach (FsxConnection.SceneryAirportObjectData.BoundaryFence boundaryFence in airportData.BoundaryFences)
            {
                strBoundary += "<LineString><tessellate>1</tessellate><coordinates>";
                foreach (FsxConnection.SceneryAirportObjectData.BoundaryFence.Vertex vertex in boundaryFence.Vertexes)
                {
                    strBoundary += XmlConvert.ToString(vertex.fLongitude) + "," + XmlConvert.ToString(vertex.fLatitude) + " ";
                }
                strBoundary += "</coordinates></LineString>";
            }
			strKMLPart = strKMLPart.Replace("%BOUNDARIES%", strBoundary);
            return strKMLPart + "</Folder></Create>";
		}

        public byte[] GetComplexAirportIcon(System.Collections.Specialized.NameValueCollection values)
        {
            if (values["ident"] != null)
            {
                try
                {
                    OleDbCommand dbCmd = new OleDbCommand("SELECT ID FROM Airports WHERE Ident='" + values["ident"] + "'", dbCon);
                    OleDbDataReader rd = dbCmd.ExecuteReader();
                    if (rd.Read())
                    {
                        values.Clear();
                        values.Add("id", rd.GetInt32(0).ToString());
                    }
                    rd.Close();
                }
                catch
                {
                }
            }
            if (values["id"] != null)
            {
                try
                {
                    Bitmap bmp = FsxConnection.RenderComplexAirportIcon(uint.Parse(values["id"]), dbCon);
                    if( bmp != null )
                        return BitmapToPngBytes(bmp);
                }
                catch
                {
                }
            }
            return GetSimpleAirportIcon(values);
        }

        public byte[] GetSimpleAirportIcon(System.Collections.Specialized.NameValueCollection values)
  		{
            float fHeading = 0;
            bool bLights = false;
            int nType = 1;
            if (values["ident"] != null)
            {
                try
                {
                    OleDbCommand dbCmd = new OleDbCommand("SELECT ID FROM Airports WHERE Ident='" + values["ident"] + "'", dbCon);
                    OleDbDataReader rd = dbCmd.ExecuteReader();
                    if (rd.Read())
                    {
                        values.Clear();
                        values.Add("id", rd.GetInt32(0).ToString());
                    }
                    rd.Close();
                }
                catch
                {
                    return null;
                }
            }
            // no else ! "Fall through"
            if (values["id"] != null)
            {
                try
                {
                    int nID = int.Parse(values["id"]);
                    OleDbCommand dbCmd = new OleDbCommand("SELECT Length, Heading, HasLights, Hardened, SurfaceType.ID FROM Runways INNER JOIN SurfaceType ON Runways.SurfaceID = SurfaceType.ID WHERE AirportID=" + nID.ToString(), dbCon);
                    OleDbDataReader rd = dbCmd.ExecuteReader();
                    float fLength = 0;
                    bool bHardened = false;
                    
                    while(rd.Read())
                    {
                        if (fLength < rd.GetFloat(0) || (bHardened == false && rd.GetBoolean(3)))
                        {
                            fLength = rd.GetFloat(0);
                            nType = rd.GetInt32(4) == 20 ? 2 : (rd.GetBoolean(3) ? 0 : 1);
                            bLights = rd.GetBoolean(2);
                            fHeading = rd.GetFloat(1);
                            bHardened = rd.GetBoolean(3);
                        }
                    }
                    rd.Close();
                }
                catch
                {
                    return null;
                }
            }
            else if( values["head"] != null )
            {
                try
                {
                    fHeading = float.Parse(values["head"], System.Globalization.NumberFormatInfo.InvariantInfo);
                    bLights = values["lights"] == "1";
                    nType = int.Parse(values["type"]);
                }
                catch
                {
                    return null;
                }
            }

			Bitmap bmp = FsxConnection.RenderSimpleAirportIcon(fHeading, (FsxConnection.RUNWAYTYPE) nType, bLights );
            return BitmapToPngBytes(bmp);
		}

        private String GenTaxiSigns(ref FsxConnection.SceneryAirportObject airport)
        {
            String strKMLPart = "<Create><Folder targetId=\"ap" + airport.ObjectID.ToString() + "\"><Folder id=\"apts" + airport.ObjectID.ToString() + "\"><name>Taxiway Signs</name>";
            foreach( FsxConnection.SceneryTaxiSignData.TaxiSign sign in airport.TaxiSignData.TaxiSigns )
            {
                String strPath = "/fsxts?" + sign.IconParams;
                //                strKMLPart += "<Placemark><name>" + XmlConvert.ToString( rd.GetFloat(3) ) + " - " + XmlConvert.ToString( fTmp ) + "</name><Point><coordinates>" + XmlConvert.ToString(fLon) + "," + XmlConvert.ToString(fLat) + "</coordinates></Point></Placemark>";
                strKMLPart += "<GroundOverlay><Icon><href><![CDATA[" + Program.Config.Server + strPath + "]]></href></Icon><LatLonBox>";
                strKMLPart += "<north>" + XmlConvert.ToString(sign.LatitudeNorth) + "</north>";
                strKMLPart += "<south>" + XmlConvert.ToString(sign.LatitudeSouth) + "</south>";
                strKMLPart += "<east>" + XmlConvert.ToString(sign.LongitudeEast) + "</east>";
                strKMLPart += "<west>" + XmlConvert.ToString(sign.LongitudeWest) + "</west>";
                strKMLPart += "<rotation>" + XmlConvert.ToString(sign.Heading) + "</rotation>";
                strKMLPart += "</LatLonBox>";
                strKMLPart += "<Region><LatLonAltBox>";
                strKMLPart += "<north>" + XmlConvert.ToString(sign.LatitudeNorth) + "</north>";
                strKMLPart += "<south>" + XmlConvert.ToString(sign.LatitudeSouth) + "</south>";
                strKMLPart += "<east>" + XmlConvert.ToString(sign.LongitudeEast) + "</east>";
                strKMLPart += "<west>" + XmlConvert.ToString(sign.LongitudeWest) + "</west>";
                strKMLPart += "</LatLonAltBox><Lod><minLodPixels>20</minLodPixels></Lod></Region></GroundOverlay>";
            }
            return strKMLPart + "</Folder></Folder></Create>";
        }

        public byte[] GetTaxiSign(System.Collections.Specialized.NameValueCollection values)
        {
            if (values["label"] != null)
            {
                byte[] bytes = System.Convert.FromBase64String(values["label"]);
                Bitmap bmp = FsxConnection.RenderTaxiwaySign(System.Text.Encoding.Default.GetString(bytes));
                return BitmapToPngBytes(bmp);
            }
            return null;
        }

        private String GenParkingSigns(ref FsxConnection.SceneryAirportObject airport)
        {
            String strKMLPart = "<Create><Folder targetId=\"ap" + airport.ObjectID.ToString() + "\"><Folder id=\"aptp" + airport.ObjectID.ToString() + "\"><name>Parking Postitions</name>";
            foreach (FsxConnection.SceneryTaxiSignData.TaxiParking park in airport.TaxiSignData.TaxiParkings)
            {
                String strPath = "/fsxtp?" + park.IconParams;
                strKMLPart += "<GroundOverlay><Icon><href><![CDATA[" + Program.Config.Server + strPath + "]]></href></Icon><LatLonBox>";
                strKMLPart += "<north>" + XmlConvert.ToString(park.LatitudeNorth) + "</north>";
                strKMLPart += "<south>" + XmlConvert.ToString(park.LatitudeSouth) + "</south>";
                strKMLPart += "<east>" + XmlConvert.ToString(park.LongitudeEast) + "</east>";
                strKMLPart += "<west>" + XmlConvert.ToString(park.LongitudeWest) + "</west>";
                strKMLPart += "<rotation>" + XmlConvert.ToString(park.Heading) + "</rotation>";
                strKMLPart += "</LatLonBox>";
                strKMLPart += "<Region><LatLonAltBox>";
                strKMLPart += "<north>" + XmlConvert.ToString(park.LatitudeNorth) + "</north>";
                strKMLPart += "<south>" + XmlConvert.ToString(park.LatitudeSouth) + "</south>";
                strKMLPart += "<east>" + XmlConvert.ToString(park.LongitudeEast) + "</east>";
                strKMLPart += "<west>" + XmlConvert.ToString(park.LongitudeWest) + "</west>";
                strKMLPart += "</LatLonAltBox><Lod><minLodPixels>" + (int)(park.Radius * park.Radius) / 10 + "</minLodPixels></Lod></Region></GroundOverlay>";
            }
            return strKMLPart + "</Folder></Folder></Create>";
        }

        public byte[] GetParkingSign(System.Collections.Specialized.NameValueCollection values)
        {
            try
            {
                Bitmap bmp = FsxConnection.RenderTaxiwayParking(float.Parse(values["radius"], System.Globalization.NumberFormatInfo.InvariantInfo), values["name"], int.Parse(values["nr"]));
                return BitmapToPngBytes(bmp);
            }
            catch
            {
                return null;
            }
        }

        private static byte[] BitmapToPngBytes(Bitmap bmp)
        {
            byte[] bufferPng = null;
            if (bmp != null)
            {
                byte[] buffer = new byte[1024 + bmp.Height * bmp.Width * 4];
                MemoryStream s = new MemoryStream(buffer);
                bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                int nSize = (int)s.Position;
                s.Close();
                bufferPng = new byte[nSize];
                for (int i = 0; i < nSize; i++)
                    bufferPng[i] = buffer[i];
            }
            return bufferPng;
        }

        public static void MovePoint(float fLongitude, float fLatitude, float fHeading, float fDistMeter, ref float fLonResult, ref float fLatResult)
		{
			double dDistMeter = (Math.PI / 10800) * fDistMeter / 1852;
			double dHeading = fHeading * dPI180;
			double dLatitude = fLatitude * dPI180;
			double dLongitude = fLongitude * dPI180;
			double dDistSin = Math.Sin(dDistMeter);
			double dDistCos = Math.Cos(dDistMeter);
			double dLatSin = Math.Sin(dLatitude);
			double dLatCos = Math.Cos(dLatitude);

			double dLatResult = Math.Asin(dLatSin * dDistCos + dLatCos * dDistSin * Math.Cos(dHeading));
			double d = -1 * (Math.Atan2(Math.Sin(dHeading) * dDistSin * dLatCos, dDistCos - dLatSin * Math.Sin(dLatResult)));
			double dLonResult = (dLongitude - d + Math.PI) - (long)((dLongitude - d + Math.PI) / 2 / Math.PI) - Math.PI;

			fLatResult = (float)(dLatResult * d180PI);
			fLonResult = (float)(dLonResult * d180PI);
		}
		public static void GetDistance(float fLon1, float fLat1, float fLon2, float fLat2, ref float fDistMeter, ref float fHeading)
		{
			double dLon1 = fLon1 * dPI180;
			double dLat1 = fLat1 * dPI180;
			double dLon2 = fLon2 * dPI180;
			double dLat2 = fLat2 * dPI180;

			double dDist = Math.Acos(Math.Sin(dLat1) * Math.Sin(dLat2) + Math.Cos(dLat1) * Math.Cos(dLat2) * Math.Cos(dLon2 - dLon1));
			fHeading = (float)(Math.Acos((Math.Sin(dLat2) - Math.Cos(dDist) * Math.Sin(dLat1)) / Math.Cos(dLat1) / Math.Sin(dDist)) * d180PI);
			if (dLon2 - dLon1 < 0)
				fHeading = 360 - fHeading;
			fDistMeter = (float)((dDist * d180PI) * 60000);
            fDistMeter *= 1.852f;
		}

		protected byte[] encodeDefault(String data)
		{
			return System.Text.Encoding.UTF8.GetBytes(data);
		}
	}
}
