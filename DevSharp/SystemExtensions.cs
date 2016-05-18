using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DevSharp
{
    public static class SystemExtensions
    {
        public static string ToJsonText(this object obj)
        {
            var jobject = JObject.FromObject(obj);
            jobject.Add(new JProperty("$type", obj.GetType().Name));
            return JsonConvert.SerializeObject(jobject, Formatting.Indented);
        }

        public static string ToJsonInline(this object obj)
        {
            var jobject = JObject.FromObject(obj);
            jobject.Add(new JProperty("$type", obj.GetType().Name));
            return JsonConvert.SerializeObject(jobject, Formatting.None);
        }
    }
}
