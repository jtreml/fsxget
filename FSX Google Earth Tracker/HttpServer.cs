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
	class HttpServer
	{
		// TODO: Add code to set prefixes somewhere

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

		public void registerFile(String path, ServerFile file)
		{
			documents.Add(path, file);
		}

		public ServerFile getFile(String path)
		{
			return (ServerFile)documents[path];
		}

		public void unregisterFile(String path)
		{
			documents.Remove(path);
		}

		public void start()
		{
			if (isRunning)
				return;

			if (listener == null)
				listener = new HttpListener();

			//listener.Prefixes.Add(UrlBase);

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

			String szUrl = request.Url.PathAndQuery;
			ServerFile file = (ServerFile)documents[szUrl];

			if (file != null)
			{
				response.AddHeader("Content-type", file.contentType);

				byte[] buffer = file.getContent();
				response.ContentLength64 = buffer.Length;

				System.IO.Stream output = response.OutputStream;
				output.Write(buffer, 0, buffer.Length);
				output.Close();
			}
			else
			{
				// TODO: Http 404, File not found!
			}
		}
	}
}
