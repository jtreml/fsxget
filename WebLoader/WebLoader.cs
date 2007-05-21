using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Web;

namespace WebLoader
{
	public class WebLoader
	{
		public enum Method
		{
			GET,
			POST
		}

		protected const String szUserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.8.1.2) Gecko/20070208 Firefox/2.0.0.2";

		public WebLoader()
		{
		}

		public WebLoaderResultTXT getRawData(String urlString)
		{
			return getRawData(urlString, null, Method.GET);
		}

		public WebLoaderResultTXT getRawData(String urlString, List<PostEntry> postData, Method method)
		{
			// Create post / get data string
			String szPostString = "";
			if (postData != null && postData.Count > 0)
			{


				int iCount = 0;
				foreach (PostEntry peLoop in postData)
				{
					iCount++;

					if (iCount > 1)
						szPostString += "&";

					szPostString += HttpUtility.UrlEncode(peLoop.fieldName) + "=" + HttpUtility.UrlEncode(peLoop.fieldValue);
				}
			}

			HttpWebRequest hwrThis;

			if (method == Method.GET && szPostString != "")
				hwrThis = (HttpWebRequest)WebRequest.Create(urlString + "?" + szPostString);
			else
				hwrThis = (HttpWebRequest)WebRequest.Create(urlString);

			hwrThis.AllowAutoRedirect = true;
			hwrThis.UserAgent = szUserAgent;

			Encoding encDefault = System.Text.Encoding.GetEncoding(1252);


			if (method == Method.POST && szPostString != "")
			{
				hwrThis.Method = "POST";
				hwrThis.ContentType = "application/x-www-form-urlencoded";

				byte[] byPostBuffer = encDefault.GetBytes(szPostString);
				hwrThis.ContentLength = byPostBuffer.Length;

				Stream strPostData = hwrThis.GetRequestStream();
				strPostData.Write(byPostBuffer, 0, byPostBuffer.Length);
				strPostData.Close();
			}
			else
				hwrThis.Method = "GET";

			try
			{
				HttpWebResponse hwreThis = (HttpWebResponse)hwrThis.GetResponse();
				StreamReader strrResponse = new StreamReader(hwreThis.GetResponseStream(), encDefault);

				String szResult = strrResponse.ReadToEnd();

				hwreThis.Close();
				strrResponse.Close();

				return new WebLoaderResultTXT(false, szResult);
			}
			catch
			{
				return new WebLoaderResultTXT(true, "");
			}
		}

		public WebLoaderResultHTML getHtmlData(String urlString)
		{
			return getHtmlData(urlString, null, Method.GET);
		}

		public WebLoaderResultHTML getHtmlData(String urlString, List<PostEntry> postData, Method method)
		{
			WebLoaderResultTXT wlrt = getRawData(urlString, postData, method);

			if (wlrt.errorOccured)
				return new WebLoaderResultHTML(true, null);
			else
			{
				HtmlDocument htmldTemp = new HtmlDocument();
				htmldTemp.LoadHtml(wlrt.getData());
				return new WebLoaderResultHTML(false, htmldTemp);
			}
		}

		public WebLoaderResultXML getXmlData(String urlString)
		{
			return getXmlData(urlString, null, Method.GET);
		}

		public WebLoaderResultXML getXmlData(String urlString, List<PostEntry> postData, Method method)
		{
			WebLoaderResultHTML wlrh = getHtmlData(urlString, postData, method);

			if (wlrh.errorOccured)
				return new WebLoaderResultXML(true, null);
			else
			{
				HtmlDocument htmldTemp = wlrh.getData();
				htmldTemp.OptionOutputAsXml = true;

				StringWriter swTemp = new StringWriter();
				htmldTemp.Save(swTemp);
				swTemp.Flush();
				swTemp.Close();

				XmlDocument xmldTemp = new XmlDocument();

				xmldTemp.LoadXml(swTemp.ToString());

				return new WebLoaderResultXML(false, xmldTemp);
			}
		}
	}

	public struct PostEntry
	{
		public PostEntry(String fieldName, String fieldValue)
		{
			this.fieldName = fieldName;
			this.fieldValue = fieldValue;
		}

		public String fieldName;

		public String fieldValue;
	}

	public abstract class WebLoaderResult
	{
		bool bErrorOccured = false;


		internal void setErrorOccured(bool bErrorOccured)
		{
			this.bErrorOccured = bErrorOccured;
		}

		public bool errorOccured
		{
			get
			{
				return bErrorOccured;
			}
		}
	}


	public class WebLoaderResultXML : WebLoaderResult
	{
		XmlDocument xmldData;


		public WebLoaderResultXML(bool bErrorOccured, XmlDocument xmldData)
		{
			setData(xmldData);
			setErrorOccured(bErrorOccured);
		}

		internal void setData(XmlDocument xmldData)
		{
			this.xmldData = xmldData;
		}

		public XmlDocument getData()
		{
			return xmldData;
		}
	}

	public class WebLoaderResultHTML : WebLoaderResult
	{
		HtmlDocument htmldData;


		public WebLoaderResultHTML(bool bErrorOccured, HtmlDocument htmldData)
		{
			setData(htmldData);
			setErrorOccured(bErrorOccured);
		}

		internal void setData(HtmlDocument htmldData)
		{
			this.htmldData = htmldData;
		}

		public HtmlDocument getData()
		{
			return htmldData;
		}
	}

	public class WebLoaderResultTXT : WebLoaderResult
	{
		String szData;


		public WebLoaderResultTXT(bool bErrorOccured, String szData)
		{
			setData(szData);
			setErrorOccured(bErrorOccured);
		}

		internal void setData(String szData)
		{
			this.szData = szData;
		}

		public String getData()
		{
			return szData;
		}
	}
}
