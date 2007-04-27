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


namespace FSX_Google_Earth_Tracker
{
    #region SettingsClasses
    public class Attribute
    {
        protected enum TYPE
        {
            INT = 0,
            FLOAT,
            STRING,
            BOOL
        } ;
        protected String strName;
        protected TYPE tType;
        protected object value;

        public Attribute(String strAttribute)
        {
            int nPos1 = strAttribute.IndexOf('<');
            int nPos2 = strAttribute.IndexOf('>');
            if (nPos1 < 1 || nPos2 < 1)
                throw new Exception("Invalid attribute format");
            strName = strAttribute.Substring(0, nPos1);
            String strType = strAttribute.Substring(nPos1 + 1, nPos2 - nPos1 - 1).ToLower();
            if (strType == "int")
                tType = TYPE.INT;
            else if (strType == "float")
                tType = TYPE.FLOAT;
            else if (strType == "string")
                tType = TYPE.STRING;
            else if (strType == "bool")
                tType = TYPE.BOOL;
            else
                throw new Exception("Invalid attribute type");
        }

        public void ReadFromXML(ref XmlNode xmln)
        {
            String strValue = GetXMLAttribute(ref xmln).Value;
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

        public void WriteToXML(ref XmlNode xmln)
        {
            XmlAttribute xmla = GetXMLAttribute(ref xmln);

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
            }
        }

        protected XmlAttribute GetXMLAttribute(ref XmlNode xmln)
        {
            XmlAttribute xmla = xmln.Attributes[strName];
            if (xmla == null)
            {
                xmla = xmln.OwnerDocument.CreateAttribute(strName);
                xmln.Attributes.Append(xmla);
            }
            return xmla;

        }

