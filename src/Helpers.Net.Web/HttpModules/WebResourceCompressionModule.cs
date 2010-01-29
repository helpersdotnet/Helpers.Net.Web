using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Web;
using System.Web.UI;

namespace Helpers.Net.Web.HttpModules
{
    public class WebResourceCompressionModule : IHttpModule
    {
        public void Dispose() { }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
            context.EndRequest += new EventHandler(context_EndRequest);
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            if (app.Request.Path.Contains("WebResource.axd"))
            {
                SetCachingHeaders(app);

                if (IsBrowserSupported() && app.Context.Request.QueryString["c"] == null &&
                    (WebHelper.AcceptEncoding.IsDeflateEncodingAccepted(app.Context) ||
                    WebHelper.AcceptEncoding.IsGzipEncodingAccepted(app.Context)))
                {
                    app.CompleteRequest();
                }
            }
        }

        void context_EndRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            if (IsBrowserSupported() && app.Context.Request.QueryString["c"] == null &&
                   (!WebHelper.AcceptEncoding.IsDeflateEncodingAccepted(app.Context) &&
                   !WebHelper.AcceptEncoding.IsGzipEncodingAccepted(app.Context)))
            {
                return;
            }

            string key = app.Request.QueryString.ToString();

            if (app.Request.Path.Contains("WebResource.axd") && app.Context.Request.QueryString["c"] == null)
            {
                if (app.Application[key] == null)
                    AddCompressedBytesToCache(app, key);

                WebHelper.AcceptEncoding.SetEncoding((string)app.Application[key + "enc"]);
                app.Context.Response.ContentType = "text/javascript";
                app.Context.Response.BinaryWrite((byte[])app.Application[key]);
            }
        }

        private void AddCompressedBytesToCache(HttpApplication app, string key)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(app.Context.Request.Url.OriginalString + "&c=1");
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                Stream responseStream = response.GetResponseStream();

                using (MemoryStream ms = CompressResponse(responseStream, app, key))
                {
                    app.Application.Add(key, ms.ToArray());
                }
            }
        }

        private static MemoryStream CompressResponse(Stream responseStream, HttpApplication app, string key)
        {
            MemoryStream dataStream = new MemoryStream();
            StreamCopy(responseStream, dataStream);
            responseStream.Dispose();

            byte[] buffer = dataStream.ToArray();
            dataStream.Dispose();

            MemoryStream ms = new MemoryStream();
            Stream compress = null;

            if (WebHelper.AcceptEncoding.IsDeflateEncodingAccepted())
            {
                compress = new DeflateStream(ms, CompressionMode.Compress);
                app.Application.Add(key + "enc", WebHelper.AcceptEncoding.DEFLATE);
            }
            else if (WebHelper.AcceptEncoding.IsGzipEncodingAccepted())
            {
                compress = new GZipStream(ms, CompressionMode.Compress);
                app.Application.Add(key + "enc", WebHelper.AcceptEncoding.GZIP);
            }

            compress.Write(buffer, 0, buffer.Length);
            compress.Dispose();
            return ms;
        }

        private static void StreamCopy(Stream input, Stream output)
        {
            byte[] buffer = new byte[2048];
            int read;
            do
            {
                read = input.Read(buffer, 0, buffer.Length);
                output.Write(buffer, 0, read);
            } while (read > 0);
        }

        private bool IsBrowserSupported()
        {
            HttpContext context = HttpContext.Current;
            return !(context.Request.UserAgent != null && context.Request.UserAgent.Contains("MSIE 6"));
        }

        private void SetCachingHeaders(HttpApplication app)
        {
            string etag = "\"" + app.Context.Request.QueryString.ToString().GetHashCode().ToString() + "\"";
            string incomingTag = app.Request.Headers["If-None-Match"];

            app.Response.Cache.VaryByHeaders["Accept-Encoding"] = true;
            app.Response.Cache.SetExpires(DateTime.Now.AddDays(10));
            app.Response.Cache.SetCacheability(HttpCacheability.Public);
            app.Response.Cache.SetLastModified(DateTime.Now.AddDays(-10));
            app.Response.Cache.SetETag(etag);

            if (string.Compare(incomingTag, etag) == 0)
            {
                app.Response.StatusCode = (int)HttpStatusCode.NotModified;
                app.Response.End();
            }
        }

    }
}
