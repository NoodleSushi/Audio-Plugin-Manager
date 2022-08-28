using Godot;
using Newtonsoft.Json.Linq;

namespace PluginManager
{
    public static class Utils
    {
        public static void MakeFreeable(this Popup popup)
        {
            popup.Connect("popup_hide", popup, "queue_free");
        }

        public static T GetValue<T>(this JObject jObject, string key, T defaultValue = default)
        {
            if (jObject.ContainsKey(key))
                return jObject[key].ToObject<T>() ?? defaultValue;
            return defaultValue;
        }

        public static T GetOrMakeValue<T>(this JObject jObject, string key, T defaultValue = default) where T : JToken
        {
            if (jObject.ContainsKey(key) && jObject[key].ToObject<T>() is T t)
                return t;
            jObject.Add(key, defaultValue);
            return defaultValue;
        }
    }
}