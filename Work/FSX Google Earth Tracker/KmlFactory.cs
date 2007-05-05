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
                "fsxdme.png",
                "fsxndb.png",
                "fsxairport.png",
                "fsx-p-user.png",
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

//      public static int nFileNr = 1;

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

            lstImgs.Add(new ObjectImage( "Logo", "/gfx/logo.png", Assembly.GetCallingAssembly().GetManifestResourceStream("Fsxget.pub.gfx.logo.png")));
            imgNoImage = new ObjectImage("NoImage", "/gfx/noimage.png", Assembly.GetCallingAssembly().GetManifestResourceStream("Fsxget.pub.gfx.noimage.png"));

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
                            strKMLPart += (String)htKMLParts["fsxuc"];
                            strKMLPart = strKMLPart.Replace("%ID%", "id=\"" + fsxCon.objUserAircraft.ObjectID.ToString()+"\"");
                            strKMLPart = strKMLPart.Replace("%ICON%", Program.Config.Server + "/gfx/ge/icons/fsxu.png");
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
                            strKMLPart = strKMLPart.Replace("%ICON%", Program.Config.Server + "/gfx/ge/icons/fsxu.png");
                            strKMLPart += "</Change></Update>";
                            strKMLPart += (String)htKMLParts["fsxview"];
                            fsxCon.objUserAircraft.ReplaceObjectInfos(ref strKMLPart);
                            break;
                        case FsxConnection.SceneryObject.STATE.DELETED:
                            strKMLPart += "<Delete><Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "\"/></Placemark></Delete>";
                            strKMLPart += "<Delete><Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "p\"/></Placemark></Delete>";
                            strKMLPart += "<Delete><Placemark targetId=\"" + fsxCon.objUserAircraft.ObjectID.ToString() + "pp\"/></Placemark></Delete>";
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
                    if( fsxCon.objUserAircraft != null )
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
                return strUpdateKMLHeader + strKMLPart + "</Update>" + strUpdateKMLFooter;
            }                
        }
        public String GenAIAircraftUpdate()
        {
            String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
            lock (fsxCon.objAIAircrafts.lockObject)
            {
                strKML += GetAIObjectUpdate(fsxCon.objAIAircrafts.htObjects, "aia", "fsxau", KML_ICON_TYPES.AI_AIRCRAFT, KML_ICON_TYPES.AI_AIRCRAFT_PREDICTION_POINT, "9fd20091");
            }
            strKML += "</Update>" + strUpdateKMLFooter;
            return strKML;
        }
        public String GenAIHelicpoterUpdate()
        {
            String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
            lock (fsxCon.objAIHelicopters.lockObject)
            {
                strKML += GetAIObjectUpdate(fsxCon.objAIHelicopters.htObjects, "aih", "fsxhu", KML_ICON_TYPES.AI_HELICOPTER, KML_ICON_TYPES.AI_HELICOPTER_PREDICTION_POINT, "9fd20091");
            }
            strKML += "</Update>" + strUpdateKMLFooter;
            return strKML;
        }
        public String GenAIBoatUpdate()
        {
            String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_BOATS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
            lock (fsxCon.objAIBoats.lockObject)
            {
                strKML += GetAIObjectUpdate(fsxCon.objAIBoats.htObjects, "aib", "fsxbu", KML_ICON_TYPES.AI_BOAT, KML_ICON_TYPES.AI_BOAT_PREDICTION_POINT, "9fd20091");
            }
            strKML += "</Update>" + strUpdateKMLFooter;
//            File.WriteAllText(String.Format("C:\\temp\\boatupd{0}.kml", nFileNr++), strKML, Encoding.UTF8);
            return strKML;
        }
        public String GenAIGroundUnitUpdate()
        {
            String strKML = strUpdateKMLHeader + GetExpireString((uint)Program.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["Interval"].IntValue / 1000) + "<Update><targetHref>" + Program.Config.Server + "/fsxobjs.kml</targetHref>";
            lock (fsxCon.objAIGroundUnits.lockObject)
            {
                strKML += GetAIObjectUpdate(fsxCon.objAIGroundUnits.htObjects, "aig", "fsxgu", KML_ICON_TYPES.AI_GROUND_UNIT, KML_ICON_TYPES.AI_GROUND_PREDICTION_POINT, "9fd20091");
            }
            strKML += "</Update>" + strUpdateKMLFooter;
            return strKML;
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
                        strKMLPart = strKMLPart.Replace("%ICON%", GetIconLink( icoObject ) );
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
                        strKMLPart = strKMLPart.Replace("%ICON%", GetIconLink( icoObject ));
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
            return Program.Config.Server + "/gfx/ge/icons/" + strIconNames[(int)icon];
        }

        public String GetTemplate(String strName)
        {
            return (String)htKMLParts[strName];
        }

        public String GenVorKML(double dLongitude, double dLatitude, double dMagVar)
        {
            String strKMLPart = "";
            strKMLPart += "<?xml version=\"1.0\" encoding=\"UTF-8\"?><kml xmlns=\"http://earth.google.com/kml/2.1\"><Placemark><Style><LineStyle><width>3</width></LineStyle></Style><MultiGeometry>";
            strKMLPart += "<LineString><coordinates>";

            String strLines = "";
            double dLatResult = 0;
            double dLonResult = 0;
            double dLatResult2 = 0;
            double dLonResult2 = 0;
            double dHeading = 0;
            for (int i = 0; i <= 360; i += 5)
            {
                dHeading = i + dMagVar;
                MovePoint(dLongitude, dLatitude, dHeading, 5000, ref dLonResult, ref dLatResult);
                
                strKMLPart += XmlConvert.ToString(dLonResult) + "," + XmlConvert.ToString(dLatResult) + ",0 ";
                if (i % 10 == 0)
                {
                    bool b30 = i % 30 == 0;
                    strLines += "<LineString>";
                    strLines += "<coordinates>";
                    MovePoint(dLongitude, dLatitude, dHeading, b30 ? 4000 : 4500, ref dLonResult2, ref dLatResult2);
                    strLines += XmlConvert.ToString(dLonResult) + "," + XmlConvert.ToString(dLatResult) + ",0 ";
                    strLines += XmlConvert.ToString(dLonResult2) + "," + XmlConvert.ToString(dLatResult2) + ",0 ";
                    strLines += "</coordinates></LineString>";
                }
            }
            strKMLPart += "</coordinates></LineString>" + strLines;

            strKMLPart += "</MultiGeometry></Placemark></kml>";

            return strKMLPart;
        }

        public String GenVorKML2(double dLongitude, double dLatitude, double dMagVar)
        {
            String strKMLPart = (String)htKMLParts["fsxvoroverlay"];

            double dLatResult = 0;
            double dLonResult = 0;
            double dRadius = 5000;

            MovePoint(dLongitude, dLatitude, 0, dRadius, ref dLonResult, ref dLatResult);
            strKMLPart = strKMLPart.Replace( "%NORTH%", XmlConvert.ToString(dLatResult));
            MovePoint(dLongitude, dLatitude, 90, dRadius, ref dLonResult, ref dLatResult);
            strKMLPart = strKMLPart.Replace( "%EAST%", XmlConvert.ToString(dLonResult));
            MovePoint(dLongitude, dLatitude, 180, dRadius, ref dLonResult, ref dLatResult);
            strKMLPart = strKMLPart.Replace( "%SOUTH%", XmlConvert.ToString(dLatResult));
            MovePoint(dLongitude, dLatitude, 270, dRadius, ref dLonResult, ref dLatResult);
            strKMLPart = strKMLPart.Replace( "%WEST%", XmlConvert.ToString(dLonResult));
            strKMLPart = strKMLPart.Replace( "%MAGVAR%", XmlConvert.ToString(dMagVar));

            return strKMLPart;
        }

        private void MovePoint(double dLongitude, double dLatitude, double dHeading, double dDistMeter, ref double dLonResult, ref double dLatResult)
        {
            double dPI180 = Math.PI / 180;
            double d180PI = 180 / Math.PI;
            dDistMeter = (Math.PI / 10800) * dDistMeter / 1000;
            dHeading *= dPI180;
            dLatitude *= dPI180;
            dLongitude *= dPI180;
            double dDistSin = Math.Sin(dDistMeter);
            double dDistCos = Math.Cos(dDistMeter);
            double dLatSin = Math.Sin(dLatitude);
            double dLatCos = Math.Cos(dLatitude);

            dLatResult = Math.Asin(dLatSin * dDistCos + dLatCos * dDistSin * Math.Cos(dHeading));
            double d = -1 * (Math.Atan2(Math.Sin(dHeading) * dDistSin * dLatCos, dDistCos - dLatSin * Math.Sin(dLatResult)));
            dLonResult = (dLongitude - d + Math.PI) - (long)((dLongitude - d + Math.PI) / 2 / Math.PI) - Math.PI;

            dLatResult *= d180PI;
            dLonResult *= d180PI;
        }

    }
}
