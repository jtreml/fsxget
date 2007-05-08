using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.FlightSimulator.SimConnect;
using System.Timers;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO;
using System.Threading;
using System.Data.OleDb;

namespace Fsxget
{
/*      http://www.fsdeveloper.com/
        http://www.scruffyduckscenery.co.uk/
        http://www.scenery.org/design_utilities_e.htm
*/
    public class FsxConnection
    {

        #region Classes
        public class ObjectData<T>
        {
            private bool bModified;
            private T tValue;

            public ObjectData()
            {
                tValue = default(T);
                bModified = false;
            }

            public T Value
            {
                get
                {
                    bModified = false;
                    return tValue;
                }
                set
                {
                    if ((tValue == null && value != null) || !tValue.Equals(value))
                    {
                        bModified = true;
                        tValue = value;
                    }
                }
            }

            public bool IsModified
            {
                get
                {
                    return bModified;
                }
            }
        }
        public class ObjectPosition
        {
            private ObjectData<float> fLon;
            private ObjectData<float> fLat;
            private ObjectData<float> fAlt ;
            private double dTime;

            public ObjectPosition()
            {
                fLon = new ObjectData<float>();
                fLat = new ObjectData<float>();
                fAlt  = new ObjectData<float>();
            }

            public ObjectData<float> Longitude
            {
                get
                {
                    return fLon;
                }
            }
            public ObjectData<float> Latitude
            {
                get
                {
                    return fLat;
                }
            }
            public ObjectData<float> Altitude
            {
                get
                {
                    return fAlt ;
                }
            }
            public double Time
            {
                get
                {
                    return dTime;
                }
                set
                {
                    dTime = value;
                }
            }
            public bool HasMoved
            {
                get
                {
                    return (Longitude.IsModified || Latitude.IsModified || Altitude.IsModified);
                }
            }
            public String Coordinate
            {
                get
                {
                    return XmlConvert.ToString(fLon.Value) + "," + XmlConvert.ToString(fLat.Value) + "," + XmlConvert.ToString(fAlt .Value);
                }
            }
        }
        public class SceneryObject
        {
            public enum STATE
            {
                NEW,
                UNCHANGED,
                MODIFIED,
                DELETED,
                DATAREAD,
            }

            private STATE tState;
            private DATA_REQUESTS tType;
            private uint unID;
            public bool bDataRecieved;

            public SceneryObject(uint unID, DATA_REQUESTS tType)
            {
                tState = STATE.NEW;
                bDataRecieved = true;
                this.tType = tType;
                this.unID = unID;
            }
        
            public STATE State
            {
                get
                {
                    return tState;
                }
                set
                {
                    tState = value;
                }
            }
            public uint ObjectID
            {
                get
                {
                    return unID;
                }
            }
            public DATA_REQUESTS ObjectType
            {
                get
                {
                    return tType;
                }
            }
        }
        public class SceneryMovingObject : SceneryObject
        {
            #region Classes
            public class ObjectPath
            {
                private String strCoordinates;
                STATE tState;

                public ObjectPath()
                {
                    strCoordinates = "";
                    tState = STATE.NEW;
                }

                public ObjectPath( ref StructBasicMovingSceneryObject obj )
                {
                    tState = STATE.NEW;
                    AddPosition(ref obj);
                }

                public void AddPosition( ref StructBasicMovingSceneryObject obj )
                {
                    strCoordinates += XmlConvert.ToString((float)obj.dLongitude) + "," + XmlConvert.ToString((float)obj.dLatitude) + "," + XmlConvert.ToString((float)obj.dAltitude) + " ";
                    if (tState == STATE.DATAREAD)
                        tState = STATE.MODIFIED;
                }

                public void Clear()
                {
                    strCoordinates = "";
                    if (tState == STATE.DATAREAD)
                        tState = STATE.MODIFIED;
                }

                public STATE State
                {
                    get
                    {
                        return tState;
                    }
                    set
                    {
                        tState = value;
                    }
                }

                public String Coordinates
                {
                    get
                    {
                        return strCoordinates;
                    }
                }
            }
            public class PathPrediction
            {
                private bool bPredictionPoints;
                public ObjectPosition[] positions;
                private double dTimeElapsed;
                STATE tState;

                public PathPrediction(bool bWithPoints)
                {
                    tState = STATE.NEW;
                    HasPoints = bWithPoints;
                    dTimeElapsed = 0;
                }

                public void Update(ref StructBasicMovingSceneryObject obj)
                {
                    dTimeElapsed = obj.dTime - positions[0].Time;
                    if (dTimeElapsed > 0)
                    {
                        for (int i = 1; i < positions.Length; i++)
                        {
                            CalcPositionByTime(ref obj, ref positions[i]);
                        }
                        positions[0].Longitude.Value = (float)obj.dLongitude;
                        positions[0].Latitude.Value = (float)obj.dLatitude;
                        positions[0].Altitude.Value = (float)obj.dAltitude;
                        positions[0].Time = obj.dTime;

                        if (tState == STATE.DATAREAD)
                        {
                            if (positions[0].HasMoved)
                                tState = STATE.MODIFIED;
                            else
                                tState = STATE.UNCHANGED;
                        }
                    }
                }

                private void CalcPositionByTime(ref StructBasicMovingSceneryObject objNew, ref ObjectPosition tResultPos)
                {
                    double dScale = tResultPos.Time / dTimeElapsed;

                    tResultPos.Latitude.Value = (float) (objNew.dLatitude + dScale * (objNew.dLatitude - positions[0].Latitude.Value));
                    tResultPos.Longitude.Value = (float) (objNew.dLongitude + dScale * (objNew.dLongitude - positions[0].Longitude.Value));
                    tResultPos.Altitude.Value = (float) (objNew.dAltitude + dScale * (objNew.dAltitude - positions[0].Altitude.Value));
                }

                public bool HasPoints
                {
                    get
                    {
                        return bPredictionPoints;
                    }
                    set
                    {
                        if( bPredictionPoints != value )    
                        {
                            bPredictionPoints = value;
                            SettingsList lstPoint = (SettingsList)Program.Config[Config.SETTING.PREDICTION_POINTS];
                            if (bPredictionPoints)
                            {
                                positions = new ObjectPosition[lstPoint.listSettings.Count + 1];
                                for (int i = 0; i < positions.Length; i++)
                                {
                                    positions[i] = new ObjectPosition();
                                    if( i > 0 )
                                        positions[i].Time = (double)lstPoint["Time", i - 1].IntValue;
                                }
                            }
                        }
                        if (!bPredictionPoints && positions == null)
                        {
                            positions = new ObjectPosition[2];
                            positions[0] = new ObjectPosition();
                            positions[1] = new ObjectPosition();
                            positions[1].Time = 1200;
                        }
                        positions[0].Time = 0;
                    }
                }
                public ObjectPosition[] Positions
                {
                    get
                    {
                        return positions;
                    }
                }
                
