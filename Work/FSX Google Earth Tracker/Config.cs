using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml;
using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;
using System.Reflection;


namespace Fsxget
{

    #region SettingsClasses
    public class ConfigValue
    {
        #region Variables
        protected enum TYPE
        {
            VOID = 0,
            INT,
            FLOAT,
            STRING,
            BOOL
        } ;
        protected TYPE tType;
        protected object value;
        #endregion 

        #region Construction
        public ConfigValue(String strType)
        {
            Init(strType);
        }
        protected void Init(String strType)
        {
            if (strType == "int")
                tType = TYPE.INT;
            else if (strType == "float")
                tType = TYPE.FLOAT;
            else if (strType == "string")
                tType = TYPE.STRING;
            else if (strType == "bool")
                tType = TYPE.BOOL;
            else if (strType == "void")
                tType = TYPE.VOID;
            else
                throw new Exception("Invalid value type");
        }
        protected ConfigValue()
        {
            tType = TYPE.VOID;
        }
        #endregion

        #region XML-Handling
        public virtual void ReadFromXML(ref XmlNode xmln)
        {
            XmlNode xmlnText = xmln.FirstChild;
            if (xmlnText == null)
                throw new InvalidDataException("XmlNode has no data");
            switch (tType)
            {
                case TYPE.INT:
                    value = int.Parse(xmln.FirstChild.Value);
                    break;
                case TYPE.FLOAT:
                    value = float.Parse(xmln.FirstChild.Value);
                    break;
                case TYPE.STRING:
                    value = xmln.FirstChild.Value;
                    break;
                case TYPE.BOOL:
                    if (xmln.FirstChild.Value.ToLower() == "true" || xmln.FirstChild.Value == "1")
                        value = true;
                    else
                        value = false;
                    break;
            }
        }
        public virtual void WriteToXML(ref XmlNode xmln)
        {
            if (value != null)
            {
                xmln.AppendChild(xmln.OwnerDocument.CreateTextNode(""));
                switch (tType)
                {
                    case TYPE.INT:
                        xmln.FirstChild.Value = XmlConvert.ToString((int)value);
                        break;
                    case TYPE.FLOAT:
                        xmln.FirstChild.Value = XmlConvert.ToString((float)value);
                        break;
                    case TYPE.STRING:
                        xmln.FirstChild.Value = (String)value;
                        break;
                    case TYPE.BOOL:
                        xmln.FirstChild.Value = XmlConvert.ToString((bool)value);
                        break;
                }
            }
            else
                throw new InvalidDataException("No data assigned to XmlNode");
        }
        #endregion

        #region Accessors
        public int IntValue
        {
            get
            {
                return (int)value;
            }
            set
            {
                if (tType == TYPE.INT)
                    this.value = value;
                else
                    throw new InvalidCastException("Value is not of int type");
            }
        }
        public float FloatValue
        {
            get
            {
                return (float)value;
            }
            set
            {
                if (tType == TYPE.FLOAT)
                    this.value = value;
                else
                    throw new InvalidCastException("Value is not of float type");
            }
        }
        public String StringValue
        {
            get
            {
                return (String)value;
            }
            set
            {
                if (tType == TYPE.STRING)
                    this.value = value;
                else
                    throw new InvalidCastException("Value is not of string type");
            }
        }
        public bool BoolValue
        {
            get
            {
                return (bool)value;
            }
            set
            {
                if (tType == TYPE.BOOL)
                    this.value = value;
                else
                    throw new InvalidCastException("Value is not of bool type");
            }
        }
        #endregion
    }
    public class ConfigAttribute : ConfigValue
    {
        #region Variables
        protected String strName;
        #endregion

        #region Construction
        public ConfigAttribute(String strAttribute)
        {
            int nPos1 = strAttribute.IndexOf('<');
            int nPos2 = strAttribute.IndexOf('>');
            if (nPos1 < 1 || nPos2 < 1)
                throw new Exception("Invalid attribute format");
            strName = strAttribute.Substring(0, nPos1);
            String strType = strAttribute.Substring(nPos1 + 1, nPos2 - nPos1 - 1).ToLower();
            Init(strType);
            if (tType == TYPE.VOID)
            {
                throw new InvalidDataException("Attribute can not have the type void");
            }
        }
        #endregion

