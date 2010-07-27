namespace Helpers.Net.Web.HttpModules
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Web;
    using System.Web.UI;

    public class PageCompressionModule : IHttpModule
    {
        public void Dispose() { }

        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += new EventHandler(context_PreRequestHandlerExecute);
        }

        void context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            if (!(app.Context.CurrentHandler is Page ||
                app.Context.CurrentHandler.GetType().Name == "SyncSessionlessHandler") ||
                app.Request["HTTP_X_MICROSOFTAJAX"] != null)
            {
                return;
            }

            string acceptEncoding = app.Request.Headers["Accept-Encoding"] ?? string.Empty;
            Stream uncompressedStream = app.Response.Filter;

            acceptEncoding = acceptEncoding.ToLower();

            if (acceptEncoding.Contains("deflate") || acceptEncoding == "*")
            {
                app.Response.Filter = new DeflateStream(uncompressedStream, CompressionMode.Compress);
                app.Response.AppendHeader("Content-Encoding", "deflate");
            }
            else if (acceptEncoding.Contains("gzip"))
            {
                app.Response.Filter = new GZipStream(uncompressedStream, CompressionMode.Compress);
                app.Response.AppendHeader("Content-Encoding", "gzip");
            }
        }
    }
}
