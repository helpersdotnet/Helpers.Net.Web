namespace Helpers.Net.Web
{
    using System.Web;

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
        }
    }
}
