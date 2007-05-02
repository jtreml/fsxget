using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Fsxget
{
    public class KmlFactory
    {
        public class ObjectImage
        {
            private String strTitle;
            private String strPath;
            private byte[] bData;

            public ObjectImage(SettingsObject img)
            {
                strTitle = img["Name"].StringValue;
                strPath = img["Img"].StringValue;
                try
                {
                    bData = File.ReadAllBytes(Program.Config.FilePathPub + strPath);
                }
                catch
                {
                    bData = null;
                }
            }
            public ObjectImage(String strTitle, String strPath, Stream s)
            {
                this.strTitle = strTitle;
                this.strPath = strPath;
                bData = new byte[s.Length];
                s.Read(bData, 0, (int)s.Length);
            }

            public String Title
            {
                get
                {
                    return strTitle;
                }
            }
            public String Path
            {
                get
                {
                    return strPath;
                }
            }
            public byte[] ImgData
            {
                get
                {
                    return bData;
                }
            }
        };

        #region Structs and Enums
        enum KML_ICON_TYPES
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
            PLAN_VOR,
            PLAN_NDB,
            PLAN_USER,
            PLAN_PORT,
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
                "fsx-p-vor.png",
                "fsx-p-ndb.png",
                "fsx-p-user.png",
                "fsx-p-port.png",
                "fsx-p-inter.png",
                ""
            };
        
        protected List<ObjectImage> lstImgs;
        ObjectImage imgNoImage;
        protected FsxConnection fsxCon;
        protected String strUpdateKMLHeader;
        protected String strUpdateKMLFooter;
        protected Hashtable htKMLParts;
        #endregion

        public KmlFactory(ref FsxConnection fsxCon)
        {
            this.fsxCon = fsxCon;
            htKMLParts = new Hashtable();
            SettingsList lstImg = (SettingsList)Program.Config[Config.SETTING.GE_IMG_LIST];
            lstImgs = new List<ObjectImage>(lstImg.listSettings.Count);
            foreach (SettingsObject img in lstImg.listSettings)
            {
                lstImgs.Add(new ObjectImage(img));
            }

            lstImg = (SettingsList)Program.Config[Config.SETTING.AIR_IMG_LIST];
            foreach (SettingsObject img in lstImg.listSettings)
            {
                lstImgs.Add(new ObjectImage(img));
            }

            lstImg = (SettingsList)Program.Config[Config.SETTING.WATER_IMG_LIST];
            foreach (SettingsObject img in lstImg.listSettings)
            {
                lstImgs.Add(new ObjectImage(img));
            }

            lstImg = (SettingsList)Program.Config[Config.SETTING.GROUND_IMG_LIST];
            foreach (SettingsObject img in lstImg.listSettings)
            {
                lstImgs.Add(new ObjectImage(img));
            }

            lstImgs.Add(new ObjectImage( "Logo", "/gfx/logo.png", Assembly.GetCallingAssembly().GetManifestResourceStream("FSX_Google_Earth_Tracker.pub.gfx.logo.png")));
            imgNoImage = new ObjectImage("NoImage", "/gfx/noimage.png", Assembly.GetCallingAssembly().GetManifestResourceStream("FSX_Google_Earth_Tracker.pub.gfx.noimage.png"));

            String[] strPartFiles = Directory.GetFiles(Program.Config.AppPath + "\\data", "*.part");
            foreach (String strPartFile in strPartFiles)
            {
                String strPart = File.ReadAllText(strPartFile);
                htKMLParts.Add(Path.GetFileNameWithoutExtension(strPartFile), strPart);
            }

            strUpdateKMLHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><kml xmlns=\"http://earth.google.com/kml/2.1\"><NetworkLinkControl>";
            strUpdateKMLFooter = "</NetworkLinkControl></kml>";
        }

        public void CreateStartupKML(String strFile)
        {
            String strKML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><kml xmlns=\"http://earth.google.com/kml/2.1\"><NetworkLink>";
            strKML += "<name>" + Program.Config.AssemblyTitle + "</name>";
            strKML += "<Link><href>" + Program.Config.Server + "/fsxobjs.kml</href></Link></NetworkLink></kml>";
            File.WriteAllText(strFile, strKML, Encoding.UTF8);
        }

        public byte[] GetImage(String strPath)
        {
            foreach (ObjectImage img in lstImgs)
            {
                if (img.Path == strPath)
                    return img.ImgData;
            }
            return imgNoImage.ImgData;
        }

        public String GenFSXObjects()
        {
            return ((String)htKMLParts["fsxobjs"]).Replace("%SERVER%", Program.Config.Server);
        }
        public String GenUserPositionUpdate()
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
                            strKMLPart += (String)htKMLParts["fsxuu"];
                            strKMLPart = strKMLPart.Replace("%ID%", "id=\"" + fsxCon.objUserAircraft.ObjectID.ToString()+"\"");
                            strKMLPart = strKMLPart.Replace("%HEADLINE%", "Microsoft Flightsimulator X - User Aircraft");
                            strKMLPart = strKMLPart.Replace("%ICON%", Program.Config.Server + "/gfx/ge/icons/fsxu.png");
                            fsxCon.objUserAircraft.ReplaceObjectInfos(ref strKMLPart);
                            strKMLPart += "</Folder></Create></Update>";
                            strKMLPart += "<LookAt><longitude>" + XmlConvert.ToString(fsxCon.objUserAircraft.ObjectPosition.Longitude.Value) + "</longitude>";
                            strKMLPart += "<latitude>" + XmlConvert.ToString(fsxCon.objUserAircraft.ObjectPosition.Latitude.Value) + "</latitude>";
                            strKMLPart += "<heading>" + XmlConvert.ToString(fsxCon.objUserAircraft.Heading.Value) + "</heading></LookAt>";
                            break;
                        case FsxConnection.SceneryMovingObject.STATE.MODIFIED:
                            strKMLPart += "<Change>";
                            if (fsxCon.objUserAircraft.HasChanged)
                            {
                                strKMLPart += (String)htKMLParts["fsxuu"];
                                strKMLPart = strKMLPart.Replace("%ID%", "targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "\"");
                                strKMLPart = strKMLPart.Replace("%HEADLINE%", "Microsoft Flightsimulator X - User Aircraft");
                                strKMLPart = strKMLPart.Replace("%ICON%", Program.Config.Server + "/gfx/ge/icons/fsxu.png");
                                fsxCon.objUserAircraft.ReplaceObjectInfos(ref strKMLPart);
                            }
                            else if (fsxCon.objUserAircraft.HasMoved)
                                strKMLPart += "<Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "\"><Point><coordinates>" + fsxCon.objUserAircraft.Coordinates + "</coordinates></Point></Placemark>";
                            strKMLPart += "</Change></Update>";
                            strKMLPart += "<LookAt><longitude>" + XmlConvert.ToString(fsxCon.objUserAircraft.ObjectPosition.Longitude.Value) + "</longitude>";
                            strKMLPart += "<latitude>" + XmlConvert.ToString(fsxCon.objUserAircraft.ObjectPosition.Latitude.Value) + "</latitude>";
                            strKMLPart += "<heading>" + XmlConvert.ToString(fsxCon.objUserAircraft.Heading.Value) + "</heading></LookAt>";
                            break;
                        default:
                            strKMLPart += "</Update>";
                            break;
                    }
                    fsxCon.objUserAircraft.State = FsxConnection.SceneryObject.STATE.DATAREAD;
                }
                else   
                    strKMLPart += "</Update>";
                return strUpdateKMLHeader + strKMLPart + strUpdateKMLFooter;
            }
        }
        public String GenUserPath()
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
                return strUpdateKMLHeader + strKMLPart + "</Update>" + strUpdateKMLFooter;
            }
        }
        public String GenUserPrediction()
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
                            strKMLPart += GenPredictionPoints(ref fsxCon.objUserAircraft, KML_ICON_TYPES.USER_AIRCRAFT_POSITION, "uacprepts");
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
                return strUpdateKMLHeader + strKMLPart + "</Update>" + strUpdateKMLFooter;
            }                
        }
        public String GenAIAircraftUpdate()
        {
            String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
            lock (fsxCon.objAIAircrafts.lockObject)
            {
                strKML += GetAIObjectUpdate(fsxCon.objAIAircrafts.htObjects, "aia", "fsxahu", "AI Aircraft", KML_ICON_TYPES.AI_AIRCRAFT, KML_ICON_TYPES.AI_AIRCRAFT_PREDICTION_POINT, "9fd20091");
            }
            strKML += "</Update>" + strUpdateKMLFooter;
            return strKML;
        }
        public String GenAIHelicpoterUpdate()
        {
            String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
            lock (fsxCon.objAIHelicopters.lockObject)
            {
                strKML += GetAIObjectUpdate(fsxCon.objAIHelicopters.htObjects, "aih", "fsxahu", "AI Helicopter", KML_ICON_TYPES.AI_HELICOPTER, KML_ICON_TYPES.AI_HELICOPTER_PREDICTION_POINT, "9fd20091");
            }
            strKML += "</Update>" + strUpdateKMLFooter;
            return strKML;
        }
        public String GenAIBoatUpdate()
        {
            String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_BOATS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
            lock (fsxCon.objAIBoats.lockObject)
            {
                strKML += GetAIObjectUpdate(fsxCon.objAIBoats.htObjects, "aib", "fsxbgu", "AI Boat", KML_ICON_TYPES.AI_BOAT, KML_ICON_TYPES.AI_BOAT_PREDICTION_POINT, "9fd20091");
            }
            strKML += "</Update>" + strUpdateKMLFooter;
            return strKML;
        }
        public String GenAIGroundUnitUpdate()
        {
            String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
            lock (fsxCon.objAIGroundUnits.lockObject)
            {
                strKML += GetAIObjectUpdate(fsxCon.objAIGroundUnits.htObjects, "aig", "fsxbgu", "AI Ground Unit", KML_ICON_TYPES.AI_GROUND_UNIT, KML_ICON_TYPES.AI_GROUND_PREDICTION_POINT, "9fd20091");
            }
            strKML += "</Update>" + strUpdateKMLFooter;
            return strKML;
        }

        private String GetAIObjectUpdate(Hashtable ht, String strFolderPrefix, String strPartFile, String strObjectName, KML_ICON_TYPES icoObject, KML_ICON_TYPES icoPredictionPoint, String strPredictionColor)
        {
            String strKMLPart = "";
            foreach (DictionaryEntry entry in ht)
            {
                FsxConnection.SceneryMovingObject obj = (FsxConnection.SceneryMovingObject)entry.Value;
                switch (obj.State)
                {
                    case FsxConnection.SceneryMovingObject.STATE.NEW:
                        strKMLPart += "<Create><Folder targetId=\"" + strFolderPrefix + "p\">";
                        strKMLPart += (String)htKMLParts[strPartFile];
                        strKMLPart = strKMLPart.Replace("%ID%", "id=\"" + obj.ObjectID.ToString() + "\"");
                        strKMLPart = strKMLPart.Replace("%HEADLINE%", "Microsoft Flightsimulator X - " + strObjectName );
                        strKMLPart = strKMLPart.Replace("%ICON%", GetIconLink( icoObject ) );
                        obj.ReplaceObjectInfos(ref strKMLPart);
                        strKMLPart += "</Folder></Create>";
                        if (obj.pathPrediction != null)
                        {
                            strKMLPart += "<Create><Folder targetId=\"" + strFolderPrefix + "c\"><Placemark id=\"" + obj.ObjectID.ToString() + "pp\">";
                            strKMLPart += "<name>" + strObjectName + " Path Prediction</name><description>Path prediction of the " + strObjectName +".</description>";
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
                            strKMLPart += (String)htKMLParts[strPartFile];
                            strKMLPart = strKMLPart.Replace("%ID%", "targetId=\"" + obj.ObjectID.ToString() + "\"");
                            strKMLPart = strKMLPart.Replace("%HEADLINE%", "Microsoft Flightsimulator X - " + strObjectName );
                            strKMLPart = strKMLPart.Replace("%ICON%", GetIconLink( icoObject ));
                            obj.ReplaceObjectInfos(ref strKMLPart);
                        }
                        if (obj.HasMoved)
                        {
                            strKMLPart += "<Placemark targetId=\"" + obj.ObjectID.ToString() + "\"><Point><coordinates>" + obj.Coordinates + "</coordinates></Point></Placemark>";
                        }
                        strKMLPart += "</Change>";
                        if (obj.pathPrediction != null && obj.HasMoved)
                        {
                            strKMLPart += "<Change><Placemark targetId=\"" + obj.ObjectID.ToString() + "pp\"><LineString><coordinates>";
                            strKMLPart += obj.pathPrediction.Positions[0].Coordinate + " " + obj.pathPrediction.Positions[obj.pathPrediction.Positions.Length - 1].Coordinate;
                            strKMLPart += "</coordinates></LineString></Placemark></Change>";
                            strKMLPart += GenPredictionPoints(ref obj);
                            obj.pathPrediction.State = FsxConnection.SceneryObject.STATE.DATAREAD;
                        }
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

        private String GetIconLink(KML_ICON_TYPES icon)
        {
            return Program.Config.Server + "/gfx/ge/icons/" + strIconNames[(int)icon];
        }
    }
}
