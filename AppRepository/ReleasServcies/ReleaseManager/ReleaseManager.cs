using System.Globalization;

namespace Servcies.ReleasServcies.ReleaseManager
{
    public class ReleaseManager
    {
        private string regionCode;
        private static ReleaseManager instance;

        private ReleaseManager()
        {
        }

        public static ReleaseManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ReleaseManager();
                }
                return instance;
            }
        }

        public int GetTimeZone()
        {
            int timeZone = 0;

            if (regionCode == "en")
            {
                timeZone = 1;
            }
            else if (regionCode == "ru")
            {
                timeZone = 0;
            }

            return timeZone;
        }
        public void SetRegionCode(string code)
        {
            regionCode = code;
        }
        public string GetRegionCode()
        {
            return regionCode;
        }

        public CultureInfo GetCultureInfo()
        {

            CultureInfo culture = CultureInfo.InvariantCulture;

            if (regionCode == "en")
            {
                culture = new CultureInfo("en-US");
            }
            else if (regionCode == "ru")
            {
                culture = new CultureInfo("ru-Ru");
            }

            return culture;

        }
    }
}
