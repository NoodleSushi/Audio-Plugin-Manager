using Godot;
using PluginManager.Editor;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree.Components
{
    public class Name : Component
    {
        public string NameString;
        [Signal]
        public delegate void NameChanged(string newName);

        public override void ModifyTreeItem(TreeItem treeItem)
        {
            TreeEntity.Label = NameString;
        }

        public override void GenerateProperties()
        {
            LineEdit lineEdit = new LineEdit();
            EditorServer.Instance.AddProperty(lineEdit);
            lineEdit.Text = NameString;
            lineEdit.Connect("text_entered", this, nameof(OnLineEditTextEntered));
        }

        public void OnLineEditTextEntered(string new_text)
        {
            NameString = new_text;
            TreeEntity.UpdateTreeItem();
            EmitSignal(nameof(NameChanged), NameString);
        }

        public override void Serialize(JObject jobj)
        {
            jobj.Add("name", NameString);
        }

        public override void Deserialize(JObject jobj, TreeEntityLookup TEL)
        {
            NameString = (string) jobj["name"];
        }
    }
}
