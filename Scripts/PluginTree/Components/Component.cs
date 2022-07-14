using Godot;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree.Components
{
    public abstract class Component : Godot.Object
    {
        public TreeEntity TreeEntity;
        virtual public void ModifyTreeItem(TreeItem treeItem) { }
        virtual public void GenerateProperties() { }
        virtual public void Serialize(JObject jobj) { }
        virtual public void Deserialize(JObject jobj, TreeEntityLookup TEL) { }
    }
}