                public STATE State
                {
                    get
                    {
                        return tState;
                    }
                    set
                    {
                        tState = value;
                    }
                }
            }
            #endregion

            #region Variables
            private ObjectData<String> strTitle;
            private ObjectData<String> strATCType;
            private ObjectData<String> strATCModel;
            private ObjectData<String> strATCID;
            private ObjectData<String> strATCAirline;
            private ObjectData<String> strATCFlightNumber;
            private ObjectPosition objPos;
            private ObjectData<float> fHeading;
            private double dTime;
            public ObjectPath objPath;
            public PathPrediction pathPrediction;
            #endregion

            public SceneryMovingObject(uint unID, DATA_REQUESTS tType, ref StructBasicMovingSceneryObject obj)
                : base( unID, tType )
            {
                strTitle = new ObjectData<String>();
                strATCType = new ObjectData<String>();
                strATCModel = new ObjectData<String>();
                strATCID = new ObjectData<String>();
                strATCAirline = new ObjectData<String>();
                strATCFlightNumber = new ObjectData<String>();
                objPos = new ObjectPosition();
                objPath = new ObjectPath(ref obj);
                fHeading = new ObjectData<float>();
                strTitle.Value = obj.szTitle;
                strATCType.Value = obj.szATCType;
                strATCModel.Value = obj.szATCModel;
                strATCID.Value = obj.szATCID;
                strATCAirline.Value = obj.szATCAirline;
                strATCFlightNumber.Value = obj.szATCFlightNumber;
                objPos.Longitude.Value = (float)obj.dLongitude;
                objPos.Latitude.Value = (float)obj.dLatitude;
                objPos.Altitude.Value = (float)obj.dAltitude;
                fHeading.Value = (float)obj.dHeading;
                dTime = obj.dTime;
                ConfigChanged();
            }

            public void Update(ref StructBasicMovingSceneryObject obj)
            {
                if (State == STATE.DELETED)
                    return;
                if (obj.dTime != dTime && pathPrediction != null)
                {
                    pathPrediction.Update( ref obj );
                }
                if (objPath != null && (obj.dLongitude != objPos.Longitude.Value || obj.dLatitude != objPos.Latitude.Value || obj.dAltitude != objPos.Altitude.Value))
                {
                    objPath.AddPosition(ref obj);
                }

                objPos.Longitude.Value = (float) obj.dLongitude;
                objPos.Latitude.Value = (float) obj.dLatitude;
                objPos.Altitude.Value = (float) obj.dAltitude;
                strTitle.Value = obj.szTitle;
                strATCType.Value = obj.szATCType;
                strATCModel.Value = obj.szATCModel;
                strATCID.Value = obj.szATCID;
                strATCAirline.Value = obj.szATCAirline;
                strATCFlightNumber.Value = obj.szATCFlightNumber;
                fHeading.Value = (float) obj.dHeading;
                dTime = obj.dTime;
                if (State == STATE.DATAREAD || State == STATE.UNCHANGED )
                {
                    if (HasMoved || HasChanged)
                        State = STATE.MODIFIED;
                    else
                        State = STATE.UNCHANGED;
                }
                bDataRecieved = true;
            }

            public void ConfigChanged()
            {
                bool bPath = false;
                bool bPrediction = false;
                bool bPredictionPoints = false;
                switch (ObjectType)
                {
                    case DATA_REQUESTS.REQUEST_USER_AIRCRAFT:
                        bPrediction = Program.Config[Config.SETTING.USER_PATH_PREDICTION]["Enabled"].BoolValue;
                        bPredictionPoints = true;
                        bPath = Program.Config[Config.SETTING.QUERY_USER_PATH]["Enabled"].BoolValue;
                        break;
                    case DATA_REQUESTS.REQUEST_AI_PLANE:
                        bPrediction = Program.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["Prediction"].BoolValue;
                        bPredictionPoints = Program.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["PredictionPoints"].BoolValue;
                        break;
                    case DATA_REQUESTS.REQUEST_AI_HELICOPTER:
                        bPrediction = Program.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["Prediction"].BoolValue;
                        bPredictionPoints = Program.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["PredictionPoints"].BoolValue;
                        break;
                    case DATA_REQUESTS.REQUEST_AI_BOAT:
                        bPrediction = Program.Config[Config.SETTING.QUERY_AI_BOATS]["Prediction"].BoolValue;
                        bPredictionPoints = Program.Config[Config.SETTING.QUERY_AI_BOATS]["PredictionPoints"].BoolValue;
                        break;
                    case DATA_REQUESTS.REQUEST_AI_GROUND:
                        bPrediction = Program.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["Prediction"].BoolValue;
                        bPredictionPoints = Program.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["PredictionPoints"].BoolValue;
                        break;
                }
                if (bPath && objPath == null)
                    objPath = new ObjectPath();
                else if (!bPath)
                    objPath = null;

                if (bPrediction)
                {
                    if (pathPrediction == null)
                        pathPrediction = new PathPrediction(bPredictionPoints);
                    else
                        pathPrediction.HasPoints = bPredictionPoints;
                }
                else
                {
                    pathPrediction = null;
                }
            }

            public void ReplaceObjectInfos(ref String str)
            {
                str = str.Replace("%TITLE%", Title.Value);
                str = str.Replace("%ATCTYPE%", ATCType.Value);
                str = str.Replace("%ATCMODEL%", ATCModel.Value);
                str = str.Replace("%ATCID%", ATCID.Value);
                str = str.Replace("%ATCFLIGHTNUMBER%", ATCFlightNumber.Value);
                str = str.Replace("%ATCAIRLINE%", ATCAirline.Value);
                str = str.Replace("%LONGITUDE%", XmlConvert.ToString(ObjectPosition.Longitude.Value));
                str = str.Replace("%LATITUDE%", XmlConvert.ToString(ObjectPosition.Latitude.Value));
                str = str.Replace("%ALTITUDE_UF%", String.Format("{0:F0}ft", ObjectPosition.Altitude.Value * 3.28095));
                str = str.Replace("%ALTITUDE%", XmlConvert.ToString(ObjectPosition.Altitude.Value));
                str = str.Replace("%HEADING%", XmlConvert.ToString(Heading.Value));
                str = str.Replace("%IMAGE%", Program.Config.Server + "/" + Title.Value);
            }

            #region Accessors
            public ObjectData<String> Title
            {
                get
                {
                    return strTitle;
                }
            }

            public ObjectData<String> ATCType
            {
                get
                {
                    return strATCType;
                }
            }

            public ObjectData<String> ATCModel
            {
                get
                {
                    return strATCModel;
                }
            }
            
            public ObjectData<String> ATCID
            {
                get
                {
                    return strATCID;
                }
            }

            public ObjectData<String> ATCAirline
            {
                get
                {
                    return strATCAirline;
                }
            }

