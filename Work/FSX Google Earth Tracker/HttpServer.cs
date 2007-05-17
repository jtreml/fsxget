using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Net;

namespace Fsxget
{
	/// <summary>
	/// HTTP server with which other obejcts can register documents to be served. Therfore a 
	/// document path at which the document should reside as well as a ServerFile document.
	/// </summary>
	/// <remarks>The server keeps a list of all documents registered and when requested, calls 
	/// the document's ServerFile.getContent() method to retrieve the data. Depending on the 
	/// document type, this data may already be in memory or be retrieved from the disc just in 
	/// time.</remarks>
	public class HttpServer
	{
		// TODO: The code for restricting the HTTP server to localhost, etc. is still missing
		private class ServerFileInternal
		{
			public ServerFileInternal(String path, ServerFile file)
			{
				Path = path;
				File = file;
				Persistent = true;
			}

			public ServerFileInternal(String path, ServerFile file, bool persistent)
			{
				Path = path;
				File = file;
				Persistent = persistent;
			}

			public ServerFile File;
			public bool Persistent;
			public String Path;
		}

		Hashtable documents;

		protected HttpListener listener = new HttpListener();
		protected bool isRunning = false;

		public HttpServer()
		{
			documents = new Hashtable();
		}

		public HttpServer(int capacity)
		{
			documents = new Hashtable(capacity);
		}

		public bool registerFile(String path, ServerFile file)
		{
			lock (documents)
			{
                try
                {
                    documents.Add(path, new ServerFileInternal(path, file));
                }
                catch
                {
                    return false;
                }
			}
            return true;
			//Console.WriteLine(path);
		}

        public bool registerOneTimeFile(String path, ServerFile file)
		{
			lock (documents)
			{
                try
                {
                    documents.Add(path, new ServerFileInternal(path, file, false));
                }
                catch
                {
                    return false;
                }
			}
            return true;
			//Console.WriteLine(path);
		}

		public ServerFile getFile(String path)
		{
			lock (documents)
			{
				return (ServerFile)documents[path];
			}
		}

		public void unregisterFile(String path)
		{
			lock (documents)
			{
				documents.Remove(path);
			}
		}

		public void start()
		{
			if (isRunning)
				return;

			if (listener == null)
				listener = new HttpListener();

			isRunning = true;
			listener.Start();

			IAsyncResult result = listener.BeginGetContext(new AsyncCallback(WebRequestCallback), listener);
		}

		public void stop()
		{
			if (listener != null)
			{
				listener.Close();
				listener = null;

				isRunning = false;
			}
		}

		protected void WebRequestCallback(IAsyncResult result)
		{
			if (listener == null)
				return;

			HttpListenerContext context = listener.EndGetContext(result);

			listener.BeginGetContext(new AsyncCallback(WebRequestCallback), listener);

			processRequest(context);
		}

		protected void processRequest(HttpListenerContext context)
		{
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;

			String szUrl = request.Url.LocalPath;

			ServerFileInternal file;
			lock (documents)
			{
				file = (ServerFileInternal)documents[szUrl];
			}

			if (file != null)
			{
				String szQuery = request.Url.Query;

				byte[] buffer = file.File.getContentBytes(szQuery);

				if (buffer != null)
				{
					response.ContentLength64 = buffer.Length;

					response.AddHeader("Content-type", file.File.ContentType);

					System.IO.Stream output = response.OutputStream;
					output.Write(buffer, 0, buffer.Length);
					output.Close();

					if (!file.Persistent)
						unregisterFile(file.Path);
				}
				else
				{
					// TODO: To improve usability and human readability, add content to the 
					// repsonse representing an error page stating that there has been a 500 error

					response.StatusCode = 500;
					response.StatusDescription = "Internal Server Error";
					response.Close();
				}
			}
			else
			{
				// TODO: To improve usability and human readability, add content to the 
				// repsonse representing an error page stating that there has been a 404 error

				response.StatusCode = 404;
				response.StatusDescription = "Not Found";
				response.Close();
			}
		}

		public void addPrefix(String prefix)
		{
			listener.Prefixes.Add(prefix);
		}

		public void removePrefix(String prefix)
		{
			listener.Prefixes.Remove(prefix);
		}

		public bool fileExists(String url)
		{
			lock (documents)
			{
				return documents.Contains(url);
			}
		}
	}
}
