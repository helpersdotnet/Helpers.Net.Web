
namespace Helpers.Net.Web
{
    public static partial class WebHelper
    {
        public static class HttpRequest
        {
            /// <summary>
            /// Returns true if HttpRequest supports gzip compression
            /// </summary>
            /// <param name="request"><see cref="System.Web.HttpRequest"/> to check.</param>
            /// <returns>Returns true if gzip is supported else false.</returns>
            public static bool IsGzipSupported(System.Web.HttpRequest request)
            {
                if (request == null) return false;

                string acceptEncoding = request.Headers["Accept-Encoding"];

                if (!string.IsNullOrEmpty(acceptEncoding) &&
                    (acceptEncoding.Contains("gzip") || acceptEncoding.Contains("deflate")))
                    return true;
                
                return false;
            }

            /// <summary>
            /// Returns true if the current HttpRequest supports gzip compression
            /// </summary>
            /// <returns>Returns true if gzip is supported else false.</returns>
            public static bool IsGzipSupported()
            {
                return IsGzipSupported(System.Web.HttpContext.Current.Request);
            }
        }
    }
}