            public ObjectData<String> ATCFlightNumber
            {
                get
                {
                    return strATCFlightNumber;
                }
            }

            public ObjectPosition ObjectPosition
            {
                get
                {
                    return objPos;
                }
            }

            public ObjectData<float> Heading
            {
                get
                {
                    return fHeading;
                }
            }

            public String Coordinates
            {
                get
                {
                    return XmlConvert.ToString(objPos.Longitude.Value) + "," + XmlConvert.ToString(objPos.Latitude.Value) + "," + XmlConvert.ToString(objPos.Altitude.Value);
                }
            }
            
            public double Time
            {
                get
                {
                    return dTime;
                }
            }

            public bool HasMoved
            {
                get
                {
                    return objPos.HasMoved || fHeading.IsModified;
                }
            }
            
            public bool HasChanged
            {
                get
                {
                    if( strTitle.IsModified || 
                        strATCType.IsModified ||
                        strATCModel.IsModified ||
                        strATCID.IsModified ||
                        strATCAirline.IsModified ||
                        strATCFlightNumber.IsModified )
                        return true;
                    else
                        return false;
                }
            }

            #endregion
        }
        public class FlightPlan : SceneryObject
        {
            public class Waypoint
            {
                #region Variables
                private String strName;
                private float fLon;
                private float fLat;
                KmlFactory.KML_ICON_TYPES tIconType;
                #endregion

                public Waypoint(String strName, float fLon, float fLat, KmlFactory.KML_ICON_TYPES tIconType)
                {
                    this.strName = strName;
                    this.fLon = fLon;
                    this.fLat = fLat;
                    this.tIconType = tIconType;
                }

                #region Accessors
                public String Name
                {
                    get
                    {
                        return strName;
                    }
                }
                public float Longitude
                {
                    get
                    {
                        return fLon;
                    }
                }
                public float Latitude
                {
                    get
                    {
                        return fLat;
                    }
                }
                public KmlFactory.KML_ICON_TYPES IconType
                {
                    get
                    {
                        return tIconType;
                    }
                }
                #endregion
            }

            private List<Waypoint> lstWaypoints;
            private String strName;

            public FlightPlan(uint unID, DATA_REQUESTS tType)
                : base( unID, tType )
            {
                lstWaypoints = new List<Waypoint>();
            }

            public void AddWaypoint(String strName, float fLon, float fLat, KmlFactory.KML_ICON_TYPES tIconType)
            {
                lstWaypoints.Add( new Waypoint( String.Format( "Waypoint {0}: {1} ", lstWaypoints.Count+1, strName ), fLon, fLat, tIconType ));
            }
            public void AddWaypoint(XmlNode xmln)
            {
                String str;
                KmlFactory.KML_ICON_TYPES tIconType = KmlFactory.KML_ICON_TYPES.NONE;
                String strName = "";
                float fLon = 0;
                float fLat = 0;

                if (xmln.Name != "ATCWaypoint")
                    throw new InvalidDataException("XmlNode must have the name ATCWaypoint");

                for (XmlNode node = xmln.FirstChild; node != null; node = node.NextSibling)
                {
                    if (node.Name == "ATCWaypointType")
                    {
                        str = node.InnerText.ToLower();
                        if (str == "intersection")
                        {
                            strName += "Intersection ";
                            tIconType = KmlFactory.KML_ICON_TYPES.PLAN_INTER;
                        }
                        else if (str == "vor")
                        {
                            strName += "VOR ";
                            tIconType = KmlFactory.KML_ICON_TYPES.VOR;
                        }
                        else if (str == "airport")
                        {
                            strName += "Airport ";
                            tIconType = KmlFactory.KML_ICON_TYPES.AIRPORT;
                        }
                        else if (str == "ndb")
                        {
                            strName += "NDB ";
                            tIconType = KmlFactory.KML_ICON_TYPES.NDB;
                        }
                        else if (str == "user")
                        {
                            strName += "User ";
                            tIconType = KmlFactory.KML_ICON_TYPES.PLAN_USER;
                        }
                        else
                        {
                            strName += xmln.InnerText + " ";
                            tIconType = KmlFactory.KML_ICON_TYPES.NONE;
                        }
                    }
                    else if (node.Name == "WorldPosition")
                    {
                        String[] strCoords = node.InnerText.Split( ',' );
                        if (strCoords.Length != 3)
                            throw new InvalidDataException("Invalid coordinateformat");
                        fLat = FsxConnection.ConvertDegToFloat(strCoords[0]);
                        fLon = FsxConnection.ConvertDegToFloat(strCoords[1]);
                    }
                }
                if (xmln["ICAO"]["ICAOIdent"] != null)
                    strName += xmln["ICAO"]["ICAOIdent"].InnerText;
                else if (xmln.Attributes["id"] != null)
                    strName += xmln.Attributes["id"].Value;

                AddWaypoint(strName, fLon, fLat, tIconType);
            }

            public String Name
            {
                get
                {
                    return strName;
                }
                set
                {
                    strName = value;
                }
            }
            public List<Waypoint> Waypoints
            {
                get
                {
                    return lstWaypoints;
                }
            }
        }

        public class SceneryNavAid : SceneryObject
        {
            private float fLon;
            private float fLat;
            private float fAlt;
            private float fFreq;
            private float fRange;
            private float fMagVar;
            private String strIdent;
            private String strName;
            private KmlFactory.KML_ICON_TYPES tIconType;

            public SceneryNavAid(uint unID, int nType, String strIdent, String strName, float fLon, float fLat, float fAlt, float fFreq, float fRange, float fMagVar)
                : base( unID, DATA_REQUESTS.NAVAIDS )
            {
                switch (nType)
                {
                    case 0:
                        tIconType = KmlFactory.KML_ICON_TYPES.DME;
                        break;
                    case 1:
                        tIconType = KmlFactory.KML_ICON_TYPES.VOR;
                        break;
                    case 2:
                        tIconType = KmlFactory.KML_ICON_TYPES.VORDME;
                        break;
                    case 3:
                        tIconType = KmlFactory.KML_ICON_TYPES.NDB;
                        break;
                    default:
                        tIconType = KmlFactory.KML_ICON_TYPES.NONE;
                        break;
                }
                this.strIdent = strIdent;
                this.strName = strName;
                this.fLon = fLon;
                this.fLat = fLat;
                this.fAlt = fAlt;
                this.fFreq = fFreq;
                this.fRange = fRange;
                this.fMagVar = fMagVar;
                this.State = STATE.NEW;
            }

            bool IsInRegion(float fNorth, float fEast, float fSouth, float fWest)
            {
                return fLon >= fWest && fLon <= fEast && fLat >= fNorth && fLat <= fSouth;
            }

