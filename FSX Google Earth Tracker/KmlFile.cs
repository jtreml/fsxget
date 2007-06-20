using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Drawing;
using Geometry;

namespace Fsxget
{
	public abstract class KmlFile
	{
		private Hashtable htKmlParts;
		protected const String strKmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><kml xmlns=\"http://earth.google.com/kml/2.1\">";
		protected HttpServer httpServer;

		public KmlFile(ref HttpServer httpServer, String strName)
		{
			this.httpServer = httpServer;
			htKmlParts = new Hashtable();
			if (strName != null)
				httpServer.registerFile("/" + strName, new ServerFileDynamic("application/vnd.google-earth.kml+xml", GetKmlFileEncoded));
		}

		protected void LoadKmlPart(String strName)
		{
			String strKmlPart = File.ReadAllText(App.Config.AppPath + "\\data\\" + strName + ".part");
			htKmlParts.Add(strName, Translate(strKmlPart));
		}
		protected String GetKmlPart(String strName)
		{
			return (String)htKmlParts[strName];
		}

		protected static String Translate(String strKml)
		{
			strKml = strKml.Replace("%SERVER%", App.Config.Server);
			// TODO: Translate static Labels in the current language
			strKml = strKml.Replace("%TITLE%", App.Config.AssemblyTitle);
			return strKml;
		}
		protected static String GetExpireString(uint uiSeconds)
		{
			DateTime date = DateTime.Now;
			date = date.AddSeconds(uiSeconds);
			date = date.ToUniversalTime();

			return "<expires>" + date.ToString("yyyy-MM-ddTHH:mm:ssZ") + "</expires>";
		}

		public byte[] GetKmlFileEncoded(System.Collections.Specialized.NameValueCollection values)
		{
			return Encoding.UTF8.GetBytes(GetKmlFile());
		}
		public abstract String GetKmlFile();
	}

	public class KmlFileObjects : KmlFile
	{
		public KmlFileObjects(ref HttpServer httpServer)
			: base(ref httpServer, "fsxobjs.kml")
		{
			LoadKmlPart("fsxobjs");
		}

		public override string GetKmlFile()
		{
			return GetKmlPart("fsxobjs");
		}
	}

	public abstract class KmlFileFsx : KmlFile
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
		#endregion

		public KmlFileFsx(ref FsxConnection fsxCon, ref HttpServer httpServer , String strName)
			: base(ref httpServer, strName)
		{
			this.fsxCon = fsxCon;
		}

		protected StringBuilder GetStringBuilder(int nInterval)
		{
			StringBuilder strKml = new StringBuilder(strKmlHeader);
			strKml.Append("<NetworkLinkControl>");
			strKml.Append(GetExpireString((uint)nInterval));
			strKml.Append("<Update><targetHref>");
			strKml.Append(App.Config.Server);
			strKml.Append("/fsxobjs.kml</targetHref>");
			return strKml;
		}
		protected String GetIconLink(KML_ICON_TYPES icon)
		{
			if (httpServer.fileExists("/gfx/ge/icons/" + strIconNames[(int)icon]))
				return App.Config.Server + "/gfx/ge/icons/" + strIconNames[(int)icon];
			else
				return App.Config.Server + "/gfx/noimage.png";
		}

