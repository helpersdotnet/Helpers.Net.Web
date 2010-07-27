using System;

namespace Helpers.Net.Web
{
    public static partial class WebHelper
    {
        /// <summary>
        /// Terminates the current application. The application restarts the next time a request is recieved for it.<br/>
        /// Requires either Full Trust or write access to web.config.
        /// </summary>
        /// <returns>True if successfull else false.</returns>
        public static bool RestartWebApplication()
        {
            throw new NotImplementedException();
        }
    }
}