            #region Accessors
            public String Ident
            {
                get
                {
                    return strIdent;
                }
            }
            public String Name
            {
                get
                {
                    return strName;
                }
            }
            public String MorseCode
            {
                get
                {
                    return FsxConnection.GetMorseCode(strIdent);
                }
            }
            public float Longitude
            {
                get
                {
                    return fLon;
                }
            }
            public float Latitude
            {
                get
                {
                    return fLat;
                }
            }
            public float Altitude
            {
                get
                {
                    return fAlt;
                }
            }
            public float Range
            {
                get
                {
                    return fRange;
                }
            }
            public float Frequency
            {
                get
                {
                    return fFreq;
                }
            }
            public float MagVar
            {
                get
                {
                    return fMagVar;
                }
            }
            public KmlFactory.KML_ICON_TYPES IconType
            {
                get
                {
                    return tIconType;
                }
            }
            #endregion
        }
        #endregion

        #region Variables
        private FsxgetForm frmMain;
        private IntPtr frmMainHandle;
        private const int WM_USER_SIMCONNECT = 0x0402;
        private System.Timers.Timer timerConnect;
        private System.Timers.Timer timerQueryUserAircraft;
        public SceneryMovingObject objUserAircraft;
        public Object lockUserAircraft;
        private uint uiUserAircraftID;
        private SimConnect simconnect;
        public Object lockSimConnect;
        public StructObjectContainer objAIAircrafts;
        public StructObjectContainer objAIHelicopters;
        public StructObjectContainer objAIBoats;
        public StructObjectContainer objAIGroundUnits;
        public StructObjectContainer objNavAids;
        public Hashtable htFlightPlans;
        private uint unFlightPlanNr;
        private OleDbConnection dbCon;
        static String[] strMorseSigns = new String[]
        {
            "-----",
            ".----",
            "..---",
            "...--",
            "....-",
            ".....",
            "-....",
            "--...",
            "---..",
            "----.",
            ".- ",
            "-... ",
            "-.-. ",
            "-.. ",
            ". ",
            "..-. ",
            "--. ",
            ".... ",
            ".. ",
            ".--- ",
            "-.- ",
            ".-.. ",
            "-- ",
            "-. ",
            "--- ",
            ".--. ",
            "--.- ",
            ".-. ",
            "... ",
            "- ",
            "..- ",
            "...- ",
            ".-- ",
            "-..- ",
            "-.-- ",
            "--.. ",
        };
        #endregion

        #region Structs & Enums
        public enum EVENT_ID
        {
            EVENT_MENU,
            EVENT_MENU_START,
            EVENT_MENU_STOP,
            EVENT_MENU_OPTIONS,
            EVENT_MENU_CLEAR_USER_PATH,
            EVENT_SET_NAV1,
            EVENT_SET_NAV2,
            EVENT_SET_ADF,
        };
        public enum GROUP_ID
        {
            GROUP_USER,
        }
        public enum DEFINITIONS
        {
            StructBasicMovingSceneryObject,
        };

        public enum DATA_REQUESTS
        {
            REQUEST_USER_AIRCRAFT,
            REQUEST_AI_HELICOPTER,
            REQUEST_AI_PLANE,
            REQUEST_AI_BOAT,
            REQUEST_AI_GROUND,
            FLIGHTPLAN,
            NAVAIDS
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct StructBasicMovingSceneryObject
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
            public double dTime;
            public double dHeading;
        };

        public struct StructObjectContainer
        {
            public Object lockObject;
            public Hashtable htObjects;
            public System.Timers.Timer timer;
            public int nPreAnz;
            public int nPostAnz;
        }
        #endregion

        #region Construction
        public FsxConnection(FsxgetForm frmMain, bool bAddOn)
        {
            this.frmMain = frmMain;
            this.frmMainHandle = frmMain.Handle;
            unFlightPlanNr = 1;
            simconnect = null;
            if (bAddOn)
            {
                if (openConnection())
                {
                    AddMenuItems();
                }
            }
            else
            {
                timerConnect = new System.Timers.Timer();
                timerConnect.Interval = 3000;
                timerConnect.Elapsed += new ElapsedEventHandler(OnTimerConnectElapsed);
            }

            objAIAircrafts = new StructObjectContainer();
            objAIAircrafts.lockObject = new Object();
            objAIAircrafts.htObjects = new Hashtable();
            objAIAircrafts.timer = new System.Timers.Timer();
            objAIAircrafts.timer.Elapsed += new ElapsedEventHandler(OnTimerQueryAIAircraftsElapsed);

            objAIHelicopters = new StructObjectContainer();
            objAIHelicopters.lockObject = new Object();
            objAIHelicopters.htObjects = new Hashtable();
            objAIHelicopters.timer = new System.Timers.Timer();
            objAIHelicopters.timer.Elapsed += new ElapsedEventHandler(OnTimerQueryAIHelicoptersElapsed);

            objAIBoats= new StructObjectContainer();
            objAIBoats.lockObject = new Object();
            objAIBoats.htObjects = new Hashtable();
            objAIBoats.timer = new System.Timers.Timer();
            objAIBoats.timer.Elapsed += new ElapsedEventHandler(OntimerQueryAIBoatsElapsed);

            objAIGroundUnits = new StructObjectContainer();
            objAIGroundUnits.lockObject = new Object();
            objAIGroundUnits.htObjects = new Hashtable();
            objAIGroundUnits.timer = new System.Timers.Timer();
            objAIGroundUnits.timer.Elapsed += new ElapsedEventHandler(OnTimerQueryAIGroundUnitsElapsed);

            objNavAids = new StructObjectContainer();
            objNavAids.lockObject = new Object();
            objNavAids.htObjects = new Hashtable();
            objNavAids.timer = new System.Timers.Timer();
            objNavAids.timer.Elapsed += new ElapsedEventHandler(OnTimerQueryNavAidsElapsed);

            timerQueryUserAircraft = new System.Timers.Timer();
            timerQueryUserAircraft.Elapsed += new ElapsedEventHandler(OnTimerQueryUserAircraftElapsed);

            lockUserAircraft = new Object();
            lockSimConnect = new Object();

            htFlightPlans = new Hashtable();

            dbCon = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Program.Config.AppPath + "\\data\\fsxget.mdb");
            dbCon.Open();
        }

        #endregion

