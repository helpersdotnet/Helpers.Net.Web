namespace Helpers.Net.Web.HttpModules
{
    using System;
    using System.Web;
    using System.Web.UI;

    public class PageCompressionModule : IHttpModule
    {

        void context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            if (!(app.Context.CurrentHandler is Page ||
                app.Context.CurrentHandler.GetType().Name == "SyncSessionlessHandler") ||
                app.Request["HTTP_X_MICROSOFTAJAX"] != null)
            {
                return;
            }

            WebHelper.AcceptEncoding.TryCompress(app.Context);
        }

        #region Implementation of IHttpModule

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application 
        /// </param>
        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += context_PreRequestHandlerExecute;
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion
    }
}
