using System.Web.UI;

namespace Helpers.Net.Web.Extensions
{
    public static partial class WebExtensions
    {
        public static string GetUICultureCode(this Page page)
        {
            return WebHelper.GetUICultureCode();
        }
    }
}
