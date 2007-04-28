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

	public abstract class ConfigAttributeBase
	{
		protected String szName;

		public ConfigAttributeBase() { }

		public ConfigAttributeBase(String szName)
		{
			this.szName = szName;
		}

		public String Name
		{
			get
			{
				return szName;
			}
		}

		public abstract void readFromXml(ref XmlNode xmln);

		public abstract void writeToXml(ref XmlNode xmln);
	}

	public class ConfigAttribute<T> : ConfigAttributeBase
	{
		protected T tValue;

		public T Value
		{
			get
			{
				return tValue;
			}
			set
			{
				tValue = value;
			}
		}

		public override void readFromXml(ref XmlNode xmln)
		{
			String strValue = GetXMLAttribute(ref xmln).Value;

			Type type = typeof(T);

			String[] szParams = new String[1];
			szParams[0] = strValue;

			try
			{
				if (type.IsValueType)
					tValue = (T)type.InvokeMember("Parse", BindingFlags.InvokeMethod, null, type, szParams);
				else if (type == typeof(String))
					tValue = (T)type.InvokeMember("Copy", BindingFlags.InvokeMethod, null, type, szParams);
				else
				{
					ConstructorInfo ci = type.GetConstructor(new System.Type[0]);
					tValue = (T)ci.Invoke(null);
					type.InvokeMember("fromString", BindingFlags.InvokeMethod, null, tValue, szParams);
				}
			}
			catch
			{
				throw new Exception("Invalid type exception");
			}
		}

		//public static void readFromGiven(String strValue)
		//{
		//    T szTest;
		//    T tTest;
		//    T to;

		//    if (typeof(T) == typeof(String))
		//    {
		//        Type type = typeof(T);
		//        String[] szParams = new String[1];
		//        szParams[0] = strValue;
		//        szTest = (T)type.InvokeMember("Copy", BindingFlags.InvokeMethod, null, type, szParams);
		//    }
		//    else
		//    {
		//        T t;
		//        Type type = typeof(T);
		//        String[] szParams = new String[1];
		//        szParams[0] = strValue;
		//        try
		//        {
		//            if (type.IsValueType)
		//            {
		//                t = (T)type.InvokeMember("Parse", BindingFlags.InvokeMethod, null, type, szParams);
		//                tTest = t;
		//            }
		//            else
		//            {
		//                ConstructorInfo ci = type.GetConstructor(new System.Type[0]);
		//                to = (T)ci.Invoke(null);
		//                type.InvokeMember("fromString", BindingFlags.InvokeMethod, null, to, szParams);
		//            }
		//        }
		//        catch
		//        {
		//            throw new Exception("Invalid type exception");
		//        }
		//    }
		//}

		public override void writeToXml(ref XmlNode xmln)
		{
			XmlAttribute xmla = GetXMLAttribute(ref xmln);

			try
			{
				if (tValue != null)
					xmla.Value = tValue.ToString();
				else
					xmla.Value = "";
			}
			catch
			{
				throw new Exception("Invalid type exception");
			}
		}

		protected XmlAttribute GetXMLAttribute(ref XmlNode xmln)
		{
			XmlAttribute xmla = xmln.Attributes[szName];
			if (xmla == null)
			{
				xmla = xmln.OwnerDocument.CreateAttribute(szName);
				xmln.Attributes.Append(xmla);
			}
			return xmla;

		}
	}

	public abstract class ConfigAttributeObjectBase
	{
		public ConfigAttributeObjectBase() { }

		public abstract void fromString(String szValue);
	}

	//public class TestObject : ConfigAttributeObjectBase
	//{
	//    String szTemp = "";

	//    public override void fromString(String value)
	//    {
	//        szTemp = value;
	//    }
	//}

	abstract class Settings2
	{
		protected String[] strPathParts;

		public Settings2(String strXMLPath)
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

		public abstract ConfigAttributeBase GetAttribute(String strName);

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

	}

	class SettingsObject2 : Settings2
	{

		protected Hashtable attributes;

		public SettingsObject2(String strXMLPath, params ConfigAttributeBase[] strAttributes)
			: base(strXMLPath)
		{
			//String[] strAttributesParts = strAttributes.Split(';');
			attributes = new Hashtable();
			foreach (ConfigAttributeBase strAttribute in strAttributes)
			{
				//ConfigAttributeBase attribute = new ConfigAttributeBase(strAttribute);
				attributes.Add(strAttribute.Name, strAttribute);
			}
		}

		public override void ReadFromXML(ref XmlNode xmln)
		{
			foreach (DictionaryEntry entry in attributes)
			{
				((ConfigAttributeBase)entry.Value).ReadFromXML(ref xmln);
			}
		}

		public override void WriteToXML(ref XmlNode xmln)
		{
			foreach (DictionaryEntry entry in attributes)
			{
				((ConfigAttributeBase)entry.Value).WriteToXML(ref xmln);
			}
		}

		public override ConfigAttributeBase GetAttribute(String strName)
		{
			return (ConfigAttributeBase)attributes[strName];
		}
	}

	class SettingsList2 : Settings2
	{
		public List<SettingsObject2> listSettings;
		protected String strXMLPath;
		protected ConfigAttributeBase[] strAttributes;
		protected String strElementName;

		public SettingsList2(String strXMLPath, String strElementName, params ConfigAttributeBase[] strAttributes)
			: base(strXMLPath)
		{
			listSettings = new List<SettingsObject2>();
			this.strXMLPath = strXMLPath;
			this.strAttributes = strAttributes;
			this.strElementName = strElementName;
		}

		public override void ReadFromXML(ref XmlNode xmln)
		{
			for (XmlNode xmlnChild = xmln.FirstChild; xmlnChild != null; xmlnChild = xmlnChild.NextSibling)
			{
				SettingsObject2 setting = new SettingsObject2(strXMLPath + "/" + strElementName, strAttributes);
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

		public override ConfigAttributeBase GetAttribute(String strName)
		{
			return GetAttribute(strName, 0);
		}

		public Attribute GetAttribute(String strName, int nIdx)
		{
			return listSettings[nIdx].GetAttribute(strName);
		}
	}
	#endregion

	class Config2
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

		public Config2()
		{
			settings = new List<Settings>(Enum.GetValues(typeof(SETTING)).Length);

			settings.Add(new SettingsObject("fsxget/settings/options/general/enable-on-startup", "Enabled<bool>"));
			settings.Add(new SettingsObject("fsxget/settings/options/general/show-balloon-tips", "Enabled<bool>"));
			settings.Add(new SettingsObject("fsxget/settings/options/general/load-kml-file", "Enabled<bool>"));
			settings.Add(new SettingsObject("fsxget/settings/options/general/update-check", "Enabled<bool>"));

			settings.Add(new SettingsObject("fsxget/settings/options/fsx/query-user-aircraft", "Enabled<bool>;Interval<int>"));
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
			//Settings obj;

			//settings[(int)SETTING.ENABLE_ON_STARTUP].GetAttribute("Enabled").BoolValue = true;
			//settings[(int)SETTING.SHOW_BALLOON_TIPS].GetAttribute("Enabled").BoolValue = true;
			//settings[(int)SETTING.LOAD_KML_FILE].GetAttribute("Enabled").BoolValue = true;
			//settings[(int)SETTING.UPDATE_CHECK].GetAttribute("Enabled").BoolValue = true;

			//obj = settings[(int)SETTING.QUERY_USER_AIRCRAFT];
			//obj.GetAttribute("Enabled").BoolValue = true;
			//obj.GetAttribute("Interval").IntValue = 1000;

			//obj = settings[(int)SETTING.QUERY_USER_PATH];
			//obj.GetAttribute("Enabled").BoolValue = true;
			//obj.GetAttribute("Interval").IntValue = 5000;

			//obj = settings[(int)SETTING.USER_PATH_PREDICTION];
			//obj.GetAttribute("Enabled").BoolValue = true;
			//obj.GetAttribute("Interval").IntValue = 5000;

			//obj = new SettingsObject("fsxget/settings/options/fsx/user-path-prediction/prediction-point", "Time<int>");
			//obj.GetAttribute("Time").IntValue = 30;
			//((SettingsList)settings[(int)SETTING.PREDITCTION_POINTS]).listSettings.Add((SettingsObject)obj);
			//obj = new SettingsObject("fsxget/settings/options/fsx/user-path-prediction/prediction-point", "Time<int>");
			//obj.GetAttribute("Time").IntValue = 150;
			//((SettingsList)settings[(int)SETTING.PREDITCTION_POINTS]).listSettings.Add((SettingsObject)obj);
			//obj = new SettingsObject("fsxget/settings/options/fsx/user-path-prediction/prediction-point", "Time<int>");
			//obj.GetAttribute("Time").IntValue = 300;
			//((SettingsList)settings[(int)SETTING.PREDITCTION_POINTS]).listSettings.Add((SettingsObject)obj);
			//obj = new SettingsObject("fsxget/settings/options/fsx/user-path-prediction/prediction-point", "Time<int>");
			//obj.GetAttribute("Time").IntValue = 600;
			//((SettingsList)settings[(int)SETTING.PREDITCTION_POINTS]).listSettings.Add((SettingsObject)obj);
			//obj = new SettingsObject("fsxget/settings/options/fsx/user-path-prediction/prediction-point", "Time<int>");
			//obj.GetAttribute("Time").IntValue = 1200;
			//((SettingsList)settings[(int)SETTING.PREDITCTION_POINTS]).listSettings.Add((SettingsObject)obj);

			//settings[(int)SETTING.QUERY_AI_OBJECTS].GetAttribute("Enabled").BoolValue = true;

			//obj = settings[(int)SETTING.QUERY_AI_AIRCRAFTS];
			//obj.GetAttribute("Enabled").BoolValue = true;
			//obj.GetAttribute("Interval").IntValue = 3000;
			//obj.GetAttribute("Range").IntValue = 50000;
			//obj.GetAttribute("Prediction").BoolValue = true;
			//obj.GetAttribute("PredictionPoints").BoolValue = false;

			//obj = settings[(int)SETTING.QUERY_AI_HELICOPTERS];
			//obj.GetAttribute("Enabled").BoolValue = true;
			//obj.GetAttribute("Interval").IntValue = 3000;
			//obj.GetAttribute("Range").IntValue = 50000;
			//obj.GetAttribute("Prediction").BoolValue = true;
			//obj.GetAttribute("PredictionPoints").BoolValue = false;

			//obj = settings[(int)SETTING.QUERY_AI_BOATS];
			//obj.GetAttribute("Enabled").BoolValue = true;
			//obj.GetAttribute("Interval").IntValue = 5000;
			//obj.GetAttribute("Range").IntValue = 100000;
			//obj.GetAttribute("Prediction").BoolValue = false;
			//obj.GetAttribute("PredictionPoints").BoolValue = false;

			//obj = settings[(int)SETTING.QUERY_AI_GROUND_UNITS];
			//obj.GetAttribute("Enabled").BoolValue = true;
			//obj.GetAttribute("Interval").IntValue = 5000;
			//obj.GetAttribute("Range").IntValue = 100000;
			//obj.GetAttribute("Prediction").BoolValue = false;
			//obj.GetAttribute("PredictionPoints").BoolValue = false;

			//settings[(int)SETTING.GE_SERVER_PORT].GetAttribute("Value").IntValue = 8087;
			//settings[(int)SETTING.GE_ACCESS_LEVEL].GetAttribute("Value").IntValue = 1;

			/*
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
*/
			WriteToXML();
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
