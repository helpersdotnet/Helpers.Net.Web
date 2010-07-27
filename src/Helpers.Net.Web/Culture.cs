using System.Globalization;
namespace Helpers.Net.Web
{
    public static partial class WebHelper
    {
        public static string GetUICultureCode()
        {
            return CultureInfo.CurrentUICulture.Name;
        }
    }
}