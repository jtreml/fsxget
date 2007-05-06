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
            private ObjectData<double> dLon;
            private ObjectData<double> dLat;
            private ObjectData<double> dAlt;
            private double dTime;

            public ObjectPosition()
            {
                dLon = new ObjectData<double>();
                dLat = new ObjectData<double>();
                dAlt = new ObjectData<double>();
            }

            public ObjectData<double> Longitude
            {
                get
                {
                    return dLon;
                }
            }
            public ObjectData<double> Latitude
            {
                get
                {
                    return dLat;
                }
            }
            public ObjectData<double> Altitude
            {
                get
                {
                    return dAlt;
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
                    return XmlConvert.ToString(dLon.Value) + "," + XmlConvert.ToString(dLat.Value) + "," + XmlConvert.ToString(dAlt.Value);
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

            protected STATE tState;
            protected DATA_REQUESTS tType;
            protected uint unID;
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
                protected String strCoordinates;
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
                    strCoordinates += XmlConvert.ToString(obj.dLongitude) + "," + XmlConvert.ToString(obj.dLatitude) + "," + XmlConvert.ToString(obj.dAltitude) + " ";
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
                protected bool bPredictionPoints;
                public ObjectPosition[] positions;
                protected double dTimeElapsed;
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
                        positions[0].Longitude.Value = obj.dLongitude;
                        positions[0].Latitude.Value = obj.dLatitude;
                        positions[0].Altitude.Value = obj.dAltitude;
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

                    tResultPos.Latitude.Value = objNew.dLatitude + dScale * (objNew.dLatitude - positions[0].Latitude.Value);
                    tResultPos.Longitude.Value = objNew.dLongitude + dScale * (objNew.dLongitude - positions[0].Longitude.Value);
                    tResultPos.Altitude.Value = objNew.dAltitude + dScale * (objNew.dAltitude - positions[0].Altitude.Value);
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
            protected ObjectData<String> strTitle;
            protected ObjectData<String> strATCType;
            protected ObjectData<String> strATCModel;
            protected ObjectData<String> strATCID;
            protected ObjectData<String> strATCAirline;
            protected ObjectData<String> strATCFlightNumber;
            protected ObjectPosition objPos;
            protected ObjectData<double> dHeading;
            protected double dTime;
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
                dHeading = new ObjectData<double>();
                strTitle.Value = obj.szTitle;
                strATCType.Value = obj.szATCType;
                strATCModel.Value = obj.szATCModel;
                strATCID.Value = obj.szATCID;
                strATCAirline.Value = obj.szATCAirline;
                strATCFlightNumber.Value = obj.szATCFlightNumber;
                objPos.Longitude.Value = obj.dLongitude;
                objPos.Latitude.Value = obj.dLatitude;
                objPos.Altitude.Value = obj.dAltitude;
                dHeading.Value = obj.dHeading;
                dTime = obj.dTime;
                ConfigChanged();
            }

            public void Update(ref StructBasicMovingSceneryObject obj)
            {
                if (tState == STATE.DELETED)
                    return;
                if (obj.dTime != dTime && pathPrediction != null)
                {
                    pathPrediction.Update( ref obj );
                }
                if (objPath != null && (obj.dLongitude != objPos.Longitude.Value || obj.dLatitude != objPos.Latitude.Value || obj.dAltitude != objPos.Altitude.Value))
                {
                    objPath.AddPosition(ref obj);
                }

                objPos.Longitude.Value = obj.dLongitude;
                objPos.Latitude.Value = obj.dLatitude;
                objPos.Altitude.Value = obj.dAltitude;
                strTitle.Value = obj.szTitle;
                strATCType.Value = obj.szATCType;
                strATCModel.Value = obj.szATCModel;
                strATCID.Value = obj.szATCID;
                strATCAirline.Value = obj.szATCAirline;
                strATCFlightNumber.Value = obj.szATCFlightNumber;
                dHeading.Value = obj.dHeading;
                dTime = obj.dTime;
                if (tState == STATE.DATAREAD || tState == STATE.UNCHANGED )
                {
                    if (HasMoved || HasChanged)
                        tState = STATE.MODIFIED;
                    else
                        tState = STATE.UNCHANGED;
                }
                bDataRecieved = true;
            }

            public void ConfigChanged()
            {
                bool bPath = false;
                bool bPrediction = false;
                bool bPredictionPoints = false;
                switch (tType)
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

            public ObjectData<double> Heading
            {
                get
                {
                    return dHeading;
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
                    return objPos.HasMoved || dHeading.IsModified;
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
                protected String strName;
                protected double dLon;
                protected double dLat;
                KmlFactory.KML_ICON_TYPES tIconType;
                #endregion

                public Waypoint(String strName, double dLon, double dLat, KmlFactory.KML_ICON_TYPES tIconType)
                {
                    this.strName = strName;
                    this.dLon = dLon;
                    this.dLat = dLat;
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
                public double Longitude
                {
                    get
                    {
                        return dLon;
                    }
                }
                public double Latitude
                {
                    get
                    {
                        return dLat;
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

            protected List<Waypoint> lstWaypoints;
            protected String strName;

            public FlightPlan(uint unID, DATA_REQUESTS tType)
                : base( unID, tType )
            {
                lstWaypoints = new List<Waypoint>();
            }

            public void AddWaypoint(String strName, double dLon, double dLat, KmlFactory.KML_ICON_TYPES tIconType)
            {
                lstWaypoints.Add( new Waypoint( String.Format( "Waypoint {0}: {1} ", lstWaypoints.Count+1, strName ), dLon, dLat, tIconType ));
            }
            public void AddWaypoint(XmlNode xmln)
            {
                String str;
                KmlFactory.KML_ICON_TYPES tIconType = KmlFactory.KML_ICON_TYPES.NONE;
                String strName = "";
                double dLon = 0;
                double dLat = 0;

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
                        dLat = FsxConnection.ConvertDegToDouble(strCoords[0]);
                        dLon = FsxConnection.ConvertDegToDouble(strCoords[1]);
                    }
                }
                if (xmln["ICAO"]["ICAOIdent"] != null)
                    strName += xmln["ICAO"]["ICAOIdent"].InnerText;
                else if (xmln.Attributes["id"] != null)
                    strName += xmln.Attributes["id"].Value;

                AddWaypoint(strName, dLon, dLat, tIconType);
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
        public Hashtable htFlightPlans;
        private uint unFlightPlanNr;
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
        enum EVENT_ID
        {
            EVENT_MENU,
            EVENT_MENU_START,
            EVENT_MENU_STOP,
            EVENT_MENU_OPTIONS,
            EVENT_MENU_CLEAR_USER_PATH
        };
        
        enum DEFINITIONS
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
            
            timerQueryUserAircraft = new System.Timers.Timer();
            timerQueryUserAircraft.Elapsed += new ElapsedEventHandler(OnTimerQueryUserAircraftElapsed);

            lockUserAircraft = new Object();
            lockSimConnect = new Object();

            htFlightPlans = new Hashtable();
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
        void simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
 
        }
        void simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            DeleteAllObjects();
            closeConnection();
            frmMain.Connected = false;
            if (timerConnect != null)
                timerConnect.Start();
            else
                frmMain.Close();
        }
        void simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            frmMain.NotifyError( "FSX Exception!" );
        }
        void simconnect_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            switch ((EVENT_ID)data.uEventID)
            {
                case EVENT_ID.EVENT_MENU_OPTIONS:
                    frmMain.Show();
                    break;
            }
        }
        void simconnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
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

        protected void HandleSimObjectRecieved(ref StructObjectContainer objs, ref SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
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

        protected void MarkDeletedObjects(ref Hashtable ht)
        {
            foreach (DictionaryEntry entry in ht)
            {
                if (!((SceneryMovingObject)entry.Value).bDataRecieved)
                {
                    ((SceneryMovingObject)entry.Value).State = SceneryMovingObject.STATE.DELETED;
                }
                ((SceneryMovingObject)entry.Value).bDataRecieved = false;
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
                catch (COMException ex)
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
                catch (COMException ex)
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
                catch (COMException ex)
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
                catch (COMException ex)
                {
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

        public struct StructNavAid
        {
            public String strName;
            public String strIdent;
            public String strRegion;
            public KmlFactory.KML_ICON_TYPES tIconType;
            public double dLon;
            public double dLat;
            public String strFreq;
            public double dMagVar;
            public double dAlt;
            public double dRange;
        }

        public void GetSceneryObjects(String strFileName)
        {
            String strPath = Path.GetDirectoryName(Program.Config.FSXPath);
            String strBGL2XMLPath = @"C:\Programme\Microsoft Games\Microsoft Flight Simulator X SDK\Tools\BGL2XML_CMD\Bgl2Xml.exe";
            strPath += "\\Scenery";
            String strTmpFile = Path.GetTempFileName();
            String[] strFiles = Directory.GetFiles(strPath, "NVX*.bgl", SearchOption.AllDirectories);

            int nVORs = 0;
            int nNDBs = 0;
            
            List<StructNavAid> lstVOR = new List<StructNavAid>();
            List<StructNavAid> lstNDB = new List<StructNavAid>();

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
                    StructNavAid navaid;
                    XmlNodeList nodes = xmld.GetElementsByTagName("Vor");

                    foreach (XmlNode xmln in nodes)
                    {
                        bool bDme = false;
                        bool bDmeOnly = false;
                        navaid = new StructNavAid();
                        foreach (XmlAttribute xmla in xmln.Attributes)
                        {
                            if (xmla.Name == "dme")
                                bDme = xmla.Value.ToLower() == "true";
                            else if (xmla.Name == "dmeOnly")
                                bDmeOnly = xmla.Value.ToLower() == "true";
                            else if (xmla.Name == "lat")
                                navaid.dLat = double.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "lon")
                                navaid.dLon = double.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "alt")
                            {
                                navaid.dAlt = double.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
                            }
                            else if (xmla.Name == "range")
                                navaid.dRange = double.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "frequency")
                                navaid.strFreq = xmla.Value;
                            else if (xmla.Name == "magvar")
                            {
                                navaid.dMagVar = double.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            }
                            else if (xmla.Name == "ident")
                            {
                                navaid.strIdent = xmla.Value;
                            }
                            else if (xmla.Name == "name")
                                navaid.strName = xmla.Value;
                            else if (xmla.Name == "region")
                                navaid.strRegion = xmla.Value;
                        }

                        if (bDmeOnly)
                        {
                            navaid.tIconType = KmlFactory.KML_ICON_TYPES.DME;
                        }
                        else
                        {
                            if (bDme)
                            {
                                navaid.tIconType = KmlFactory.KML_ICON_TYPES.VORDME;
                            }
                            else
                                navaid.tIconType = KmlFactory.KML_ICON_TYPES.VOR;
                        }
                        nVORs++;
                        lstVOR.Add(navaid);
                    }
                    nodes = xmld.GetElementsByTagName("Ndb");
                    foreach (XmlNode xmln in nodes)
                    {
                        navaid = new StructNavAid();
                        foreach (XmlAttribute xmla in xmln.Attributes)
                        {
                            if (xmla.Name == "lat")
                                navaid.dLat = double.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "lon")
                                navaid.dLon = double.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "alt")
                            {
                                navaid.dAlt = double.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
                            }
                            else if (xmla.Name == "range")
                                navaid.dRange = double.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "frequency")
                                navaid.strFreq = xmla.Value;
                            else if (xmla.Name == "magvar")
                                navaid.dMagVar = double.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                            else if (xmla.Name == "ident")
                            {
                                navaid.strIdent = xmla.Value;
                            }
                            else if (xmla.Name == "name")
                                navaid.strName = xmla.Value;
                        }
                        navaid.tIconType = KmlFactory.KML_ICON_TYPES.NDB;
                        nNDBs++;
                        lstNDB.Add(navaid);
                    }

                    xmld = null;
                }
                catch
                {
                }
            }

            frmMain.kmlFactory.CreateNavAidsKML(strFileName, ref lstVOR, ref lstNDB);
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
        static public double ConvertDegToDouble(String szDeg)
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

    }
}
