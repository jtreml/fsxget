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
			this.fsxCon = fsxCon;
			this.httpServer = httpServer;

			dbCon = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Program.Config.AppPath + "\\data\\fsxget.mdb");
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
			httpServer.registerFile("/fsxsapi", new ServerFileDynamic("image/png", GetAirportIcon));
            httpServer.registerFile("/fsxts", new ServerFileDynamic("image/png", GetTaxiSign));

			// Register other documents with the HTTP server
			httpServer.registerFile("/setfreq.html", new ServerFileDynamic("text/html", GenSetFreqHtml));
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

		public byte[] GenFSXObjects(String query)
		{
			return encodeDefault(((String)htKMLParts["fsxobjs"]).Replace("%SERVER%", Program.Config.Server));
		}

		public byte[] GenUserPositionUpdate(String query)
		{
			lock (fsxCon.lockUserAircraft)
			{
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
							break;
						case FsxConnection.SceneryObject.STATE.DELETED:
							strKMLPart += "<Delete><Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "\"/></Delete>";
							strKMLPart += "<Delete><Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "p\"/></Delete>";
							strKMLPart += "<Delete><Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "pp\"/></Delete>";
							if (fsxCon.objUserAircraft.pathPrediction.HasPoints)
							{
								for (int i = 1; i < fsxCon.objUserAircraft.pathPrediction.Positions.Length; i++)
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

		public byte[] GenUserPath(String query)
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

		public byte[] GenUserPrediction(String query)
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

		public byte[] GenAIAircraftUpdate(String query)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_PLANE].lockObject)
			{
				strKML += GetAIObjectUpdate(fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_PLANE].htObjects, "aia", "fsxau", KML_ICON_TYPES.AI_AIRCRAFT, KML_ICON_TYPES.AI_AIRCRAFT_PREDICTION_POINT, "9fd20091");
			}
			strKML += "</Update>" + strUpdateKMLFooter;
			return encodeDefault(strKML);
		}

		public byte[] GenAIHelicpoterUpdate(String query)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_HELICOPTER].lockObject)
			{
				strKML += GetAIObjectUpdate(fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_HELICOPTER].htObjects, "aih", "fsxhu", KML_ICON_TYPES.AI_HELICOPTER, KML_ICON_TYPES.AI_HELICOPTER_PREDICTION_POINT, "9fd20091");
			}
			strKML += "</Update>" + strUpdateKMLFooter;
			return encodeDefault(strKML);
		}

		public byte[] GenAIBoatUpdate(String query)
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

		public byte[] GenAIGroundUnitUpdate(String query)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_GROUND].lockObject)
			{
				strKML += GetAIObjectUpdate(fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AI_GROUND].htObjects, "aig", "fsxgu", KML_ICON_TYPES.AI_GROUND_UNIT, KML_ICON_TYPES.AI_GROUND_PREDICTION_POINT, "9fd20091");
			}
			strKML += "</Update>" + strUpdateKMLFooter;
			return encodeDefault(strKML);
		}

		public byte[] GenFlightplanUpdate(String query)
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

		public byte[] GenNavAdisUpdate(String query)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_NAVAIDS]["Interval"].IntValue) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objects[(int)FsxConnection.OBJCONTAINER.NAVAIDS].lockObject)
			{
				foreach (DictionaryEntry entry in fsxCon.objects[(int)FsxConnection.OBJCONTAINER.NAVAIDS].htObjects)
				{
					FsxConnection.SceneryObject navaid = (FsxConnection.SceneryObject)entry.Value;
					switch (navaid.State)
					{
						case FsxConnection.SceneryObject.STATE.NEW:
							strKML += GenNavAidKml(ref navaid);
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

		public byte[] GenSetFreqHtml(String query)
		{
			char[] cSep = { '=', '&' };
			bool bError = false;

			if (query == null || query == "" || query[0] != '?')
			{
				bError = true;
			}
			else
			{
				query = query.Substring(1);

				String[] strParts = query.Split(cSep);
				if (strParts.Length == 4)
				{
					if (strParts[0] == "type" && strParts[2] == "freq")
					{
						try
						{
							if (!fsxCon.SetFrequency(strParts[1], double.Parse(strParts[3], System.Globalization.NumberFormatInfo.InvariantInfo)))
								bError = true;
						}
						catch
						{
							bError = true;
						}
					}
					else
						bError = true;
				}
				else
					bError = true;
			}



			if (bError)
				return encodeDefault("<html><body>" + System.Web.HttpUtility.HtmlEncode(Program.getText("Kml_SetFreq_Error")) + "</body></html>");
			else
				return encodeDefault("<html><body>" + System.Web.HttpUtility.HtmlEncode(Program.getText("Kml_SetFreq_Ok")) + "</body></html>");
		}

		public byte[] GenAirportUpdate(String query)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_NAVAIDS]["Interval"].IntValue) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AIRPORTS].lockObject)
			{
				foreach (DictionaryEntry entry in fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AIRPORTS].htObjects)
				{
                    FsxConnection.SceneryAirportObject airport = (FsxConnection.SceneryAirportObject)entry.Value;
					switch (airport.State)
					{
						case FsxConnection.SceneryObject.STATE.NEW:
							strKML += GenAirportKml(ref airport);
							airport.State = FsxConnection.SceneryObject.STATE.DATAREAD;
							break;
						case FsxConnection.SceneryObject.STATE.DELETED:
							strKML += "<Delete><Folder targetId=\"ap" + airport.ObjectID.ToString() + "\"/></Delete>";
							break;
					}
                    if (!airport.HasTaxiSigns && airport.TaxiSignState == FsxConnection.SceneryObject.STATE.DELETED)
                    {
                        strKML += "<Delete><Folder targetId=\"apts" + airport.ObjectID.ToString() + "\"/></Delete>";
                        airport.TaxiSignState = FsxConnection.SceneryObject.STATE.DATAREAD;
                    }
                    else if (airport.HasTaxiSigns && airport.TaxiSignState == FsxConnection.SceneryObject.STATE.NEW)
                    {
                        strKML += GenTaxiSignsKML(airport.ObjectID);
                        airport.TaxiSignState = FsxConnection.SceneryObject.STATE.DATAREAD;
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

		private String GenNavAidKml(ref FsxConnection.SceneryObject navaid)
		{
			String strKMLPart = "<Create><Folder targetId=\"";
			OleDbCommand cmd = new OleDbCommand("SELECT Ident, Name, TypeID, Longitude, Latitude, Altitude, MagVar, Range, Freq FROM navaids WHERE ID=" + navaid.ObjectID.ToString(), dbCon);
			OleDbDataReader rd = cmd.ExecuteReader();
			if (rd.Read())
			{
				KML_ICON_TYPES tIconType = KML_ICON_TYPES.NONE;
				switch (rd.GetInt32(2))
				{
					case 1:
						strKMLPart += "fsxnavor\">";
						strKMLPart += (String)htKMLParts["fsxvor"];
						strKMLPart = strKMLPart.Replace("%TYPE%", "DME");
						tIconType = KML_ICON_TYPES.DME;
						break;
					case 2:
						strKMLPart += "fsxnavor\">";
						strKMLPart += (String)htKMLParts["fsxvor"];
						strKMLPart = strKMLPart.Replace("%TYPE%", "VOR");
						strKMLPart += GenVorKML(navaid.ObjectID, rd.GetFloat(3), rd.GetFloat(4), rd.GetFloat(6));
						tIconType = KML_ICON_TYPES.VOR;
						break;
					case 3:
						strKMLPart += "fsxnavor\">";
						strKMLPart += (String)htKMLParts["fsxvor"];
						strKMLPart = strKMLPart.Replace("%TYPE%", "VOR / DME");
						strKMLPart += GenVorKML(navaid.ObjectID, rd.GetFloat(3), rd.GetFloat(4), rd.GetFloat(6));
						tIconType = KML_ICON_TYPES.VORDME;
						break;
					case 4:
						strKMLPart += "fsxnandb\">";
						strKMLPart += (String)htKMLParts["fsxndb"];
						strKMLPart = strKMLPart.Replace("%TYPE%", "NDB");
						tIconType = KML_ICON_TYPES.NDB;
						break;
					default:
						return "";
				}
				strKMLPart = strKMLPart.Replace("%ICON%", GetIconLink(tIconType));
				strKMLPart = strKMLPart.Replace("%ID%", "id=\"na" + navaid.ObjectID.ToString() + "\"");
				strKMLPart = strKMLPart.Replace("%NAME%", rd.GetString(1));
				strKMLPart = strKMLPart.Replace("%MAGVAR%", XmlConvert.ToString(rd.GetFloat(6)));
				strKMLPart = strKMLPart.Replace("%IDENT%", rd.GetString(0));
				strKMLPart = strKMLPart.Replace("%MORSE%", FsxConnection.GetMorseCode(rd.GetString(0)));
				strKMLPart = strKMLPart.Replace("%FREQUENCY_UF%", String.Format("{0:F2}", rd.GetFloat(8)));
				strKMLPart = strKMLPart.Replace("%FREQUENCY%", XmlConvert.ToString(rd.GetFloat(8)));
				strKMLPart = strKMLPart.Replace("%ALTITUDE%", XmlConvert.ToString(rd.GetFloat(5)));
				strKMLPart = strKMLPart.Replace("%ALTITUDE_UF%", String.Format("{0:F2}ft", rd.GetFloat(5) * 3.28095));
				strKMLPart = strKMLPart.Replace("%LONGITUDE%", XmlConvert.ToString(rd.GetFloat(3)));
				strKMLPart = strKMLPart.Replace("%LATITUDE%", XmlConvert.ToString(rd.GetFloat(4)));
				strKMLPart = strKMLPart.Replace("%SERVER%", Program.Config.Server);
			}
			rd.Close();
			return strKMLPart + "</Folder></Create>";
		}

		private String GenAirportKml(ref FsxConnection.SceneryAirportObject airport)
		{
            OleDbConnection dbCon2 = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Program.Config.AppPath + "\\data\\fsxget.mdb"); 
            String strKMLPart = "<Create><Folder targetId=\"fsxap\">";
            dbCon2.Open();
            OleDbCommand cmd = new OleDbCommand("SELECT Ident, Name, Longitude, Latitude, Altitude, MagVar FROM airports WHERE ID=" + airport.ObjectID.ToString() + " ORDER BY Ident", dbCon);
			OleDbDataReader rd = cmd.ExecuteReader();
            float fAlt = 0;
            if (rd.Read())
            {
                fAlt = rd.GetFloat(4);
                strKMLPart += htKMLParts["fsxapu"];
                strKMLPart = strKMLPart.Replace("%IDENT%", rd.GetString(0));
                strKMLPart = strKMLPart.Replace("%NAME%", rd.GetString(1));
                strKMLPart = strKMLPart.Replace("%LONGITUDE%", XmlConvert.ToString(rd.GetFloat(2)));
                strKMLPart = strKMLPart.Replace("%LATITUDE%", XmlConvert.ToString(rd.GetFloat(3)));
                strKMLPart = strKMLPart.Replace("%ALTITUDE_UF%", String.Format("{0:F2}ft", fAlt * 3.28095));
                strKMLPart = strKMLPart.Replace("%MAGVAR%", XmlConvert.ToString(rd.GetFloat(5)));
                strKMLPart = strKMLPart.Replace("%ID%", "id=\"ap" + airport.ObjectID.ToString() + "\"");
                strKMLPart = strKMLPart.Replace("%IDTS%", "id=\"apts" + airport.ObjectID.ToString() + "\"");
            }
			rd.Close();
            
            // Com-Frequencies
            cmd.CommandText = "SELECT TypeID, AirportComTypes.Name, Freq FROM AirportComs INNER JOIN AirportComTypes ON AirportComs.TypeID = AirportComTypes.ID WHERE AirportID=" + airport.ObjectID.ToString() + " ORDER BY AirportComTypes.Name";
            rd = cmd.ExecuteReader();
            int nType = 0;
            String strComs = "";
            String strCom = "";
            String strFreq = ((String)htKMLParts["fsxapfreq"]).Replace( "%SERVER%", Program.Config.Server );
            String strFreqs = "";
            while (rd.Read())
            {
                if (rd.GetInt32(0) != nType)
                {
                    strComs += strCom.Replace( "%FREQ%", strFreqs );
                    strFreqs = "";
                    strCom = (String)htKMLParts["fsxapcom"];
                    strCom = strCom.Replace("%TYPE%", rd.GetString(1));
                    nType = rd.GetInt32(0);
                }
                String strTmp = strFreq;
                strTmp = strTmp.Replace("%FREQ_UF%", rd.GetFloat(2).ToString());
                strTmp = strTmp.Replace("%FREQ%", XmlConvert.ToString(rd.GetFloat(2)));
                strFreqs += strTmp;
            }
            rd.Close();
            strComs += strCom.Replace("%FREQ%", strFreqs);
            strKMLPart = strKMLPart.Replace("%COMS%", ((String)htKMLParts["fsxapcoms"]).Replace("%COMS%", strComs));

            // Runways
            cmd.CommandText = "SELECT [Number], PrimaryDesignator, SecondaryDesignator, Length, SurfaceID, Name, HasLights, Hardened, Heading, PrimaryTakeoff, PrimaryLanding, SecondaryTakeoff, SecondaryLanding, Runways.ID, PatternAltitude, PrimaryPatternRight, SecondaryPatternRight FROM Runways INNER JOIN SurfaceType ON Runways.SurfaceID = SurfaceType.ID WHERE AirportID=" + airport.ObjectID.ToString();
            rd = cmd.ExecuteReader();
            float fHeading = 0;
            nType = 0;
            bool bLights = false;
            float fLength = 0;
            bool bHardened = false;
            String strRunways = "";
            String strIlsTmpl = (String)htKMLParts["fsxils"];
            String strPattern = "";
            strIlsTmpl = strIlsTmpl.Replace("%SERVER%", Program.Config.Server);
            while (rd.Read())
            {
                String strRunway = (String)htKMLParts["fsxaprw"];
                if (fLength < rd.GetFloat(3) || (bHardened == false && rd.GetBoolean(7) ))
                {
                    fLength = rd.GetFloat(3);
                    nType = rd.GetInt32(4) == 20 ? 2 : (rd.GetBoolean(7) ? 0 : 1);
                    bLights = rd.GetBoolean(6);
                    fHeading = rd.GetFloat(8);
                    bHardened = rd.GetBoolean(7);
                }
                int nNr = rd.GetInt16(0);
                String strName = "";
                String strDes = "";
                String strIls = "";
                if (rd.GetBoolean(9) || rd.GetBoolean(10) || (!rd.GetBoolean(9) && !rd.GetBoolean(10) && !rd.GetBoolean(11) && !rd.GetBoolean(12)))
                {
                    if (nNr >= 1000)
                        strName = strRunwayDirections[(nNr - 1000) / 45];
                    else
                        strName = String.Format("{0:00}", nNr == 0 ? 36 : nNr);
                    strDes = rd.GetString(1);
                    if( strDes != "N" )
                        strName += strDes;

                    strPattern = (String)htKMLParts["fsxpat"];
                    strPattern = strPattern.Replace( "%ALTITUDE_UF%", String.Format("{0:F2}ft", (fAlt+rd.GetFloat(14) * 3.28095)));
                    strPattern = strPattern.Replace( "%TRAFFIC%", rd.GetBoolean(15) ? "RIGHT" : "LEFT" );
                    
                    OleDbCommand cmdIls = new OleDbCommand("SELECT Ident, Name, Heading, Freq FROM RunwayILS WHERE EndSecondary=false AND RunwayID=" + rd.GetInt32(13).ToString(), dbCon2);
                    OleDbDataReader rdIls = cmdIls.ExecuteReader();
                    
                    if(rdIls.Read())
                    {
                        strIls = strIlsTmpl;
                        strIls = strIls.Replace("%IDENT%", rdIls.GetString(0));
                        strIls = strIls.Replace("%NAME%", rdIls.GetString(1));
                        strIls = strIls.Replace("%FREQ_UF%", rdIls.GetFloat(3).ToString());
                        strIls = strIls.Replace("%FREQ%", rdIls.GetFloat(3).ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                        strIls = strIls.Replace("%HEADING%", rdIls.GetFloat(2).ToString());
                    }
                    rdIls.Close();

                    strRunway = strRunway.Replace("%RUNWAY%", strName);
                    strRunway = strRunway.Replace("%LENGTH%", rd.GetFloat(3).ToString());
                    strRunway = strRunway.Replace("%SURFACE%", rd.GetString(5));
                    strRunway = strRunway.Replace("%PATTERN%", strPattern);
                    strRunway = strRunway.Replace("%ILS%", strIls );

                    strRunways += strRunway;
                }
                if (rd.GetBoolean(11) || rd.GetBoolean(12) || (!rd.GetBoolean(9) && !rd.GetBoolean(10) && !rd.GetBoolean(11) && !rd.GetBoolean(12)))
                {
                    strDes = rd.GetString(2);
                    strRunway = (String)htKMLParts["fsxaprw"];
                    if (nNr >= 1000)
                    {
                        nNr -= 1000;
                        if (nNr >= 180)
                            nNr -= 180;
                        else
                            nNr += 180;
                        strName = strRunwayDirections[nNr / 45];
                    }
                    else
                    {
                        if (nNr >= 18)
                            nNr -= 18;
                        else
                            nNr += 18;
                        strName = String.Format("{0:00}", nNr == 0 ? 36 : nNr);
                    }
                    if (strDes != "N")
                        strName += strDes;

                    strPattern = (String)htKMLParts["fsxpat"];
                    strPattern = strPattern.Replace( "%ALTITUDE_UF%", String.Format("{0:F2}ft", (fAlt+rd.GetFloat(14) * 3.28095)));
                    strPattern = strPattern.Replace( "%TRAFFIC%", rd.GetBoolean(16) ? "RIGHT" : "LEFT" );

                    OleDbCommand cmdIls = new OleDbCommand("SELECT Ident, Name, Heading, Freq FROM RunwayILS WHERE EndSecondary=true AND RunwayID=" + rd.GetInt32(13).ToString(), dbCon2);
                    OleDbDataReader rdIls = cmdIls.ExecuteReader();
                    if(rdIls.Read())
                    {
                        strIls = strIlsTmpl;
                        strIls = strIls.Replace("%IDENT%", rdIls.GetString(0));
                        strIls = strIls.Replace("%NAME%", rdIls.GetString(1));
                        strIls = strIls.Replace("%FREQ_UF%", rdIls.GetFloat(3).ToString());
                        strIls = strIls.Replace("%FREQ%", rdIls.GetFloat(3).ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                        strIls = strIls.Replace("%HEADING%", rdIls.GetFloat(2).ToString());
                    }
                    rdIls.Close();
                    strRunway = strRunway.Replace("%RUNWAY%", strName);
                    strRunway = strRunway.Replace("%LENGTH%", rd.GetFloat(3).ToString());
                    strRunway = strRunway.Replace("%SURFACE%", rd.GetString(5));
                    strRunway = strRunway.Replace("%PATTERN%", strPattern);
                    strRunway = strRunway.Replace("%ILS%", strIls );
                    strRunways += strRunway;
                }
            }
            rd.Close();
            strKMLPart = strKMLPart.Replace("%RUNWAYS%", ((String)htKMLParts["fsxaprws"]).Replace("%RUNWAYS%", strRunways));
            strKMLPart = strKMLPart.Replace("%ICON%", String.Format( "{0}/fsxsapi?head={1}&amp;type={2}&amp;lights={3}", Program.Config.Server, fHeading.ToString(System.Globalization.NumberFormatInfo.InvariantInfo), nType, bLights ? "1" : "0" ) );

            // Boundary-Fences
            cmd.CommandText = "SELECT [Number], Longitude, Latitude FROM AirportBoundary INNER JOIN AirportBoundaryVertex ON AirportBoundary.ID=AirportBoundaryVertex.BoundaryID WHERE AirportID=" + airport.ObjectID.ToString() + " ORDER BY [Number],SortNr";
			rd = cmd.ExecuteReader();
			String strBoundary = "";
			int nNumber = -1;
			while (rd.Read())
			{
				if (rd.GetInt32(0) != nNumber)
				{
					if (nNumber > -1)
						strBoundary += "</coordinates></LineString>";
					nNumber = rd.GetInt32(0);
					strBoundary += "<LineString><tessellate>1</tessellate><coordinates>";
				}
				strBoundary += XmlConvert.ToString(rd.GetFloat(1)) + "," + XmlConvert.ToString(rd.GetFloat(2)) + " ";
			}
			rd.Close();
			if (strBoundary.Length > 0)
				strBoundary += "</coordinates></LineString>";
			strKMLPart = strKMLPart.Replace("%BOUNDARIES%", strBoundary);
            dbCon2.Close();
            return strKMLPart + "</Folder></Create>";
		}

		public byte[] GetAirportIcon(String query)
  		{
            Hashtable ht = ParseQuery(query);
            float fHeading = 0;
            bool bLights = false;
            int nType = 1;

            if (ht.ContainsKey("ident"))
            {
                try
                {
                    String strIdent = (String)ht["ident"];
                    OleDbCommand dbCmd = new OleDbCommand("SELECT ID FROM Airports WHERE Ident='" + strIdent.ToString() + "'", dbCon);
                    OleDbDataReader rd = dbCmd.ExecuteReader();
                    if (rd.Read())
                    {
                        ht.Clear();
                        ht.Add("id", rd.GetInt32(0).ToString());
                    }
                    rd.Close();
                }
                catch
                {
                    return null;
                }
            }
            // no else ! "Fall through"
            if (ht.ContainsKey("id"))
            {
                try
                {
                    int nID = int.Parse((String)ht["id"]);
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
            else if( ht.ContainsKey("head" ) )
            {
                try
                {
                    fHeading = float.Parse((String)ht["head"], System.Globalization.NumberFormatInfo.InvariantInfo);
                    bLights = (String)ht["lights"] == "1";
                    nType = int.Parse((String)ht["type"]);
                }
                catch
                {
                    return null;
                }
            }

			Bitmap bmp = FsxConnection.RenderSimpleAirportIcon(fHeading, (FsxConnection.RUNWAYTYPE) nType, bLights );
			if (bmp != null)
			{
				byte[] bBuffer = new byte[4096];
				MemoryStream ms = new MemoryStream(bBuffer);
				bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
				ms.Close();
				return bBuffer;
			}
			else
				return null;
		}
        
        private String GenTaxiSignsKML(uint nID)
        {
            String strKML = "<Create><Folder targetId=\"ap" + nID.ToString() + "\"><Folder id=\"apts" + nID.ToString() + "\">";
            OleDbCommand cmd = new OleDbCommand("SELECT Longitude, Latitude, Label, Heading, JustifyRight FROM TaxiwaySigns WHERE AirportID=" + nID.ToString(), dbCon);
            OleDbDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                Bitmap bmp = FsxConnection.RenderTaxiwaySign(rd.GetString(2));
                int nWidth = bmp.Width / 8;
                int nHeigth = bmp.Height / 8;
                float fLon = rd.GetFloat(0);
                float fLat = rd.GetFloat(1);
                float fLonE = 0;
                float fLatN = 0;
                float fLonW = 0;
                float fLatS = 0;
                float fTmp = 0;
                KmlFactory.MovePoint(fLon, fLat, 90, nWidth / 2, ref fLonE, ref fTmp);
                KmlFactory.MovePoint(fLon, fLat, 180, nHeigth / 2, ref fTmp, ref fLatS);
                KmlFactory.MovePoint(fLon, fLat, 270, nWidth / 2, ref fLonW, ref fTmp);
                KmlFactory.MovePoint(fLon, fLat, 0, nHeigth / 2, ref fTmp, ref fLatN);
                fTmp = rd.GetFloat(3);
                if (rd.GetBoolean(4))
                    fTmp += 90;
                else
                    fTmp -= 90;
                if (fTmp > 360)
                    fTmp -= 360;
                if (fTmp > 180)
                {
                    fTmp = (360 - fTmp) * -1;
                }
                fTmp *= -1;
                byte[] bytes = System.Text.Encoding.Default.GetBytes(rd.GetString(2));
                String strBase64 = System.Convert.ToBase64String(bytes);
                String strPath = "/fsxts?" + strBase64;
                //                strKML += "<Placemark><name>" + XmlConvert.ToString( rd.GetFloat(3) ) + " - " + XmlConvert.ToString( fTmp ) + "</name><Point><coordinates>" + XmlConvert.ToString(fLon) + "," + XmlConvert.ToString(fLat) + "</coordinates></Point></Placemark>";
                strKML += "<GroundOverlay><Icon><href>" + Program.Config.Server + strPath + "</href></Icon><LatLonBox>";
                strKML += "<north>" + XmlConvert.ToString(fLatN) + "</north>";
                strKML += "<south>" + XmlConvert.ToString(fLatS) + "</south>";
                strKML += "<east>" + XmlConvert.ToString(fLonE) + "</east>";
                strKML += "<west>" + XmlConvert.ToString(fLonW) + "</west>";
                strKML += "<rotation>" + XmlConvert.ToString(fTmp) + "</rotation>";
                strKML += "</LatLonBox></GroundOverlay>";
/*
                bytes = new byte[10000];
			    MemoryStream ms = new MemoryStream(bytes);
			    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
			    ms.Close();

               httpServer.registerOneTimeFile( strPath, new ServerFileCached( "image/png", bytes ));
*/
            }
            rd.Close();
            return strKML + "</Folder></Folder></Create>";
        }

        public byte[] GetTaxiSign(String query)
        {
            byte[] bytes = System.Convert.FromBase64String(query.Substring(1));
            Bitmap bmp = FsxConnection.RenderTaxiwaySign(System.Text.Encoding.Default.GetString(bytes));
            if (bmp != null)
            {
                bytes = new byte[10000];
			    MemoryStream ms = new MemoryStream(bytes);
			    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
			    ms.Close();
			    return bytes;
            }
            return null;
        }

		public static Hashtable ParseQuery( String strQuery )
        {
            String[] strParts = strQuery.Substring(1).Split('&');
            Hashtable ht = new Hashtable();
            foreach (String strPart in strParts)
            {
                String[] strComponents = strPart.Split('=');
                ht.Add(strComponents[0].ToLower(), strComponents.Length == 2 ? strComponents[1] : "");
            }
            return ht;
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
