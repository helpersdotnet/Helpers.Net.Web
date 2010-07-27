namespace Helpers.Net.Web
{
    using System.Web;

    public static partial class WebHelper
    {
        public static class AcceptEncoding
        {
            public const string GZIP = "gzip";
            public const string DEFLATE = "deflate";

            public static bool IsEncodingAccepted(HttpContext context, string encoding)
            {
                return context.Request.Headers["Accept-encoding"] != null && context.Request.Headers["Accept-encoding"].Contains(encoding);
            }

            public static bool IsEncodingAccepted(string encoding)
            {
                return IsEncodingAccepted(HttpContext.Current, encoding);
            }

            public static bool IsDeflateEncodingAccepted(HttpContext context)
            {
                return IsEncodingAccepted(context, DEFLATE) || IsEncodingAccepted(context, "*");
            }

            public static bool IsDeflateEncodingAccepted()
            {
                return IsDeflateEncodingAccepted(HttpContext.Current);
            }

            public static bool IsGzipEncodingAccepted(HttpContext context)
            {
                return IsEncodingAccepted(context, GZIP) || IsEncodingAccepted(context, "*");
            }

            public static bool IsGzipEncodingAccepted()
            {
                return IsGzipEncodingAccepted(HttpContext.Current);
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
