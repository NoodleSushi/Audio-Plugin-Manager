using Godot;
using PluginManager.Editor;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree.Components
{
    public abstract class Component : Godot.Object
    {
        public TreeEntity TreeEntity;
        public bool Visible = true;
        public bool ShallModifyTreeItem = true;

        virtual public string GetName() => "Component";

        virtual public void ModifyTreeItem(TreeItem treeItem) { }

        virtual public void GenerateProperties()
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

        virtual public void Serialize(JObject jobj, TreeEntityLookup TEL) { }

        virtual public void Deserialize(JObject jobj, TreeEntityLookup TEL) { }

        abstract public Component Clone();
    }
}
