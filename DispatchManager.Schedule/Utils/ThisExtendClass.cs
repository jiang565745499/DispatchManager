using System.Text;

namespace DispatchManager.Schedule.Utils
{
    public static class ThisExtendClass
    {
        public static string ToStrings(this IDictionary<string, string> sources)
        {
            if (sources == null) { return ""; }
            StringBuilder sb = new StringBuilder();
            foreach ((string key, string value) in sources)
            {
                sb.Append($@"{key}:{value} ");
            }
            return sb.ToString();
        }
    }
}
