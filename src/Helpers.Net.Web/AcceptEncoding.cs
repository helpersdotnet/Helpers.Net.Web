namespace Helpers.Net.Web
{
    using System.Web;
    using System.IO.Compression;

    public static partial class WebHelper
    {
        public static class AcceptEncoding
        {
            public const string GZIP = "gzip";
            public const string DEFLATE = "deflate";

            public static bool SupportsEncoding(HttpContext context, string encoding)
            {
                return context.Request.Headers["Accept-encoding"] != null && context.Request.Headers["Accept-encoding"].Contains(encoding);
            }

            public static bool SupportsEncoding(string encoding)
            {
                return SupportsEncoding(HttpContext.Current, encoding);
            }

            public static bool SupportsDeflateEncoding(HttpContext context)
            {
                return SupportsEncoding(context, DEFLATE) || SupportsEncoding(context, "*");
            }

            public static bool SupportsDeflateEncoding()
            {
                return SupportsDeflateEncoding(HttpContext.Current);
            }

            public static bool SupportsGzipEncoding(HttpContext context)
            {
                return SupportsEncoding(context, GZIP) || SupportsEncoding(context, "*");
            }

            public static bool SupportsGzipEncoding()
            {
                return SupportsGzipEncoding(HttpContext.Current);
            }

            public static void SetEncoding(HttpContext context, string encoding)
            {
                context.Response.AppendHeader("Content-Encoding", encoding);
            }

            public static void SetEncoding(string encoding)
            {
                SetEncoding(HttpContext.Current, encoding);
            }

            public static void SetGZipEncoding(HttpContext context)
            {
                SetEncoding(context, GZIP);
            }

            public static void SetGZipEncoding()
            {
                SetGZipEncoding(HttpContext.Current);
            }

            public static void SetDeflateEncoding(HttpContext context)
            {
                SetEncoding(context, DEFLATE);
            }

            public static void SetDeflateEncoding()
            {
                SetDeflateEncoding(HttpContext.Current);
            }

            /// <summary>
            /// Automatically tries to compress if it supports compression. deflate then gzip.
            /// </summary>
            /// <param name="context">
            /// The context.
            /// </param>
            public static void TryCompress(HttpContext context)
            {
                if (context != null)
                {
                    if (SupportsDeflateEncoding(context))
                    {
                        context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress);
                        SetDeflateEncoding(context);
                    }
                    else if (SupportsGzipEncoding(context))
                    {
                        context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
                        SetGZipEncoding(context);
                    }
                }
            }
        }
    }
}
