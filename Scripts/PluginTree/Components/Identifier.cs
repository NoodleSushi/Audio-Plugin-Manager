using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree.Components
{
    public class Identifier : Component
    {
        public string Value;

        public Identifier()
        {
            Visible = false;
        }
        public override void Serialize(JObject jobj, TreeEntityLookup TEL)
        {
            jobj.Add("type", Value);
        }

        public override Component Clone(Component newComponent = null)
        {
            Identifier newComp = newComponent as Identifier ?? new Identifier();
            newComp.Value = this.Value;
            return newComp;
        }
    }
}
