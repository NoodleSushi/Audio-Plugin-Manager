using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree.Components
{
    public class Identifier : Component
    {
        public string value;

        public override void Serialize(JObject jobj)
        {
            jobj.Add("type", value);
        }
    }
}