        public String Name
        {
            get
            {
                return strName;
            }
        }

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
                    throw new InvalidCastException("Attribute is not of int type");
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
                    throw new InvalidCastException("Attribute is not of float type");
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
                    throw new InvalidCastException("Attribute is not of string type");
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
                    throw new InvalidCastException("Attribute is not of bool type");
            }
        }
    }
    abstract class Settings
    {
        protected String[] strPathParts;

        public Settings(String strXMLPath)
        {
            this.strPathParts = strXMLPath.Split('/');
        }

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

        public abstract Attribute GetAttribute(String strName);

        protected XmlNode GetXmlNode(ref XmlDocument xmld)
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

        protected XmlAttribute GetXMLAttribute(ref XmlNode xmln, String strName)
        {
            XmlAttribute xmla = xmln.Attributes[strName];
            if (xmla == null)
            {
                xmla = xmln.OwnerDocument.CreateAttribute(strName);
                xmln.Attributes.Append(xmla);
            }
            return xmla;
        }

    }
    class SettingsObject : Settings
    {

        protected Hashtable attributes;

        public SettingsObject(String strXMLPath, String strAttributes)
            : base( strXMLPath )
        {
            String[] strAttributesParts = strAttributes.Split(';');
            attributes = new Hashtable();
            foreach (String strAttribute in strAttributesParts)
            {
                Attribute attribute = new Attribute(strAttribute);
                attributes.Add(attribute.Name, attribute);
            }
        }

        public override void ReadFromXML(ref XmlNode xmln)
        {
            foreach (DictionaryEntry entry in attributes)
            {
                ((Attribute)entry.Value).ReadFromXML(ref xmln);
            }
        }

        public override void WriteToXML(ref XmlNode xmln)
        {
            foreach (DictionaryEntry entry in attributes)
            {
                ((Attribute)entry.Value).WriteToXML(ref xmln);
            }
        }


        public override Attribute GetAttribute(String strName)
        {
            return (Attribute)attributes[strName];
        }
    }
    class SettingsList : Settings
    {
        public List<SettingsObject> listSettings;
        protected String strXMLPath;
        protected String strAttributes;
        protected String strElementName;
        
        public SettingsList(String strXMLPath, String strElementName, String strAttributes)
            : base(strXMLPath)
        {
            listSettings = new List<SettingsObject>();
            this.strXMLPath = strXMLPath;
            this.strAttributes = strAttributes;
            this.strElementName = strElementName;
        }
        
        public override void ReadFromXML(ref XmlNode xmln)
        {
            for (XmlNode xmlnChild = xmln.FirstChild; xmlnChild != null; xmlnChild = xmlnChild.NextSibling)
            {
                SettingsObject setting = new SettingsObject(strXMLPath + "/" + strElementName, strAttributes);
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

        public override Attribute GetAttribute(String strName)
        {
            return GetAttribute(strName, 0);   
        }
        
        public Attribute GetAttribute(String strName, int nIdx)
        {
            return listSettings[nIdx].GetAttribute(strName);
        }
    }
    #endregion
    
    class Config
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
            PREDITCTION_POINTS,
            QUERY_AI_OBJECTS,
            QUERY_AI_AIRCRAFTS,
            QUERY_AI_HELICOPTERS,
            QUERY_AI_BOATS,
            QUERY_AI_GROUND_UNITS,
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
       
        private const string strRegKeyRun = "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run";

        private bool bCanRunGE;
        private bool bCanRunFSX;

        #endregion

        public Config()
        {
            settings = new List<Settings>(Enum.GetValues(typeof(SETTING)).Length);

            settings.Add(new SettingsObject( "fsxget/settings/options/general/enable-on-startup", "Enabled<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/general/show-balloon-tips", "Enabled<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/general/load-kml-file", "Enabled<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/general/update-check", "Enabled<bool>"));
            
            settings.Add(new SettingsObject( "fsxget/settings/options/fsx/query-user-aircraft", "Enabled<bool>;Interval<int>"));
            settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-user-path", "Enabled<bool>;Interval<int>"));
            settings.Add(new SettingsObject("fsxget/settings/options/fsx/user-path-prediction", "Enabled<bool>;Interval<int>"));
            settings.Add(new SettingsList("fsxget/settings/options/fsx/user-path-prediction", "prediction-point", "Time<int>"));

            settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-ai-objects", "Enabled<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-ai-objects/query-ai-aircrafts", "Enabled<bool>;Interval<int>;Range<int>;Prediction<bool>;PredictionPoints<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-ai-objects/query-ai-helicopters", "Enabled<bool>;Interval<int>;Range<int>;Prediction<bool>;PredictionPoints<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-ai-objects/query-ai-boats", "Enabled<bool>;Interval<int>;Range<int>;Prediction<bool>;PredictionPoints<bool>"));
            settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-ai-objects/query-ai-ground-units", "Enabled<bool>;Interval<int>;Range<int>;Prediction<bool>;PredictionPoints<bool>"));

            settings.Add(new SettingsObject("fsxget/settings/options/ge/server-settings/port", "Value<int>"));
            settings.Add(new SettingsObject("fsxget/settings/options/ge/server-settings/access-level", "Value<int>"));

            settings.Add(new SettingsList("fsxget/gfx/program/icons", "icon", "Name<String>;Img<String>"));
            settings.Add(new SettingsList("fsxget/gfx/scenery/air", "aircraft", "Name<String>;Img<String>"));
            settings.Add(new SettingsList("fsxget/gfx/scenery/water", "boat", "Name<String>;Img<String>"));
            settings.Add(new SettingsList("fsxget/gfx/scenery/ground", "ground_unit", "Name<String>;Img<String>"));
            settings.Add(new SettingsList("fsxget/gfx/ge/icons", "icon", "Name<String>;Img<String>"));

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

            // Check if config file for current user exists
            if (!File.Exists(strXMLFile))
            {
                if (!Directory.Exists(strUserAppPath))
                    Directory.CreateDirectory(strUserAppPath);
                SetDefaults();
            }
            else
                ReadFromXML();

            const String strRegKeyFSX = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Microsoft Games\\flight simulator\\10.0";
            const String strRegKeyGE = "HKEY_CLASSES_ROOT\\.kml";

            String strPathGE = (String)Registry.GetValue(strRegKeyGE, "", "");
            String strPathFSX = (String)Registry.GetValue(strRegKeyFSX, "SetupPath", "");

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
        } 

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
        }

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
        public Settings GetSettings(SETTING idx)
        {
            return settings[(int)idx];
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
