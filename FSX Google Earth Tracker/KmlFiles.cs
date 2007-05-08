using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Fsxget
{
	public abstract class KmlFile
	{
		String szName;

		public KmlFile(String name)
		{
			szName = name;
		}

		public String Name
		{
			get
			{
				return szName;
			}
		}

		public abstract byte[] getData();
	}

	public class KmlFileStartUp : KmlFile
	{
		public KmlFileStartUp(String name) : base(name) { }

		public override byte[] getData()
		{
			String strKML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><kml xmlns=\"http://earth.google.com/kml/2.1\"><NetworkLink>";
			strKML += "<name>" + Program.Config.AssemblyTitle + "</name>";
			strKML += "<Link><href>" + Program.Config.Server + "/fsxobjs.kml</href></Link></NetworkLink></kml>";

			return System.Text.Encoding.UTF8.GetBytes(strKML);
		}
	}

	public abstract class KmlFileMovingObjects : KmlFile
	{
		public KmlFileMovingObjects(String name) : base(name) { }

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
	}

	public abstract class KmlFileAiObjects : KmlFileMovingObjects
	{
		public KmlFileAiObjects(String name) : base(name) { }

		protected String GetAIObjectUpdate(Hashtable ht, String strFolderPrefix, String strPartFile, KML_ICON_TYPES icoObject, KML_ICON_TYPES icoPredictionPoint, String strPredictionColor)
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
	}
}