        #region FSX-Handling
        public void Connect()
        {
            lock (lockSimConnect)
            {
                if (simconnect == null)
                    timerConnect.Start();
            }
        }
        public void Disconnect()
        {
            closeConnection();
            timerConnect.Stop();
        }
        private bool openConnection()
        {
            if (simconnect == null)
            {
                try
                {
                    simconnect = new SimConnect(frmMain.Text, frmMainHandle, WM_USER_SIMCONNECT, null, 0);
                    if (initDataRequest())
                    {
                        InitializeTimers();
                        return true;
                    }
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
        public void closeConnection()
        {
            lock (lockSimConnect)
            {
                if (simconnect != null)
                {
                    EnableTimers(false);
                    DeleteAllObjects();
                    frmMain.Connected = false;
                    simconnect.Dispose();
                    simconnect = null;
                }
            }
        }
        private bool initDataRequest()
        {
            try
            {
                // listen to connect and quit msgs
                simconnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(simconnect_OnRecvOpen);
                simconnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(simconnect_OnRecvQuit);
                simconnect.OnRecvEvent += new SimConnect.RecvEventEventHandler(simconnect_OnRecvEvent);

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
                simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "Absolute Time", "seconds", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "PLANE HEADING DEGREES TRUE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                simconnect.MapClientEventToSimEvent(EVENT_ID.EVENT_SET_NAV1, "NAV1_RADIO_SET");
                simconnect.MapClientEventToSimEvent(EVENT_ID.EVENT_SET_NAV2, "NAV2_RADIO_SET");
                simconnect.MapClientEventToSimEvent(EVENT_ID.EVENT_SET_ADF, "ADF_SET");
                //                simconnect.SetNotificationGroupPriority(GROUP_ID.GROUP_USER, SimConnect.SIMCONNECT_GROUP_PRIORITY_HIGHEST);
                // IMPORTANT: register it with the simconnect managed wrapper marshaller
                // if you skip this step, you will only receive a uint in the .dwData field.
                simconnect.RegisterDataDefineStruct<StructBasicMovingSceneryObject>(DEFINITIONS.StructBasicMovingSceneryObject);

                // catch a simobject data request
                simconnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(simconnect_OnRecvSimobjectDataBytype);

                return true;
            }
            catch (COMException ex)
            {
                frmMain.NotifyError("FSX Exception!\n\n" + ex.Message);
                return false;
            }
        }
        private void AddMenuItems()
        {
            if (simconnect != null)
            {
                try
                {
                    simconnect.MenuAddItem(frmMain.Text, EVENT_ID.EVENT_MENU, 0);
                    simconnect.MenuAddSubItem(EVENT_ID.EVENT_MENU, "&Start", EVENT_ID.EVENT_MENU_START, 0);
                    simconnect.MenuAddSubItem(EVENT_ID.EVENT_MENU, "Sto&p", EVENT_ID.EVENT_MENU_STOP, 0);
                    simconnect.MenuAddSubItem(EVENT_ID.EVENT_MENU, "&Options", EVENT_ID.EVENT_MENU_OPTIONS, 0);
                    simconnect.MenuAddSubItem(EVENT_ID.EVENT_MENU, "&Clear User Aircraft Path", EVENT_ID.EVENT_MENU_CLEAR_USER_PATH, 0);
                }
                catch (COMException e)
                {
                    frmMain.NotifyError("FSX Add MenuItem failed!\n\n" + e.Message);
                }
            }
        }
        private void simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
 
        }
        private void simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            DeleteAllObjects();
            closeConnection();
            frmMain.Connected = false;
            if (timerConnect != null)
                timerConnect.Start();
            else
                frmMain.Close();
        }
        private void simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            frmMain.NotifyError( "FSX Exception!" );
        }
        private void simconnect_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            switch ((EVENT_ID)data.uEventID)
            {
                case EVENT_ID.EVENT_MENU_OPTIONS:
                    frmMain.Show();
                    break;
            }
        }
        private void simconnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            StructBasicMovingSceneryObject obj = (StructBasicMovingSceneryObject)data.dwData[0];

            switch ((DATA_REQUESTS)data.dwRequestID)
            {
                case DATA_REQUESTS.REQUEST_USER_AIRCRAFT:
                    lock (lockUserAircraft)
                    {
                        if (objUserAircraft != null)
                        {
                            objUserAircraft.Update(ref obj);
                            uiUserAircraftID = data.dwObjectID;
                        }
                        else
                            objUserAircraft = new SceneryMovingObject(data.dwObjectID, DATA_REQUESTS.REQUEST_USER_AIRCRAFT, ref obj);
                    }
                    break;
                case DATA_REQUESTS.REQUEST_AI_PLANE:
                    lock (objAIAircrafts.lockObject)
                    {
                        if (data.dwObjectID != uiUserAircraftID)
                        {
                            HandleSimObjectRecieved(ref objAIAircrafts, ref data );
                        }
                    }
                    break;
                case DATA_REQUESTS.REQUEST_AI_HELICOPTER:
                    lock (objAIHelicopters.lockObject)
                    {
                        if (data.dwObjectID != uiUserAircraftID)
                        {
                            HandleSimObjectRecieved(ref objAIHelicopters, ref data);
                        }
                    }
                    break;
                case DATA_REQUESTS.REQUEST_AI_BOAT:
                    lock (objAIBoats.lockObject)
                    {
                        HandleSimObjectRecieved(ref objAIBoats, ref data);
                    }
                    break;
                case DATA_REQUESTS.REQUEST_AI_GROUND:
                    lock (objAIGroundUnits.lockObject)
                    {
                        HandleSimObjectRecieved(ref objAIGroundUnits, ref data);
                    }
                    break;
                default:
#if DEBUG
                    frmMain.NotifyError("Received unknown data from FSX!");
#endif
                    break;
            }
        }
        private void HandleSimObjectRecieved(ref StructObjectContainer objs, ref SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            StructBasicMovingSceneryObject obj = (StructBasicMovingSceneryObject)data.dwData[0];
            if (data.dwoutof == 0)
                MarkDeletedObjects(ref objs.htObjects);
            else
            {
                if( data.dwentrynumber == 1 )   
                {
                    objs.nPreAnz = objs.htObjects.Count;
                    objs.nPostAnz = 0;
                }
                if (objs.htObjects.ContainsKey(data.dwObjectID))
                {
                    ((SceneryMovingObject)objs.htObjects[data.dwObjectID]).Update(ref obj);
                    objs.nPostAnz++;
                }
                else
                {
                    objs.htObjects.Add(data.dwObjectID, new SceneryMovingObject(data.dwObjectID, (DATA_REQUESTS)data.dwRequestID, ref obj));
                }
                if (data.dwentrynumber == data.dwoutof && objs.nPostAnz < objs.nPreAnz)
                {
                    MarkDeletedObjects(ref objs.htObjects);
                }
            }
        }

        private void MarkDeletedObjects(ref Hashtable ht)
        {
            foreach (DictionaryEntry entry in ht)
            {
                if (!((SceneryObject)entry.Value).bDataRecieved)
                {
                    ((SceneryObject)entry.Value).State = SceneryMovingObject.STATE.DELETED;
                }
                ((SceneryObject)entry.Value).bDataRecieved = false;
            }
        }
        public void DeleteAllObjects()
        {
            lock (lockUserAircraft)
            {
                objUserAircraft.State = SceneryObject.STATE.DELETED;
            }
            lock (objAIAircrafts.lockObject)
            {
                foreach (DictionaryEntry entry in objAIAircrafts.htObjects)
                {
                    ((SceneryObject)(entry.Value)).State = SceneryObject.STATE.DELETED; 
                }
            }
            lock (objAIHelicopters.lockObject)
            {
                foreach (DictionaryEntry entry in objAIHelicopters.htObjects)
                {
                    ((SceneryObject)(entry.Value)).State = SceneryObject.STATE.DELETED;
                }
            }
            lock (objAIBoats.lockObject)
            {
                foreach (DictionaryEntry entry in objAIBoats.htObjects)
                {
                    ((SceneryObject)(entry.Value)).State = SceneryObject.STATE.DELETED;
                }
            }
            lock (objAIGroundUnits.lockObject)
            {
                foreach (DictionaryEntry entry in objAIGroundUnits.htObjects)
                {
                    ((SceneryObject)(entry.Value)).State = SceneryObject.STATE.DELETED;
                }
            }
            lock (objNavAids.lockObject)
            {
                foreach (DictionaryEntry entry in objNavAids.htObjects)
                {
                    ((SceneryObject)(entry.Value)).State = SceneryObject.STATE.DELETED;
                }
            }
        }
        public void CleanupHashtable(ref Hashtable ht)
        {
            ArrayList toDel = new ArrayList();
            foreach (DictionaryEntry entry in ht)
            {
                if (((SceneryObject)entry.Value).State == SceneryObject.STATE.DELETED )
                {
                    toDel.Add(entry.Key);
                }
            }
            foreach (object key in toDel)
            {
                ht.Remove(key);
            }
        }
        public bool SetFrequency(String strType, double dFreq)
        {
            bool bRet = true;
            try
            {
                strType = strType.ToLower();
                if (strType == "nav1")
                {
                    
                    simconnect.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_SET_NAV1, UIntToBCD((uint)(dFreq*100 )), GROUP_ID.GROUP_USER, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                }
                else if (strType == "nav2")
                    simconnect.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_SET_NAV2, UIntToBCD((uint)(dFreq * 100)), GROUP_ID.GROUP_USER, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                else if (strType == "adf")
                {
                    // TODO: ADF settings are wrong
                    simconnect.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_SET_ADF, UIntToBCD((uint)(dFreq * 10)), GROUP_ID.GROUP_USER, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                }
                else
                    bRet = false;
            }
            catch 
            {
                bRet = false;
            }
            return bRet;
        }

        public bool OnMessageReceive(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == WM_USER_SIMCONNECT)
            {
                if (simconnect != null)
                {
                    try
                    {
                        simconnect.ReceiveMessage();
                    }
                    catch(Exception e)
                    {
#if DEBUG
                        frmMain.NotifyError("Error receiving data from FSX!\n\n" + e.Message );
#endif
                    }
                }
                return true;
            }
  
            return false;
        }
        #endregion

        #region Timerfunctions
        public void InitializeTimers()
        {
            timerQueryUserAircraft.Stop();
            timerQueryUserAircraft.Interval = Program.Config[Config.SETTING.QUERY_USER_AIRCRAFT]["Interval"].IntValue;

            objAIAircrafts.timer.Stop();
            objAIAircrafts.timer.Interval = Program.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["Interval"].IntValue;

            objAIHelicopters.timer.Stop();
            objAIHelicopters.timer.Interval = Program.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["Interval"].IntValue;

            objAIBoats.timer.Stop();
            objAIBoats.timer.Interval = Program.Config[Config.SETTING.QUERY_AI_BOATS]["Interval"].IntValue;

            objAIGroundUnits.timer.Stop();
            objAIGroundUnits.timer.Interval = Program.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["Interval"].IntValue;

            objNavAids.timer.Stop();
            objNavAids.timer.Interval = Program.Config[Config.SETTING.QUERY_NAVAIDS]["Interval"].IntValue * 1000;

            EnableTimers();
        }
        public void EnableTimers()
        {
            EnableTimers(true);
        }
        public void EnableTimers(bool bEnable)
        {
            bool bQueryAI = Program.Config[Config.SETTING.QUERY_AI_OBJECTS]["Enabled"].BoolValue;
            timerQueryUserAircraft.Enabled = bEnable && Program.Config[Config.SETTING.QUERY_USER_AIRCRAFT]["Enabled"].BoolValue;
            objAIAircrafts.timer.Enabled = bEnable && bQueryAI && Program.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["Enabled"].BoolValue;
            objAIHelicopters.timer.Enabled = bEnable && bQueryAI && Program.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["Enabled"].BoolValue;
            objAIBoats.timer.Enabled = bEnable && bQueryAI && Program.Config[Config.SETTING.QUERY_AI_BOATS]["Enabled"].BoolValue;
            objAIGroundUnits.timer.Enabled = bEnable && bQueryAI && Program.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["Enabled"].BoolValue;
            objNavAids.timer.Enabled = bEnable && Program.Config[Config.SETTING.QUERY_NAVAIDS]["Enabled"].BoolValue;
        }

        private void OnTimerConnectElapsed(object sender, ElapsedEventArgs e)
        {
            if (openConnection())
            {
                frmMain.Connected = true;
                timerConnect.Stop();
            }
        }
        private void OnTimerQueryUserAircraftElapsed(object sender, ElapsedEventArgs e)
        {
            lock (lockSimConnect)
            {
                try
                {
                    simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_USER_AIRCRAFT, DEFINITIONS.StructBasicMovingSceneryObject, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
                }
                catch
                {
                }
            }
        }
        private void OnTimerQueryAIAircraftsElapsed(object sender, ElapsedEventArgs e)
        {
            lock (lockSimConnect)
            {
                try
                {
                    simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_AI_PLANE, DEFINITIONS.StructBasicMovingSceneryObject, (uint)Program.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["Range"].IntValue, SIMCONNECT_SIMOBJECT_TYPE.AIRCRAFT);
                
                }
                catch (COMException ex)
                {
                    frmMain.NotifyError(ex.Message);
                }
            }
        }
        private void OnTimerQueryAIHelicoptersElapsed(object sender, ElapsedEventArgs e)
        {
            lock (lockSimConnect)
            {
                try
                {
                    simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_AI_HELICOPTER, DEFINITIONS.StructBasicMovingSceneryObject, (uint)Program.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["Range"].IntValue, SIMCONNECT_SIMOBJECT_TYPE.HELICOPTER);
                }
                catch
                {
                }
            }
        }
        private void OntimerQueryAIBoatsElapsed(object sender, ElapsedEventArgs e)
        {
            lock (lockSimConnect)
            {
                try
                {
                    simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_AI_BOAT, DEFINITIONS.StructBasicMovingSceneryObject, (uint)Program.Config[Config.SETTING.QUERY_AI_BOATS]["Range"].IntValue, SIMCONNECT_SIMOBJECT_TYPE.BOAT);
                }
                catch
                {
                }
            }
        }
        private void OnTimerQueryAIGroundUnitsElapsed(object sender, ElapsedEventArgs e)
        {
            lock (lockSimConnect)
            {
                try
                {
                    simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_AI_GROUND, DEFINITIONS.StructBasicMovingSceneryObject, (uint)Program.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["Range"].IntValue, SIMCONNECT_SIMOBJECT_TYPE.GROUND);
                }
                catch
                {
                }
            }
        }
        private void OnTimerQueryNavAidsElapsed(object sender, ElapsedEventArgs e)
        {
            lock (objNavAids.lockObject)
            {
                if (objUserAircraft != null)
                {
                    float fNorth = 0;
                    float fEast = 0;
                    float fSouth = 0;
                    float fWest = 0;
                    float fTmp = 0;
                    KmlFactory.MovePoint(objUserAircraft.ObjectPosition.Longitude.Value, objUserAircraft.ObjectPosition.Latitude.Value, 0, Program.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fTmp, ref fNorth);
                    KmlFactory.MovePoint(objUserAircraft.ObjectPosition.Longitude.Value, objUserAircraft.ObjectPosition.Latitude.Value, 90, Program.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fEast, ref fTmp);
                    KmlFactory.MovePoint(objUserAircraft.ObjectPosition.Longitude.Value, objUserAircraft.ObjectPosition.Latitude.Value, 180, Program.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fTmp, ref fSouth);
                    KmlFactory.MovePoint(objUserAircraft.ObjectPosition.Longitude.Value, objUserAircraft.ObjectPosition.Latitude.Value, 270, Program.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fWest, ref fTmp);

                    OleDbCommand cmd = new OleDbCommand("SELECT ID, Ident, Name, Type, Longitude, Latitude, Altitude, MagVar, Range, Freq FROM navaids WHERE " +
                        "Latitude >= " + fSouth.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + " AND " +
                        "Latitude <= " + fNorth.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + " AND " +
                        "Longitude >= " + fWest.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + " AND " +
                        "Longitude <= " + fEast.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + ";", dbCon);

                    OleDbDataReader rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        uint unID = (uint)rd.GetInt32(0);
                        if (objNavAids.htObjects.ContainsKey(unID))
                        {
                            ((SceneryObject)objNavAids.htObjects[unID]).bDataRecieved = true;
                        }
                        else
                        {
                            objNavAids.htObjects.Add(unID, new SceneryNavAid(unID, (int)rd.GetByte(3), rd.GetString(1), rd.GetString(2), rd.GetFloat(4), rd.GetFloat(5), rd.GetFloat(6), rd.GetFloat(9), rd.GetFloat(8), rd.GetFloat(7)));
                        }
                    }
                    MarkDeletedObjects(ref objNavAids.htObjects);
                    rd.Close();
                }
            }
        }
        #endregion

        public void AddFlightPlan(String strFileName)
        {
            try
            {
                XmlDocument xmld = new XmlDocument();
                xmld.Load(strFileName);

                FlightPlan flightPlan = new FlightPlan(unFlightPlanNr, DATA_REQUESTS.FLIGHTPLAN);

                XmlElement xmle = xmld["SimBase.Document"]["FlightPlan.FlightPlan"];
                if (xmle == null)
                    throw new InvalidDataException("This is not a FSX flightplan");

                xmle = xmle["Title"];
                if (xmle != null)
                    flightPlan.Name = xmle.InnerText;
                else
                    flightPlan.Name = "Flightplan";

                xmle = xmle.ParentNode["FPType"];
                if (xmle != null)
                    flightPlan.Name += " (" + xmle.InnerText + ")";

                XmlNodeList xmlnWP = xmld.GetElementsByTagName("ATCWaypoint");
                foreach (XmlNode xmln in xmlnWP)
                {
                    flightPlan.AddWaypoint(xmln);
                }
                htFlightPlans.Add(unFlightPlanNr++, flightPlan);
            }
            catch
            {
                frmMain.NotifyError("Can not load the flight plan");
            }
        }

        public void GetSceneryObjects()
        {
            String strPath = Path.GetDirectoryName(Program.Config.FSXPath);
            String strBGL2XMLPath = @"C:\Programme\Microsoft Games\Microsoft Flight Simulator X SDK\Tools\BGL2XML_CMD\Bgl2Xml.exe";
            strPath += "\\Scenery";
            String strTmpFile = Path.GetTempFileName();
            String[] strFiles = Directory.GetFiles(strPath, "NVX*.bgl", SearchOption.AllDirectories);

            int nVORs = 0;
            int nNDBs = 0;

            String strName = "";
            String strIdent = "";
            String strRegion = "";
            float fLon = 0.0f;
            float fLat = 0.0f;
            float fFreq = 0.0f;
            float fMagVar = 0.0f;
            float fAlt = 0.0f;
            float fRange = 0.0f;

            OleDbConnection dbCon = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Program.Config.AppPath + "\\data\\fsxget.mdb");
            dbCon.Open();
            OleDbCommand cmd = new OleDbCommand("DELETE * FROM navaids", dbCon);
            cmd.ExecuteNonQuery();

            foreach (String strBGLFile in strFiles)
            {
                System.Diagnostics.Trace.WriteLine(strBGLFile);
                try
                {
                    System.Diagnostics.ProcessStartInfo ps = new System.Diagnostics.ProcessStartInfo(strBGL2XMLPath, "\"" + strBGLFile + "\" \"" + strTmpFile + "\"");
                    ps.CreateNoWindow = true;
                    ps.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(ps);
                    int nSecs = 0;
                    while (!p.HasExited && nSecs < 10)
                    {
                        nSecs++;
                        Thread.Sleep(1000);
                    }
                    if (!p.HasExited)
                    {
                        System.Diagnostics.Trace.WriteLine("Killed");
                        p.Kill();
                        continue;
                    }
                }
                catch
                {
                }

                try
                {
                    XmlDocument xmld = new XmlDocument();
                    xmld.Load(strTmpFile);

                    XmlNodeList nodes = xmld.GetElementsByTagName("Vor");

                    foreach (XmlNode xmln in nodes)
                    {
                        bool bDme = false;
                        bool bDmeOnly = false;
                        foreach (XmlAttribute xmla in xmln.Attributes)
                        {
                            if (xmla.Name == "dme")
                                bDme = xmla.Value.ToLower() == "true";
                            else if (xmla.Name == "dmeOnly")
                                bDmeOnly = xmla.Value.ToLower() == "true";
                            else if (xmla.Name == "lat")
                                fLat = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "lon")
                                fLon = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "alt")
                            {
                                fAlt  = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
                            }
                            else if (xmla.Name == "range")
                                fRange = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "frequency")
                                fFreq = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "magvar")
                            {
                                fMagVar = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            }
                            else if (xmla.Name == "ident")
                            {
                                strIdent = xmla.Value;
                            }
                            else if (xmla.Name == "name")
                                strName = xmla.Value;
                            else if (xmla.Name == "region")
                                strRegion = xmla.Value;
                        }
                        int nType;
                        if (bDmeOnly)
                        {
                            nType = 0;      // Only DME
                        }
                        else
                        {
                            if (bDme)
                            {
                                nType = 2;  // VOR / DME
                            }
                            else
                            {
                                nType = 1;  // VOR
                            }
                        }
                        nVORs++;

                        cmd.CommandText = "INSERT INTO navaids ( Ident, Name, Type, Longitude, Latitude, Altitude, MagVar, Range, Freq ) VALUES ( '" +
                            strIdent + "', '" +
                            strName.Replace("'", "''") + "', " +
                            nType.ToString() + ", " +
                            fLon.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
                            fLat.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
                            fAlt.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
                            fMagVar.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
                            fRange.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
                            fFreq.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + ");";
                        cmd.ExecuteNonQuery();
                    }
                    nodes = xmld.GetElementsByTagName("Ndb");
                    foreach (XmlNode xmln in nodes)
                    {
                        foreach (XmlAttribute xmla in xmln.Attributes)
                        {
                            if (xmla.Name == "lat")
                                fLat = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "lon")
                                fLon = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "alt")
                            {
                                fAlt  = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
                            }
                            else if (xmla.Name == "range")
                                fRange = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "frequency")
                                fFreq = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "magvar")
                                fMagVar = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "ident")
                            {
                                strIdent = xmla.Value;
                            }
                            else if (xmla.Name == "name")
                                strName = xmla.Value;
                        }
                        nNDBs++;

                        cmd.CommandText = "INSERT INTO navaids ( Ident, Name, Type, Longitude, Latitude, Altitude, MagVar, Range, Freq ) VALUES ( '" +
                            strIdent + "', '" +
                            strName.Replace( "'", "''" ) + "', 3," +
                            fLon.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
                            fLat.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
                            fAlt.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
                            fMagVar.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
                            fRange.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
                            fFreq.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + ");";
                        cmd.ExecuteNonQuery();
                    }

                    xmld = null;
                }
                catch(Exception e)
                {
                }
            }

            dbCon.Close();
