using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Web;
using System.Data.OleDb;
using System.Drawing;

namespace Fsxget
{
	public class KmlFactory
    {
        #region Variables
        private FsxConnection fsxCon;
        private HttpServer httpServer;
        private List<KmlFile> lstKmlFiles;
		private const double dPI180 = Math.PI / 180;
		private const double d180PI = 180 / Math.PI;
		#endregion

		public KmlFactory(ref FsxConnection fsxCon, ref HttpServer httpServer)
		{
            this.fsxCon = fsxCon;
            this.httpServer = httpServer;

            lstKmlFiles = new List<KmlFile>();
            lstKmlFiles.Add(new KmlFileObjects(ref httpServer));
            lstKmlFiles.Add( new KmlFileUserPosition(ref fsxCon, ref httpServer));
            lstKmlFiles.Add(new KmlFileUserPathPrediction(ref fsxCon, ref httpServer));
            lstKmlFiles.Add(new KmlFileUserPath(ref fsxCon, ref httpServer));
            lstKmlFiles.Add(new KmlFileAIObject(ref fsxCon, ref httpServer, "fsxaipu.kml", 
                                                FsxConnection.OBJCONTAINER.AI_PLANE, 
                                                Config.SETTING.QUERY_AI_AIRCRAFTS, 
                                                "aia", "fsxau", 
                                                KmlFileFsx.KML_ICON_TYPES.AI_AIRCRAFT, 
                                                KmlFileFsx.KML_ICON_TYPES.AI_AIRCRAFT_PREDICTION_POINT, 
                                                "9fd20091" ));
            lstKmlFiles.Add(new KmlFileAIObject(ref fsxCon, ref httpServer, "fsxaihu.kml",
                                                FsxConnection.OBJCONTAINER.AI_HELICOPTER,
                                                Config.SETTING.QUERY_AI_HELICOPTERS,
                                                "aih", "fsxhu",
                                                KmlFileFsx.KML_ICON_TYPES.AI_HELICOPTER,
                                                KmlFileFsx.KML_ICON_TYPES.AI_HELICOPTER_PREDICTION_POINT,
                                                "9fd20091"));
            lstKmlFiles.Add(new KmlFileAIObject(ref fsxCon, ref httpServer, "fsxaibu.kml",
                                                FsxConnection.OBJCONTAINER.AI_BOAT,
                                                Config.SETTING.QUERY_AI_BOATS,
                                                "aib", "fsxbu",
                                                KmlFileFsx.KML_ICON_TYPES.AI_BOAT,
                                                KmlFileFsx.KML_ICON_TYPES.AI_BOAT_PREDICTION_POINT,
                                                "9fd20091"));
            lstKmlFiles.Add(new KmlFileAIObject(ref fsxCon, ref httpServer, "fsxaigu.kml",
                                                FsxConnection.OBJCONTAINER.AI_GROUND,
                                                Config.SETTING.QUERY_AI_GROUND_UNITS,
                                                "aig", "fsxgu",
                                                KmlFileFsx.KML_ICON_TYPES.AI_GROUND_UNIT,
                                                KmlFileFsx.KML_ICON_TYPES.AI_GROUND_PREDICTION_POINT,
                                                "9fd20091"));
            lstKmlFiles.Add(new KmlFileNavaid(ref fsxCon, ref httpServer));
            lstKmlFiles.Add(new KmlFileAirport(ref fsxCon, ref httpServer));
            lstKmlFiles.Add(new KmlFileFlightPlan(ref fsxCon, ref httpServer));


			// TODO: Hardcoded paths or parts of it should be avoided. Maybe we 
			// should put the GE icons in the resource file

			String[] strFiles = Directory.GetFiles(App.Config.AppPath + "\\pub\\gfx\\ge\\icons");
			int nIdx = App.Config.AppPath.Length + 4;
			foreach (String strFile in strFiles)
				httpServer.registerFile(strFile.Substring(nIdx).Replace('\\', '/'), new ServerFileDisc("image/png", strFile));

			SettingsList lstImg = (SettingsList)App.Config[Config.SETTING.AIR_IMG_LIST];
			foreach (SettingsObject img in lstImg.listSettings)
				httpServer.registerFile("/gfx/" + img["Name"].StringValue + ".png", new ServerFileDisc("image/png", App.Config.FilePathPub + img["Img"].StringValue));

			lstImg = (SettingsList)App.Config[Config.SETTING.WATER_IMG_LIST];
			foreach (SettingsObject img in lstImg.listSettings)
				httpServer.registerFile("/gfx/" + img["Name"].StringValue + ".png", new ServerFileDisc("image/png", App.Config.FilePathPub + img["Img"].StringValue));

			lstImg = (SettingsList)App.Config[Config.SETTING.GROUND_IMG_LIST];
			foreach (SettingsObject img in lstImg.listSettings)
				httpServer.registerFile("/gfx/" + img["Name"].StringValue + ".png", new ServerFileDisc("image/png", App.Config.FilePathPub + img["Img"].StringValue));

            Stream s = Assembly.GetCallingAssembly().GetManifestResourceStream("FSXGET.pub.gfx.logo.png");
			byte[] bTemp = new byte[s.Length];
			s.Read(bTemp, 0, (int)s.Length);
			httpServer.registerFile("/gfx/logo.png", new ServerFileCached("image/png", bTemp));

			// TODO: The no image functionality is still missing due to migrating to the 
			// new HTTP server class. We should consider to drop it anyway and instead, include 
			// object images in the KML files only if they really exist.

            s = Assembly.GetCallingAssembly().GetManifestResourceStream("FSXGET.pub.gfx.noimage.png");
			bTemp = new byte[s.Length];
			s.Read(bTemp, 0, (int)s.Length);
			httpServer.registerFile("/gfx/noimage.png", new ServerFileCached("image/png", bTemp));

			// Register other documents with the HTTP server
			httpServer.registerFile("/setfreq.html", new ServerFileDynamic("text/html", GenSetFreqHtml));
            httpServer.registerFile("/goto.html", new ServerFileDynamic("text/html", GenGotoHtml));
        }

