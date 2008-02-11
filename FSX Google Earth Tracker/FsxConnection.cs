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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Reflection;
using System.Windows.Interop;

namespace Fsxget
{
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
			private ObjectData<float> fAlt;
			private double dTime;

			public ObjectPosition()
			{
				fLon = new ObjectData<float>();
				fLat = new ObjectData<float>();
				fAlt = new ObjectData<float>();
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
					return fAlt;
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
					return XmlConvert.ToString(fLon.Value) + "," + XmlConvert.ToString(fLat.Value) + "," + XmlConvert.ToString(fAlt.Value) + " ";
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
			private bool bDataRecieved;

			public SceneryObject(uint unID, DATA_REQUESTS tType)
			{
				tState = STATE.NEW;
				bDataRecieved = true;
				this.tType = tType;
				this.unID = unID;
			}

			public virtual STATE State
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
			public virtual uint ObjectID
			{
				get
				{
					return unID;
				}
			}
			public virtual DATA_REQUESTS ObjectType
			{
				get
				{
					return tType;
				}
			}
			public virtual bool DataRecieved
			{
				get
				{
					return bDataRecieved;
				}
				set
				{
					bDataRecieved = value;
				}
			}
		}

		public class SceneryMovingObject : SceneryObject
		{
			#region Classes
			public class ObjectPath
			{
				private ObjectPosition lastPos;
				private String strCoordinates;
				STATE tState;

				public ObjectPath()
				{
					lastPos = new ObjectPosition();
					strCoordinates = "";
					tState = STATE.NEW;
				}

				public ObjectPath(ref StructBasicMovingSceneryObject obj)
				{
					lastPos = new ObjectPosition();
					tState = STATE.NEW;
					AddPosition(ref obj);
				}

				public void AddPosition(ref StructBasicMovingSceneryObject obj)
				{
					lastPos.Longitude.Value = (float)obj.dLongitude;
					lastPos.Latitude.Value = (float)obj.dLatitude;
					lastPos.Altitude.Value = (float)obj.dAltitude;
					if (lastPos.HasMoved)
					{
						strCoordinates += XmlConvert.ToString(lastPos.Longitude.Value) + "," + XmlConvert.ToString(lastPos.Latitude.Value) + "," + XmlConvert.ToString(lastPos.Altitude.Value) + " ";
						if (tState == STATE.DATAREAD)
							tState = STATE.MODIFIED;
					}
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

					tResultPos.Latitude.Value = (float)(objNew.dLatitude + dScale * (objNew.dLatitude - positions[0].Latitude.Value));
					tResultPos.Longitude.Value = (float)(objNew.dLongitude + dScale * (objNew.dLongitude - positions[0].Longitude.Value));
					tResultPos.Altitude.Value = (float)(objNew.dAltitude + dScale * (objNew.dAltitude - positions[0].Altitude.Value));
				}