//            frmMain.kmlFactory.CreateNavAidsKML(strFileName, ref lstVOR, ref lstNDB);
/*           
            strFiles = Directory.GetFiles(strPath, "APX*.bgl", SearchOption.AllDirectories);
            foreach (String strBGLFile in strFiles)
            {
                System.Diagnostics.Trace.WriteLine(strBGLFile);
                try
                {
                    System.Diagnostics.ProcessStartInfo ps = new System.Diagnostics.ProcessStartInfo(strBGL2XMLPath, "\"" + strBGLFile + "\" \"" + strTmpFile + "\"");
                    ps.CreateNoWindow = true;
                    ps.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(ps);
                    int nSecs = 0;
                    while (!p.HasExited && nSecs < 10)
                    {
                        nSecs++;
                        Thread.Sleep(1000);
                    }
                    if (!p.HasExited)
                    {
                        System.Diagnostics.Trace.WriteLine("Killed");
                        p.Kill();
                        continue;
                    }
                }
                catch
                {
                }

                try
                {
                    XmlDocument xmld = new XmlDocument();
                    xmld.Load(strTmpFile);
                    XmlNodeList nodes = xmld.GetElementsByTagName("Airport");
                }
                catch
                {
                }
            }
 */
            File.Delete(strTmpFile);
        }
        static public String GetMorseCode(String str)
        {
            str = str.ToUpper();
            String strMorseCode = "";
            for (int i = 0; i < str.Length; i++)
            {
                if( str[i] >= 'A' && str[i] <= 'Z' )
                    strMorseCode += strMorseSigns[str[i] - 'A' + 10];
                else if( str[i] >= '0' && str[i] <= '9' )
                    strMorseCode += strMorseSigns[str[i] - '0'];
                else 
                    strMorseCode += "? ";
            }
            return strMorseCode;
        }

        static public String GetRegionName(String strICAORegionCode)
        {
            String strRegion = "Unbekannt";
            if (strICAORegionCode != null && strICAORegionCode.Length >= 1 )
            {
                switch (strICAORegionCode[0])
                {
                    case 'A':
                        strRegion = "Südwest-Pazifik";
                        break;
                    case 'B':
                        strRegion = "Polarregion";
                        break;
                    case 'C':
                        strRegion = "Kanada";
                        break;
                    case 'D':
                        strRegion = "Westafrika";
                        break;
                    case 'E':
                        strRegion = "Nordeuropa";
                        break;
                    case 'F':
                        strRegion = "Südafrika";
                        break;
                    case 'G':
                        strRegion = "Westafrikanische Küste";
                        break;
                    case 'H':
                        strRegion = "Ostafrika";
                        break;
                    case 'K':
                        strRegion = "USA";
                        break;
                    case 'L':
                        strRegion = "Südeuropa";
                        break;
                    case 'M':
                        strRegion = "Zentralamerika";
                        break;
                    case 'N':
                        strRegion = "Südpazifik";
                        break;
                    case 'O':
                        strRegion = "Naher Osten";
                        break;
                    case 'P':
                        strRegion = "Nördlicher Pazifik";
                        break;
                    case 'R':
                        strRegion = "Ostasien";
                        break;
                    case 'S':
                        strRegion = "Südamerika";
                        break;
                    case 'T':
                        strRegion = "Karibik";
                        break;
                    case 'U':
                        strRegion = "Russische Föderation";
                        break;
                    case 'V':
                        strRegion = "Südasien";
                        break;
                    case 'W':
                        strRegion = "Südostasien";
                        break;
                    case 'Y':
                        strRegion = "Australien";
                        break;
                    case 'Z':
                        strRegion = "China";
                        break;
                }
            }
            return strRegion;
        }
        static public float ConvertDegToFloat(String szDeg)
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


            float f1 = float.Parse(szParts[0], System.Globalization.NumberFormatInfo.InvariantInfo);
            int iSign = Math.Sign(f1);
            f1 = Math.Abs(f1);
            float f2 = float.Parse(szParts[1], System.Globalization.NumberFormatInfo.InvariantInfo);
            float f3 = float.Parse(szParts[2], System.Globalization.NumberFormatInfo.InvariantInfo);

            return (float)(iSign * (f1 + (f2 * 60.0 + f3) / 3600.0));
        }
        static uint UIntToBCD(uint nData)
        {
            String str = nData.ToString();
            nData = 0;
            for (int i = 0; i < str.Length; i++)
            {
                nData *= 16;
                nData += (uint)(str[i] - '0');
            }
            return nData;
        }
    }
}
