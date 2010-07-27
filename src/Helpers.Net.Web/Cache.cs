namespace Helpers.Net.Web
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Caching;

    public static partial class WebHelper
    {
        public static void PurgeCacheItems(Cache cache, string prefix)
        {
            List<string> itemsToRemove = new List<string>();

            IDictionaryEnumerator enumerator = cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Key.ToString().StartsWith(prefix))
                    itemsToRemove.Add(enumerator.Key.ToString());
            }

            foreach (string itemToRemove in itemsToRemove)
                cache.Remove(itemToRemove);
        }

        public static void PurgeCacheItems(HttpContext context, string prefix)
        {
            PurgeCacheItems(context.Cache, prefix);
        }
    }
}