        #region XML-Handling
        public override void ReadFromXML(ref XmlNode xmln)
        {
            String strValue = GetXmlAttribute(ref xmln).Value;
            switch (tType)
            {
                case TYPE.INT:
                    value = int.Parse(strValue);
                    break;
                case TYPE.FLOAT:
                    value = float.Parse(strValue);
                    break;
                case TYPE.STRING:
                    value = strValue;
                    break;
                case TYPE.BOOL:
                    if (strValue.ToLower() == "true" || strValue == "1")
                        value = true;
                    else
                        value = false;
                    break;
            }
        }
        public override void WriteToXML(ref XmlNode xmln)
        {
            XmlAttribute xmla = GetXmlAttribute(ref xmln);
            if (value != null)
            {
                switch (tType)
                {
                    case TYPE.INT:
                        xmla.Value = XmlConvert.ToString((int)value);
                        break;
                    case TYPE.FLOAT:
                        xmla.Value = XmlConvert.ToString((float)value);
                        break;
                    case TYPE.STRING:
                        xmla.Value = (String)value;
                        break;
                    case TYPE.BOOL:
                        xmla.Value = XmlConvert.ToString((bool)value);
                        break;
                }
            }
            else
                throw new InvalidDataException("Attribute has no data");
        }
        protected XmlAttribute GetXmlAttribute(ref XmlNode xmln)
        {
            XmlAttribute xmla = xmln.Attributes[strName];
            if (xmla == null)
            {
                xmla = xmln.OwnerDocument.CreateAttribute(strName);
                xmln.Attributes.Append(xmla);
            }
            return xmla;

        }
        #endregion

        #region Accessors
        public String Name
        {
            get
            {
                return strName;
            }
        }
        #endregion
    }
    public abstract class Settings
    {
        #region Variables
        protected String[] strPathParts;
        #endregion

        #region Construction
        public Settings(String strXMLPath)
        {
            this.strPathParts = strXMLPath.Split('/');
        }
        #endregion

        #region XML-Handling
        public virtual void ReadFromXML(ref XmlDocument xmld)
        {
            XmlNode xmln = GetXmlNode(ref xmld);
            ReadFromXML(ref xmln);
        }
        public virtual void WriteToXML(ref XmlDocument xmld)
        {
            XmlNode xmln = GetXmlNode(ref xmld);
            WriteToXML(ref xmln);
        }
        public abstract void ReadFromXML(ref XmlNode xmln);
        public abstract void WriteToXML(ref XmlNode xmln);
        protected virtual XmlNode GetXmlNode(ref XmlDocument xmld)
        {
            XmlNode xmln = null;
            for (int i = 0; i < strPathParts.Length; i++)
            {
                XmlNode xmlnTmp = xmln == null ? xmld[strPathParts[i]] : xmln[strPathParts[i]];
                if (xmlnTmp == null)
                {
                    xmlnTmp = xmld.CreateElement(strPathParts[i]);
                    if (xmln == null)
                        xmld.AppendChild(xmlnTmp);
                    else
                        xmln.AppendChild(xmlnTmp);
                }
                xmln = xmlnTmp;
            }
            return xmln;
        }
        protected XmlAttribute GetXmlAttribute(ref XmlNode xmln, String strName)
        {
            XmlAttribute xmla = xmln.Attributes[strName];
            if (xmla == null)
            {
                xmla = xmln.OwnerDocument.CreateAttribute(strName);
                xmln.Attributes.Append(xmla);
            }
            return xmla;
        }
        #endregion

        #region Accessors
        public virtual ConfigAttribute this[String strName]
        {
            get
            {
                return this[strName, 0];
            }
        }
        public abstract ConfigAttribute this[String strName, int nIdx]
        {
            get;
        }

        public virtual ConfigValue Value
        {
            get
            {
                return this[0];
            }
            set
            {
                this[0] = value;
            }
        }
        public abstract ConfigValue this[int nIdx]
        {
            get;
            set;
        }
        
        #endregion

    }
    public class SettingsObject : Settings
    {
        #region Variables
        protected ConfigValue value;
        protected Hashtable attributes;
        #endregion

