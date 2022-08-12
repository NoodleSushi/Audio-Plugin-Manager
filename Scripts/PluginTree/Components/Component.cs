using Godot;
using PluginManager.Editor;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree.Components
{
    public abstract class Component : Resource
    {
        public TreeEntity TreeEntity;
        public bool Visible = true;
        public bool ShallModifyTreeItem = true;

        public new virtual string GetName() => "Component";

        public virtual void ModifyTreeItem(TreeItem treeItem) { }

        public virtual void GenerateProperties()
        {
            Label label = new();
            ColorRect colorRect = new();
            label.AddChild(colorRect);
            EditorServer.Instance.AddProperty(label);
            label.Text = GetName();
            label.Align = Label.AlignEnum.Center;
            label.Valign = Label.VAlign.Center;
            colorRect.Color = new Color("#24c8c8c8");
            colorRect.ShowBehindParent = true;
            colorRect.AnchorRight = 1;
            colorRect.AnchorBottom = 1;
        }

        public virtual void Serialize(JObject jobj, TreeEntityLookup TEL) { }

        public virtual void Deserialize(JObject jobj, TreeEntityLookup TEL) { }

        public abstract Component Clone(Component newComponent = null);
    }
}
