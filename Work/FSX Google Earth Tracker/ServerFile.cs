using System;
using System.Collections.Generic;
using System.Text;

namespace Fsxget
{
	/// <summary>
	/// The base class for a file to be registered with the HttpServer class.
	/// </summary>
	/// <remarks>
	/// Defines everything related to content type management which is common to all 
	/// ServerFile subclasses and defines the abstract method getContent() which requires all 
	/// subclasses to inherit this method.</remarks>
	public abstract class ServerFile
	{
		protected String szContenType;

		public ServerFile(String contentType)
		{
			szContenType = contentType;
		}

		public String contentType
		{
			get
			{
				return szContenType;
			}
			set
			{
				szContenType = value;
			}
		}

		public abstract byte[] getContent();
	}
}
