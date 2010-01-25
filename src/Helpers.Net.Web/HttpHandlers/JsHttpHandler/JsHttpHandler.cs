using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web;

namespace Helpers.Net.Web.HttpHandlers
{
    public class JsHttpHandler : IHttpHandler
    {
        private readonly static TimeSpan CACHE_DURATION = TimeSpan.FromDays(30);

        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext context)
        {

            HttpRequest request = context.Request;

            // read setname,version and compression, purgecache
            // dont define both setname and file
            // will omit checking to reduce number of if else
            // compression is checked only if filename is especified
            // by default filename is compressed and has higher preference over setname.
            // to specify multiple files use comma seperated values
            string setName = request["s"] ?? string.Empty;
            string version = request["v"] ?? string.Empty;
            string purgecache = request["purgecache"] ?? string.Empty; //anyvalue is fine

            bool isPurgeCache = !string.IsNullOrEmpty(purgecache);
            if (isPurgeCache)
                PurgeAllCache(context);

            string[] files = (request["f"] ?? string.Empty).Split(
                new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(setName) && files.Length > 0)
            { // if both setname and files are declared its an error.
                throw new HttpException(404, "Both setname and file list cannot be specified.");
            }

            // Decide if browser supports compressed response
            bool isCompressed = false;

            // Response is written as UTF8 encoding. If you are using languages like
            // Arabic, you should change this to proper encoding 
            UTF8Encoding encoding = new UTF8Encoding(false);
            if (files.Length > 0)
            {
                #region Check whether to compress or not
                int compress = 1;
                if (!int.TryParse(request["c"], out compress))
                    compress = 1;
                if (compress != 0)
                    isCompressed = true;
                #endregion

                if (isCompressed)
                {
                    // check if browser really supports compression
                    isCompressed = CanGZip(context.Request);
                }

                setName = GenerateSetName(files, context);

                // If the set has already been cached, write the response directly from
                // cache. Otherwise generate the response and cache it
                if (!this.WriteFromCache(context, setName, version, isCompressed, setName.GetHashCode()))
                {
                    using (MemoryStream memoryStream = new MemoryStream(5000))
                    {
                        // Decide regular stream or GZipStream based on whether the response
                        // can be cached or not
                        using (Stream writer = isCompressed ?
                            (Stream)(new GZipStream(memoryStream, CompressionMode.Compress)) :
                            memoryStream)
                        {
                            foreach (string fileName in files)
                            {
                                if (!fileName.EndsWith(".js")) //security check so only .js files are loaded
                                {
                                    throw new HttpException(403, "403 Forbidden");
                                }

                                byte[] fileBytes = this.GetFileBytes(context, fileName.Trim(), encoding);
                                writer.Write(fileBytes, 0, fileBytes.Length);
                            }

                            writer.Close();
                        }

                        // Cache the combined response so that it can be directly written
                        // in subsequent calls 
                        byte[] responseBytes = memoryStream.ToArray();
                        context.Cache.Insert(GetCacheKey(setName, version, isCompressed),
                            responseBytes, null, System.Web.Caching.Cache.NoAbsoluteExpiration,
                            CACHE_DURATION);

                        // Generate the response
                        this.WriteBytes(responseBytes, context, isCompressed, setName.GetHashCode(), isPurgeCache);
                    }
                }
            }
            else // if using setname append _ by default
            {
                setName = "_" + setName;
            }
        }

        private string GenerateSetName(string[] files, HttpContext context)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string file in files)
            {
                if (file.StartsWith("http://") || file.StartsWith("https://"))
                {
                    sb.Append(file);
                }
                else
                {
                    string physicalPath = context.Server.MapPath(file);
                    if (!File.Exists(physicalPath))
                        throw new HttpException(404, "Javascript not found.");
                    sb.Append(physicalPath);
                }
            }
            return sb.ToString();
        }

        private byte[] GetFileBytes(HttpContext context, string virtualPath, Encoding encoding)
        {
            if (virtualPath.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                using (WebClient client = new WebClient())
                {
                    return client.DownloadData(virtualPath);
                }
            }
            else
            {
                string physicalPath = context.Server.MapPath(virtualPath);
                if (!File.Exists(physicalPath))
                    throw new HttpException(404, "Javascript not found.");
                byte[] bytes = File.ReadAllBytes(physicalPath);
                // TODO: Convert unicode files to specified encoding. For now, assuming
                // files are either ASCII or UTF8
                return bytes;
            }
        }

        private bool WriteFromCache(HttpContext context, string setName, string version,
            bool isCompressed, int hashCode)
        {
            byte[] responseBytes = context.Cache[GetCacheKey(setName, version, isCompressed)] as byte[];

            if (null == responseBytes || 0 == responseBytes.Length) return false;

            this.WriteBytes(responseBytes, context, isCompressed, hashCode, false);
            return true;
        }

        private void WriteBytes(byte[] bytes, HttpContext context, bool isCompressed, int hashCode, bool isPurgeCache)
        {
            HttpResponse response = context.Response;

            response.AppendHeader("Content-Length", bytes.Length.ToString());
            response.ContentType = "text/javascript";
            if (isCompressed)
                response.AppendHeader("Content-Encoding", "gzip");

            context.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.Now.Add(CACHE_DURATION));
            context.Response.Cache.SetMaxAge(CACHE_DURATION);

            string etag = "\"" + hashCode.ToString() + "\"";
            string incomingEtag = context.Request.Headers["If-None-Match"];

            context.Response.Cache.SetETag(etag);

            if (!isPurgeCache)
            {
                if (incomingEtag == etag)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                    context.Response.SuppressContent = true;
                }
            }

            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.Flush();
        }

        private bool CanGZip(HttpRequest request)
        {
            string acceptEncoding = request.Headers["Accept-Encoding"];
            if (!string.IsNullOrEmpty(acceptEncoding) &&
                 (acceptEncoding.Contains("gzip") || acceptEncoding.Contains("deflate")))
                return true;
            return false;
        }

        private string GetCacheKey(string setName, string version, bool isCompressed)
        {
            return "Helpers.Net.Js." + setName + "." + version + "." + isCompressed;
        }

        public static void PurgeAllCache(HttpContext context)
        {
            WebHelper.PurgeCacheItems(context, "Helpers.Net.Js.");
        }

    }
}