        #region Construction
        public SettingsObject(String strXMLPath, String strValueType, String strAttributes)
            : base(strXMLPath)
        {
            if (strAttributes != null)
            {
                String[] strAttributesParts = strAttributes.Split(';');
                attributes = new Hashtable();
                foreach (String strAttribute in strAttributesParts)
                {
                    ConfigAttribute attribute = new ConfigAttribute(strAttribute);
                    attributes.Add(attribute.Name, attribute);
                }
            }
            if (strValueType != null && strValueType.Length > 0)
                value = new ConfigValue(strValueType);
        }
        #endregion

        #region XML-Handling
        public override void ReadFromXML(ref XmlNode xmln)
        {
            if (value != null)
                value.ReadFromXML(ref xmln);
            if (attributes != null)
            {
                foreach (DictionaryEntry entry in attributes)
                {
                    ((ConfigAttribute)entry.Value).ReadFromXML(ref xmln);
                }
            }
        }
        public override void WriteToXML(ref XmlNode xmln)
        {
            if( value != null )
                value.WriteToXML(ref xmln);
            if (attributes != null)
            {
                foreach (DictionaryEntry entry in attributes)
                {
                    ((ConfigAttribute)entry.Value).WriteToXML(ref xmln);
                }
            }
        }
        #endregion

        #region Accessors
        public override ConfigValue this[int nIdx]
        {
            get
            {
                if (nIdx != 0)
                    throw new IndexOutOfRangeException();
                return value;
            }
            set
            {
                if (nIdx != 0)
                    throw new IndexOutOfRangeException();
                this.value = value;
            }
        }
        public override ConfigAttribute this[String strName, int nIdx]
        {
            get
            {
                if (nIdx != 0)
                    throw new IndexOutOfRangeException();
                return (ConfigAttribute)attributes[strName];
            }
        }
        #endregion
    }
    public class SettingsList : Settings
    {
        #region Variables
        public List<SettingsObject> listSettings;
        protected String strXMLPath;
        protected String strValueType;
        protected String strAttributes;
        protected String strElementName;
        #endregion

        #region Construction
        public SettingsList(String strXMLPath, String strElementName, String strValueType, String strAttributes)
            : base(strXMLPath)
        {
            listSettings = new List<SettingsObject>();
            this.strXMLPath = strXMLPath;
            this.strValueType = strValueType;
            this.strAttributes = strAttributes;
            this.strElementName = strElementName;
        }
        #endregion

        #region XML-Handling
        public override void ReadFromXML(ref XmlNode xmln)
        {
            for (XmlNode xmlnChild = xmln.FirstChild; xmlnChild != null; xmlnChild = xmlnChild.NextSibling)
            {
                SettingsObject setting = new SettingsObject(strXMLPath + "/" + strElementName, strValueType, strAttributes);
                setting.ReadFromXML(ref xmlnChild);
                listSettings.Add(setting);
            }
        }
        public override void WriteToXML(ref XmlNode xmln)
        {
            XmlNode xmlnChild;
            for (xmlnChild = xmln.FirstChild; xmlnChild != null; xmlnChild = xmlnChild.NextSibling)
            {
                xmln.RemoveChild(xmlnChild);
            }

            foreach (SettingsObject setting in listSettings)
            {
                xmlnChild = xmln.OwnerDocument.CreateElement(strElementName);
                setting.WriteToXML(ref xmlnChild);
                xmln.AppendChild(xmlnChild);
            }
        }
        #endregion

        #region Accessors
        public override ConfigAttribute this[String strName]
        {
            get
            {
                return this[strName, 0];
            }
        }
        public override ConfigValue this[int nIdx]
        {
            get
            {
                return listSettings[nIdx].Value;
            }
            set
            {
                listSettings[nIdx].Value = value;
            }
        }
        public override ConfigAttribute this[String strName,int nIdx]
        {
            get
            {
                return listSettings[nIdx][strName];
            }
        }
        #endregion
    }
    #endregion

    public class Config
    {

