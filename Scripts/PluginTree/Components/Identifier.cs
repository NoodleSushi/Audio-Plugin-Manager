using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree.Components
{
    public class Identifier : Component
    {
        public string value;

        public Identifier()
        {
            Visible = false;
        }
        public override void Serialize(JObject jobj, TreeEntityLookup TEL)
        {
            jobj.Add("type", value);
        }

        public override Component Clone()
        {
            Identifier newComp = new()
            {
                value = this.value
            };
            return newComp;
        }
    }
}
