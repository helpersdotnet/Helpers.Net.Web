namespace Helpers.Net.Web
{
    using System.Globalization;

    public static partial class WebHelper
    {
        public static string GetUICultureCode()
        {
            return CultureInfo.CurrentUICulture.Name;
        }
    }
}