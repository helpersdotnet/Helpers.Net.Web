namespace Helpers.Net.Web.Extensions
{
    using System.Web.UI;

    public static partial class WebExtensions
    {
        public static string GetUICultureCode(this Page page)
        {
            return WebHelper.GetUICultureCode();
        }
    }
}
