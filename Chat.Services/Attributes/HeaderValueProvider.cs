using System.Globalization;
using System.Net.Http.Headers;
using System.Linq;
using System.Web.Http.ValueProviders;
using Newtonsoft.Json;

namespace Forum.WebApi.Attributes
{
    public class HeaderValueProvider<t> : IValueProvider where t : class
    {
        private const string HeaderPrefix = "X-";
        private readonly HttpRequestHeaders _headers;

        public HeaderValueProvider(HttpRequestHeaders headers)
        {
            _headers = headers;
        }

        public bool ContainsPrefix(string prefix)
        {
            var contains = _headers.Any(h => h.Key.Contains(HeaderPrefix + prefix));
            return contains;
        }

        public ValueProviderResult GetValue(string key)
        {
            var contains = _headers.Any(h => h.Key.Contains(HeaderPrefix + key));
            if (!contains)
                return null;

            var value = _headers.GetValues(HeaderPrefix + key).First();
            //var obj = JsonConvert.DeserializeObject<t>(value,
            //  new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return new ValueProviderResult(value, value, CultureInfo.InvariantCulture);
        }
    }
}