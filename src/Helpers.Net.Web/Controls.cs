using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System;

namespace Helpers.Net.Web
{
    public static partial class WebHelper
    {
        /// <summary>
        /// Finds a Control Recursively.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startingControl"></param>
        /// <param name="id">ID of the control to search</param>
        /// <returns></returns>
        public static T FindControlRecursive<T>(Control startingControl, string id)
              where T : Control
        {
            T found = null;
            foreach (Control activeControl in startingControl.Controls)
            {
                found = activeControl as T;
                if (found == null)
                    found = FindControlRecursive<T>(activeControl, id);
                else if (string.Compare(id, found.ID, true) != 0)
                    found = null;
                if (found != null)
                    break;
            }
            return found;
        }

        /// <summary>
        /// Finds a Control recursively.
        /// </summary>
        public static Control FindControlRecursive(Control Root, string Id)
        {
            if (Root.ID == Id)
                return Root;

            foreach (Control Ctl in Root.Controls)
            {
                Control FoundCtl = FindControlRecursive(Ctl, Id);
                if (FoundCtl != null)
                    return FoundCtl;
            }

            return null;

        }

        /// <summary>
        /// Loads the usercontrol and returns html.
        /// </summary>
        /// <param name="userControlLocation">Location of the usercontrol.</param>
        /// <returns>Html returned by the usercontrol.</returns>
        public static string GetHtmlForUserControl(string userControlLocation, bool enableViewState)
        {
            Page page = new Page();
            page.EnableEventValidation = false;
            UserControl userControl = (UserControl)page.LoadControl(userControlLocation);
            userControl.EnableViewState = false;

            HtmlForm form = new HtmlForm();
            form.Controls.Add(userControl);

            page.Controls.Add(form);

            StringWriter writer = new StringWriter();

            HttpContext.Current.Server.Execute(page, writer, false);

            string html = writer.ToString();

            if (!enableViewState)
                html = WebHelper.ViewState.RemoveViewState(html);

            return Regex.Replace(html, @"<[/]?(form)[^>]*?>", "", RegexOptions.IgnoreCase);
        }

    }
}