        #region Variable-Declaration
        public enum SETTING
        {
            ENABLE_ON_STARTUP = 0,
            SHOW_BALLOON_TIPS,
            LOAD_KML_FILE,
            UPDATE_CHECK,
            QUERY_USER_AIRCRAFT,
            QUERY_USER_PATH,
            USER_PATH_PREDICTION,
            PREDICTION_POINTS,
            QUERY_AI_OBJECTS,
            QUERY_AI_AIRCRAFTS,
            QUERY_AI_HELICOPTERS,
            QUERY_AI_BOATS,
            QUERY_AI_GROUND_UNITS,
            REFRESH_USER_AIRCRAFT,
            REFRESH_AI_AIRCRAFTS,
            REFRESH_AI_HELICOPTERS,
            REFRESH_AI_BOATS,
            REFRESH_AI_GROUND_UNITS,
            GE_SERVER_PORT,
            GE_ACCESS_LEVEL,
            PRG_ICON_LIST,
            AIR_IMG_LIST,
            WATER_IMG_LIST,
            GROUND_IMG_LIST,
            GE_IMG_LIST

        };

        private String strXMLFile;
        private List<Settings> settings;

        private String strAppPath;
        private String strUserAppPath;

        private String strFilePathPub;
        private String strFilePathData;
        private String strServer;
        private const string strRegKeyRun = "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run";

        private bool bCanRunGE;
        private bool bCanRunFSX;

        private String strPathFSX;
        #endregion

        #region Construction
        public Config()
        {
            #region Create Settings-Array
            settings = new List<Settings>(Enum.GetValues(typeof(SETTING)).Length);

            settings.Add(new SettingsObject("fsxget/settings/options/general/enable-on-startup", null, "Enabled<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/general/show-balloon-tips", null, "Enabled<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/general/load-kml-file", null, "Enabled<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/general/update-check", null, "Enabled<bool>"));

            settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-user-aircraft", null, "Enabled<bool>;Interval<int>"));
            settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-user-path", null, "Enabled<bool>;Interval<int>"));
            settings.Add(new SettingsObject("fsxget/settings/options/fsx/user-path-prediction", null, "Enabled<bool>;Interval<int>"));
            settings.Add(new SettingsList("fsxget/settings/options/fsx/user-path-prediction", "prediction-point", null, "Time<int>"));

            settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-ai-objects", null, "Enabled<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-ai-objects/query-ai-aircrafts", null, "Enabled<bool>;Interval<int>;Range<int>;Prediction<bool>;PredictionPoints<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-ai-objects/query-ai-helicopters", null, "Enabled<bool>;Interval<int>;Range<int>;Prediction<bool>;PredictionPoints<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-ai-objects/query-ai-boats", null, "Enabled<bool>;Interval<int>;Range<int>;Prediction<bool>;PredictionPoints<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-ai-objects/query-ai-ground-units", null, "Enabled<bool>;Interval<int>;Range<int>;Prediction<bool>;PredictionPoints<bool>"));

            settings.Add(new SettingsObject("fsxget/settings/options/ge/refresh-rates/user-aircraft", null, "Interval<int>" ));
            settings.Add(new SettingsObject("fsxget/settings/options/ge/refresh-rates/ai-aircrafts", null, "Interval<int>"));
            settings.Add(new SettingsObject("fsxget/settings/options/ge/refresh-rates/ai-helicopters", null, "Interval<int>"));
            settings.Add(new SettingsObject("fsxget/settings/options/ge/refresh-rates/ai-boats", null, "Interval<int>"));
            settings.Add(new SettingsObject("fsxget/settings/options/ge/refresh-rates/ai-ground-units", null, "Interval<int>"));
            
            settings.Add(new SettingsObject("fsxget/settings/options/ge/server-settings/port", null, "Value<int>"));
            settings.Add(new SettingsObject("fsxget/settings/options/ge/server-settings/access-level", null, "Value<int>"));

            settings.Add(new SettingsList("fsxget/gfx/program/icons", "icon", null, "Name<String>;Img<String>"));
            settings.Add(new SettingsList("fsxget/gfx/scenery/air", "aircraft", null, "Name<String>;Img<String>"));
            settings.Add(new SettingsList("fsxget/gfx/scenery/water", "boat", null, "Name<String>;Img<String>"));
            settings.Add(new SettingsList("fsxget/gfx/scenery/ground", "ground_unit", null, "Name<String>;Img<String>"));
            settings.Add(new SettingsList("fsxget/gfx/ge/icons", "icon", null, "Name<String>;Img<String>"));
            #endregion

            #region Initialize non xml-settings

#if DEBUG
            strAppPath = Application.StartupPath + "\\..\\..";
            strUserAppPath = strAppPath + "\\User's Application Data Folder";
#else
            strAppPath = Application.StartupPath;
            strUserAppPath = Application.UserAppDataPath;
#endif
            strFilePathPub = strAppPath + "\\pub";
            strFilePathData = strAppPath + "\\data";

            strXMLFile = strUserAppPath + "\\settings.cfg";


            const String strRegKeyFSX = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Microsoft Games\\flight simulator\\10.0";
            const String strRegKeyGE = "HKEY_CLASSES_ROOT\\.kml";

            String strPathGE = (String)Registry.GetValue(strRegKeyGE, "", "");
            strPathFSX = (String)Registry.GetValue(strRegKeyFSX, "SetupPath", "");

            if (strPathGE == "Google Earth.kmlfile")
                bCanRunGE = true;
            else
                bCanRunGE = false;

            if (strPathFSX != "")
            {
                strPathFSX += "fsx.exe";
                if (File.Exists(strPathFSX))
                    bCanRunFSX = true;
                else
                    bCanRunFSX = false;
            }
            else
                bCanRunFSX = false;

            strServer = "http://localhost";
            #endregion


            #region Fill Settings-objects
            // Check if config file for current user exists
            if (!File.Exists(strXMLFile))
            {
                if (!Directory.Exists(strUserAppPath))
                    Directory.CreateDirectory(strUserAppPath);
                SetDefaults();
            }
            else
                ReadFromXML();
            #endregion

        }
        #endregion

