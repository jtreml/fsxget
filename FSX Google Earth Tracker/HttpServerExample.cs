using System;
using System.Collections.Generic;
using System.Text;

namespace Fsxget
{
	class HttpServerExample
	{
		public HttpServerExample()
		{
			HttpServer server = new HttpServer(3);


			// Example for static files (loaded from disc each time when requested) (for example main kml file)
			server.registerFile("/main.kml", new ServerFileDisc("application/vnd.google-earth.kml+xml", "C:\test.kml"));


			// Example for cached files (loaded from disc and residing in memory) (specified by path)
			server.registerFile("/logo.png", new ServerFileCached("image/png", "C:\test.png"));


			// Example for cached files (passed as a byte array and residing in memory)
			String szTest = "<html><body>Hello!</body></html>";
			server.registerFile("/test.html", new ServerFileCached("text/html", System.Text.Encoding.UTF8.GetBytes(szTest)));


			// Example for dynamic content (kml update files)
			ServerFileDynamicDelegate myDelegate = someUpdateFunction;
			server.registerFile("/update.kml", new ServerFileDynamic("application/vnd.google-earth.kml+xml", myDelegate));
		}

		protected byte[] someUpdateFunction(String query)
		{
			String szTest = "Here goes a lot of update stuff ....";

			return System.Text.Encoding.UTF8.GetBytes(szTest);
		}
	}
}
