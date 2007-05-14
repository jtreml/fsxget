using System;
using System.Collections.Generic;
using System.Text;

namespace Fsxget
{
	public delegate byte[] ServerFileDynamicDelegate(String query);

	/// <summary>
	/// A ServerFile type for extremely dynamic content.
	/// </summary>
	/// <remarks>This type is meant for documents that change practically each time 
	/// they are requested. A callback function will be registered in the document 
	/// which will be called each time the server requests the document. Thus it can 
	/// be updated on demand. See ServerFileDisc or ServerFileCached for more static
	/// ways to server documents.</remarks>
	class ServerFileDynamic : ServerFile
	{
		ServerFileDynamicDelegate dataDelegate;

		public ServerFileDynamic(String contentType, ServerFileDynamicDelegate dataDelegate)
			: base(contentType)
		{
			this.dataDelegate = dataDelegate;
		}

		public ServerFileDynamicDelegate Delegate
		{
			get
			{
				return dataDelegate;
			}
			set
			{
				dataDelegate = value;
			}
		}

		protected override byte[] getContent(String query)
		{
			if (dataDelegate == null)
				return null;

			return dataDelegate.Invoke(query);
		}
	}
}
