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
            context.PreRequestHandlerExecute += context_PreRequestHandlerExecute;
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

            Stream uncompressedStream = app.Response.Filter;

            if (WebHelper.AcceptEncoding.SupportsDeflateEncoding(app.Context))
            {
                app.Response.Filter = new DeflateStream(uncompressedStream, CompressionMode.Compress);
                WebHelper.AcceptEncoding.SetDeflateEncoding(app.Context);
            }
            else if (WebHelper.AcceptEncoding.SupportsGzipEncoding(app.Context))
            {
                app.Response.Filter = new GZipStream(uncompressedStream, CompressionMode.Compress);
                WebHelper.AcceptEncoding.SetGZipEncoding(app.Context);
            }
        }
    }
}
