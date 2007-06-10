using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Fsxget
{
	/// <summary>
	/// The most simple ServerFile type to register with a HttpServer. The file 
	/// directly contains the data to be served, i.e. it will already be in memory 
	/// when requested through the server.
	/// </summary>
	/// <remarks>This type is meant for static documents (i.e. with a rarely changing 
	/// content) that are requested quite often thus that you can't afford retrieving 
	/// them from the hard disc each time they are requested. For the latter type please 
	/// see ServerFileDisc and for extremely dynamic content, please have a look at 
	/// ServerFileDynamic.</remarks>
	class ServerFileCached : ServerFile
	{
		byte[] data;

		public ServerFileCached(String contentType, String path)
			: base(contentType)
		{
			try
			{
				data = File.ReadAllBytes(path);
			}
			catch
			{
				data = null;
				throw new Exception("File load exception!");
			}
		}

		public ServerFileCached(String contentType, byte[] data)
			: base(contentType)
		{
			this.data = data;
		}

		public byte[] Data
		{
			get
			{
				return data;
			}

			set
			{
				data = value;
			}
		}

		protected override byte[] getContent(System.Collections.Specialized.NameValueCollection values)
		{
			return data;
		}
	}
}
