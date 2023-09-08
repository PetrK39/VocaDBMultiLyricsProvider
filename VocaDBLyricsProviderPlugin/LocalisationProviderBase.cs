using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using MultiLyricsProviderInterface;

namespace VocaDBLyricsProviderPlugin
{
    public abstract class LocalisationProviderBase : ILocalisationProvider
    {
        public string GetResource(string key, CultureInfo culture)
        {
            if (!GetAvailableLanguages().Contains(culture))
                culture = CultureInfo.CreateSpecificCulture("en-US");

            return Properties.Resources.ResourceManager.GetString(key, culture);
        }
        private static IEnumerable<CultureInfo> GetAvailableLanguages()
        {
            var result = new List<CultureInfo>();

            var rm = new ResourceManager(typeof(Properties.Resources));

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach (var culture in cultures)
            {
                try
                {
                    if (culture.Equals(CultureInfo.InvariantCulture)) continue;

                    var rs = rm.GetResourceSet(culture, true, false);

                    if (rs != null) result.Add(culture);
                }
                catch (CultureNotFoundException)
                {
                    // NOP
                }
            }
            return result;
        }
    }
}
