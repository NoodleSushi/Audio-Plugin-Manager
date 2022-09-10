using Godot;
using PluginManager.Editor;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree.Components
{
    public class Name : BaseOptional
    {
        public string NameString;

        [Signal]
        public delegate void NameChanged(string newName);

        public override string GetName() => "Name";

        public override string SerializeIdentifier() => "name";

        public override void ModifyTreeItem(TreeItem treeItem)
        {
            TreeEntity.Label = NameString;
        }

        public override void GenerateProperties()
        {
            base.GenerateProperties();
            if (!Active)
                return;
            LineEdit lineEdit = new()
            {
                Text = NameString
            };
            EditorServer.Instance.AddProperty(lineEdit);
            lineEdit.Connect("text_changed", this, nameof(OnLineEditTextChanged));
        }

        public void OnLineEditTextChanged(string new_text)
        {
            NameString = new_text;
            TreeEntity.DeferredUpdateTreeItem();
            EmitSignal(nameof(NameChanged), NameString);
        }

        protected override void OptionalSerialize(JObject jobj, TreeEntityLookup TEL)
        {
            jobj.Add("name", NameString);
        }

        protected override void OptionalDeserialize(JObject jobj, TreeEntityLookup TEL)
        {
            NameString = jobj.GetValue("name", "");
        }

        public override Component Clone(Component newComponent = null)
        {
            Name newComp = newComponent as Name ?? new Name();
            base.Clone(newComp);
            newComp.NameString = this.NameString;
            return newComp;
        }
    }
}
