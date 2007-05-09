using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Web;

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

		protected FsxConnection fsxCon;
		protected HttpServer httpServer;

		protected String strUpdateKMLHeader;
		protected String strUpdateKMLFooter;
		protected Hashtable htKMLParts;
		protected Hashtable htFlightPlans;

		#endregion

		public KmlFactory(ref FsxConnection fsxCon, ref HttpServer httpServer)
		{
			this.fsxCon = fsxCon;
			this.httpServer = httpServer;

			htKMLParts = new Hashtable();

			// TODO: Hardcoded paths or parts of it should be avoided. Maybe we 
			// should put the GE icons in the resource file

			String[] strFiles = Directory.GetFiles(Program.Config.AppPath + "\\pub\\gfx\\ge\\icons");
			int nIdx = Program.Config.AppPath.Length + 4;
			foreach (String strFile in strFiles)
				httpServer.registerFile(strFile.Substring(nIdx).Replace('\\', '/'), new ServerFileDisc("image/png", Program.Config.AppPath + "\\pub" + strFile));

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

			// Register other documents with the HTTP server
			httpServer.registerFile("/setfreq.html", new ServerFileDynamic("text/html", GenSetFreqHtml));
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
							strKMLPart += GenPredictionPoints(ref fsxCon.objUserAircraft, KML_ICON_TYPES.USER_PREDICTION_POINT, "uacprepts");
							break;
						case FsxConnection.SceneryObject.STATE.MODIFIED:
							strKMLPart += "<Change><Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "pp\"><LineString><coordinates>";
							strKMLPart += fsxCon.objUserAircraft.pathPrediction.Positions[0].Coordinate + " " + fsxCon.objUserAircraft.pathPrediction.Positions[fsxCon.objUserAircraft.pathPrediction.Positions.Length - 1].Coordinate;
							strKMLPart += "</coordinates></LineString></Placemark></Change>";
							strKMLPart += GenPredictionPoints(ref fsxCon.objUserAircraft);
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
			lock (fsxCon.objAIAircrafts.lockObject)
			{
				strKML += GetAIObjectUpdate(fsxCon.objAIAircrafts.htObjects, "aia", "fsxau", KML_ICON_TYPES.AI_AIRCRAFT, KML_ICON_TYPES.AI_AIRCRAFT_PREDICTION_POINT, "9fd20091");
			}
			strKML += "</Update>" + strUpdateKMLFooter;
			return encodeDefault(strKML);
		}

		public byte[] GenAIHelicpoterUpdate(String query)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objAIHelicopters.lockObject)
			{
				strKML += GetAIObjectUpdate(fsxCon.objAIHelicopters.htObjects, "aih", "fsxhu", KML_ICON_TYPES.AI_HELICOPTER, KML_ICON_TYPES.AI_HELICOPTER_PREDICTION_POINT, "9fd20091");
			}
			strKML += "</Update>" + strUpdateKMLFooter;
			return encodeDefault(strKML);
		}

		public byte[] GenAIBoatUpdate(String query)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_BOATS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objAIBoats.lockObject)
			{
				strKML += GetAIObjectUpdate(fsxCon.objAIBoats.htObjects, "aib", "fsxbu", KML_ICON_TYPES.AI_BOAT, KML_ICON_TYPES.AI_BOAT_PREDICTION_POINT, "9fd20091");
			}
			strKML += "</Update>" + strUpdateKMLFooter;
			//            File.WriteAllText(String.Format("C:\\temp\\boatupd{0}.kml", nFileNr++), strKML, Encoding.UTF8);
			return encodeDefault(strKML);
		}

		public byte[] GenAIGroundUnitUpdate(String query)
		{
			String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
			lock (fsxCon.objAIGroundUnits.lockObject)
			{
				strKML += GetAIObjectUpdate(fsxCon.objAIGroundUnits.htObjects, "aig", "fsxgu", KML_ICON_TYPES.AI_GROUND_UNIT, KML_ICON_TYPES.AI_GROUND_PREDICTION_POINT, "9fd20091");
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
			lock (fsxCon.objNavAids.lockObject)
			{
				foreach (DictionaryEntry entry in fsxCon.objNavAids.htObjects)
				{
					FsxConnection.SceneryNavAid navaid = (FsxConnection.SceneryNavAid)entry.Value;
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
				fsxCon.CleanupHashtable(ref fsxCon.objNavAids.htObjects);
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
							strKMLPart += GenPredictionPoints(ref obj, icoPredictionPoint, strFolderPrefix + "c");
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
							strKMLPart += GenPredictionPoints(ref obj);
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

		private String GenPredictionPoints(ref FsxConnection.SceneryMovingObject obj)
		{
			return GenPredictionPoints(ref obj, KML_ICON_TYPES.NONE, null);
		}

		private String GenPredictionPoints(ref FsxConnection.SceneryMovingObject obj, KML_ICON_TYPES icon, String strFolder)
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
			if(httpServer.fileExists("/gfx/ge/icons/" + strIconNames[(int)icon]))
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

		private String GenNavAidKml(ref FsxConnection.SceneryNavAid navaid)
		{
			String strKMLPart = "<Create><Folder targetId=\"";
			switch (navaid.IconType)
			{
				case KML_ICON_TYPES.DME:
					strKMLPart += "fsxnavor\">";
					strKMLPart += (String)htKMLParts["fsxvor"];
					strKMLPart = strKMLPart.Replace("%TYPE%", "DME");
					break;
				case KML_ICON_TYPES.VOR:
					strKMLPart += "fsxnavor\">";
					strKMLPart += (String)htKMLParts["fsxvor"];
					strKMLPart = strKMLPart.Replace("%TYPE%", "VOR");
					strKMLPart += GenVorKML(navaid.ObjectID, navaid.Longitude, navaid.Latitude, navaid.MagVar);
					break;
				case KML_ICON_TYPES.VORDME:
					strKMLPart += "fsxnavor\">";
					strKMLPart += (String)htKMLParts["fsxvor"];
					strKMLPart = strKMLPart.Replace("%TYPE%", "VOR / DME");
					strKMLPart += GenVorKML(navaid.ObjectID, navaid.Longitude, navaid.Latitude, navaid.MagVar);
					break;
				case KML_ICON_TYPES.NDB:
					strKMLPart += "fsxnandb\">";
					strKMLPart += (String)htKMLParts["fsxndb"];
					strKMLPart = strKMLPart.Replace("%TYPE%", "NDB");
					break;
				default:
					return "";
			}
			strKMLPart = strKMLPart.Replace("%ICON%", GetIconLink(navaid.IconType));
			strKMLPart = strKMLPart.Replace("%ID%", "id=\"na" + navaid.ObjectID.ToString() + "\"");
			strKMLPart = strKMLPart.Replace("%NAME%", navaid.Name);
			strKMLPart = strKMLPart.Replace("%MAGVAR%", navaid.MagVar.ToString());
			strKMLPart = strKMLPart.Replace("%IDENT%", navaid.Ident);
			strKMLPart = strKMLPart.Replace("%MORSE%", navaid.MorseCode);
			strKMLPart = strKMLPart.Replace("%FREQUENCY_UF%", String.Format("{0:F2}", navaid.Frequency));
			strKMLPart = strKMLPart.Replace("%FREQUENCY%", XmlConvert.ToString(navaid.Frequency));
			strKMLPart = strKMLPart.Replace("%ALTITUDE%", XmlConvert.ToString(navaid.Altitude));
			strKMLPart = strKMLPart.Replace("%ALTITUDE_UF%", String.Format("{0:F2}ft", navaid.Altitude * 3.28095));
			strKMLPart = strKMLPart.Replace("%LONGITUDE%", XmlConvert.ToString(navaid.Longitude));
			strKMLPart = strKMLPart.Replace("%LATITUDE%", XmlConvert.ToString(navaid.Latitude));
			strKMLPart = strKMLPart.Replace("%SERVER%", Program.Config.Server);
			return strKMLPart + "</Folder></Create>";
		}

		public static void MovePoint(float fLongitude, float fLatitude, float fHeading, float fDistMeter, ref float fLonResult, ref float fLatResult)
		{
			double dPI180 = Math.PI / 180;
			double d180PI = 180 / Math.PI;
			double dDistMeter = (Math.PI / 10800) * fDistMeter / 1000;
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


		protected byte[] encodeDefault(String data)
		{
			return System.Text.Encoding.UTF8.GetBytes(data);
		}
	}
}
