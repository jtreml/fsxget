using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Fsxget
{
	/// <summary>
	/// A ServerFile type to register with a HttpServer for files that are requested rarely. The file 
	/// will be read from the hard disc each time it is requested through the server.
	/// </summary>
	/// <remarks>This type is meant for static documents that are requested only every now an then. 
	/// To minimize memory consumption they will be loaded from the hard disc every time they are needed.
	/// See ServerFileCached for basic memory cached documents and ServerFileDynamic for constantly changing
	/// content.</remarks>
	class ServerFileDisc : ServerFile
	{
		String szPath;

		public ServerFileDisc(String contentType, String path)
			: base(contentType)
		{
			szPath = path;
		}

		public String Path
		{
			get
			{
				return szPath;
			}
			set
			{
				szPath = value;
			}
		}

		protected override byte[] getContent(String query)
		{
			try
			{
				return File.ReadAllBytes(szPath);
			}
			catch
			{
				return null;
			}
		}
	}
}