        #region Read, Write and Defaults
        protected void ReadFromXML()
        {
            XmlTextReader xmlrSeetingsFile = new XmlTextReader(strXMLFile);
            XmlDocument xmldSettings = new XmlDocument();
            xmldSettings.Load(xmlrSeetingsFile);
            xmlrSeetingsFile.Close();
            xmlrSeetingsFile = null;

            foreach (Settings setting in settings)
            {
                setting.ReadFromXML(ref xmldSettings);
            }
        }
        protected void WriteToXML()
        {
            XmlDocument xmld = new XmlDocument();

            foreach (Settings setting in settings)
            {
                setting.WriteToXML(ref xmld);
            }

            XmlTextWriter xmlwSeetingsFile = new XmlTextWriter("C:\\test.xml", Encoding.UTF8);
            xmlwSeetingsFile.Formatting = Formatting.Indented;
            xmld.Save(xmlwSeetingsFile);
            xmlwSeetingsFile.Close();
        }
        public void SetDefaults()
        {
/*
            Settings obj;

            settings[(int)SETTING.ENABLE_ON_STARTUP]["Enabled"].BoolValue = true;
            settings[(int)SETTING.SHOW_BALLOON_TIPS]["Enabled"].BoolValue = true;
            settings[(int)SETTING.LOAD_KML_FILE]["Enabled"].BoolValue = true;
            settings[(int)SETTING.UPDATE_CHECK]["Enabled"].BoolValue = true;

            obj = settings[(int)SETTING.QUERY_USER_AIRCRAFT];
            obj["Enabled"].BoolValue = true;
            obj["Interval"].IntValue = 1000;

            obj = settings[(int)SETTING.QUERY_USER_PATH];
            obj["Enabled"].BoolValue = true;
            obj["Interval"].IntValue = 5000;

            obj = settings[(int)SETTING.USER_PATH_PREDICTION];
            obj["Enabled"].BoolValue = true;
            obj["Interval"].IntValue = 5000;


            for (int i = 0; i < 5; i++)
            {
                ((SettingsList)settings[(int)SETTING.PREDICTION_POINTS]).listSettings.Add(new SettingsObject("fsxget/settings/options/fsx/user-path-prediction/prediction-point", null, "Time<int>"));
            }
            settings[(int)SETTING.PREDICTION_POINTS]["Time", 0].IntValue = 30;
            settings[(int)SETTING.PREDICTION_POINTS]["Time", 1].IntValue = 150;
            settings[(int)SETTING.PREDICTION_POINTS]["Time", 2].IntValue = 300;
            settings[(int)SETTING.PREDICTION_POINTS]["Time", 3].IntValue = 600;
            settings[(int)SETTING.PREDICTION_POINTS]["Time", 4].IntValue = 1200;

            settings[(int)SETTING.QUERY_AI_OBJECTS]["Enabled"].BoolValue = true;

            obj = settings[(int)SETTING.QUERY_AI_AIRCRAFTS];
            obj["Enabled"].BoolValue = true;
            obj["Interval"].IntValue = 3000;
            obj["Range"].IntValue = 50000;
            obj["Prediction"].BoolValue = true;
            obj["PredictionPoints"].BoolValue = false;

            obj = settings[(int)SETTING.QUERY_AI_HELICOPTERS];
            obj["Enabled"].BoolValue = true;
            obj["Interval"].IntValue = 3000;
            obj["Range"].IntValue = 50000;
            obj["Prediction"].BoolValue = true;
            obj["PredictionPoints"].BoolValue = false;

            obj = settings[(int)SETTING.QUERY_AI_BOATS];
            obj["Enabled"].BoolValue = true;
            obj["Interval"].IntValue = 5000;
            obj["Range"].IntValue = 100000;
            obj["Prediction"].BoolValue = false;
            obj["PredictionPoints"].BoolValue = false;

            obj = settings[(int)SETTING.QUERY_AI_GROUND_UNITS];
            obj["Enabled"].BoolValue = true;
            obj["Interval"].IntValue = 5000;
            obj["Range"].IntValue = 100000;
            obj["Prediction"].BoolValue = false;
            obj["PredictionPoints"].BoolValue = false;

            settings[(int)SETTING.GE_SERVER_PORT]["Value"].IntValue = 8087;
            settings[(int)SETTING.GE_ACCESS_LEVEL]["Value"].IntValue = 1;
 */
            File.Copy(strAppPath + "\\data\\settings.default", strUserAppPath + "\\settings.cfg");
            ReadFromXML();
        }
        #endregion

