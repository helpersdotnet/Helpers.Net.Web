namespace Helpers.Net.Web
{
    public static partial class WebHelper
    {
        public static partial class ViewState
        {
            /// <summary>
            /// Finds the location of viewstate
            /// </summary>
            /// <param name="html">Html string to find where the viewstate is.</param>
            /// <param name="startIndex">Returns the index where the viewstate starts.</param>
            /// <param name="endIndex">Returns the index where the viewstate ends.</param>
            /// <returns>Returns true if viewstate is found otherwise false.</returns>
            public static bool FindViewState(string html, out int startIndex, out int endIndex)
            {
                startIndex = html.IndexOf("<input type=\"hidden\" name=\"__VIEWSTATE\" id=\"__VIEWSTATE\"");

                if (startIndex > 0)
                {
                    endIndex = html.IndexOf("/>", startIndex) + 2;
                    return true;
                }
                else
                {
                    endIndex = 0;
                    return false;
                }
            }

            /// <summary>
            /// Extract view state input tag
            /// </summary>
            /// <param name="html">Html String</param>
            /// <param name="remove">Whether you want to remove viewstate or not form html</param>
            /// <returns>Returns the ViewState input tag. If ViewState is not found it returns empty string.</returns>
            public static string ExtractViewStateTag(string html, bool remove)
            {
                int startIndex, endIndex;

                if (FindViewState(html, out startIndex, out endIndex))
                {
                    int till = endIndex - startIndex;

                    string viewState = html.Substring(startIndex, till);

                    if (remove)
                        html = html.Remove(startIndex, till);

                    return viewState;
                }
                else
                    return string.Empty;
            }

            /// <summary>
            /// Removes the ViewState form the given Html string.
            /// </summary>
            /// <param name="html">Html String to remove ViewState.</param>
            /// <returns>Html after removing ViewState</returns>
            public static string RemoveViewState(string html)
            {
                int startIndex, endIndex;

                if (FindViewState(html, out startIndex, out endIndex))
                    html = html.Remove(startIndex, endIndex - startIndex);

                return html;
            }
        }
    }
}