		protected static byte[] BitmapToPngBytes(Bitmap bmp)
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

	}

	public abstract class KmlFileMovingObject : KmlFileFsx
	{
		public KmlFileMovingObject(ref FsxConnection fsxCon, ref HttpServer httpServer, String strName)
			: base(ref fsxCon, ref httpServer , strName)
		{
		}

		protected String GenPredictionPoints(FsxConnection.SceneryMovingObject obj)
		{
			return GenPredictionPoints(obj, KML_ICON_TYPES.NONE, null);
		}
		protected String GenPredictionPoints(FsxConnection.SceneryMovingObject obj, KML_ICON_TYPES icon, String strFolder)
		{
			StringBuilder strbKmlPart = new StringBuilder();
			if (obj.pathPrediction.HasPoints)
			{
				if (strFolder == null)
				{
					for (int i = 1; i < obj.pathPrediction.Positions.Length; i++)
					{
						strbKmlPart.Append("<Change><Placemark targetId=\"" + obj.ObjectID.ToString() + "pp" + i.ToString() + "\">");
						strbKmlPart.Append("<Point><coordinates>" + obj.pathPrediction.Positions[i].Coordinate + "</coordinates>");
						strbKmlPart.Append("</Point></Placemark></Change>");
					}
				}
				else
				{
					for (int i = 1; i < obj.pathPrediction.Positions.Length; i++)
					{
						strbKmlPart.AppendFormat("<Create><Folder targetId=\"{0}\"><Placemark id=\"{1}pp{2}\">", strFolder, obj.ObjectID, i);
						strbKmlPart.AppendFormat("<name>ETA {0}</name>", ((obj.pathPrediction.Positions[i].Time < 60.0) ? (((int)obj.pathPrediction.Positions[i].Time).ToString() + " sec") : (obj.pathPrediction.Positions[i].Time / 60.0 + " min")));
						strbKmlPart.Append("<visibility>1</visibility><open>0</open><description>Esitmated Position</description>");
						strbKmlPart.Append("<Style><IconStyle><Icon><href>");
						strbKmlPart.Append(GetIconLink(icon));
						strbKmlPart.Append("</href></Icon>");
						strbKmlPart.Append("<scale>0.3</scale></IconStyle><LabelStyle><scale>0.6</scale></LabelStyle></Style>");
						strbKmlPart.Append("<Point><altitudeMode>absolute</altitudeMode><coordinates>");
						strbKmlPart.Append(obj.pathPrediction.Positions[i].Coordinate);
						strbKmlPart.Append("</coordinates><extrude>1</extrude></Point></Placemark></Folder></Create>");
					}
				}
			}
			return strbKmlPart.ToString();
		}

	}

	public class KmlFileUserPosition : KmlFileFsx
	{
		protected static float[][] fSpeedAlt;

		public KmlFileUserPosition(ref FsxConnection fsxCon, ref HttpServer httpServer)
			: base(ref fsxCon, ref httpServer , "fsxuu.kml")
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
				fSpeedAlt[i][2] = (fSpeedAlt[i][1] - fSpeedAlt[i - 1][1]) / Math.Max(1, (fSpeedAlt[i][0] - fSpeedAlt[i - 1][0]));
			}

			LoadKmlPart("fsxuc");
			LoadKmlPart("fsxum");
			LoadKmlPart("fsxview");
		}

		public override String GetKmlFile()
		{
			StringBuilder strbKml = GetStringBuilder(App.Config[Config.SETTING.QUERY_USER_AIRCRAFT]["Interval"].IntValue / 1000);
			lock (fsxCon.lockUserAircraft)
			{
				String strId = "";
				int i;
				if (fsxCon.objUserAircraft != null)
				{
					switch (fsxCon.objUserAircraft.State)
					{
						case FsxConnection.SceneryMovingObject.STATE.NEW:
							strbKml.Append("<Create><Folder targetId=\"uacpos\">");
							strbKml.Append(GetKmlPart("fsxuc"));
							strbKml.Append("</Folder></Create></Update>");
							strbKml.Append(GetKmlPart("fsxview"));
							strId = "id=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "\"";
							break;
						case FsxConnection.SceneryMovingObject.STATE.MODIFIED:
							strbKml.Append("<Change>");
							if (fsxCon.objUserAircraft.HasChanged)
								strbKml.Append(GetKmlPart("fsxuc"));
							else if (fsxCon.objUserAircraft.HasMoved)
								strbKml.Append(GetKmlPart("fsxum"));
							strId = "targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "\"";
							strbKml.Append("</Change></Update>");
							strbKml.Append(GetKmlPart("fsxview"));
							break;
						case FsxConnection.SceneryObject.STATE.DELETED:
							strbKml.Append(String.Format("<Delete><Placemark targetId=\"{0}\"/></Delete>", fsxCon.objUserAircraft.ObjectID));
							strbKml.Append(String.Format("<Delete><Placemark targetId=\"{0}p\"/></Delete>", fsxCon.objUserAircraft.ObjectID));
							strbKml.Append(String.Format("<Delete><Placemark targetId=\"{0}pp\"/></Delete>", fsxCon.objUserAircraft.ObjectID));
							if (fsxCon.objUserAircraft.pathPrediction.HasPoints)
							{
								for (i = 1; i < fsxCon.objUserAircraft.pathPrediction.Positions.Length; i++)
								{
									strbKml.Append(String.Format("<Delete><Placemark targetId=\"{0}pp{1}\"/></Delete>", fsxCon.objUserAircraft.ObjectID, i));
								}
							}
							strbKml.Append("</Update>");
							fsxCon.objUserAircraft = null;
							break;
						default:
							strbKml.Append("</Update>");
							break;
					}
					if (fsxCon.objUserAircraft != null)
					{
						fsxCon.objUserAircraft.State = FsxConnection.SceneryObject.STATE.DATAREAD;
					}
				}
				else
					strbKml.Append("</Update>");
				strbKml.Append("</NetworkLinkControl></kml>");
				String strKml = strbKml.ToString();
				strKml = strKml.Replace("%ID%", strId);
				strKml = strKml.Replace("%ICON%", GetIconLink(KML_ICON_TYPES.USER_AIRCRAFT_POSITION));
				if (fsxCon.objUserAircraft != null)
				{
					fsxCon.objUserAircraft.ReplaceObjectInfos(ref strKml);
					int nAlt = 0;
					for (i = 1; i < fSpeedAlt.Length - 1; i++)
					{
						if (fSpeedAlt[i][0] > fsxCon.objUserAircraft.GroundSpeed)
							break;
					}
					nAlt = (int)(fSpeedAlt[i - 1][1] + fSpeedAlt[i][2] * (fsxCon.objUserAircraft.GroundSpeed - fSpeedAlt[i - 1][0]));
					strKml = strKml.Replace("%RANGE%", nAlt.ToString());
				}
				return strKml;
			}
		}
	}

	public class KmlFileUserPathPrediction : KmlFileMovingObject
	{
		public KmlFileUserPathPrediction(ref FsxConnection fsxCon, ref HttpServer httpServer)
			: base(ref fsxCon, ref httpServer , "fsxpreu.kml")
		{
		}

		public override string GetKmlFile()
		{
			StringBuilder strbKml = GetStringBuilder(App.Config[Config.SETTING.USER_PATH_PREDICTION]["Interval"].IntValue / 1000);
			lock (fsxCon.lockUserAircraft)
			{
				if (fsxCon.objUserAircraft != null)
				{
					switch (fsxCon.objUserAircraft.pathPrediction.State)
					{
						case FsxConnection.SceneryObject.STATE.NEW:
							strbKml.Append(String.Format("<Create><Folder targetId=\"uacpre\"><Placemark id=\"{0}pp\">", fsxCon.objUserAircraft.ObjectID));
							strbKml.Append("<name>User Aircraft Path Prediction</name><description>Path prediction of the user aircraft.</description>");
							strbKml.Append("<visibility>1</visibility><open>0</open><Style><LineStyle><color>9f00ffff</color><width>2</width>");
							strbKml.Append("</LineStyle></Style><LineString><altitudeMode>absolute</altitudeMode><coordinates>");
							strbKml.Append(fsxCon.objUserAircraft.pathPrediction.Positions[0].Coordinate);
							strbKml.Append(fsxCon.objUserAircraft.pathPrediction.Positions[fsxCon.objUserAircraft.pathPrediction.Positions.Length - 1].Coordinate);
							strbKml.Append("</coordinates></LineString></Placemark></Folder></Create>");
							strbKml.Append(GenPredictionPoints(fsxCon.objUserAircraft, KML_ICON_TYPES.USER_PREDICTION_POINT, "uacprepts"));
							break;
						case FsxConnection.SceneryObject.STATE.MODIFIED:
							strbKml.Append(String.Format("<Change><Placemark targetId=\"{0}pp\"><LineString><coordinates>", fsxCon.objUserAircraft.ObjectID));
							strbKml.Append(fsxCon.objUserAircraft.pathPrediction.Positions[0].Coordinate);
							strbKml.Append(fsxCon.objUserAircraft.pathPrediction.Positions[fsxCon.objUserAircraft.pathPrediction.Positions.Length - 1].Coordinate);
							strbKml.Append("</coordinates></LineString></Placemark></Change>");
							strbKml.Append(GenPredictionPoints(fsxCon.objUserAircraft));
							break;
					}
					fsxCon.objUserAircraft.pathPrediction.State = FsxConnection.SceneryObject.STATE.DATAREAD;
				}
				strbKml.Append("</Update></NetworkLinkControl></kml>");
				return strbKml.ToString();
			}
		}
	}

	public class KmlFileUserPath : KmlFileFsx
	{
		public KmlFileUserPath(ref FsxConnection fsxCon, ref HttpServer httpServer)
			: base(ref fsxCon, ref httpServer , "fsxpu.kml")
		{
		}

		public override string GetKmlFile()
		{
			StringBuilder strbKml = GetStringBuilder(App.Config[Config.SETTING.QUERY_USER_PATH]["Interval"].IntValue / 1000);
			lock (fsxCon.lockUserAircraft)
			{
				if (fsxCon.objUserAircraft != null)
				{
					if (fsxCon.objUserAircraft != null)
					{
						switch (fsxCon.objUserAircraft.objPath.State)
						{
							case FsxConnection.SceneryObject.STATE.NEW:
								strbKml.AppendFormat("<Create><Folder targetId=\"uacpath\"><Placemark id=\"{0}p\">", fsxCon.objUserAircraft.ObjectID);
								strbKml.Append("<name>User Aircraft Path</name><description>Path of the user aircraft since tracking started.</description>");
								strbKml.Append("<visibility>1</visibility><open>0</open><Style><LineStyle><color>9fffffff</color><width>2</width></LineStyle>");
								strbKml.Append("</Style><LineString><altitudeMode>absolute</altitudeMode><coordinates>");
								strbKml.Append(fsxCon.objUserAircraft.objPath.Coordinates);
								strbKml.Append("</coordinates></LineString></Placemark></Folder></Create>");
								break;
							case FsxConnection.SceneryObject.STATE.MODIFIED:
								strbKml.AppendFormat("<Change><Placemark targetId=\"{0}p\"><LineString><coordinates>", fsxCon.objUserAircraft.ObjectID);
								strbKml.Append(fsxCon.objUserAircraft.objPath.Coordinates);
								strbKml.Append("</coordinates></LineString></Placemark></Change>");
								break;
						}
						fsxCon.objUserAircraft.objPath.State = FsxConnection.SceneryObject.STATE.DATAREAD;
					}
				}
				strbKml.Append("</Update></NetworkLinkControl></kml>");
				return strbKml.ToString();
			}
		}
	}

	public class KmlFileAIObject : KmlFileMovingObject
	{
		private String strFolder;
		private String strPartFile;
		private KML_ICON_TYPES tIconObject;
		private KML_ICON_TYPES tIconPPP;
		private String strColor;
		private int objContainer;
		private Config.SETTING cfgSetting;

		public KmlFileAIObject(ref FsxConnection fsxCon, ref HttpServer httpServer , String strName, FsxConnection.OBJCONTAINER objContainer, Config.SETTING cfgSetting, String strFolder, String strPartFile, KML_ICON_TYPES tIconObject, KML_ICON_TYPES tIconPPP, String strColor)
			: base(ref fsxCon, ref httpServer , strName)
		{
			this.strFolder = strFolder;
			this.strPartFile = strPartFile;
			this.tIconObject = tIconObject;
			this.tIconPPP = tIconPPP;
			this.strColor = strColor;
			this.objContainer = (int)objContainer;
			this.cfgSetting = cfgSetting;
			LoadKmlPart(strPartFile + "c");
			LoadKmlPart(strPartFile + "m");
		}

		protected String GetAIObjectUpdate(FsxConnection.SceneryMovingObject obj)
		{
			StringBuilder strbKmlPart = new StringBuilder();
			String strId = "";
			switch (obj.State)
			{
				case FsxConnection.SceneryMovingObject.STATE.NEW:
					strbKmlPart.AppendFormat("<Create><Folder targetId=\"{0}p\">", strFolder);
					strbKmlPart.Append(GetKmlPart(strPartFile + "c"));
					strbKmlPart.Append("</Folder></Create>");
					strId = String.Format("id=\"{0}\"", obj.ObjectID);
					if (obj.pathPrediction != null)
					{
						strbKmlPart.AppendFormat("<Create><Folder targetId=\"{0}c\"><Placemark id=\"{1}pp\">", strFolder, obj.ObjectID);
						strbKmlPart.Append("<name>Path Prediction</name><description>Path prediction</description>");
						strbKmlPart.Append("<visibility>1</visibility><open>0</open><Style><LineStyle><color>");
						strbKmlPart.Append(strColor);
						strbKmlPart.Append("</color><width>2</width></LineStyle></Style><LineString><altitudeMode>absolute</altitudeMode><coordinates>");
						strbKmlPart.Append(obj.pathPrediction.Positions[0].Coordinate);
						strbKmlPart.Append(obj.pathPrediction.Positions[obj.pathPrediction.Positions.Length - 1].Coordinate);
						strbKmlPart.Append("</coordinates></LineString></Placemark></Folder></Create>");
						strbKmlPart.Append(GenPredictionPoints(obj, tIconPPP, strFolder + "c"));
						obj.pathPrediction.State = FsxConnection.SceneryObject.STATE.DATAREAD;
					}
					obj.State = FsxConnection.SceneryObject.STATE.DATAREAD;
					break;
				case FsxConnection.SceneryMovingObject.STATE.MODIFIED:
					strbKmlPart.Append("<Change>");
					if (obj.HasChanged)
					{
						strbKmlPart.Append(GetKmlPart(strPartFile + "c"));
					}
					if (obj.HasMoved)
					{
						strbKmlPart.Append(GetKmlPart(strPartFile + "m"));
					}
					strId = String.Format("targetId=\"{0}\"", obj.ObjectID);
					strbKmlPart.Append("</Change>");
					if (obj.pathPrediction != null && obj.HasMoved)
					{
						strbKmlPart.AppendFormat("<Change><Placemark targetId=\"{0}pp\"><LineString><coordinates>", obj.ObjectID);
						strbKmlPart.Append(obj.pathPrediction.Positions[0].Coordinate);
						strbKmlPart.Append(obj.pathPrediction.Positions[obj.pathPrediction.Positions.Length - 1].Coordinate);
						strbKmlPart.Append("</coordinates></LineString></Placemark></Change>");
						strbKmlPart.Append(GenPredictionPoints(obj));
						obj.pathPrediction.State = FsxConnection.SceneryObject.STATE.DATAREAD;
					}
					obj.State = FsxConnection.SceneryObject.STATE.DATAREAD;
					break;
				case FsxConnection.SceneryMovingObject.STATE.DELETED:
					strbKmlPart.Append("<Delete>");
					strbKmlPart.AppendFormat("<Placemark targetId=\"{0}\"/>", obj.ObjectID);
					strbKmlPart.Append("</Delete>");
					if (obj.objPath != null)
						strbKmlPart.AppendFormat("<Delete><Placemark targetId=\"{0}p\"/></Delete>", obj.ObjectID);
					if (obj.pathPrediction != null)
					{
						strbKmlPart.AppendFormat("<Delete><Placemark targetId=\"{0}pp\"/></Delete>", obj.ObjectID);
						if (obj.pathPrediction.HasPoints)
						{
							for (int i = 1; i < obj.pathPrediction.Positions.Length; i++)
							{
								strbKmlPart.AppendFormat("<Delete><Placemark targetId=\"{0}pp{1}\"/></Delete>", obj.ObjectID, i);
							}
						}
					}
					break;
			}
			String strKmlPart = strbKmlPart.ToString();
			if (obj.State != FsxConnection.SceneryObject.STATE.DELETED)
			{
				strKmlPart = strKmlPart.Replace("%ID%", strId);
				strKmlPart = strKmlPart.Replace("%ICON%", GetIconLink(tIconObject));
				obj.ReplaceObjectInfos(ref strKmlPart);
			}
			return strKmlPart;
		}

		public override string GetKmlFile()
		{
			StringBuilder strbKml = GetStringBuilder(App.Config[cfgSetting]["Interval"].IntValue / 1000);
			lock (fsxCon.objects[objContainer].lockObject)
			{
				foreach (DictionaryEntry entry in fsxCon.objects[objContainer].htObjects)
				{
					strbKml.Append(GetAIObjectUpdate((FsxConnection.SceneryMovingObject)entry.Value));
				}
				fsxCon.CleanupHashtable(ref fsxCon.objects[objContainer].htObjects);
			}
			strbKml.Append("</Update></NetworkLinkControl></kml>");
			return strbKml.ToString();
		}
	}

	public class KmlFileNavaid : KmlFileFsx
	{
		public KmlFileNavaid(ref FsxConnection fsxCon, ref HttpServer httpServer)
			: base(ref fsxCon, ref httpServer , "fsxnau.kml")
		{
			LoadKmlPart("fsxvor");
			LoadKmlPart("fsxndb");
			LoadKmlPart("fsxvoroverlay");
		}

		private String GenVorKml(uint unID, float dLongitude, float dLatitude, float dMagVar)
		{
			String strKmlPart = GetKmlPart("fsxvoroverlay");

			float dLatResult = 0;
			float dLonResult = 0;
			float dRadius = 5000;

			KmlFactory.MovePoint(dLongitude, dLatitude, 0, dRadius, ref dLonResult, ref dLatResult);
			strKmlPart = strKmlPart.Replace("%NORTH%", XmlConvert.ToString(dLatResult));
			KmlFactory.MovePoint(dLongitude, dLatitude, 90, dRadius, ref dLonResult, ref dLatResult);
			strKmlPart = strKmlPart.Replace("%EAST%", XmlConvert.ToString(dLonResult));
			KmlFactory.MovePoint(dLongitude, dLatitude, 180, dRadius, ref dLonResult, ref dLatResult);
			strKmlPart = strKmlPart.Replace("%SOUTH%", XmlConvert.ToString(dLatResult));
			KmlFactory.MovePoint(dLongitude, dLatitude, 270, dRadius, ref dLonResult, ref dLatResult);
			strKmlPart = strKmlPart.Replace("%WEST%", XmlConvert.ToString(dLonResult));
			strKmlPart = strKmlPart.Replace("%MAGVAR%", XmlConvert.ToString(dMagVar));
			strKmlPart = strKmlPart.Replace("%ICON%", GetIconLink(KML_ICON_TYPES.VOR_OVERLAY));
			strKmlPart = strKmlPart.Replace("%ID%", "id=\"na" + unID.ToString() + "ov\"");
			return strKmlPart;
		}

		private String GenNavAidKml(ref FsxConnection.SceneryNavaidObjectData navaidData)
		{
			String strKmlPart = "<Create><Folder targetId=\"";
			KML_ICON_TYPES tIconType = KML_ICON_TYPES.NONE;
			switch (navaidData.Type)
			{
				case FsxConnection.SceneryNavaidObjectData.TYPE.DME:
					strKmlPart += "fsxnavor\">";
					strKmlPart += GetKmlPart("fsxvor");
					tIconType = KML_ICON_TYPES.DME;
					break;
				case FsxConnection.SceneryNavaidObjectData.TYPE.VOR:
					strKmlPart += "fsxnavor\">";
					strKmlPart += GetKmlPart("fsxvor");
					strKmlPart += GenVorKml(navaidData.ObjectID, navaidData.Longitude, navaidData.Latitude, navaidData.MagVar);
					tIconType = KML_ICON_TYPES.VOR;
					break;
				case FsxConnection.SceneryNavaidObjectData.TYPE.VORDME:
					strKmlPart += "fsxnavor\">";
					strKmlPart += GetKmlPart("fsxvor");
					strKmlPart += GenVorKml(navaidData.ObjectID, navaidData.Longitude, navaidData.Latitude, navaidData.MagVar);
					tIconType = KML_ICON_TYPES.VORDME;
					break;
				case FsxConnection.SceneryNavaidObjectData.TYPE.NDB:
					strKmlPart += "fsxnandb\">";
					strKmlPart += GetKmlPart("fsxndb");
					tIconType = KML_ICON_TYPES.NDB;
					break;
				default:
					return "";
			}
			strKmlPart = strKmlPart.Replace("%TYPE%", navaidData.TypeName);
			strKmlPart = strKmlPart.Replace("%ICON%", GetIconLink(tIconType));
			strKmlPart = strKmlPart.Replace("%ID%", "id=\"na" + navaidData.ObjectID.ToString() + "\"");
			strKmlPart = strKmlPart.Replace("%NAME%", navaidData.Name);
			strKmlPart = strKmlPart.Replace("%MAGVAR%", XmlConvert.ToString(navaidData.MagVar));
			strKmlPart = strKmlPart.Replace("%IDENT%", navaidData.Ident);
			strKmlPart = strKmlPart.Replace("%MORSE%", FsxConnection.GetMorseCode(navaidData.Ident));
			strKmlPart = strKmlPart.Replace("%FREQUENCY_UF%", String.Format("{0:F2}", navaidData.Frequency));
			strKmlPart = strKmlPart.Replace("%FREQUENCY%", XmlConvert.ToString(navaidData.Frequency));
			strKmlPart = strKmlPart.Replace("%ALTITUDE%", XmlConvert.ToString(navaidData.Altitude));
			strKmlPart = strKmlPart.Replace("%ALTITUDE_UF%", String.Format("{0:F2}ft", navaidData.Altitude * 3.28095));
			strKmlPart = strKmlPart.Replace("%LONGITUDE%", XmlConvert.ToString(navaidData.Longitude));
			strKmlPart = strKmlPart.Replace("%LATITUDE%", XmlConvert.ToString(navaidData.Latitude));
			strKmlPart = strKmlPart.Replace("%SERVER%", App.Config.Server);
			return strKmlPart + "</Folder></Create>";
		}

		public override string GetKmlFile()
		{
			StringBuilder strbKml = GetStringBuilder(App.Config[Config.SETTING.QUERY_NAVAIDS]["Interval"].IntValue);
			lock (fsxCon.objects[(int)FsxConnection.OBJCONTAINER.NAVAIDS].lockObject)
			{
				foreach (DictionaryEntry entry in fsxCon.objects[(int)FsxConnection.OBJCONTAINER.NAVAIDS].htObjects)
				{
					FsxConnection.SceneryStaticObject navaid = (FsxConnection.SceneryStaticObject)entry.Value;
					FsxConnection.SceneryNavaidObjectData navaidData = (FsxConnection.SceneryNavaidObjectData)navaid.Data;
					switch (navaid.State)
					{
						case FsxConnection.SceneryObject.STATE.NEW:
							strbKml.Append(GenNavAidKml(ref navaidData));
							navaid.State = FsxConnection.SceneryObject.STATE.DATAREAD;
							break;
						case FsxConnection.SceneryObject.STATE.DELETED:
							strbKml.AppendFormat("<Delete><Placemark targetId=\"na{0}\"/></Delete>", navaid.ObjectID);
							strbKml.AppendFormat("<Delete><GroundOverlay targetId=\"na{0}ov\"/></Delete>", navaid.ObjectID);
							break;
					}
				}
				fsxCon.CleanupHashtable(ref fsxCon.objects[(int)FsxConnection.OBJCONTAINER.NAVAIDS].htObjects);
			}
			strbKml.Append("</Update></NetworkLinkControl></kml>");
			return strbKml.ToString();
		}
	}

	public class KmlFileAirport : KmlFileFsx
	{
		public KmlFileAirport(ref FsxConnection fsxCon, ref HttpServer httpServer)
			: base(ref fsxCon, ref httpServer , "fsxapu.kml")
		{
			httpServer.registerFile("/fsxsapi", new ServerFileDynamic("image/png", GetSimpleAirportIcon));
			httpServer.registerFile("/fsxcapi", new ServerFileDynamic("image/png", GetComplexAirportIcon));
			httpServer.registerFile("/fsxts", new ServerFileDynamic("image/png", GetTaxiSign));
			httpServer.registerFile("/fsxtp", new ServerFileDynamic("image/png", GetParkingSign));

			LoadKmlPart("fsxapu");
			LoadKmlPart("fsxapfreq");
			LoadKmlPart("fsxapcom");
			LoadKmlPart("fsxapcoms");
			LoadKmlPart("fsxaprw");
			LoadKmlPart("fsxaprws");
			LoadKmlPart("fsxpat");
			LoadKmlPart("fsxils");
			LoadKmlPart("fsxilst");
			LoadKmlPart("fsxilstp1");
			LoadKmlPart("fsxilstp2");
		}

		public byte[] GetComplexAirportIcon(System.Collections.Specialized.NameValueCollection values)
		{
			Bitmap bmp = null;
			if (values["ident"] != null)
			{
				bmp = FsxConnection.RenderComplexAirportIcon(values["ident"]);
			}
			if (values["id"] != null)
			{
				try
				{
					bmp = FsxConnection.RenderComplexAirportIcon(uint.Parse(values["id"]));
				}
				catch
				{
				}
			}
			return BitmapToPngBytes(bmp);
		}

		public byte[] GetSimpleAirportIcon(System.Collections.Specialized.NameValueCollection values)
		{
			Bitmap bmp = null;
			if (values["ident"] != null)
			{
				bmp = FsxConnection.RenderSimpleAirportIcon(values["ident"]);
			}
			else if (values["id"] != null)
			{
				try
				{
					bmp = FsxConnection.RenderSimpleAirportIcon(uint.Parse(values["id"]));
				}
				catch
				{
				}
			}
			else if (values["head"] != null)
			{
				try
				{
					bmp = FsxConnection.RenderSimpleAirportIcon(float.Parse(values["head"], System.Globalization.NumberFormatInfo.InvariantInfo), (FsxConnection.SceneryAirportObjectData.Runway.RUNWAYTYPE)int.Parse(values["type"]), values["lights"] == "1" ? true : false);
				}
				catch
				{
				}
			}
			return BitmapToPngBytes(bmp);
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

		private String GenAirportUpdate(ref FsxConnection.SceneryAirportObjectData airportData)
		{
			StringBuilder strbKmlPart = new StringBuilder("<Create><Folder targetId=\"fsxap\">");

			String strKmlPart = GetKmlPart("fsxapu");
			strKmlPart = strKmlPart.Replace("%IDENT%", airportData.Ident);
			strKmlPart = strKmlPart.Replace("%NAME%", airportData.Name);
			strKmlPart = strKmlPart.Replace("%LONGITUDE%", XmlConvert.ToString(airportData.Longitude));
			strKmlPart = strKmlPart.Replace("%LATITUDE%", XmlConvert.ToString(airportData.Latitude));
			strKmlPart = strKmlPart.Replace("%ALTITUDE_UF%", String.Format("{0:F2}ft", airportData.Altitude * 3.28095));
			strKmlPart = strKmlPart.Replace("%MAGVAR%", XmlConvert.ToString(airportData.MagVar));
			strKmlPart = strKmlPart.Replace("%ID%", "id=\"ap" + airportData.ObjectID.ToString() + "\"");

			FsxConnection.SceneryAirportObjectData.ComFrequency.COMTYPE tType = 0;
			StringBuilder strbComs = new StringBuilder();
			String strCom = "";
			String strFreq = GetKmlPart("fsxapfreq");
			StringBuilder strbFreqs = new StringBuilder();
			foreach (FsxConnection.SceneryAirportObjectData.ComFrequency com in airportData.ComFrequencies)
			{
				if (com.ComType != tType)
				{
					strbComs.Append(strCom.Replace("%FREQ%", strbFreqs.ToString()));
					strbFreqs = new StringBuilder();
					strCom = GetKmlPart("fsxapcom");
					strCom = strCom.Replace("%TYPE%", com.Name);
					tType = com.ComType;
				}
				String strTmp = strFreq;
				strTmp = strTmp.Replace("%FREQ_UF%", com.Frequency.ToString());
				strTmp = strTmp.Replace("%FREQ%", XmlConvert.ToString(com.Frequency));
				strbFreqs.Append(strTmp);
			}
			strbComs.Append(strCom.Replace("%FREQ%", strbFreqs.ToString()));
			strKmlPart = strKmlPart.Replace("%COMS%", GetKmlPart("fsxapcoms").Replace("%COMS%", strbComs.ToString()));

			// Runways
			StringBuilder strbRunways = new StringBuilder();
			String strIlsTmpl = GetKmlPart("fsxils");
			String strPattern = "";
			strIlsTmpl = strIlsTmpl.Replace("%SERVER%", App.Config.Server);
			String strRunwayTmpl = GetKmlPart("fsxaprw");
			String strIlsTunnels = "";
			foreach (FsxConnection.SceneryAirportObjectData.Runway runway in airportData.Runways)
			{
				String strRunway = strRunwayTmpl;

				strRunway = strRunway.Replace("%LONGITUDE%", XmlConvert.ToString(runway.Longitude));
				strRunway = strRunway.Replace("%LATITUDE%", XmlConvert.ToString(runway.Latitude));
				strRunway = strRunway.Replace("%HEADING%", XmlConvert.ToString(runway.Heading));
				strRunway = strRunway.Replace("%ALTITUDE%", XmlConvert.ToString(runway.Altitude));

				strPattern = GetKmlPart("fsxpat");
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

					strIlsTunnels += GenIlsTunnels(runway.ILSData);
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
				strRunway = strRunway.Replace("%SURFACE%", runway.SurfaceName);
				strRunway = strRunway.Replace("%PATTERN%", strPattern);
				strRunway = strRunway.Replace("%ILS%", strIls);

				strbRunways.Append(strRunway);
			}
			strKmlPart = strKmlPart.Replace("%RUNWAYS%", GetKmlPart("fsxaprws").Replace("%RUNWAYS%", strbRunways.ToString()));
			if (airportData.ComplexIcon)
				strKmlPart = strKmlPart.Replace("%ICON%", String.Format("<![CDATA[{0}/fsxcapi?{1}]]>", App.Config.Server, airportData.IconParams));
			else
				strKmlPart = strKmlPart.Replace("%ICON%", String.Format("<![CDATA[{0}/fsxsapi?{1}]]>", App.Config.Server, airportData.IconParams));

			// Boundary-Fences
			StringBuilder strbBoundary = new StringBuilder();
			foreach (FsxConnection.SceneryAirportObjectData.BoundaryFence boundaryFence in airportData.BoundaryFences)
			{
				strbBoundary.Append("<LineString><tessellate>1</tessellate><coordinates>");
				foreach (FsxConnection.SceneryAirportObjectData.BoundaryFence.Vertex vertex in boundaryFence.Vertexes)
				{
					strbBoundary.Append(XmlConvert.ToString(vertex.fLongitude));
					strbBoundary.Append(',');
					strbBoundary.Append(XmlConvert.ToString(vertex.fLatitude));
					strbBoundary.Append(' ');
				}
				strbBoundary.Append("</coordinates></LineString>");
			}
			strKmlPart = strKmlPart.Replace("%BOUNDARIES%", strbBoundary.ToString());

			strKmlPart = strKmlPart.Replace("%ILSTUNNELS%", strIlsTunnels);
			
			strbKmlPart.Append(strKmlPart);
			strbKmlPart.Append("</Folder></Create>");
			return strbKmlPart.ToString();
		}

		private String GenTaxiSigns(ref FsxConnection.SceneryAirportObject airport)
		{
			String strKmlPart = "<Create><Folder targetId=\"ap" + airport.ObjectID.ToString() + "\"><Folder id=\"apts" + airport.ObjectID.ToString() + "\"><name>Taxiway Signs</name>";
			foreach (FsxConnection.SceneryTaxiSignData.TaxiSign sign in airport.TaxiSignData.TaxiSigns)
			{
				String strPath = "/fsxts?" + sign.IconParams;
				//                strKmlPart += "<Placemark><name>" + XmlConvert.ToString( rd.GetFloat(3) ) + " - " + XmlConvert.ToString( fTmp ) + "</name><Point><coordinates>" + XmlConvert.ToString(fLon) + "," + XmlConvert.ToString(fLat) + "</coordinates></Point></Placemark>";
				strKmlPart += "<GroundOverlay><Icon><href><![CDATA[" + App.Config.Server + strPath + "]]></href></Icon><LatLonBox>";
				strKmlPart += "<north>" + XmlConvert.ToString(sign.LatitudeNorth) + "</north>";
				strKmlPart += "<south>" + XmlConvert.ToString(sign.LatitudeSouth) + "</south>";
				strKmlPart += "<east>" + XmlConvert.ToString(sign.LongitudeEast) + "</east>";
				strKmlPart += "<west>" + XmlConvert.ToString(sign.LongitudeWest) + "</west>";
				strKmlPart += "<rotation>" + XmlConvert.ToString(sign.Heading) + "</rotation>";
				strKmlPart += "</LatLonBox>";
				strKmlPart += "<Region><LatLonAltBox>";
				strKmlPart += "<north>" + XmlConvert.ToString(sign.LatitudeNorth) + "</north>";
				strKmlPart += "<south>" + XmlConvert.ToString(sign.LatitudeSouth) + "</south>";
				strKmlPart += "<east>" + XmlConvert.ToString(sign.LongitudeEast) + "</east>";
				strKmlPart += "<west>" + XmlConvert.ToString(sign.LongitudeWest) + "</west>";
				strKmlPart += "</LatLonAltBox><Lod><minLodPixels>20</minLodPixels></Lod></Region></GroundOverlay>";
			}
			return strKmlPart + "</Folder></Folder></Create>";
		}

		private String GenParkingSigns(ref FsxConnection.SceneryAirportObject airport)
		{
			String strKmlPart = "<Create><Folder targetId=\"ap" + airport.ObjectID.ToString() + "\"><Folder id=\"aptp" + airport.ObjectID.ToString() + "\"><name>Parking Postitions</name>";
			foreach (FsxConnection.SceneryTaxiSignData.TaxiParking park in airport.TaxiSignData.TaxiParkings)
			{
				String strPath = "/fsxtp?" + park.IconParams;
				strKmlPart += "<GroundOverlay><Icon><href><![CDATA[" + App.Config.Server + strPath + "]]></href></Icon><LatLonBox>";
				strKmlPart += "<north>" + XmlConvert.ToString(park.LatitudeNorth) + "</north>";
				strKmlPart += "<south>" + XmlConvert.ToString(park.LatitudeSouth) + "</south>";
				strKmlPart += "<east>" + XmlConvert.ToString(park.LongitudeEast) + "</east>";
				strKmlPart += "<west>" + XmlConvert.ToString(park.LongitudeWest) + "</west>";
				strKmlPart += "<rotation>" + XmlConvert.ToString(park.Heading) + "</rotation>";
				strKmlPart += "</LatLonBox>";
				strKmlPart += "<Region><LatLonAltBox>";
				strKmlPart += "<north>" + XmlConvert.ToString(park.LatitudeNorth) + "</north>";
				strKmlPart += "<south>" + XmlConvert.ToString(park.LatitudeSouth) + "</south>";
				strKmlPart += "<east>" + XmlConvert.ToString(park.LongitudeEast) + "</east>";
				strKmlPart += "<west>" + XmlConvert.ToString(park.LongitudeWest) + "</west>";
				strKmlPart += "</LatLonAltBox><Lod><minLodPixels>" + (int)(park.Radius * park.Radius) / 10 + "</minLodPixels></Lod></Region></GroundOverlay>";
			}
			return strKmlPart + "</Folder></Folder></Create>";
		}

		private String GenIlsTunnels(FsxConnection.SceneryAirportObjectData.Runway.ILS Ils)
		{
			// Ils gilde angle
			const double glideAngle = 2.9;

			// Ils cross width and height
			const double e = 1.5;

			
			// EDDM Runway ILS
			GeoPoint gpIls = new GeoPoint(Ils.Latitude, Ils.Longitude, Ils.Altitude);
			double ilsHeading = Ils.Heading + 180.0;
			double ilsRange = Ils.Range;
			double ilsWidth = Ils.Width;
			
			// Get vector for ILS position
			Geometry.Point pIlsPos = EarthCalculator.geo2xyz(gpIls);
			Vector vIlsPos = new Vector(pIlsPos);

			// Some direction vectors for the ILS
			Vector vIlsGround = EarthCalculator.getTangentialVector(pIlsPos, ilsHeading).getNormalized();
			Vector vIlsGroundPerp = Vector.crossProduct(vIlsPos, vIlsGround).getNormalized();

			Vector vIlsCenterLine = vIlsGround.rotateAroundPerpAxis(vIlsGroundPerp, -glideAngle).getNormalized();
			Vector vIlsCenterLinePerp = Vector.crossProduct(vIlsCenterLine, vIlsGroundPerp).getNormalized();
			Vector vIlsCenterLinePerp45 = vIlsCenterLinePerp.rotateAroundPerpAxis(vIlsCenterLine, 45.0).getNormalized();
			Vector vIlsCenterLinePerp45N = vIlsCenterLinePerp.rotateAroundPerpAxis(vIlsCenterLine, -45.0).getNormalized();
			Vector vIlsCenterLinePerp90 = vIlsCenterLinePerp.rotateAroundPerpAxis(vIlsCenterLine, 90.0).getNormalized();

			// Get the ILS signals radius at the given range according to the given width
			double d = Math.Tan((ilsWidth / 2.0) / 180.0 * Math.PI) * ilsRange;

			// ILS center position at given range
			Vector vIlsCenterPos = vIlsPos + vIlsCenterLine * ilsRange;
			GeoPoint gpIlsCenterPos = EarthCalculator.xyz2geo(new Geometry.Point(vIlsCenterPos));

			// Various ILS positions at given range
			Vector vIlsUpPos = vIlsCenterPos + vIlsCenterLinePerp * d;
			GeoPoint gpIlsUpPos = EarthCalculator.xyz2geo(new Geometry.Point(vIlsUpPos));

			Vector vIlsUpPosExt = vIlsCenterPos + vIlsCenterLinePerp * d * e;
			GeoPoint gpIlsUpPosExt = EarthCalculator.xyz2geo(new Geometry.Point(vIlsUpPosExt));

			Vector vIlsLeftPos = vIlsCenterPos + vIlsCenterLinePerp90 * d;
			GeoPoint gpIlsLeftPos = EarthCalculator.xyz2geo(new Geometry.Point(vIlsLeftPos));

			Vector vIlsLeftPosExt = vIlsCenterPos + vIlsCenterLinePerp90 * d * e;
			GeoPoint gpIlsLeftPosExt = EarthCalculator.xyz2geo(new Geometry.Point(vIlsLeftPosExt));

			Vector vIlsUpLeftPos = vIlsCenterPos + vIlsCenterLinePerp45 * d;
			GeoPoint gpIlsUpLeftPos = EarthCalculator.xyz2geo(new Geometry.Point(vIlsUpLeftPos));

			Vector vIlsUpRightPos = vIlsCenterPos + vIlsCenterLinePerp45N * d;
			GeoPoint gpIlsUpRightPos = EarthCalculator.xyz2geo(new Geometry.Point(vIlsUpRightPos));

			Vector vIlsDownPos = vIlsCenterPos - vIlsCenterLinePerp * d;
			GeoPoint gpIlsDownPos = EarthCalculator.xyz2geo(new Geometry.Point(vIlsDownPos));

			Vector vIlsDownPosExt = vIlsCenterPos - vIlsCenterLinePerp * d * e;
			GeoPoint gpIlsDownPosExt = EarthCalculator.xyz2geo(new Geometry.Point(vIlsDownPosExt));

			Vector vIlsRightPos = vIlsCenterPos - vIlsCenterLinePerp90 * d;
			GeoPoint gpIlsRightPos = EarthCalculator.xyz2geo(new Geometry.Point(vIlsRightPos));

			Vector vIlsRightPosExt = vIlsCenterPos - vIlsCenterLinePerp90 * d * e;
			GeoPoint gpIlsRightPosExt = EarthCalculator.xyz2geo(new Geometry.Point(vIlsRightPosExt));

			Vector vIlsDownRightPos = vIlsCenterPos - vIlsCenterLinePerp45 * d;
			GeoPoint gpIlsDownRightPos = EarthCalculator.xyz2geo(new Geometry.Point(vIlsDownRightPos));

			Vector vIlsDownLeftPos = vIlsCenterPos - vIlsCenterLinePerp45N * d;
			GeoPoint gpIlsDownLeftPos = EarthCalculator.xyz2geo(new Geometry.Point(vIlsDownLeftPos));


			String strKmlIlsTunnelPolygon1 = GetKmlPart("fsxilstp1");
			String strKmlPolygons1 = "";
			strKmlPolygons1 += strKmlIlsTunnelPolygon1.Replace("%SIDE%", "HL").Replace("%COORDINATES%", gpIls + "\n" + gpIlsLeftPosExt + "\n" + gpIlsCenterPos + "\n" + gpIls);
			strKmlPolygons1 += strKmlIlsTunnelPolygon1.Replace("%SIDE%", "HR").Replace("%COORDINATES%", gpIls + "\n" + gpIlsCenterPos + "\n" + gpIlsRightPosExt + "\n" + gpIls);
			strKmlPolygons1 += strKmlIlsTunnelPolygon1.Replace("%SIDE%", "VT").Replace("%COORDINATES%", gpIls + "\n" + gpIlsUpPosExt + "\n" + gpIlsCenterPos + "\n" + gpIls);
			strKmlPolygons1 += strKmlIlsTunnelPolygon1.Replace("%SIDE%", "VD").Replace("%COORDINATES%", gpIls + "\n" + gpIlsCenterPos + "\n" + gpIlsDownPosExt + "\n" + gpIls);

			String strKmlIlsTunnelPolygon2 = GetKmlPart("fsxilstp2");
			String strKmlPolygons2 = "";
			strKmlPolygons2 += strKmlIlsTunnelPolygon2.Replace("%SIDE%", "1").Replace("%COORDINATES%", gpIls + "\n" + gpIlsUpPos + "\n" + gpIlsUpRightPos + "\n" + gpIls);
			strKmlPolygons2 += strKmlIlsTunnelPolygon2.Replace("%SIDE%", "2").Replace("%COORDINATES%", gpIls + "\n" + gpIlsUpRightPos + "\n" + gpIlsRightPos + "\n" + gpIls);
			strKmlPolygons2 += strKmlIlsTunnelPolygon2.Replace("%SIDE%", "3").Replace("%COORDINATES%", gpIls + "\n" + gpIlsRightPos + "\n" + gpIlsDownRightPos + "\n" + gpIls);
			strKmlPolygons2 += strKmlIlsTunnelPolygon2.Replace("%SIDE%", "4").Replace("%COORDINATES%", gpIls + "\n" + gpIlsDownRightPos + "\n" + gpIlsDownPos + "\n" + gpIls);
			strKmlPolygons2 += strKmlIlsTunnelPolygon2.Replace("%SIDE%", "5").Replace("%COORDINATES%", gpIls + "\n" + gpIlsDownPos + "\n" + gpIlsDownLeftPos + "\n" + gpIls);
			strKmlPolygons2 += strKmlIlsTunnelPolygon2.Replace("%SIDE%", "6").Replace("%COORDINATES%", gpIls + "\n" + gpIlsDownLeftPos + "\n" + gpIlsLeftPos + "\n" + gpIls);
			strKmlPolygons2 += strKmlIlsTunnelPolygon2.Replace("%SIDE%", "7").Replace("%COORDINATES%", gpIls + "\n" + gpIlsLeftPos + "\n" + gpIlsUpLeftPos + "\n" + gpIls);
			strKmlPolygons2 += strKmlIlsTunnelPolygon2.Replace("%SIDE%", "8").Replace("%COORDINATES%", gpIls + "\n" + gpIlsUpLeftPos + "\n" + gpIlsUpPos + "\n" + gpIls);

			String strKmlIlsTunnel = GetKmlPart("fsxilst");


			return strKmlIlsTunnel.Replace("%NAME%", Ils.Name).Replace("%IDENT%", Ils.Ident).Replace("%POLYGONS1%", strKmlPolygons1).Replace("%POLYGONS2%", strKmlPolygons2);
		}

		public override String GetKmlFile()
		{
			StringBuilder strbKml = GetStringBuilder(App.Config[Config.SETTING.QUERY_NAVAIDS]["Interval"].IntValue);
			lock (fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AIRPORTS].lockObject)
			{
				foreach (DictionaryEntry entry in fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AIRPORTS].htObjects)
				{
					FsxConnection.SceneryAirportObject airport = (FsxConnection.SceneryAirportObject)entry.Value;
					FsxConnection.SceneryAirportObjectData airportData = airport.AirportData;
					switch (airport.State)
					{
						case FsxConnection.SceneryObject.STATE.NEW:
							strbKml.Append(GenAirportUpdate(ref airportData));
							airport.State = FsxConnection.SceneryObject.STATE.DATAREAD;
							break;
						case FsxConnection.SceneryObject.STATE.DELETED:
							strbKml.AppendFormat("<Delete><Folder targetId=\"ap{0}\"/></Delete>", airport.ObjectID);
							break;
					}
					if (airport.TaxiSignData == null && airport.TaxiSignsState == FsxConnection.SceneryObject.STATE.DELETED)
					{
						strbKml.AppendFormat("<Delete><Folder targetId=\"apts{0}\"/></Delete>", airport.ObjectID);
						strbKml.AppendFormat("<Delete><Folder targetId=\"aptp{0}\"/></Delete>", airport.ObjectID);
						airport.TaxiSignsState = FsxConnection.SceneryObject.STATE.DATAREAD;
					}
					else if (airport.TaxiSignData != null && airport.TaxiSignsState == FsxConnection.SceneryObject.STATE.NEW)
					{
						strbKml.Append(GenTaxiSigns(ref airport));
						strbKml.Append(GenParkingSigns(ref airport));
						airport.TaxiSignsState = FsxConnection.SceneryObject.STATE.DATAREAD;
					}
				}
				fsxCon.CleanupHashtable(ref fsxCon.objects[(int)FsxConnection.OBJCONTAINER.AIRPORTS].htObjects);
			}
			strbKml.Append("</Update></NetworkLinkControl></kml>");
			return strbKml.ToString();
		}
	}

	public class KmlFileFlightPlan : KmlFileFsx
	{
		public KmlFileFlightPlan(ref FsxConnection fsxCon, ref HttpServer httpServer)
			: base(ref fsxCon, ref httpServer , "fsxfpu.kml")
		{
			LoadKmlPart("fsxfpwp");
			LoadKmlPart("fsxfppath");
		}

		public override string GetKmlFile()
		{
			StringBuilder strbKml = GetStringBuilder(10);
			foreach (DictionaryEntry entry in fsxCon.htFlightPlans)
			{
				FsxConnection.FlightPlan obj = (FsxConnection.FlightPlan)entry.Value;
				switch (obj.State)
				{
					case FsxConnection.SceneryObject.STATE.NEW:
						String str;
						strbKml.AppendFormat("<Create><Folder targetId=\"fsxfp\"><Folder id=\"fp{0}\"><name>", obj.ObjectID);
						strbKml.Append(obj.Name);
						strbKml.Append("</name>");
						StringBuilder strbCoords = new StringBuilder();
						foreach (FsxConnection.FlightPlan.Waypoint wp in obj.Waypoints)
						{
							strbCoords.Append(XmlConvert.ToString(wp.Longitude));
							strbCoords.Append(',');
							strbCoords.Append(XmlConvert.ToString(wp.Latitude));
							strbCoords.Append(' ');
							str = GetKmlPart("fsxfpwp");
							str = str.Replace("%NAME%", wp.Name);
							str = str.Replace("%ICON%", GetIconLink(wp.IconType));
							str = str.Replace("%LONGITUDE%", XmlConvert.ToString(wp.Longitude));
							str = str.Replace("%LATITUDE%", XmlConvert.ToString(wp.Latitude));
							strbKml.Append(str);
						}
						str = GetKmlPart("fsxfppath");
						str = str.Replace("%COORDINATES%", strbCoords.ToString());
						strbKml.Append(str);
						strbKml.Append("</Folder></Folder></Create>");
						obj.State = FsxConnection.SceneryObject.STATE.DATAREAD;
						break;
					case FsxConnection.SceneryObject.STATE.DELETED:
						strbKml.AppendFormat("<Delete><Placemark targetId=\"fp{0}\"/></Delete>", obj.ObjectID);
						break;
				}
			}
			fsxCon.CleanupHashtable(ref fsxCon.htFlightPlans);
			strbKml.Append("</Update></NetworkLinkControl></kml>");
			return strbKml.ToString();
		}
	}
}