				public bool HasPoints
				{
					get
					{
						return bPredictionPoints;
					}
					set
					{
						if (bPredictionPoints != value)
						{
							bPredictionPoints = value;
							SettingsList lstPoint = (SettingsList)App.Config[Config.SETTING.PREDICTION_POINTS];
							if (bPredictionPoints)
							{
								positions = new ObjectPosition[lstPoint.listSettings.Count + 1];
								for (int i = 0; i < positions.Length; i++)
								{
									positions[i] = new ObjectPosition();
									if (i > 0)
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
			private float fAltAGL;
			private float fGroundSpeed;
			private double dTime;
			public ObjectPath objPath;
			public PathPrediction pathPrediction;
			#endregion

			public SceneryMovingObject(uint unID, DATA_REQUESTS tType, ref StructBasicMovingSceneryObject obj)
				: base(unID, tType)
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
				fAltAGL = (float)obj.dAltAGL;
				dTime = obj.dTime;
				ConfigChanged();
			}

			public void Update(ref StructBasicMovingSceneryObject obj)
			{
				if (State == STATE.DELETED)
					return;
				if (obj.dTime != dTime && pathPrediction != null)
				{
					pathPrediction.Update(ref obj);
				}
				if (objPath != null)
				{
					objPath.AddPosition(ref obj);
				}

				objPos.Longitude.Value = (float)obj.dLongitude;
				objPos.Latitude.Value = (float)obj.dLatitude;
				objPos.Altitude.Value = (float)obj.dAltitude;
				fGroundSpeed = (float)(obj.dGroundSpeed * 3.600 / 1.852);
				fAltAGL = (float)obj.dAltAGL;
				strTitle.Value = obj.szTitle;
				strATCType.Value = obj.szATCType;
				strATCModel.Value = obj.szATCModel;
				strATCID.Value = obj.szATCID;
				strATCAirline.Value = obj.szATCAirline;
				strATCFlightNumber.Value = obj.szATCFlightNumber;
				fHeading.Value = (float)obj.dHeading;
				dTime = obj.dTime;
				if (State == STATE.DATAREAD || State == STATE.UNCHANGED)
				{
					if (HasMoved || HasChanged)
						State = STATE.MODIFIED;
					else
						State = STATE.UNCHANGED;
				}
				DataRecieved = true;
			}

			public void ConfigChanged()
			{
				bool bPath = false;
				bool bPrediction = false;
				bool bPredictionPoints = false;
				switch (ObjectType)
				{
					case DATA_REQUESTS.REQUEST_USER_AIRCRAFT:
						bPrediction = App.Config[Config.SETTING.USER_PATH_PREDICTION]["Enabled"].BoolValue;
						bPredictionPoints = true;
						bPath = App.Config[Config.SETTING.QUERY_USER_PATH]["Enabled"].BoolValue;
						break;
					case DATA_REQUESTS.REQUEST_AI_PLANE:
						bPrediction = App.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["Prediction"].BoolValue;
						bPredictionPoints = App.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["PredictionPoints"].BoolValue;
						break;
					case DATA_REQUESTS.REQUEST_AI_HELICOPTER:
						bPrediction = App.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["Prediction"].BoolValue;
						bPredictionPoints = App.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["PredictionPoints"].BoolValue;
						break;
					case DATA_REQUESTS.REQUEST_AI_BOAT:
						bPrediction = App.Config[Config.SETTING.QUERY_AI_BOATS]["Prediction"].BoolValue;
						bPredictionPoints = App.Config[Config.SETTING.QUERY_AI_BOATS]["PredictionPoints"].BoolValue;
						break;
					case DATA_REQUESTS.REQUEST_AI_GROUND:
						bPrediction = App.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["Prediction"].BoolValue;
						bPredictionPoints = App.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["PredictionPoints"].BoolValue;
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
				str = str.Replace("%IMAGE%", App.Config.Server + "/" + Title.Value);
				str = str.Replace("%LOOKATALT%", ((int)ObjectPosition.Altitude.Value).ToString());
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
					if (strTitle.IsModified ||
						strATCType.IsModified ||
						strATCModel.IsModified ||
						strATCID.IsModified ||
						strATCAirline.IsModified ||
						strATCFlightNumber.IsModified)
						return true;
					else
						return false;
				}
			}

			public float AltitudeAGL
			{
				get
				{
					return fAltAGL;
				}
			}

			public float GroundSpeed
			{
				get
				{
					return fGroundSpeed;
				}
			}

			#endregion
		}

		public class SceneryStaticObjectData
		{
			#region Variables
			private float fLongitude;
			private float fLatitude;
			private float fAltitude;
			public uint unID;
			#endregion

			public SceneryStaticObjectData()
			{
				unID = 0;
				fLongitude = 0;
				fLatitude = 0;
				fAltitude = 0;
			}

			#region Accessors

			public float Longitude
			{
				get
				{
					return fLongitude;
				}
				set
				{
					fLongitude = value;
				}
			}

			public float Latitude
			{
				get
				{
					return fLatitude;
				}
				set
				{
					fLatitude = value;
				}
			}

			public float Altitude
			{
				get
				{
					return fAltitude;
				}
				set
				{
					fAltitude = value;
				}
			}

			public uint ObjectID
			{
				get
				{
					return unID;
				}
				set
				{
					unID = value;
				}
			}

			#endregion
		}

		public class SceneryAirportObjectData : SceneryStaticObjectData
		{
			#region Classes

			public class ComFrequency
			{
				#region Variables
				private float fFreq;
				private COMTYPE tType;
				#endregion

				public enum COMTYPE
				{
					APPROACH = 1,
					ASOS,
					ATIS,
					AWOS,
					CENTER,
					CLEARANCE,
					CLEARANCE_PRE_TAXI,
					CTAF,
					DEPARTURE,
					FSS,
					GROUND,
					MULTICOM,
					REMOTE_CLEARANCE_DELIVERY,
					TOWER,
					UNICOM,
				}

				private ComFrequency()
				{
				}

				public static List<ComFrequency> CreateList(uint unAirportID, OleDbConnection dbCon)
				{
					List<ComFrequency> lst = new List<ComFrequency>();
					ComFrequency comFrequency = null;
					try
					{
						OleDbCommand cmd = new OleDbCommand("SELECT TypeID, Freq FROM AirportComs WHERE AirportID=" + unAirportID.ToString() + " ORDER BY TypeID", dbCon);
						OleDbDataReader rd = cmd.ExecuteReader();
						while (rd.Read())
						{
							comFrequency = new ComFrequency();
							comFrequency.fFreq = rd.GetFloat(1);
							comFrequency.tType = (COMTYPE)rd.GetInt32(0);
							lst.Add(comFrequency);
						}
						rd.Close();
					}
					catch
					{
					}
					return lst;
				}

				#region Accessors
				public float Frequency
				{
					get
					{
						return fFreq;
					}
				}
				public COMTYPE ComType
				{
					get
					{
						return tType;
					}
				}
				public String Name
				{
					get
					{
						return tType.ToString();
					}
				}
				#endregion
			}

			public class Runway : SceneryStaticObjectData
			{
				#region Classes
				public class ILS : SceneryStaticObjectData
				{
					#region Variables
					private float fFreq;
					private float fRange;
					private float fMagVar;
					private float fHeading;
					private float fWidth;
					private bool bBackCourse;
					private String strIdent;
					private String strName;
					#endregion

					private ILS()
					{
					}

					public static ILS Create(uint unID, bool bSecondary)
					{
						ILS ils = null;
						try
						{
							OleDbConnection dbCon = new OleDbConnection(App.Config.ConnectionString);
							dbCon.Open();
							OleDbCommand cmd = new OleDbCommand("SELECT Ident, Name, Heading, Freq, Longitude, Latitude, Altitude, Range, MagVar, Width, BackCourse FROM RunwayILS WHERE EndSecondary=" + bSecondary.ToString() + " AND RunwayID=" + unID.ToString(), dbCon);
							OleDbDataReader rd = cmd.ExecuteReader();
							if (rd.Read())
							{
								ils = new ILS();
								ils.strIdent = rd.GetString(0);
								ils.strName = rd.GetString(1);
								ils.fHeading = rd.GetFloat(2);
								ils.fFreq = rd.GetFloat(3);
								ils.Longitude = rd.GetFloat(4);
								ils.Latitude = rd.GetFloat(5);
								ils.Altitude = rd.GetFloat(6);
								ils.fRange = rd.GetFloat(7);
								ils.fMagVar = rd.GetFloat(8);
								ils.fWidth = rd.GetFloat(9);
								ils.bBackCourse = rd.GetBoolean(10);
							}
							rd.Close();
							dbCon.Close();
						}
						catch
						{
						}
						return ils;
					}

					#region Accessors
					public float Heading
					{
						get
						{
							return fHeading;
						}
					}
					public float Width
					{
						get
						{
							return fWidth;
						}
					}
					public float Range
					{
						get
						{
							return fRange;
						}
					}
					public String Name
					{
						get
						{
							return strName;
						}
					}
					public String Ident
					{
						get
						{
							return strIdent;
						}
					}
					public bool BackCourse
					{
						get
						{
							return bBackCourse;
						}
					}
					public float MagVar
					{
						get
						{
							return fMagVar;
						}
					}
					public float Frequency
					{
						get
						{
							return fFreq;
						}
					}

					#endregion
				}
				#endregion

				#region Variables
				private float fHeading;
				private float fLength;
				private float fWidth;
				private String strName;
				private SURFACETYPE tSurfaceType;
				private float fPatternAlt;
				private bool bPatternRight;
				private bool bHasLights;
				private ILS ils;
				private static String[] strRunwayDirections = new String[] 
                {
                    "N",
                    "NE",
                    "SE",
                    "S",
                    "SW",
                    "W",
                    "NW"
                };
				#endregion

				#region Enums
				public enum RUNWAYTYPE
				{
					HARDENED,
					FASTENED,
					WATER,
					UNKNOWN,
				}
				public enum PATTERTRAFFIC
				{
					LEFT,
					RIGHT,
				}
				public enum SURFACETYPE
				{
					ASPHALT = 1,
					BITUMINOUS,
					BRICK,
					CLAY,
					CEMENT,
					CONCRETE,
					CORAL,
					DIRT,
					GRASS,
					GRAVEL,
					ICE,
					MACADAM,
					OIL_TREATED,
					SAND,
					SHALE,
					SNOW,
					STEEL_MATS,
					TARMAC,
					UNKNWON,
					WATER,
					PLANKS,
				}
				#endregion

				private Runway()
				{
				}

				public static List<Runway> CreateList(uint unAirportID, OleDbConnection dbCon, ref float fLongestLength, ref float fLongestHeading, ref RUNWAYTYPE tType, ref bool bHasLights)
				{
					List<Runway> lst = new List<Runway>();
					tType = RUNWAYTYPE.UNKNOWN;
					fLongestLength = 0;
					fLongestHeading = 0;
					bHasLights = false;
					bool bHard = false;
					bool bLongestHard = false;
					try
					{
						Runway runway;
						OleDbCommand cmd = new OleDbCommand("SELECT [Number], PrimaryDesignator, SecondaryDesignator, Length, SurfaceID, Name, HasLights, Hardened, Heading, PrimaryTakeoff, PrimaryLanding, SecondaryTakeoff, SecondaryLanding, Runways.ID, PatternAltitude, PrimaryPatternRight, SecondaryPatternRight, Longitude, Latitude, Altitude, Width FROM Runways INNER JOIN SurfaceType ON Runways.SurfaceID = SurfaceType.ID WHERE AirportID=" + unAirportID.ToString(), dbCon);
						OleDbDataReader rd = cmd.ExecuteReader();
						while (rd.Read())
						{
							String strDes = "";
							float fLon = 0;
							float fLat = 0;
							float fHead = 0;
							int nNr = rd.GetInt16(0);
							if (rd.GetBoolean(9) || rd.GetBoolean(10) || (!rd.GetBoolean(9) && !rd.GetBoolean(10) && !rd.GetBoolean(11) && !rd.GetBoolean(12)))
							{
								runway = new Runway();
								if (nNr >= 1000)
									runway.strName = strRunwayDirections[(nNr - 1000) / 45];
								else
									runway.strName = String.Format("{0:00}", nNr == 0 ? 36 : nNr);
								strDes = rd.GetString(1);
								if (strDes != "N")
									runway.strName += strDes;
								runway.Altitude = rd.GetFloat(19);
								runway.fPatternAlt = runway.Altitude + rd.GetFloat(14);
								runway.bPatternRight = rd.GetBoolean(15);
								runway.fHeading = rd.GetFloat(8);
								runway.fLength = rd.GetFloat(3);
								runway.fWidth = rd.GetFloat(20);
								runway.bHasLights = rd.GetBoolean(6);
								runway.tSurfaceType = (SURFACETYPE)rd.GetInt32(4);
								fHead = runway.fHeading;
								fHead = fHead > 180 ? fHead - 180 : fHead + 180;
								KmlFactory.MovePoint(rd.GetFloat(17), rd.GetFloat(18), fHead, rd.GetFloat(3) / 2, ref fLon, ref fLat);
								runway.Longitude = fLon;
								runway.Latitude = fLat;
								runway.ils = ILS.Create((uint)rd.GetInt32(13), false);
								bHard = rd.GetBoolean(7);
								if (fLongestLength < runway.fLength || (bLongestHard == false && bHard))
								{
									fLongestLength = runway.fLength;
									bLongestHard = bHard;
									fLongestHeading = runway.fHeading;
									tType = runway.tSurfaceType == SURFACETYPE.WATER ? RUNWAYTYPE.WATER : (bHard ? RUNWAYTYPE.HARDENED : RUNWAYTYPE.FASTENED);
								}
								if (!bHasLights && runway.bHasLights)
									bHasLights = true;
								lst.Add(runway);
							}
							if (rd.GetBoolean(11) || rd.GetBoolean(12) || (!rd.GetBoolean(9) && !rd.GetBoolean(10) && !rd.GetBoolean(11) && !rd.GetBoolean(12)))
							{
								runway = new Runway();
								strDes = rd.GetString(2);
								if (nNr >= 1000)
								{
									nNr -= 1000;
									if (nNr >= 180)
										nNr -= 180;
									else
										nNr += 180;
									runway.strName = strRunwayDirections[nNr / 45];
								}
								else
								{
									if (nNr >= 18)
										nNr -= 18;
									else
										nNr += 18;
									runway.strName = String.Format("{0:00}", nNr == 0 ? 36 : nNr);
								}
								if (strDes != "N")
									runway.strName += strDes;

								runway.Altitude = rd.GetFloat(19);
								runway.fPatternAlt = runway.Altitude + rd.GetFloat(14);
								runway.bPatternRight = rd.GetBoolean(16);
								runway.fLength = rd.GetFloat(3);
								runway.fWidth = rd.GetFloat(20);
								runway.bHasLights = rd.GetBoolean(6);
								runway.tSurfaceType = (SURFACETYPE)rd.GetInt32(4);
								fHead = rd.GetFloat(8);
								runway.fHeading = fHead > 180 ? fHead - 180 : fHead + 180;
								KmlFactory.MovePoint(rd.GetFloat(17), rd.GetFloat(18), fHead, rd.GetFloat(3) / 2, ref fLon, ref fLat);
								runway.Longitude = fLon;
								runway.Latitude = fLat;
								runway.ils = ILS.Create((uint)rd.GetInt32(13), true);
								bHard = rd.GetBoolean(7);
								if (fLongestLength < runway.fLength || (bLongestHard == false && bHard))
								{
									fLongestLength = runway.fLength;
									bLongestHard = bHard;
									fLongestHeading = runway.fHeading;
									tType = runway.tSurfaceType == SURFACETYPE.WATER ? RUNWAYTYPE.WATER : (bHard ? RUNWAYTYPE.HARDENED : RUNWAYTYPE.FASTENED);
								}
								if (!bHasLights && runway.bHasLights)
									bHasLights = true;
								lst.Add(runway);
							}
						}
						rd.Close();
					}
					catch
					{
					}
					return lst;
				}

				#region Accessors

				public float Heading
				{
					get
					{
						return fHeading;
					}
				}

				public float Length
				{
					get
					{
						return fLength;
					}
				}

				public float Width
				{
					get
					{
						return fWidth;
					}
				}

				public String Name
				{
					get
					{
						return strName;
					}
				}

				public SURFACETYPE SurfaceType
				{
					get
					{
						return tSurfaceType;
					}
				}

				public String SurfaceName
				{
					get
					{
						return tSurfaceType.ToString();
					}
				}

				public float PatternAltitude
				{
					get
					{
						return fPatternAlt;
					}
				}

				public PATTERTRAFFIC PatternTraffic
				{
					get
					{
						return bPatternRight ? PATTERTRAFFIC.RIGHT : PATTERTRAFFIC.LEFT;
					}
				}

				public ILS ILSData
				{
					get
					{
						return ils;
					}
				}

				public bool HasLights
				{
					get
					{
						return bHasLights;
					}
				}

				#endregion
			}

			public class BoundaryFence
			{
				public struct Vertex
				{
					public float fLongitude;
					public float fLatitude;
				}
				private int nNr;
				private List<Vertex> lstVertexes;

				private BoundaryFence()
				{
					lstVertexes = new List<Vertex>();
				}

				public static List<BoundaryFence> CreateList(uint unAirportID, OleDbConnection dbCon)
				{
					List<BoundaryFence> lst = new List<BoundaryFence>();
					try
					{
						BoundaryFence boundaryFence = null;
						OleDbCommand cmd = new OleDbCommand("SELECT [Number], Longitude, Latitude FROM AirportBoundary INNER JOIN AirportBoundaryVertex ON AirportBoundary.ID=AirportBoundaryVertex.BoundaryID WHERE AirportID=" + unAirportID.ToString() + " ORDER BY [Number],SortNr", dbCon);
						OleDbDataReader rd = cmd.ExecuteReader();
						int nNumber = -1;
						while (rd.Read())
						{
							if (rd.GetInt32(0) != nNumber)
							{
								if (boundaryFence != null)
									lst.Add(boundaryFence);
								boundaryFence = new BoundaryFence();
								nNumber = rd.GetInt32(0);
								boundaryFence.nNr = nNumber;
							}
							Vertex v = new Vertex();
							v.fLongitude = rd.GetFloat(1);
							v.fLatitude = rd.GetFloat(2);
							boundaryFence.lstVertexes.Add(v);
						}
						if (boundaryFence != null)
							lst.Add(boundaryFence);
						rd.Close();
					}
					catch
					{
					}
					return lst;
				}

				#region Accessors
				public int Number
				{
					get
					{
						return nNr;
					}
				}
				public List<Vertex> Vertexes
				{
					get
					{
						return lstVertexes;
					}
				}
				#endregion
			}

			#endregion

			#region Variables
			private float fMagVar;
			private String strIdent;
			private String strName;
			private bool bComplexIcon;
			private String strIconParams;
			private List<ComFrequency> lstComFrequencies;
			private List<Runway> lstRunways;
			private List<BoundaryFence> lstBoundaryFences;
			#endregion

			public SceneryAirportObjectData(uint unID, OleDbConnection dbCon)
			{
				ObjectID = unID;
				// General Airport-Information
				OleDbCommand cmd = new OleDbCommand("SELECT Ident, Name, Longitude, Latitude, Altitude, MagVar FROM airports WHERE ID=" + unID.ToString() + " ORDER BY Ident", dbCon);
				OleDbDataReader rd = cmd.ExecuteReader();
				if (rd.Read())
				{
					Altitude = rd.GetFloat(4);
					strIdent = rd.GetString(0);
					strName = rd.GetString(1);
					Longitude = rd.GetFloat(2);
					Latitude = rd.GetFloat(3);
					fMagVar = rd.GetFloat(5);
				}
				rd.Close();

				// Com-Frequencies
				lstComFrequencies = ComFrequency.CreateList(unID, dbCon);

				// Runways
				float fLongestLength = 0;
				float fLongestHeading = 0;
				bool bHasLights = false;
				Runway.RUNWAYTYPE tType = Runway.RUNWAYTYPE.UNKNOWN;
				lstRunways = Runway.CreateList(unID, dbCon, ref fLongestLength, ref fLongestHeading, ref tType, ref bHasLights);

				// BoundaryFences
				lstBoundaryFences = BoundaryFence.CreateList(unID, dbCon);


				if (fLongestLength > 3000 && lstBoundaryFences != null && lstBoundaryFences.Count > 0)
				{
					bComplexIcon = true;
					strIconParams = "id=" + unID.ToString();
				}
				else
				{
					bComplexIcon = false;
					strIconParams = String.Format("head={0}&type={1}&lights={2}", fLongestHeading.ToString(System.Globalization.NumberFormatInfo.InvariantInfo), (int)tType, (bHasLights ? 1 : 0));
				}
			}

			#region Accessors
			public float MagVar
			{
				get
				{
					return fMagVar;
				}
			}
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
			public List<Runway> Runways
			{
				get
				{
					return lstRunways;
				}
			}
			public List<BoundaryFence> BoundaryFences
			{
				get
				{
					return lstBoundaryFences;
				}
			}
			public List<ComFrequency> ComFrequencies
			{
				get
				{
					return lstComFrequencies;
				}
			}
			public bool ComplexIcon
			{
				get
				{
					return bComplexIcon;
				}
			}
			public String IconParams
			{
				get
				{
					return strIconParams;
				}
			}
			#endregion
		}

		public class SceneryTaxiSignData : SceneryStaticObjectData
		{
			#region Classes
			public class TaxiSign
			{
				#region Variables
				private String strLabel;
				private float fHeading;
				private float fLonW;
				private float fLonE;
				private float fLatS;
				private float fLatN;
				private String strIconParams;
				#endregion

				private TaxiSign()
				{
				}

				public static List<TaxiSign> CreateList(uint unID, OleDbConnection dbCon)
				{
					List<TaxiSign> lst = new List<TaxiSign>();
					TaxiSign sign = null;
					try
					{
						OleDbCommand cmd = new OleDbCommand("SELECT Longitude, Latitude, Label, Heading, JustifyRight FROM TaxiwaySigns WHERE AirportID=" + unID.ToString(), dbCon);
						OleDbDataReader rd = cmd.ExecuteReader();
						while (rd.Read())
						{
							sign = new TaxiSign();
							sign.strLabel = rd.GetString(2);
							Bitmap bmp = FsxConnection.RenderTaxiwaySign(sign.strLabel);
							int nWidth = bmp.Width / 8;
							int nHeigth = bmp.Height / 8;
							float fLon = rd.GetFloat(0);
							float fLat = rd.GetFloat(1);
							float fTmp = 0;
							KmlFactory.MovePoint(fLon, fLat, 90, nWidth / 2, ref sign.fLonE, ref fTmp);
							KmlFactory.MovePoint(fLon, fLat, 180, nHeigth / 2, ref fTmp, ref sign.fLatS);
							KmlFactory.MovePoint(fLon, fLat, 270, nWidth / 2, ref sign.fLonW, ref fTmp);
							KmlFactory.MovePoint(fLon, fLat, 0, nHeigth / 2, ref fTmp, ref sign.fLatN);
							sign.fHeading = rd.GetFloat(3);
							if (rd.GetBoolean(4))
								sign.fHeading += 90;
							else
								sign.fHeading -= 90;
							if (sign.fHeading > 360)
								sign.fHeading -= 360;
							if (sign.fHeading > 180)
								sign.fHeading = (360 - sign.fHeading) * -1;
							sign.fHeading *= -1;
							byte[] bytes = System.Text.Encoding.Default.GetBytes(sign.strLabel);
							sign.strIconParams = "label=" + Uri.EscapeDataString(Convert.ToBase64String(bytes));
							lst.Add(sign);
						}
						rd.Close();
					}
					catch
					{
					}
					return lst;
				}

				#region Accessors
				public String Label
				{
					get
					{
						return strLabel;
					}
				}
				public float LongitudeEast
				{
					get
					{
						return fLonE;
					}
				}
				public float LongitudeWest
				{
					get
					{
						return fLonW;
					}
				}
				public float LatitudeSouth
				{
					get
					{
						return fLatS;
					}
				}
				public float LatitudeNorth
				{
					get
					{
						return fLatN;
					}
				}
				public float Heading
				{
					get
					{
						return fHeading;
					}
				}
				public String IconParams
				{
					get
					{
						return strIconParams;
					}
				}
				#endregion
			}
			public class TaxiParking
			{
				#region Variables
				private NAMES tName;
				private int nNr;
				private TYPES tType;
				private float fHeading;
				private float fLonW;
				private float fLonE;
				private float fLatS;
				private float fLatN;
				private float fRadius;
				private String strIconParams;
				#endregion

				#region Enums
				public enum NAMES
				{
					PARKING = 1,
					DOCK,
					GATE,
					GATE_A,
					GATE_B,
					GATE_C,
					GATE_D,
					GATE_E,
					GATE_F,
					GATE_G,
					GATE_H,
					GATE_I,
					GATE_J,
					GATE_K,
					GATE_L,
					GATE_M,
					GATE_N,
					GATE_O,
					GATE_P,
					GATE_Q,
					GATE_R,
					GATE_S,
					GATE_T,
					GATE_U,
					GATE_V,
					GATE_W,
					GATE_X,
					GATE_Y,
					GATE_Z,
					NONE,
					N_PARKING,
					NE_PARKING,
					NW_PARKING,
					SE_PARKING,
					S_PARKING,
					SW_PARKING,
					W_PARKING,
					E_PARKING,
				}
				public enum TYPES
				{
					NONE = 1,
					DOCK_GA,
					FUEL,
					GATE_HEAVY,
					GATE_MEDIUM,
					GATE_SMALL,
					RAMP_CARGO,
					RAMP_GA,
					RAMP_GA_LARGE,
					RAMP_GA_MEDIUM,
					RAMP_GA_SMALL,
					RAMP_MIL_CARGO,
					RAMP_MIL_COMBAT,
					VEHICLE,
				}
				#endregion

				private TaxiParking()
				{
				}

				public static List<TaxiParking> CreateList(uint unID, OleDbConnection dbCon)
				{
					List<TaxiParking> lst = new List<TaxiParking>();
					TaxiParking park = null;
					try
					{
						OleDbCommand cmd = new OleDbCommand("SELECT Longitude, Latitude, Heading, Radius, NameID, [Number], TypeID FROM TaxiwayParking WHERE TypeID <> 14 AND AirportID=" + unID.ToString(), dbCon);
						OleDbDataReader rd = cmd.ExecuteReader();
						while (rd.Read())
						{
							park = new TaxiParking();
							float fLon = rd.GetFloat(0);
							float fLat = rd.GetFloat(1);
							park.fHeading = rd.GetFloat(2);
							park.fRadius = rd.GetFloat(3);
							park.tName = (NAMES)rd.GetInt32(4);
							park.tType = (TYPES)rd.GetInt32(6);
							park.nNr = rd.GetInt16(5);
							float fTmp = 0;
							KmlFactory.MovePoint(fLon, fLat, 90, park.fRadius, ref park.fLonE, ref fTmp);
							KmlFactory.MovePoint(fLon, fLat, 180, park.fRadius, ref fTmp, ref park.fLatS);
							KmlFactory.MovePoint(fLon, fLat, 270, park.fRadius, ref park.fLonW, ref fTmp);
							KmlFactory.MovePoint(fLon, fLat, 0, park.fRadius, ref fTmp, ref park.fLatN);

							if (park.fHeading > 180)
								park.fHeading = (360 - park.fHeading) * -1;
							park.fHeading *= -1;
							park.strIconParams = String.Format("radius={0}&name={1}&nr={2}", XmlConvert.ToString(park.fRadius), Uri.EscapeDataString(park.Name), park.nNr);
							lst.Add(park);
						}
						rd.Close();
					}
					catch
					{
					}
					return lst;
				}

				#region Accessors
				public NAMES NameID
				{
					get
					{
						return tName;
					}
				}
				public String Name
				{
					get
					{
						return tName.ToString();
					}
				}
				public TYPES TypeID
				{
					get
					{
						return tType;
					}
				}
				public String Type
				{
					get
					{
						return tType.ToString();
					}
				}
				public int Number
				{
					get
					{
						return nNr;
					}
				}
				public float LongitudeEast
				{
					get
					{
						return fLonE;
					}
				}
				public float LongitudeWest
				{
					get
					{
						return fLonW;
					}
				}
				public float LatitudeSouth
				{
					get
					{
						return fLatS;
					}
				}
				public float LatitudeNorth
				{
					get
					{
						return fLatN;
					}
				}
				public float Heading
				{
					get
					{
						return fHeading;
					}
				}
				public float Radius
				{
					get
					{
						return fRadius;
					}
				}
				public String IconParams
				{
					get
					{
						return strIconParams;
					}
				}
				#endregion
			}
			#endregion

			#region Variables
			List<TaxiSign> lstTaxiSigns;
			List<TaxiParking> lstTaxiParkings;
			#endregion

			public SceneryTaxiSignData(uint unID, OleDbConnection dbCon)
			{
				ObjectID = unID;
				lstTaxiSigns = TaxiSign.CreateList(unID, dbCon);
				lstTaxiParkings = TaxiParking.CreateList(unID, dbCon);
			}

			public void Clear()
			{
				lstTaxiParkings.Clear();
				lstTaxiSigns.Clear();
			}

			#region Accessors
			public List<TaxiSign> TaxiSigns
			{
				get
				{
					return lstTaxiSigns;
				}
			}
			public List<TaxiParking> TaxiParkings
			{
				get
				{
					return lstTaxiParkings;
				}
			}
			#endregion
		}

		public class SceneryNavaidObjectData : SceneryStaticObjectData
		{
			#region Variables
			private TYPE tType;
			private String strIdent;
			private String strName;
			private float fMagVar;
			private float fFreq;
			private float fRange;
			#endregion

			#region Enums
			public enum TYPE
			{
				DME = 1,
				VOR,
				VORDME,
				NDB,
			}
			#endregion

			public SceneryNavaidObjectData(uint unID, OleDbConnection dbCon)
			{
				ObjectID = unID;
				OleDbCommand cmd = new OleDbCommand("SELECT Ident, Name, TypeID, Longitude, Latitude, Altitude, MagVar, Range, Freq FROM navaids WHERE ID=" + unID.ToString(), dbCon);
				OleDbDataReader rd = cmd.ExecuteReader();
				if (rd.Read())
				{
					strIdent = rd.GetString(0);
					strName = rd.GetString(1);
					tType = (TYPE)rd.GetInt32(2);
					Longitude = rd.GetFloat(3);
					Latitude = rd.GetFloat(4);
					Altitude = rd.GetFloat(5);
					fMagVar = rd.GetFloat(6);
					fRange = rd.GetFloat(7);
					fFreq = rd.GetFloat(8);
				}
				rd.Close();
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
			public float MagVar
			{
				get
				{
					return fMagVar;
				}
			}
			public float Frequency
			{
				get
				{
					return fFreq;
				}
			}
			public float Range
			{
				get
				{
					return fRange;
				}
			}
			public TYPE Type
			{
				get
				{
					return tType;
				}
			}
			public String TypeName
			{
				get
				{
					return tType.ToString();
				}
			}
			#endregion
		}

		public class SceneryStaticObject : SceneryObject
		{
			private SceneryStaticObjectData data;

			public SceneryStaticObject(uint unID, DATA_REQUESTS tType, SceneryStaticObjectData data)
				: base(unID, tType)
			{
				this.data = data;
			}

			public override STATE State
			{
				get
				{
					return base.State;
				}
				set
				{
					if (value == STATE.DATAREAD)
					{
						// If the data was sent to GE, its not longer nessesary to hold the data, 
						// because they are static, and for deletetion the ID is info enough
						data = null;
					}
					base.State = value;
				}
			}
			public virtual SceneryStaticObjectData Data
			{
				get
				{
					return data;
				}
			}
		}

		public class SceneryAirportObject : SceneryStaticObject
		{
			#region Variables
			private STATE tTaxiSignState;
			private SceneryTaxiSignData dataTaxiSigns;
			#endregion

			public SceneryAirportObject(uint unID, SceneryAirportObjectData data)
				: base(unID, DATA_REQUESTS.AIRPORTS, data)
			{
				dataTaxiSigns = null;
				tTaxiSignState = STATE.UNCHANGED;
			}

			#region Accessors
			public STATE TaxiSignsState
			{
				get
				{
					return tTaxiSignState;
				}
				set
				{
					tTaxiSignState = value;
					if (value == STATE.DATAREAD && dataTaxiSigns != null)
						dataTaxiSigns.Clear();
				}
			}
			public SceneryAirportObjectData AirportData
			{
				get
				{
					return (SceneryAirportObjectData)Data;
				}
			}
			public SceneryTaxiSignData TaxiSignData
			{
				get
				{
					return dataTaxiSigns;
				}
				set
				{
					if (dataTaxiSigns == null && value != null)
					{
						tTaxiSignState = STATE.NEW;
						dataTaxiSigns = value;
					}
					else if (dataTaxiSigns != null && value == null)
					{
						tTaxiSignState = STATE.DELETED;
						dataTaxiSigns = null;
					}
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
				KmlFileFsx.KML_ICON_TYPES tIconType;
				#endregion

				public Waypoint(String strName, float fLon, float fLat, KmlFileFsx.KML_ICON_TYPES tIconType)
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
				public KmlFileFsx.KML_ICON_TYPES IconType
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
				: base(unID, tType)
			{
				lstWaypoints = new List<Waypoint>();
			}

			public void AddWaypoint(String strName, float fLon, float fLat, KmlFileFsx.KML_ICON_TYPES tIconType)
			{
				lstWaypoints.Add(new Waypoint(String.Format("Waypoint {0}: {1} ", lstWaypoints.Count + 1, strName), fLon, fLat, tIconType));
			}
			public void AddWaypoint(XmlNode xmln)
			{
				String str;
				KmlFileFsx.KML_ICON_TYPES tIconType = KmlFileFsx.KML_ICON_TYPES.NONE;
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
							tIconType = KmlFileFsx.KML_ICON_TYPES.PLAN_INTER;
						}
						else if (str == "vor")
						{
							strName += "VOR ";
							tIconType = KmlFileFsx.KML_ICON_TYPES.VOR;
						}
						else if (str == "airport")
						{
							strName += "Airport ";
							tIconType = KmlFileFsx.KML_ICON_TYPES.AIRPORT;
						}
						else if (str == "ndb")
						{
							strName += "NDB ";
							tIconType = KmlFileFsx.KML_ICON_TYPES.NDB;
						}
						else if (str == "user")
						{
							strName += "User ";
							tIconType = KmlFileFsx.KML_ICON_TYPES.PLAN_USER;
						}
						else
						{
							strName += xmln.InnerText + " ";
							tIconType = KmlFileFsx.KML_ICON_TYPES.NONE;
						}
					}
					else if (node.Name == "WorldPosition")
					{
						String[] strCoords = node.InnerText.Split(',');
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
		#endregion

		#region Variables
		private WindowMain frmMain;
		private IntPtr frmMainHandle;
		private String frmMainTitle;
		private const int WM_USER_SIMCONNECT = 0x0402;
		private System.Timers.Timer timerConnect;
		private uint uiUserAircraftID;
		private SimConnect simconnect;
		public Object lockSimConnect;
		public Object lockUserAircraft;
		private System.Timers.Timer timerUserAircraft;
		public SceneryMovingObject objUserAircraft;
		public StructObjectContainer[] objects;
		public Hashtable htFlightPlans;
		private uint unFlightPlanNr;
		#region Natoalphabet
		static public String[] strNatoABC = new String[] 
        {
            "Alpha",
            "Bravo",
            "Charlie",
            "Delta",
            "Echo",
            "Foxtrott",
            "Golf",
            "Hotel",
            "India",
            "Juliett",
            "Kilo",
            "Lima",
            "Mike",
            "November",
            "Oscar",
            "Papa",
            "Quebec",
            "Romeo",
            "Sierra",
            "Tango",
            "Uniform",
            "Victor",
            "Wiskey",
            "X-Ray",
            "Yankee",
            "Zulu",
        };
		#endregion
		#region Morsecodes (0-9 A-Z)
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
			EVENT_SET_COM,
		};
		public enum GROUP_ID
		{
			GROUP_USER,
		}
		public enum DEFINITIONS
		{
			StructBasicMovingSceneryObject,
			StructInitPos,
		};

		public enum DATA_REQUESTS
		{
			REQUEST_USER_AIRCRAFT = 0,
			REQUEST_AI_HELICOPTER,
			REQUEST_AI_PLANE,
			REQUEST_AI_BOAT,
			REQUEST_AI_GROUND,
			FLIGHTPLAN,
			NAVAIDS,
			AIRPORTS
		};

		public enum OBJCONTAINER
		{
			AI_PLANE = 0,
			AI_HELICOPTER,
			AI_BOAT,
			AI_GROUND,
			NAVAIDS,
			AIRPORTS,
		}

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
			public double dAltAGL;
			public double dGroundSpeed;
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
		public FsxConnection(WindowMain frmMain, bool bAddOn)
		{
			this.frmMain = frmMain;
			this.frmMainHandle = new WindowInteropHelper(frmMain).Handle;
			this.frmMainTitle = frmMain.Title;
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

			objects = new StructObjectContainer[Enum.GetNames(typeof(OBJCONTAINER)).Length];

			lockUserAircraft = new Object();
			objUserAircraft = null;
			timerUserAircraft = new System.Timers.Timer();
			timerUserAircraft.Elapsed += new ElapsedEventHandler(OnTimerQueryUserAircraftElapsed);

			objects[(int)OBJCONTAINER.AI_PLANE] = new StructObjectContainer();
			objects[(int)OBJCONTAINER.AI_PLANE].lockObject = new Object();
			objects[(int)OBJCONTAINER.AI_PLANE].htObjects = new Hashtable();
			objects[(int)OBJCONTAINER.AI_PLANE].timer = new System.Timers.Timer();
			objects[(int)OBJCONTAINER.AI_PLANE].timer.Elapsed += new ElapsedEventHandler(OnTimerQueryAIAircraftsElapsed);

			objects[(int)OBJCONTAINER.AI_HELICOPTER] = new StructObjectContainer();
			objects[(int)OBJCONTAINER.AI_HELICOPTER].lockObject = new Object();
			objects[(int)OBJCONTAINER.AI_HELICOPTER].htObjects = new Hashtable();
			objects[(int)OBJCONTAINER.AI_HELICOPTER].timer = new System.Timers.Timer();
			objects[(int)OBJCONTAINER.AI_HELICOPTER].timer.Elapsed += new ElapsedEventHandler(OnTimerQueryAIHelicoptersElapsed);

			objects[(int)OBJCONTAINER.AI_BOAT] = new StructObjectContainer();
			objects[(int)OBJCONTAINER.AI_BOAT].lockObject = new Object();
			objects[(int)OBJCONTAINER.AI_BOAT].htObjects = new Hashtable();
			objects[(int)OBJCONTAINER.AI_BOAT].timer = new System.Timers.Timer();
			objects[(int)OBJCONTAINER.AI_BOAT].timer.Elapsed += new ElapsedEventHandler(OntimerQueryAIBoatsElapsed);

			objects[(int)OBJCONTAINER.AI_GROUND] = new StructObjectContainer();
			objects[(int)OBJCONTAINER.AI_GROUND].lockObject = new Object();
			objects[(int)OBJCONTAINER.AI_GROUND].htObjects = new Hashtable();
			objects[(int)OBJCONTAINER.AI_GROUND].timer = new System.Timers.Timer();
			objects[(int)OBJCONTAINER.AI_GROUND].timer.Elapsed += new ElapsedEventHandler(OnTimerQueryAIGroundUnitsElapsed);

			objects[(int)OBJCONTAINER.NAVAIDS] = new StructObjectContainer();
			objects[(int)OBJCONTAINER.NAVAIDS].lockObject = new Object();
			objects[(int)OBJCONTAINER.NAVAIDS].htObjects = new Hashtable();
			objects[(int)OBJCONTAINER.NAVAIDS].timer = new System.Timers.Timer();
			objects[(int)OBJCONTAINER.NAVAIDS].timer.Elapsed += new ElapsedEventHandler(OnTimerQueryNavAidsElapsed);

			objects[(int)OBJCONTAINER.AIRPORTS] = new StructObjectContainer();
			objects[(int)OBJCONTAINER.AIRPORTS].lockObject = new Object();
			objects[(int)OBJCONTAINER.AIRPORTS].htObjects = new Hashtable();
			objects[(int)OBJCONTAINER.AIRPORTS].timer = new System.Timers.Timer();
			objects[(int)OBJCONTAINER.AIRPORTS].timer.Elapsed += new ElapsedEventHandler(OnTimerQueryAirportsElapsed);

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
			lock (lockSimConnect)
			{
				if (simconnect == null)
				{
					try
					{
						simconnect = new SimConnect(frmMainTitle, frmMainHandle, WM_USER_SIMCONNECT, null, 0);
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
				simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "PLANE ALT ABOVE GROUND", "meters", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
				simconnect.AddToDataDefinition(DEFINITIONS.StructBasicMovingSceneryObject, "GPS GROUND SPEED", "meters", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);

				simconnect.AddToDataDefinition(DEFINITIONS.StructInitPos, "Initial Position", null, SIMCONNECT_DATATYPE.INITPOSITION, 0.0f, SimConnect.SIMCONNECT_UNUSED);

				simconnect.MapClientEventToSimEvent(EVENT_ID.EVENT_SET_NAV1, "NAV1_RADIO_SET");
				simconnect.MapClientEventToSimEvent(EVENT_ID.EVENT_SET_NAV2, "NAV2_RADIO_SET");
				simconnect.MapClientEventToSimEvent(EVENT_ID.EVENT_SET_ADF, "ADF_SET");
				simconnect.MapClientEventToSimEvent(EVENT_ID.EVENT_SET_COM, "COM_RADIO_SET");
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
					simconnect.MenuAddItem(frmMain.Title, EVENT_ID.EVENT_MENU, 0);
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
			frmMain.NotifyError("FSX Exception!");
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
					lock (objects[(int)OBJCONTAINER.AI_PLANE].lockObject)
					{
						if (data.dwObjectID != uiUserAircraftID)
						{
							HandleSimObjectRecieved(ref objects[(int)OBJCONTAINER.AI_PLANE], ref data);
						}
					}
					break;
				case DATA_REQUESTS.REQUEST_AI_HELICOPTER:
					lock (objects[(int)OBJCONTAINER.AI_HELICOPTER].lockObject)
					{
						if (data.dwObjectID != uiUserAircraftID)
						{
							HandleSimObjectRecieved(ref objects[(int)OBJCONTAINER.AI_HELICOPTER], ref data);
						}
					}
					break;
				case DATA_REQUESTS.REQUEST_AI_BOAT:
					lock (objects[(int)OBJCONTAINER.AI_BOAT].lockObject)
					{
						HandleSimObjectRecieved(ref objects[(int)OBJCONTAINER.AI_BOAT], ref data);
					}
					break;
				case DATA_REQUESTS.REQUEST_AI_GROUND:
					lock (objects[(int)OBJCONTAINER.AI_GROUND].lockObject)
					{
						HandleSimObjectRecieved(ref objects[(int)OBJCONTAINER.AI_GROUND], ref data);
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
				if (data.dwentrynumber == 1)
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
				if (!((SceneryObject)entry.Value).DataRecieved)
				{
					((SceneryObject)entry.Value).State = SceneryMovingObject.STATE.DELETED;
				}
				((SceneryObject)entry.Value).DataRecieved = false;
			}
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
		public void DeleteAllObjects()
		{
			foreach (OBJCONTAINER request in Enum.GetValues(typeof(OBJCONTAINER)))
			{
				lock (objects[(int)request].lockObject)
				{
					foreach (DictionaryEntry entry in objects[(int)request].htObjects)
					{
						((SceneryObject)(entry.Value)).State = SceneryObject.STATE.DELETED;
					}
				}
			}
		}
		public void CleanupHashtable(ref Hashtable ht)
		{
			ArrayList toDel = new ArrayList();
			foreach (DictionaryEntry entry in ht)
			{
				if (((SceneryObject)entry.Value).State == SceneryObject.STATE.DELETED)
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
			lock (lockSimConnect)
			{
				try
				{
					strType = strType.ToLower();
					if (strType == "nav1")
						simconnect.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_SET_NAV1, UIntToBCD((uint)(dFreq * 100)), GROUP_ID.GROUP_USER, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
					else if (strType == "nav2")
						simconnect.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_SET_NAV2, UIntToBCD((uint)(dFreq * 100)), GROUP_ID.GROUP_USER, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
					else if (strType == "adf")
						simconnect.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_SET_ADF, UIntToBCD((uint)(dFreq)), GROUP_ID.GROUP_USER, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
					else if (strType == "com")
						simconnect.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_SET_COM, UIntToBCD((uint)(dFreq * 100)), GROUP_ID.GROUP_USER, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
					else
						bRet = false;
				}
				catch
				{
					bRet = false;
				}
			}
			return bRet;
		}
		public bool Goto(float fLon, float fLat, float fAlt, float fHead)
		{
			bool bRet = false;
			lock (lockSimConnect)
			{
				if (simconnect != null)
				{
					SIMCONNECT_DATA_INITPOSITION initpos = new SIMCONNECT_DATA_INITPOSITION();
					initpos.Airspeed = 0;
					initpos.Altitude = fAlt;
					initpos.Bank = 0;
					initpos.Heading = fHead;
					initpos.Latitude = fLat;
					initpos.Longitude = fLon;
					initpos.OnGround = 1;
					initpos.Pitch = 0;
					try
					{
						simconnect.SetDataOnSimObject(DEFINITIONS.StructInitPos, SimConnect.SIMCONNECT_OBJECT_ID_USER, 0, initpos);
						bRet = true;
					}
					catch
					{
					}
				}
			}
			return bRet;
		}
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
					catch
					{
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
			timerUserAircraft.Stop();
			timerUserAircraft.Interval = App.Config[Config.SETTING.QUERY_USER_AIRCRAFT]["Interval"].IntValue;

			objects[(int)OBJCONTAINER.AI_PLANE].timer.Stop();
			objects[(int)OBJCONTAINER.AI_PLANE].timer.Interval = App.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["Interval"].IntValue;

			objects[(int)OBJCONTAINER.AI_HELICOPTER].timer.Stop();
			objects[(int)OBJCONTAINER.AI_HELICOPTER].timer.Interval = App.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["Interval"].IntValue;

			objects[(int)OBJCONTAINER.AI_BOAT].timer.Stop();
			objects[(int)OBJCONTAINER.AI_BOAT].timer.Interval = App.Config[Config.SETTING.QUERY_AI_BOATS]["Interval"].IntValue;

			objects[(int)OBJCONTAINER.AI_GROUND].timer.Stop();
			objects[(int)OBJCONTAINER.AI_GROUND].timer.Interval = App.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["Interval"].IntValue;

			objects[(int)OBJCONTAINER.NAVAIDS].timer.Stop();
			objects[(int)OBJCONTAINER.NAVAIDS].timer.Interval = App.Config[Config.SETTING.QUERY_NAVAIDS]["Interval"].IntValue * 1000;

			objects[(int)OBJCONTAINER.AIRPORTS].timer.Stop();
			objects[(int)OBJCONTAINER.AIRPORTS].timer.Interval = App.Config[Config.SETTING.QUERY_NAVAIDS]["Interval"].IntValue * 1000;

			EnableTimers();
		}
		public void EnableTimers()
		{
			EnableTimers(true);
		}
		public void EnableTimers(bool bEnable)
		{
			bool bQueryAI = App.Config[Config.SETTING.QUERY_AI_OBJECTS]["Enabled"].BoolValue;
			timerUserAircraft.Enabled = bEnable && App.Config[Config.SETTING.QUERY_USER_AIRCRAFT]["Enabled"].BoolValue;
			objects[(int)OBJCONTAINER.AI_PLANE].timer.Enabled = bEnable && bQueryAI && App.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["Enabled"].BoolValue;
			objects[(int)OBJCONTAINER.AI_HELICOPTER].timer.Enabled = bEnable && bQueryAI && App.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["Enabled"].BoolValue;
			objects[(int)OBJCONTAINER.AI_BOAT].timer.Enabled = bEnable && bQueryAI && App.Config[Config.SETTING.QUERY_AI_BOATS]["Enabled"].BoolValue;
			objects[(int)OBJCONTAINER.AI_GROUND].timer.Enabled = bEnable && bQueryAI && App.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["Enabled"].BoolValue;
			objects[(int)OBJCONTAINER.NAVAIDS].timer.Enabled = bEnable && App.Config[Config.SETTING.QUERY_NAVAIDS]["Enabled"].BoolValue;
			objects[(int)OBJCONTAINER.AIRPORTS].timer.Enabled = bEnable && App.Config[Config.SETTING.QUERY_NAVAIDS]["Enabled"].BoolValue;
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
					simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_AI_PLANE, DEFINITIONS.StructBasicMovingSceneryObject, (uint)App.Config[Config.SETTING.QUERY_AI_AIRCRAFTS]["Range"].IntValue, SIMCONNECT_SIMOBJECT_TYPE.AIRCRAFT);

				}
				//catch (COMException ex)
				catch
				{
                    // TODO: jtr: I commented out the following line cause it created an error. Didn't know why it exists anyway.
					//frmMain.NotifyError(ex.Message);
				}
			}
		}
		private void OnTimerQueryAIHelicoptersElapsed(object sender, ElapsedEventArgs e)
		{
			lock (lockSimConnect)
			{
				try
				{
					simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_AI_HELICOPTER, DEFINITIONS.StructBasicMovingSceneryObject, (uint)App.Config[Config.SETTING.QUERY_AI_HELICOPTERS]["Range"].IntValue, SIMCONNECT_SIMOBJECT_TYPE.HELICOPTER);
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
					simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_AI_BOAT, DEFINITIONS.StructBasicMovingSceneryObject, (uint)App.Config[Config.SETTING.QUERY_AI_BOATS]["Range"].IntValue, SIMCONNECT_SIMOBJECT_TYPE.BOAT);
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
					simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_AI_GROUND, DEFINITIONS.StructBasicMovingSceneryObject, (uint)App.Config[Config.SETTING.QUERY_AI_GROUND_UNITS]["Range"].IntValue, SIMCONNECT_SIMOBJECT_TYPE.GROUND);
				}
				catch
				{
				}
			}
		}
		private void OnTimerQueryNavAidsElapsed(object sender, ElapsedEventArgs e)
		{
			float fUserLon;
			float fUserLat;
			float fUserAltAGL;
			lock (lockUserAircraft)
			{
				if (objUserAircraft == null)
					return;
				fUserLon = objUserAircraft.ObjectPosition.Longitude.Value;
				fUserLat = objUserAircraft.ObjectPosition.Latitude.Value;
				fUserAltAGL = objUserAircraft.AltitudeAGL;
			}

			float fNorth = 0;
			float fEast = 0;
			float fSouth = 0;
			float fWest = 0;
			float fTmp = 0;
			KmlFactory.MovePoint(fUserLon, fUserLat, 0, App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fTmp, ref fNorth);
			KmlFactory.MovePoint(fUserLon, fUserLat, 90, App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fEast, ref fTmp);
			KmlFactory.MovePoint(fUserLon, fUserLat, 180, App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fTmp, ref fSouth);
			KmlFactory.MovePoint(fUserLon, fUserLat, 270, App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fWest, ref fTmp);

			OleDbConnection dbCon = new OleDbConnection(App.Config.ConnectionString);
			dbCon.Open();
			OleDbCommand cmd = new OleDbCommand("SELECT ID, Longitude, Latitude FROM navaids WHERE " +
				"Latitude >= " + fSouth.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + " AND " +
				"Latitude <= " + fNorth.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + " AND " +
				"Longitude >= " + fWest.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + " AND " +
				"Longitude <= " + fEast.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + ";", dbCon);

			OleDbDataReader rd = cmd.ExecuteReader();
			while (rd.Read())
			{
				float fDist = 0;
				float fHead = 0;
				KmlFactory.GetDistance(fUserLon, fUserLat, rd.GetFloat(1), rd.GetFloat(2), ref fDist, ref fHead);
				uint unID = (uint)rd.GetInt32(0);
				if (fDist <= App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue)
				{
					lock (objects[(int)OBJCONTAINER.NAVAIDS].lockObject)
					{
						if (objects[(int)OBJCONTAINER.NAVAIDS].htObjects.ContainsKey(unID))
							((SceneryStaticObject)objects[(int)OBJCONTAINER.NAVAIDS].htObjects[unID]).DataRecieved = true;
						else
							objects[(int)OBJCONTAINER.NAVAIDS].htObjects.Add(unID, new SceneryStaticObject(unID, DATA_REQUESTS.NAVAIDS, new SceneryNavaidObjectData(unID, dbCon)));
					}
				}
				else
					System.Diagnostics.Trace.WriteLine("Out of Range");
			}
			lock (objects[(int)OBJCONTAINER.NAVAIDS].lockObject)
			{
				MarkDeletedObjects(ref objects[(int)OBJCONTAINER.NAVAIDS].htObjects);
			}
			rd.Close();
			dbCon.Close();
		}

		private void OnTimerQueryAirportsElapsed(object sender, ElapsedEventArgs e)
		{
			// TODO: Code for testing purposes only, remove if no longer necessary
			// {
			//OnTimerQueryAirportsElapsedTest(sender, e);
			//return;
			// }

			float fUserLon;
			float fUserLat;
			float fUserAltAGL;
			lock (lockUserAircraft)
			{
				if (objUserAircraft == null)
					return;

				fUserLon = objUserAircraft.ObjectPosition.Longitude.Value;
				fUserLat = objUserAircraft.ObjectPosition.Latitude.Value;
				fUserAltAGL = objUserAircraft.AltitudeAGL;
			}

			float fNorth = 0;
			float fEast = 0;
			float fSouth = 0;
			float fWest = 0;
			float fTmp = 0;
			KmlFactory.MovePoint(fUserLon, fUserLat, 0, App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fTmp, ref fNorth);
			KmlFactory.MovePoint(fUserLon, fUserLat, 90, App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fEast, ref fTmp);
			KmlFactory.MovePoint(fUserLon, fUserLat, 180, App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fTmp, ref fSouth);
			KmlFactory.MovePoint(fUserLon, fUserLat, 270, App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fWest, ref fTmp);
			OleDbConnection dbCon = new OleDbConnection(App.Config.ConnectionString);
			dbCon.Open();

			OleDbCommand cmd = new OleDbCommand("SELECT ID, Longitude, Latitude FROM airports WHERE " +
				"Latitude >= " + fSouth.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + " AND " +
				"Latitude <= " + fNorth.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + " AND " +
				"Longitude >= " + fWest.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + " AND " +
				"Longitude <= " + fEast.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + ";", dbCon);


			OleDbDataReader rd = cmd.ExecuteReader();
			while (rd.Read())
			{
				float fDist = 0;
				float fHead = 0;
				KmlFactory.GetDistance(fUserLon, fUserLat, rd.GetFloat(1), rd.GetFloat(2), ref fDist, ref fHead);
				uint unID = (uint)rd.GetInt32(0);
				if (fDist <= App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue)
				{
					lock (objects[(int)OBJCONTAINER.AIRPORTS].lockObject)
					{
						SceneryAirportObject airport;
						if (objects[(int)OBJCONTAINER.AIRPORTS].htObjects.ContainsKey(unID))
						{
							airport = (SceneryAirportObject)objects[(int)OBJCONTAINER.AIRPORTS].htObjects[unID];
							airport.DataRecieved = true;
						}
						else
						{
							airport = new SceneryAirportObject(unID, new SceneryAirportObjectData(unID, dbCon));
							objects[(int)OBJCONTAINER.AIRPORTS].htObjects.Add(unID, airport);
							airport.DataRecieved = true;
						}

						if (fDist <= 8000 && fUserAltAGL < 300)
						{
							if (airport.TaxiSignData == null)
								airport.TaxiSignData = new SceneryTaxiSignData(unID, dbCon);
						}
						else
							airport.TaxiSignData = null;
					}
				}
				else
					System.Diagnostics.Trace.WriteLine("Out of Range");
			}
			lock (objects[(int)OBJCONTAINER.AIRPORTS].lockObject)
			{
				MarkDeletedObjects(ref objects[(int)OBJCONTAINER.AIRPORTS].htObjects);
			}
			rd.Close();
			dbCon.Close();
		}

		// TODO: Method just for testing purposes, remove if no longer necessary
		private void OnTimerQueryAirportsElapsedTest(object sender, ElapsedEventArgs e)
		{
			float fUserLon;
			float fUserLat;
			float fUserAltAGL;
			lock (lockUserAircraft)
			{
				if (objUserAircraft == null)
				{
					fUserLon = 0.0F;
					fUserLat = 0.0F;
					fUserAltAGL = 0.0F;
				}
				else
				{
					fUserLon = objUserAircraft.ObjectPosition.Longitude.Value;
					fUserLat = objUserAircraft.ObjectPosition.Latitude.Value;
					fUserAltAGL = objUserAircraft.AltitudeAGL;
				}
			}

			float fNorth = 0;
			float fEast = 0;
			float fSouth = 0;
			float fWest = 0;
			float fTmp = 0;
			KmlFactory.MovePoint(fUserLon, fUserLat, 0, App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fTmp, ref fNorth);
			KmlFactory.MovePoint(fUserLon, fUserLat, 90, App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fEast, ref fTmp);
			KmlFactory.MovePoint(fUserLon, fUserLat, 180, App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fTmp, ref fSouth);
			KmlFactory.MovePoint(fUserLon, fUserLat, 270, App.Config[Config.SETTING.QUERY_NAVAIDS]["Range"].IntValue, ref fWest, ref fTmp);
			OleDbConnection dbCon = new OleDbConnection(App.Config.ConnectionString);
			dbCon.Open();

			OleDbCommand cmd = new OleDbCommand("SELECT ID, Longitude, Latitude FROM airports WHERE Ident='EDDM';", dbCon);


			OleDbDataReader rd = cmd.ExecuteReader();
			while (rd.Read())
			{
				float fDist = 0;
				float fHead = 0;
				KmlFactory.GetDistance(fUserLon, fUserLat, rd.GetFloat(1), rd.GetFloat(2), ref fDist, ref fHead);
				uint unID = (uint)rd.GetInt32(0);
				lock (objects[(int)OBJCONTAINER.AIRPORTS].lockObject)
				{
					SceneryAirportObject airport;
					if (objects[(int)OBJCONTAINER.AIRPORTS].htObjects.ContainsKey(unID))
					{
						airport = (SceneryAirportObject)objects[(int)OBJCONTAINER.AIRPORTS].htObjects[unID];
						airport.DataRecieved = true;
					}
					else
					{
						airport = new SceneryAirportObject(unID, new SceneryAirportObjectData(unID, dbCon));
						objects[(int)OBJCONTAINER.AIRPORTS].htObjects.Add(unID, airport);
						airport.DataRecieved = true;
					}

					if (fDist <= 8000 && fUserAltAGL < 300)
					{
						if (airport.TaxiSignData == null)
							airport.TaxiSignData = new SceneryTaxiSignData(unID, dbCon);
					}
					else
						airport.TaxiSignData = null;
				}
			}
			lock (objects[(int)OBJCONTAINER.AIRPORTS].lockObject)
			{
				MarkDeletedObjects(ref objects[(int)OBJCONTAINER.AIRPORTS].htObjects);
			}
			rd.Close();
			dbCon.Close();
		}

		#endregion

		#region Static Helperfunctions
		static public String GetMorseCode(String str)
		{
			str = str.ToUpper();
			String strMorseCode = "";
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] >= 'A' && str[i] <= 'Z')
					strMorseCode += strMorseSigns[str[i] - 'A' + 10];
				else if (str[i] >= '0' && str[i] <= '9')
					strMorseCode += strMorseSigns[str[i] - '0'];
				else
					strMorseCode += "? ";
			}
			return strMorseCode;
		}
		static public String GetRegionName(String strICAORegionCode)
		{
			String strRegion = "Unbekannt";
			if (strICAORegionCode != null && strICAORegionCode.Length >= 1)
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
		static public float ConvertDegToFloat2(String szDeg)
		{

			String szTemp = szDeg;

			szTemp = szTemp.Replace("N", "+");
			szTemp = szTemp.Replace("S", "-");
			szTemp = szTemp.Replace("E", "+");
			szTemp = szTemp.Replace("W", "-");

			char[] szSeperator = { ' ' };
			String[] szParts = szTemp.Split(szSeperator);

			if (szParts.GetLength(0) != 2)
			{
				throw new System.Exception("Wrong coordinate format!");
			}


			float f1 = float.Parse(szParts[0], System.Globalization.NumberFormatInfo.InvariantInfo);
			int iSign = szParts[0][0] == '-' ? -1 : 1;

			f1 = Math.Abs(f1);
			float f2 = float.Parse(szParts[1], System.Globalization.NumberFormatInfo.InvariantInfo);

			return (float)(iSign * (f1 + f2 / 60));
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

		static public Bitmap RenderSimpleAirportIcon(uint unID)
		{
			return RenderSimpleAirportIcon(unID, null);
		}
		static public Bitmap RenderSimpleAirportIcon(uint unID, OleDbConnection dbCon)
		{
			bool bLocalCon = false;
			if (dbCon == null)
			{
				bLocalCon = true;
				dbCon = new OleDbConnection(App.Config.ConnectionString);
				dbCon.Open();
			}

			OleDbCommand dbCmd = new OleDbCommand("SELECT Length, Heading, HasLights, Hardened, SurfaceType.ID FROM Runways INNER JOIN SurfaceType ON Runways.SurfaceID = SurfaceType.ID WHERE AirportID=" + unID.ToString(), dbCon);
			OleDbDataReader rd = dbCmd.ExecuteReader();
			float fLength = 0;
			bool bHardened = false;
			float fHeading = 0;
			bool bLights = false;
			int nType = 1;

			while (rd.Read())
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
			if (bLocalCon)
				dbCon.Close();
			return RenderSimpleAirportIcon(fHeading, (SceneryAirportObjectData.Runway.RUNWAYTYPE)nType, bLights);

		}
		static public Bitmap RenderSimpleAirportIcon(String strIdent)
		{
			return RenderSimpleAirportIcon(strIdent, null);
		}
		static public Bitmap RenderSimpleAirportIcon(String strIdent, OleDbConnection dbCon)
		{
			bool bLocalCon = false;
			if (dbCon == null)
			{
				bLocalCon = true;
				dbCon = new OleDbConnection(App.Config.ConnectionString);
				dbCon.Open();
			}

			OleDbCommand dbCmd = new OleDbCommand("SELECT ID FROM Airports WHERE Ident='" + strIdent + "'", dbCon);
			OleDbDataReader rd = dbCmd.ExecuteReader();
			Bitmap bmp = null;
			if (rd.Read())
			{
				bmp = RenderSimpleAirportIcon((uint)rd.GetInt32(0), dbCon);
			}
			rd.Close();
			if (bLocalCon)
				dbCon.Close();
			return bmp;

		}
		static public Bitmap RenderSimpleAirportIcon(float fHeading, SceneryAirportObjectData.Runway.RUNWAYTYPE tType, bool bLights)
		{
			Bitmap bmp = null;
			if (fHeading > 180)
				fHeading -= 180;

			Stream s = Assembly.GetCallingAssembly().GetManifestResourceStream("Fsxget.pub.gfx.ge.icons.fsxapd.png");
			bmp = new Bitmap(s);

			Graphics g = Graphics.FromImage(bmp);

			if (tType != SceneryAirportObjectData.Runway.RUNWAYTYPE.WATER)
			{
				Pen pen = new Pen(Color.FromArgb(0, 0, 128));
				Brush brush = new SolidBrush(tType == SceneryAirportObjectData.Runway.RUNWAYTYPE.HARDENED ? Color.FromArgb(0, 0, 128) : Color.FromArgb(255, 255, 255));
				// x24, y24

				double dPI180 = Math.PI / 180;
				int y1 = (int)(Math.Sin((90 - fHeading) * dPI180) * 18);
				int x1 = (int)(Math.Sin(fHeading * dPI180) * 18);

				Point[] pts = new Point[4];
				pts[0] = new Point();
				pts[1] = new Point();
				pts[2] = new Point();
				pts[3] = new Point();

				int y2 = (int)(Math.Sin(fHeading * dPI180) * 3);
				int x2 = (int)(Math.Sin((fHeading + 90) * dPI180) * 3);

				pts[0].X = 24 - x1 - x2;
				pts[0].Y = 24 + y1 - y2;
				pts[1].X = 24 - x1 + x2;
				pts[1].Y = 24 + y1 + y2;

				pts[2].X = 24 + x1 + x2;
				pts[2].Y = 24 - y1 + y2;
				pts[3].X = 24 + x1 - x2;
				pts[3].Y = 24 - y1 - y2;

				g.FillPolygon(brush, pts);
				g.DrawPolygon(pen, pts);

				if (bLights)
				{
					s = Assembly.GetCallingAssembly().GetManifestResourceStream("Fsxget.pub.gfx.ge.icons.fsxapl.png");
					Bitmap bmpLight = new Bitmap(s);
					//                    g.FillEllipse(new SolidBrush(Color.White), 18, 5, 12, 12);
					g.DrawImage(bmpLight, 17, 5);
				}
				bmp.MakeTransparent(Color.White);
			}
			else
			{
				s = Assembly.GetCallingAssembly().GetManifestResourceStream("Fsxget.pub.gfx.ge.icons.fsxapw.png");
				Bitmap bmpWater = new Bitmap(s);
				g.DrawImage(bmpWater, 20, 19);
			}
			return bmp;
		}

		static public Bitmap RenderComplexAirportIcon(uint unID)
		{
			return RenderComplexAirportIcon(unID, null);
		}
		static public Bitmap RenderComplexAirportIcon(uint unID, OleDbConnection dbCon)
		{
			bool bLocalCon = false;
			if (dbCon == null)
			{
				bLocalCon = true;
				dbCon = new OleDbConnection(App.Config.ConnectionString);
				dbCon.Open();
			}

			OleDbCommand cmd = new OleDbCommand("SELECT ID FROM AirportBoundary WHERE AirportID=" + unID.ToString() + " ORDER BY [Number]", dbCon);
			String strId = "";
			OleDbDataReader rd = cmd.ExecuteReader();
			while (rd.Read())
			{
				if (strId.Length > 0)
					strId += ",";
				strId += rd.GetInt32(0).ToString();
			}
			rd.Close();

			cmd.CommandText = "SELECT BoundaryID, Longitude, Latitude FROM AirportBoundaryVertex WHERE BoundaryID IN(" + strId + ") ORDER BY BoundaryID, SortNr";
			rd = cmd.ExecuteReader();
			List<float> lstLon = new List<float>();
			List<float> lstLat = new List<float>();
			List<int> lstStartIdx = new List<int>();
			int nIdx = 0;
			int nId = 0;
			float fLatN = -90;
			float fLatS = 90;
			float fLonE = -180;
			float fLonW = 180;
			float fLon = 0;
			float fLat = 0;
			while (rd.Read())
			{
				if (nId != rd.GetInt32(0))
				{
					lstStartIdx.Add(nIdx);
					nId = rd.GetInt32(0);
				}
				fLon = rd.GetFloat(1);
				fLat = rd.GetFloat(2);
				if (fLon > fLonE)
					fLonE = fLon;
				if (fLon < fLonW)
					fLonW = fLon;
				if (fLat > fLatN)
					fLatN = fLat;
				if (fLat < fLatS)
					fLatS = fLat;
				lstLon.Add(fLon);
				lstLat.Add(fLat);
				nIdx++;
			}
			rd.Close();
			lstStartIdx.Add(nIdx);
			float fDist = 0;
			float fHead = 0;
			KmlFactory.GetDistance(fLonE, fLatN, fLonW, fLatN, ref fDist, ref fHead);
			int nWidth = (int)(fDist / 30);
			KmlFactory.GetDistance(fLonW, fLatN, fLonW, fLatS, ref fDist, ref fHead);
			int nHeight = (int)(fDist / 30);

			Bitmap bmp = new Bitmap(nWidth, nHeight);
			Pen pen = new Pen(Color.FromArgb(255, 255, 255), 3);
			Brush brush = new SolidBrush(Color.FromArgb(0, 0, 128));

			Graphics g = Graphics.FromImage(bmp);
			//            Graphics g2 = frmMain.CreateGraphics();

			for (int i = 0; i < lstLat.Count; i++)
			{
				lstLon[i] = nWidth * (lstLon[i] - fLonW) / (fLonE - fLonW);
				lstLat[i] = nHeight - (nHeight * (lstLat[i] - fLatS) / (fLatN - fLatS));
			}
			int nPart = 0;
			int nPartsDone = 0;
			bool[] bPartsDone = new bool[lstStartIdx.Count - 1];
			nIdx = 0;
			int nOff = 1;
			int nStartIdx = 0;
			int nEndIdx = lstStartIdx[1] - 1;
			Point[] pts = new Point[lstLat.Count - lstStartIdx.Count + 1];
			int nPt = 0;
			do
			{
				nIdx = nStartIdx;
				while (nIdx != nEndIdx)
				{
					pts[nPt++] = new Point((int)lstLon[nIdx], (int)lstLat[nIdx]);
					//                    g2.DrawLine(pen, lstLon[nIdx], lstLat[nIdx], lstLon[nIdx + nOff], lstLat[nIdx + nOff]);
					nIdx += nOff;
				}
				nPartsDone++;
				bPartsDone[nPart] = true;
				int nIdxNearest = 0;
				float fDistMin = nHeight * nHeight + nWidth * nWidth;
				for (int j = 0; j < lstStartIdx.Count - 1; j++)
				{
					if (!bPartsDone[j])
					{
						fDist = Math.Abs(lstLon[nIdx] - lstLon[lstStartIdx[j]]) * Math.Abs(lstLon[nIdx] - lstLon[lstStartIdx[j]]) +
									  Math.Abs(lstLat[nIdx] - lstLat[lstStartIdx[j]]) * Math.Abs(lstLat[nIdx] - lstLat[lstStartIdx[j]]);
						if (fDist < fDistMin)
						{
							nIdxNearest = lstStartIdx[j];
							fDistMin = fDist;
							nOff = 1;
							nStartIdx = lstStartIdx[j];
							nEndIdx = lstStartIdx[j + 1] - 1;
							nPart = j;
						}
						fDist = Math.Abs(lstLon[nIdx] - lstLon[lstStartIdx[j + 1] - 1]) * Math.Abs(lstLon[nIdx] - lstLon[lstStartIdx[j + 1] - 1]) +
									Math.Abs(lstLat[nIdx] - lstLat[lstStartIdx[j + 1] - 1]) * Math.Abs(lstLat[nIdx] - lstLat[lstStartIdx[j + 1] - 1]);
						if (fDist < fDistMin)
						{
							nIdxNearest = lstStartIdx[j];
							fDistMin = fDist;
							nOff = -1;
							nStartIdx = lstStartIdx[j + 1] - 1;
							nEndIdx = lstStartIdx[j];
							nPart = j;
						}
					}
				}
			} while (nPartsDone != lstStartIdx.Count - 1);

			g.FillPolygon(brush, pts);

			int x1, x2, y1, y2;

			cmd.CommandText = "SELECT Longitude, Latitude, Heading, Length, Width FROM Runways WHERE AirportID=" + unID.ToString();
			rd = cmd.ExecuteReader();
			while (rd.Read())
			{
				fHead = rd.GetFloat(2);
				fDist = rd.GetFloat(3) / 2;
				KmlFactory.MovePoint(rd.GetFloat(0), rd.GetFloat(1), fHead, fDist, ref fLon, ref fLat);
				x1 = (int)(nWidth * (fLon - fLonW) / (fLonE - fLonW));
				y1 = nHeight - (int)((nHeight * (fLat - fLatS) / (fLatN - fLatS)));
				KmlFactory.MovePoint(rd.GetFloat(0), rd.GetFloat(1), fHead >= 180 ? fHead - 180 : fHead + 180, fDist, ref fLon, ref fLat);
				x2 = (int)(nWidth * (fLon - fLonW) / (fLonE - fLonW));
				y2 = nHeight - (int)((nHeight * (fLat - fLatS) / (fLatN - fLatS)));
				g.DrawLine(pen, x1, y1, x2, y2);
				Application.DoEvents();
			}
			rd.Close();

			if (bLocalCon)
				dbCon.Close();

			return bmp;
		}
		static public Bitmap RenderComplexAirportIcon(String strIdent)
		{
			return RenderComplexAirportIcon(strIdent, null);
		}
		static public Bitmap RenderComplexAirportIcon(String strIdent, OleDbConnection dbCon)
		{
			return RenderComplexAirportIcon(getAirportId(strIdent, dbCon), dbCon);
		}

		static public uint getAirportId(String strIdent, OleDbConnection dbCon)
		{
			bool bLocalCon = false;
			if (dbCon == null)
			{
				bLocalCon = true;
				dbCon = new OleDbConnection(App.Config.ConnectionString);
				dbCon.Open();
			}

			OleDbCommand dbCmd = new OleDbCommand("SELECT ID FROM Airports WHERE Ident='" + strIdent + "'", dbCon);
			OleDbDataReader rd = dbCmd.ExecuteReader();

			uint id = 0;
			if (rd.Read())
				id = (uint)rd.GetInt32(0);

			rd.Close();
			if (bLocalCon)
				dbCon.Close();

			return id;
		}


		static public Bitmap RenderTaxiwaySign(String strSign)
		{
			List<String> strSegments = new List<String>();
			String strTypeChars = "ldmiru";
			String strAllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			String strSpecialChars = "_ ->^'<v`/\\[]x#=.|";
			int nPos = 0;
			int nPosEnd = 0;
			while ((nPos = strSign.IndexOfAny(strTypeChars.ToCharArray(), nPos)) > -1)
			{
				nPosEnd = strSign.IndexOfAny(strTypeChars.ToCharArray(), nPos + 1);
				if (nPosEnd == -1)
					nPosEnd = strSign.Length;
				strSegments.Add(strSign.Substring(nPos, nPosEnd - nPos));
				nPos = nPosEnd;
			}

			Font fnt = new Font("Arial", 20, FontStyle.Bold, GraphicsUnit.Pixel);
			Pen pen = null;
			Brush brush = null;
			Color colFG = Color.White;
			Color colBG = Color.White;

			int nHeight = 40;
			int nXBorderStart = 0;
			int nXOff = 4;
			int nYOff = 8;
			int nYMid = nHeight / 2;
			int nArrowWidth = 16;
			int nArrowWidth2 = nArrowWidth / 2;
			int nArrowSpace = 4;

			int nWidth = strSign.Length * TextRenderer.MeasureText("W", fnt).Width;

			Bitmap bmpTmp = new Bitmap(nWidth, nHeight);
			Graphics g = Graphics.FromImage(bmpTmp);
			foreach (String strSeg in strSegments)
			{
				switch (strSeg[0])
				{
					case 'l':
						colBG = Color.Black;
						colFG = Color.Yellow;
						break;
					case 'd':
						colBG = Color.Yellow;
						colFG = Color.Black;
						break;
					case 'm':
					case 'r':
						colBG = Color.Red;
						colFG = Color.White;
						break;
					case 'i':
					case 'u':
						colBG = Color.White;
						colFG = Color.Black;
						break;
				}
				brush = new SolidBrush(colBG);
				pen = new Pen(colFG, 3);
				g.FillRectangle(brush, nXOff, 4, nWidth - nXOff, nHeight);
				for (int i = 1; i < strSeg.Length; i++)
				{
					if (strAllowedChars.IndexOf(strSeg[i]) > -1)
					{
						String str = "";
						do
						{
							str += strSeg[i++];
						} while (i < str.Length && strAllowedChars.IndexOf(strSeg[i]) > -1);
						i--;
						TextRenderer.DrawText(g, str, fnt, new Point(nXOff, nYOff), colFG, colBG);
						nXOff += TextRenderer.MeasureText(str, fnt).Width;
					}
					else if (strSpecialChars.IndexOf(strSeg[i]) > -1)
					{
						switch (strSeg[i])
						{
							case ' ':
							case '_':
								nXOff += TextRenderer.MeasureText(" ", fnt).Width;
								break;
							case '-':
								nXOff += nArrowSpace;
								g.DrawLine(pen, nXOff, nYMid, nXOff + nArrowWidth, nYMid);
								nXOff += nArrowSpace + nArrowWidth;
								break;
							case '>':
								nXOff += nArrowSpace;
								g.DrawLine(pen, nXOff, nYMid, nXOff + nArrowWidth, nYMid);
								g.DrawLine(pen, nXOff + nArrowWidth2 - 1, nYMid - nArrowWidth2, nXOff + nArrowWidth, nYMid + 1);
								g.DrawLine(pen, nXOff + nArrowWidth2 - 1, nYMid + nArrowWidth2, nXOff + nArrowWidth, nYMid - 1);
								nXOff += nArrowSpace + nArrowWidth;
								break;
							case '^':
								nXOff += nArrowSpace;
								g.DrawLine(pen, nXOff + nArrowWidth2, nYMid - nArrowWidth2, nXOff + nArrowWidth2, nYMid + nArrowWidth2);
								g.DrawLine(pen, nXOff, nYMid, nXOff + nArrowWidth2 + 1, nYMid - nArrowWidth2 - 1);
								g.DrawLine(pen, nXOff + nArrowWidth, nYMid, nXOff + nArrowWidth2, nYMid - nArrowWidth2);
								nXOff += nArrowSpace + nArrowWidth;
								break;
							case '\'':
								nXOff += nArrowSpace;
								g.DrawLine(pen, nXOff + nArrowWidth2, nYMid - nArrowWidth2, nXOff + nArrowWidth + 2, nYMid - nArrowWidth2);
								g.DrawLine(pen, nXOff + nArrowWidth, nYMid - nArrowWidth2, nXOff + nArrowWidth, nYMid);
								g.DrawLine(pen, nXOff, nYMid + nArrowWidth2, nXOff + nArrowWidth, nYMid - nArrowWidth2);
								nXOff += nArrowSpace + nArrowWidth;
								break;
							case '<':
								nXOff += nArrowSpace;
								g.DrawLine(pen, nXOff, nYMid, nXOff + nArrowSpace + nArrowWidth, nYMid);
								g.DrawLine(pen, nXOff + nArrowWidth2 + 1, nYMid - nArrowWidth2, nXOff, nYMid + 1);
								g.DrawLine(pen, nXOff + nArrowWidth2 + 1, nYMid + nArrowWidth2, nXOff, nYMid - 1);
								nXOff += nArrowSpace + nArrowWidth;
								break;
							case 'v':
								nXOff += nArrowSpace;
								g.DrawLine(pen, nXOff + nArrowWidth2, nYMid - nArrowWidth2, nXOff + nArrowWidth2, nYMid + nArrowWidth2);
								g.DrawLine(pen, nXOff, nYMid, nXOff + nArrowWidth2 + 1, nYMid + nArrowWidth2 + 1);
								g.DrawLine(pen, nXOff + nArrowWidth, nYMid, nXOff + nArrowWidth2, nYMid + nArrowWidth2);
								nXOff += nArrowSpace + nArrowWidth;
								break;
							case '`':
								nXOff += nArrowSpace;
								g.DrawLine(pen, nXOff, nYMid - nArrowWidth2, nXOff, nYMid);
								g.DrawLine(pen, nXOff - 1, nYMid - nArrowWidth2, nXOff + nArrowWidth2, nYMid - nArrowWidth2);
								g.DrawLine(pen, nXOff, nYMid - nArrowWidth2, nXOff + nArrowWidth, nYMid + nArrowWidth2);
								nXOff += nArrowSpace + nArrowWidth;
								break;
							case '/':
								nXOff += nArrowSpace;
								g.DrawLine(pen, nXOff, nYMid + nArrowWidth2 + 2, nXOff, nYMid);
								g.DrawLine(pen, nXOff, nYMid + nArrowWidth2, nXOff + nArrowWidth2, nYMid + nArrowWidth2);
								g.DrawLine(pen, nXOff, nYMid + nArrowWidth2, nXOff + nArrowWidth, nYMid - nArrowWidth2);
								nXOff += nArrowSpace + nArrowWidth;
								break;
							case '\\':
								nXOff += nArrowSpace;
								g.DrawLine(pen, nXOff + nArrowWidth2, nYMid + nArrowWidth2, nXOff + nArrowWidth, nYMid + nArrowWidth2);
								g.DrawLine(pen, nXOff + nArrowWidth, nYMid, nXOff + nArrowWidth, nYMid + nArrowWidth2 + 2);
								g.DrawLine(pen, nXOff, nYMid - nArrowWidth2, nXOff + nArrowWidth, nYMid + nArrowWidth2);
								nXOff += nArrowSpace + nArrowWidth;
								break;
							case '[':
								nXOff += nArrowSpace;
								nXBorderStart = nXOff;
								nXOff += nArrowSpace;
								break;
							case ']':
								nXOff += nArrowSpace;
								g.DrawRectangle(new Pen(colFG, 2), nXBorderStart, nArrowSpace + 4, nXOff - nXBorderStart, nHeight - 2 * nArrowSpace - 8);
								nXOff += nArrowSpace;
								break;
							case 'x':
								break;
							case '=':
								break;
							case '#':
								break;
							case '.':
								nXOff += nArrowSpace;
								g.FillEllipse(new SolidBrush(colFG), new Rectangle(nXOff + nArrowWidth2 - 1, nYMid - 1, 3, 3));
								nXOff += nArrowSpace + nArrowWidth;
								break;
							case '|':
								nXOff += nArrowSpace;
								g.DrawLine(new Pen(colFG, 2), nXOff, nYMid - nArrowWidth2, nXOff, nYMid + nArrowWidth2);
								nXOff += nArrowSpace;
								break;
						}
					}
					else
						throw new InvalidDataException("Invalid Taxiwaysign-Description");
				}
			}
			nXOff += 4;

			brush = new SolidBrush(Color.LightGray);
			g.FillRectangle(brush, 0, 0, nXOff, 4);
			g.FillRectangle(brush, 0, 0, 4, nHeight);
			g.FillRectangle(brush, nXOff - 4, 0, 4, nHeight);
			g.FillRectangle(brush, 0, nHeight - 4, nXOff, 4);

			Bitmap bmp = new Bitmap(nXOff, nHeight);
			g = Graphics.FromImage(bmp);
			g.DrawImage(bmpTmp, 0, 0);

			return bmp;
		}
		static public Bitmap RenderTaxiwayParking(float fRadius, String strName, int nNr)
		{
			Bitmap bmp = new Bitmap((int)(fRadius * 16), (int)(fRadius * 16));
			Graphics g = Graphics.FromImage(bmp);

			Color colFG = Color.FromArgb(255, 255, 0);
			//            Color colFG = Color.FromArgb( 0, 0, 0 );
			Pen pen = new Pen(colFG, 3);

			g.DrawEllipse(pen, 2, 2, bmp.Width - 4, bmp.Height - 4);
			g.DrawLine(pen, bmp.Width / 2, bmp.Height / 2 - 16, bmp.Width / 2, bmp.Height / 2 + 16);
			g.DrawLine(pen, bmp.Width / 2 - 16, bmp.Height / 2, bmp.Width / 2 + 16, bmp.Height / 2);
			String strTop;
			String strBottom;

			if (strName.StartsWith("GATE_"))
			{
				strTop = "GATE";
				strBottom = strNatoABC[strName[5] - 'A'] + " " + nNr.ToString();
			}
			else
			{
				strTop = strName;
				strBottom = nNr.ToString();
			}
			Font fnt = new Font("Arial", 20, FontStyle.Bold, GraphicsUnit.Pixel);
			Size size = TextRenderer.MeasureText(strTop, fnt);
			TextRenderer.DrawText(g, strTop, fnt, new Point((bmp.Width - size.Width) / 2, bmp.Height / 2 - 40), colFG);
			size = TextRenderer.MeasureText(strBottom, fnt);
			TextRenderer.DrawText(g, strBottom, fnt, new Point((bmp.Width - size.Width) / 2, bmp.Height / 2 + 20), colFG);

			return bmp;
		}

		#endregion

		#region Database-Creation
		static String[] strComTypes = new String[] {
            "APPROACH",
            "ASOS",
            "ATIS",
            "AWOS",
            "CENTER",
            "CLEARANCE",
            "CLEARANCE_PRE_TAXI",
            "CTAF",
            "DEPARTURE",
            "FSS",
            "GROUND",
            "MULTICOM",
            "REMOTE_CLEARANCE_DELIVERY",
            "TOWER",
            "UNICOM"
        };

		static String[] strSurfaces = new String[] {
            "ASPHALT",
            "BITUMINOUS",
            "BRICK",
            "CLAY",
            "CEMENT",
            "CONCRETE",
            "CORAL",
            "DIRT",
            "GRASS",
            "GRAVEL",
            "ICE",
            "MACADAM",
            "OIL_TREATED",
            "SAND",
            "SHALE",
            "SNOW",
            "STEEL_MATS",
            "TARMAC",
            "UNKNWON",
            "WATER",
            "PLANKS",
        };

		static String[] strTaxiPointTypes = new String[] {
            "NORMAL",
            "HOLD_SHORT",
            "ILS_HOLD_SHORT",
            "HOLD_SHORT_NO_DRAW",
            "ILS_HOLD_SHORT_NO_DRAW",
        };

		static String[] strTaxiwayParkingNames = new String[] {
            "PARKING",
            "DOCK",
            "GATE",
            "GATE_A",
            "GATE_B",
            "GATE_C",
            "GATE_D",
            "GATE_E",
            "GATE_F",
            "GATE_G",
            "GATE_H",
            "GATE_I",
            "GATE_J",
            "GATE_K",
            "GATE_L",
            "GATE_M",
            "GATE_N",
            "GATE_O",
            "GATE_P",
            "GATE_Q",
            "GATE_R",
            "GATE_S",
            "GATE_T",
            "GATE_U",
            "GATE_V",
            "GATE_W",
            "GATE_X",
            "GATE_Y",
            "GATE_Z",
            "NONE",
            "N_PARKING",
            "NE_PARKING",
            "NW_PARKING",
            "SE_PARKING",
            "S_PARKING",
            "SW_PARKING",
            "W_PARKING",
            "E_PARKING",
        };

		static String[] strTaxiwayParkingTypes = new String[] {
            "NONE",
            "DOCK_GA",
            "FUEL",
            "GATE_HEAVY",
            "GATE_MEDIUM",
            "GATE_SMALL",
            "RAMP_CARGO",
            "RAMP_GA",
            "RAMP_GA_LARGE",
            "RAMP_GA_MEDIUM",
            "RAMP_GA_SMALL",
            "RAMP_MIL_CARGO",
            "RAMP_MIL_COMBAT",
            "VEHICLE",
        };

		static String[] strTaxiwayPathTypes = new String[]  {
            "RUNWAY",
            "PARKING",
            "TAXI",
            "PATH",
            "CLOSED",
            "VEHICLE",
        };


		static public void GetSceneryObjects()
		{
			String strPath = Path.GetDirectoryName(App.Config.FSXPath);
			//String strBGL2XMLPath = @"C:\Programme\Microsoft Games\Microsoft Flight Simulator X SDK\Tools\BGL2XML_CMD\Bgl2Xml.exe";
			strPath += "\\Scenery";
			String strTmpFile = Path.GetTempFileName();
			String[] strFiles = Directory.GetFiles(strPath, "*.xml", SearchOption.AllDirectories);

			String strName = "";
			String strIdent = "";
			String strRegion = "";
			float fLon = 0.0f;
			float fLat = 0.0f;
			float fFreq = 0.0f;
			float fMagVar = 0.0f;
			float fAlt = 0.0f;
			float fRange = 0.0f;

			OleDbConnection dbCon = new OleDbConnection(App.Config.ConnectionString);
			dbCon.Open();
			OleDbCommand cmd = new OleDbCommand("DELETE * FROM navaids", dbCon);
			//            cmd.ExecuteNonQuery();

			foreach (String strBGLFile in strFiles)
			{
				String strHead = Path.GetFileName(strBGLFile).Substring(0, 3).ToUpper();
				if (!(strHead == "NVX" || strHead == "APX"))
				{
					continue;
				}
				System.Diagnostics.Trace.WriteLine(strBGLFile);
				try
				{
					/*                    strTmpFile = strBGLFile + ".xml";
										System.Diagnostics.ProcessStartInfo ps = new System.Diagnostics.ProcessStartInfo(strBGL2XMLPath, "\"" + strBGLFile + "\" \"" + strTmpFile + "\"");
										ps.CreateNoWindow = true;
										ps.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
										System.Diagnostics.Process p = System.Diagnostics.Process.Start(ps);
										int nSecs = 0;
										while (!p.HasExited && nSecs < 600)
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
					 */
				}
				catch
				{
				}
				//                try
				{
					XmlDocument xmld = new XmlDocument();
					//                    xmld.Load(strTmpFile);
					xmld.Load(strBGLFile);

					XmlNodeList nodes;
					nodes = xmld.GetElementsByTagName("Vor");
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
								fAlt = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
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
							nType = 1;      // Only DME
						}
						else
						{
							if (bDme)
							{
								nType = 3;  // VOR / DME
							}
							else
							{
								nType = 2;  // VOR
							}
						}
						cmd.CommandText = "INSERT INTO navaids ( Ident, Name, TypeID, Longitude, Latitude, Altitude, MagVar, Range, Freq ) VALUES ( '" +
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
								fAlt = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
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
						cmd.CommandText = "INSERT INTO navaids ( Ident, Name, TypeID, Longitude, Latitude, Altitude, MagVar, Range, Freq ) VALUES ( '" +
							strIdent + "', '" +
							strName.Replace("'", "''") + "', 4," +
							fLon.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
							fLat.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
							fAlt.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
							fMagVar.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
							fRange.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
							fFreq.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + ");";
						cmd.ExecuteNonQuery();
					}
					nodes = xmld.GetElementsByTagName("Airport");
					foreach (XmlNode xmln in nodes)
					{
						int nBoundNr = 0;
						String strCountry = "";
						String strState = "";
						String strCity = "";
						foreach (XmlAttribute xmla in xmln.Attributes)
						{
							if (xmla.Name == "lat")
								fLat = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
							else if (xmla.Name == "lon")
								fLon = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
							else if (xmla.Name == "alt")
								fAlt = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
							else if (xmla.Name == "magvar")
								fMagVar = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
							else if (xmla.Name == "ident")
							{
								strIdent = xmla.Value;
							}
							else if (xmla.Name == "name")
								strName = xmla.Value;
							else if (xmla.Name == "region")
								strRegion = xmla.Value;
							else if (xmla.Name == "country")
								strCountry = xmla.Value;
							else if (xmla.Name == "state")
								strState = xmla.Value;
							else if (xmla.Name == "city")
								strCity = xmla.Value;
						}

						if (strCountry == "")
							strCountry = "NULL";
						else
						{
							cmd.CommandText = "SELECT ID FROM Countrys WHERE Name='" + strCountry.Replace("'", "''") + "'";
							strCountry = cmd.ExecuteScalar().ToString();
						}

						if (strState == "")
							strState = "NULL";
						else
						{
							cmd.CommandText = "SELECT ID FROM States WHERE Name='" + strState.Replace("'", "''") + "'";
							strState = cmd.ExecuteScalar().ToString();
						}


						cmd.CommandText = "INSERT INTO airports ( Ident, Name, Longitude, Latitude, Altitude, MagVar, Region, CountryID, StateID, City ) VALUES ( '" +
							strIdent + "', '" +
							strName.Replace("'", "''") + "'," +
							fLon.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
							fLat.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
							fAlt.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
							fMagVar.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + ",'" +
							strRegion.Replace("'", "''") + "'," +
							strCountry + "," +
							strState + ",'" +
							strCity.Replace("'", "''") + "');";

						cmd.ExecuteNonQuery();
						cmd.CommandText = "SELECT @@IDENTITY";
						int nAPID = (int)cmd.ExecuteScalar();

						for (XmlNode xmlnChild = xmln.FirstChild; xmlnChild != null; xmlnChild = xmlnChild.NextSibling)
						{
							int nType = 0;
							float fHeading = 0;
							float fLength = 0;
							float fWidth = 0;
							int nNumber = 0;
							char cPrimDesignator = ' ';
							char cSekDesignator = ' ';
							float fPatAlt = 0;
							bool bPrimPatternRight = false;
							bool bSekPatternRight = false;
							int nIdx = 0;
							int nName = 0;
							bool bPrimTO = true;
							bool bPrimLand = true;
							bool bSecTO = true;
							bool bSecLand = true;

							if (xmlnChild.Name == "Com")
							{
								foreach (XmlAttribute xmla in xmlnChild.Attributes)
								{
									if (xmla.Name == "frequency")
										fFreq = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "type")
									{
										foreach (String strType in strComTypes)
										{
											nType++;
											if (strType == xmla.Value)
												break;
										}
										if (nType > strComTypes.Length)
											throw new Exception("Invalid ComType");
									}
									else if (xmla.Name == "name")
										strName = xmla.Value;
								}
								cmd.CommandText = "INSERT INTO AirportComs (AirportID, Name, Freq, TypeID) VALUES (" +
									nAPID.ToString() + "," +
									"'" + strName.Replace("'", "''") + "'," +
									fFreq.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									nType.ToString() + ");";
								cmd.ExecuteNonQuery();
							}
							else if (xmlnChild.Name == "Runway")
							{
								foreach (XmlAttribute xmla in xmlnChild.Attributes)
								{
									if (xmla.Name == "lat")
										fLat = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "lon")
										fLon = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "alt")
										fAlt = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "surface")
									{
										foreach (String strSurface in strSurfaces)
										{
											nType++;
											if (strSurface == xmla.Value)
												break;
										}
										if (nType > strSurfaces.Length)
											throw new Exception("Invalid SurfaceType");
									}
									else if (xmla.Name == "heading")
										fHeading = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "length")
										fLength = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "width")
									{
										fWidth = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
									}
									else if (xmla.Name == "number")
									{
										if (xmla.Value == "EAST")
											nNumber = 1090;
										else if (xmla.Value == "NORTH")
											nNumber = 1000;
										else if (xmla.Value == "NORTHEAST")
											nNumber = 1045;
										else if (xmla.Value == "NORTHWEST")
											nNumber = 1315;
										else if (xmla.Value == "SOUTH")
											nNumber = 1180;
										else if (xmla.Value == "SOUTHEAST")
											nNumber = 1135;
										else if (xmla.Value == "SOUTHWEST")
											nNumber = 1225;
										else if (xmla.Value == "WEST")
											nNumber = 1270;
										else
											nNumber = int.Parse(xmla.Value);
									}
									else if (xmla.Name == "designator")
									{
										cPrimDesignator = xmla.Value[0];
										if (cPrimDesignator == 'L')
											cSekDesignator = 'R';
										else if (cPrimDesignator == 'R')
											cSekDesignator = 'L';
										else
											cSekDesignator = xmla.Value[0];
									}
									else if (xmla.Name == "primaryDesignator")
										cPrimDesignator = xmla.Value[0];
									else if (xmla.Name == "secondaryDesignator")
										cSekDesignator = xmla.Value[0];
									else if (xmla.Name == "patternAltitude")
										fPatAlt = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "primaryPattern")
										bPrimPatternRight = xmla.Value == "RIGHT";
									else if (xmla.Name == "secondaryPattern")
										bSekPatternRight = xmla.Value == "RIGHT";
									else if (xmla.Name == "primaryTakeoff")
										bPrimTO = xmla.Value == "YES";
									else if (xmla.Name == "primaryLanding")
										bPrimLand = xmla.Value == "YES";
									else if (xmla.Name == "secondaryLanding")
										bSecLand = xmla.Value == "YES";
									else if (xmla.Name == "secondaryTakeoff")
										bSecTO = xmla.Value == "YES";
								}
								cmd.CommandText = "INSERT INTO Runways (AirportID, Longitude, Latitude, Altitude, Heading, Length, Width, [Number], SurfaceID, PrimaryDesignator, SecondaryDesignator, PatternAltitude, PrimaryPatternRight, SecondaryPatternRight, PrimaryTakeoff, PrimaryLanding, SecondaryTakeoff, SecondaryLanding) VALUES (" +
									nAPID.ToString() + "," +
									fLon.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									fLat.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									fAlt.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									fHeading.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									fLength.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									fWidth.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									nNumber.ToString() + "," +
									nType.ToString() + "," +
									"'" + cPrimDesignator + "'," +
									"'" + cSekDesignator + "'," +
									fPatAlt.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									(bPrimPatternRight ? "1" : "0") + "," +
									(bSekPatternRight ? "1" : "0") + "," +
									(bPrimTO ? "1" : "0") + "," +
									(bPrimLand ? "1" : "0") + "," +
									(bSecTO ? "1" : "0") + "," +
									(bSecLand ? "1" : "0") + ");";
								cmd.ExecuteNonQuery();
								cmd.CommandText = "SELECT @@IDENTITY";
								int nRunwayID = (int)cmd.ExecuteScalar();
								bool bHasLights = false;
								for (XmlNode xmlnRWChild = xmlnChild.FirstChild; xmlnRWChild != null; xmlnRWChild = xmlnRWChild.NextSibling)
								{
									bool bEndSec = false;
									bool bBackCourse = false;
									if (xmlnRWChild.Name == "Ils")
									{
										foreach (XmlAttribute xmla in xmlnRWChild.Attributes)
										{
											if (xmla.Name == "lat")
												fLat = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
											else if (xmla.Name == "lon")
												fLon = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
											else if (xmla.Name == "alt")
												fAlt = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
											else if (xmla.Name == "heading")
												fHeading = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
											else if (xmla.Name == "frequency")
												fFreq = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
											else if (xmla.Name == "frequency")
												fFreq = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
											else if (xmla.Name == "end")
												bEndSec = xmla.Value == "SECONDARY";
											else if (xmla.Name == "range")
												fRange = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
											else if (xmla.Name == "magvar")
												fMagVar = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
											else if (xmla.Name == "ident")
												strIdent = xmla.Value;
											else if (xmla.Name == "width")
												fWidth = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
											else if (xmla.Name == "name")
												strName = xmla.Value;
											else if (xmla.Name == "backCourse")
												bBackCourse = xmla.Value == "TRUE";
										}
										cmd.CommandText = "INSERT INTO RunwayILS (RunwayID, Name, Longitude, Latitude, Altitude, Freq, EndSecondary, Range, MagVar, Ident, Width, Heading, BackCourse) VALUES (" +
											nRunwayID.ToString() + "," +
											"'" + strName.Replace("'", "''") + "'," +
											fLon.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
											fLat.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
											fAlt.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
											fFreq.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
											(bEndSec ? "1" : "0") + "," +
											fRange.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
											fMagVar.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
											"'" + strIdent + "'," +
											fWidth.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
											fHeading.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
											(bBackCourse ? "1" : "0") + ");";
										cmd.ExecuteNonQuery();
									}
									else if (xmlnRWChild.Name == "Lights")
									{
										XmlAttribute xmla = xmlnRWChild.Attributes["center"];
										if (xmla != null)
											bHasLights = xmla.Value != "NONE";
										if (!bHasLights)
										{
											xmla = xmlnRWChild.Attributes["edge"];
											if (xmla != null)
												bHasLights = xmla.Value != "NONE";
										}
										if (bHasLights)
										{
											cmd.CommandText = "UPDATE Runways SET HasLights=1 WHERE ID=" + nRunwayID.ToString();
											cmd.ExecuteNonQuery();
										}

									}
								}
							}
							/*                            else if (xmlnChild.Name == "TaxiwayPoint")
														{
															bool bReverse = false;
															foreach (XmlAttribute xmla in xmlnChild.Attributes)
															{
																if (xmla.Name == "index")
																	nIdx = int.Parse(xmla.Value);
																else if (xmla.Name == "type")
																{
																	foreach (String strType in strTaxiPointTypes)
																	{
																		nType++;
																		if (strType == xmla.Value)
																			break;
																	}
																	if (nType > strTaxiPointTypes.Length)
																		throw new Exception("Invalid TaxiwayPointType");
																}
																else if (xmla.Name == "orientation")
																	bReverse = xmla.Value == "REVERSE";
																else if (xmla.Name == "lat")
																	fLat = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
																else if (xmla.Name == "lon")
																	fLon = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
															}
															cmd.CommandText = "INSERT INTO TaxiwayPoints (AirportID, [Index], TypeID, Longitude, Latitude, [Reverse]) VALUES (" +
																nAPID.ToString() + "," +
																nIdx.ToString() + "," +
																nType.ToString() + "," +
																fLon.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
																fLat.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
																(bReverse ? "1" : "0") + ");";
															cmd.ExecuteNonQuery();
														}
							*/
							else if (xmlnChild.Name == "TaxiwayParking")
							{
								foreach (XmlAttribute xmla in xmlnChild.Attributes)
								{
									if (xmla.Name == "index")
										nIdx = int.Parse(xmla.Value);
									else if (xmla.Name == "lat")
										fLat = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "lon")
										fLon = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "heading")
										fHeading = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "radius")
										fRange = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "type")
									{
										foreach (String strType in strTaxiwayParkingTypes)
										{
											nType++;
											if (strType == xmla.Value)
												break;
										}
										if (nType > strTaxiwayParkingTypes.Length)
											throw new Exception("Invalid TaxiwayParkingType");
									}
									else if (xmla.Name == "name")
									{
										foreach (String str in strTaxiwayParkingNames)
										{
											nName++;
											if (str == xmla.Value)
												break;
										}
										if (nName > strTaxiwayParkingNames.Length)
											throw new Exception("Invalid TaxiwayParkingName");
									}
									else if (xmla.Name == "number")
										nNumber = int.Parse(xmla.Value);
								}
								cmd.CommandText = "INSERT INTO TaxiwayParking (AirportID, [Index], Longitude, Latitude, Heading, Radius, TypeID, NameID, [Number]) VALUES (" +
									nAPID.ToString() + "," +
									nIdx.ToString() + "," +
									fLon.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									fLat.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									fHeading.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									fRange.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									nType.ToString() + "," +
									nName.ToString() + "," +
									nNumber.ToString() + ");";
								cmd.ExecuteNonQuery();
							}
							/*                            else if (xmlnChild.Name == "TaxiwayPath")
														{
															int nSurface = 0;
															int nIdxEnd = 0;
															foreach (XmlAttribute xmla in xmlnChild.Attributes)
															{
																if (xmla.Name == "type")
																{
																	foreach (String str in strTaxiwayPathTypes)
																	{
																		nType++;
																		if (str == xmla.Value)
																			break;
																	}
																	if (nType > strTaxiwayPathTypes.Length)
																		throw new Exception("Invalid TaxiwayPathType");
																}
																else if (xmla.Name == "start")
																	nIdx = int.Parse(xmla.Value);
																else if (xmla.Name == "end")
																	nIdxEnd = int.Parse(xmla.Value);
																else if (xmla.Name == "width")
																	fWidth = float.Parse(xmla.Value.Substring(0, xmla.Value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
																else if (xmla.Name == "surface")
																{
																	foreach (String str in strSurfaces)
																	{
																		nSurface++;
																		if (str == xmla.Value)
																			break;
																	}
																	if (nSurface > strSurfaces.Length)
																		throw new Exception("Invalid SurfaceType");
																}
																else if (xmla.Name == "number")
																{
																	if (xmla.Value == "EAST")
																		nNumber = 1090;
																	else if (xmla.Value == "NORTH")
																		nNumber = 1000;
																	else if (xmla.Value == "NORTHEAST")
																		nNumber = 1045;
																	else if (xmla.Value == "NORTHWEST")
																		nNumber = 1315;
																	else if (xmla.Value == "SOUTH")
																		nNumber = 1180;
																	else if (xmla.Value == "SOUTHEAST")
																		nNumber = 1135;
																	else if (xmla.Value == "SOUTHWEST")
																		nNumber = 1225;
																	else if (xmla.Value == "WEST")
																		nNumber = 1270;
																	else
																		nNumber = int.Parse(xmla.Value);
																}
																else if (xmla.Name == "designator")
																	cPrimDesignator = xmla.Value[0];
																else if (xmla.Name == "name")
																	nName = int.Parse(xmla.Value);
															}
															cmd.CommandText = "INSERT INTO TaxiwayPaths (AirportID, StartPointIndex, EndPointIndex, NameIndex, TypeID, Width, SurfaceID, [Number], Designator) VALUES (" +
																nAPID.ToString() + "," +
																nIdx.ToString() + "," +
																nIdxEnd.ToString() + "," +
																nName.ToString() + "," +
																nType.ToString() + "," +
																fWidth.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
																nSurface.ToString() + "," +
																nNumber.ToString() + "," +
																"'" + cPrimDesignator + "');";
															cmd.ExecuteNonQuery();
														}
														else if (xmlnChild.Name == "TaxiName")
														{
															foreach (XmlAttribute xmla in xmlnChild.Attributes)
															{
																if (xmla.Name == "index")
																	nIdx = int.Parse(xmla.Value);
																else if (xmla.Name == "name")
																	strName = xmla.Value;
															}
															cmd.CommandText = "INSERT INTO TaxiNames (AirportID, [Index], Name) VALUES (" +
																nAPID.ToString() + "," +
																nIdx.ToString() + "," +
																"'" + strName.Replace("'", "''") + "');";
															cmd.ExecuteNonQuery();
														}
							 */
							else if (xmlnChild.Name == "TaxiwaySign")
							{
								foreach (XmlAttribute xmla in xmlnChild.Attributes)
								{
									if (xmla.Name == "lat")
										fLat = FsxConnection.ConvertDegToFloat2(xmla.Value);//float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "lon")
										fLon = FsxConnection.ConvertDegToFloat2(xmla.Value); //float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "heading")
										fHeading = float.Parse(xmla.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
									else if (xmla.Name == "label")
										strName = xmla.Value;
									else if (xmla.Name == "size")
										nIdx = xmla.Value[4] - '0';
									else if (xmla.Name == "justification")
										bPrimPatternRight = xmla.Value == "RIGHT";
								}
								cmd.CommandText = "INSERT INTO TaxiwaySigns (AirportID, Longitude, Latitude, Heading, Label, JustifyRight, [Size]) VALUES (" +
									nAPID.ToString() + "," +
									fLon.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									fLat.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									fHeading.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," +
									"'" + strName.Replace("'", "''") + "'," +
									(bPrimPatternRight ? "1" : "0") + "," +
									nIdx.ToString() + ");";
								cmd.ExecuteNonQuery();
							}
							else if (xmlnChild.Name == "BoundaryFence")
							{
								nIdx = 0;
								cmd.CommandText = "INSERT INTO AirportBoundary (AirportID, [Number]) VALUES (" +
									nAPID.ToString() + "," +
									nBoundNr.ToString() + ");";
								cmd.ExecuteNonQuery();
								cmd.CommandText = "SELECT @@IDENTITY";
								int nBoundID = (int)cmd.ExecuteScalar();
								for (XmlNode xmlnVertex = xmlnChild.FirstChild; xmlnVertex != null; xmlnVertex = xmlnVertex.NextSibling)
								{
									cmd.CommandText = "INSERT INTO AirportBoundaryVertex (BoundaryID, SortNr, Longitude, Latitude) VALUES (" +
										nBoundID.ToString() + "," +
										nIdx.ToString() + "," +
										xmlnVertex.Attributes["lon"].Value + "," +
										xmlnVertex.Attributes["lat"].Value + ");";
									cmd.ExecuteNonQuery();
									nIdx++;
								}
								nBoundNr++;
							}
						}
					}

					xmld = null;
				}
				//                catch( Exception e )
				{
					//                    System.Diagnostics.Trace.WriteLine(e.Message);
				}
			}
			dbCon.Close();
			//            File.Delete(strTmpFile);
		}
		#endregion
	}
}