		~KmlFactory()
		{
		}

		public void CreateStartupKML(String strFile)
		{
			String strKML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><kml xmlns=\"http://earth.google.com/kml/2.1\"><NetworkLink>";
			strKML += "<name>" + App.Config.AssemblyTitle + "</name>";
			strKML += "<Link><href>" + App.Config.Server + "/fsxobjs.kml</href></Link></NetworkLink></kml>";
			File.WriteAllText(strFile, strKML, Encoding.UTF8);
		}

		public byte[] GenSetFreqHtml(System.Collections.Specialized.NameValueCollection values)
		{
            bool bError = true;
            try
            {
                bError = !fsxCon.SetFrequency(values["type"], float.Parse(values["freq"], System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            catch
            {
            }

			if (bError)
				return encodeDefault("<html><body>" + System.Web.HttpUtility.HtmlEncode("An error occured while trying to change the frequency! The Frequency has not been changed.") + "</body></html>");
			else
				return encodeDefault("<html><body>" + System.Web.HttpUtility.HtmlEncode("The frequency change has been successfully initiated.") + "</body></html>");
		}

        public byte[] GenGotoHtml(System.Collections.Specialized.NameValueCollection values)
        {
            bool bError = true;
            try
            {
                bError = !fsxCon.Goto(float.Parse(values["lon"], System.Globalization.NumberFormatInfo.InvariantInfo),
                                      float.Parse(values["lat"], System.Globalization.NumberFormatInfo.InvariantInfo),
                                      float.Parse(values["alt"], System.Globalization.NumberFormatInfo.InvariantInfo),
                                      float.Parse(values["head"], System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            catch
            {
                bError = true;
            }
            if (bError)
                return encodeDefault("<html><body>" + System.Web.HttpUtility.HtmlEncode("Position not set") + "</body></html>");
            else
                return encodeDefault("<html><body>" + System.Web.HttpUtility.HtmlEncode("Position set") + "</body></html>");
        }

		protected byte[] encodeDefault(String data)
		{
			return System.Text.Encoding.UTF8.GetBytes(data);
		}

        public static void MovePoint(float fLongitude, float fLatitude, float fHeading, float fDistMeter, ref float fLonResult, ref float fLatResult)
		{
			double dDistMeter = (Math.PI / 10800) * fDistMeter / 1852;
			double dHeading = fHeading * dPI180;
			double dLatitude = fLatitude * dPI180;
			double dLongitude = fLongitude * dPI180;
			double dDistSin = Math.Sin(dDistMeter);
			double dDistCos = Math.Cos(dDistMeter);
			double dLatSin = Math.Sin(dLatitude);
			double dLatCos = Math.Cos(dLatitude);

			double dLatResult = Math.Asin(dLatSin * dDistCos + dLatCos * dDistSin * Math.Cos(dHeading));
			double d = -1 * (Math.Atan2(Math.Sin(dHeading) * dDistSin * dLatCos, dDistCos - dLatSin * Math.Sin(dLatResult)));
			double dLonResult = (dLongitude - d + Math.PI) - (long)((dLongitude - d + Math.PI) / 2 / Math.PI) - Math.PI;

			fLatResult = (float)(dLatResult * d180PI);
			fLonResult = (float)(dLonResult * d180PI);
		}
		public static void GetDistance(float fLon1, float fLat1, float fLon2, float fLat2, ref float fDistMeter, ref float fHeading)
		{
			double dLon1 = fLon1 * dPI180;
			double dLat1 = fLat1 * dPI180;
			double dLon2 = fLon2 * dPI180;
			double dLat2 = fLat2 * dPI180;

			double dDist = Math.Acos(Math.Sin(dLat1) * Math.Sin(dLat2) + Math.Cos(dLat1) * Math.Cos(dLat2) * Math.Cos(dLon2 - dLon1));
			fHeading = (float)(Math.Acos((Math.Sin(dLat2) - Math.Cos(dDist) * Math.Sin(dLat1)) / Math.Cos(dLat1) / Math.Sin(dDist)) * d180PI);
			if (dLon2 - dLon1 < 0)
				fHeading = 360 - fHeading;
			fDistMeter = (float)((dDist * d180PI) * 60000);
            fDistMeter *= 1.852f;
		}
    }
}