        #region Accessors
        public bool CanRunGE
        {
            get
            {
                return bCanRunGE;
            }
        }
        public bool CanRunFSX
        {
            get
            {
                return bCanRunFSX;
            }
        }
        public String AppPath
        {
            get
            {
                return strAppPath;
            }
        }
        public String UserDataPath
        {
            get
            {
                return strUserAppPath;
            }
        }
        public String FilePathPub
        {
            get
            {
                return strFilePathPub;
            }
        }
        public String FilePathData
        {
            get
            {
                return strFilePathData;
            }
        }
        public bool RunOnStartup
        {
            get
            {
                String strRun = (String)Registry.GetValue(strRegKeyRun, AssemblyTitle, "");
                if (strRun != Application.ExecutablePath)
                    return false;
                else
                    return true;
            }
            set
            {
                if (value)
                {
                    Registry.SetValue(strRegKeyRun, AssemblyTitle, Application.ExecutablePath);
                }
                else
                {
                    RegistryKey regkTemp = Registry.CurrentUser.OpenSubKey(strRegKeyRun, true);
                    regkTemp.DeleteValue(AssemblyTitle);
                }
            }
        }
        public String Server
        {
            get
            {
                return strServer + ":" + this[SETTING.GE_SERVER_PORT]["Value"].IntValue.ToString();
            }
            set
            {
                strServer = value;
            }
        }
        public Settings this[SETTING idx]
        {
            get
            {
                return settings[(int)idx];
            }
        }
        public String FSXPath
        {
            get
            {
                return strPathFSX;
            }
        }
        #endregion

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                // Get all Title attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                // If there is at least one Title attribute
                if (attributes.Length > 0)
                {
                    // Select the first one
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    // If it is not an empty string, return it
                    if (titleAttribute.Title != "")
                        return titleAttribute.Title;
                }
                // If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                // Get all Description attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                // If there aren't any Description attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Description attribute, return its value
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                // Get all Product attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                // If there aren't any Product attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Product attribute, return its value
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                // Get all Copyright attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                // If there aren't any Copyright attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Copyright attribute, return its value
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                // Get all Company attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                // If there aren't any Company attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Company attribute, return its value
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        #endregion

    }
}
