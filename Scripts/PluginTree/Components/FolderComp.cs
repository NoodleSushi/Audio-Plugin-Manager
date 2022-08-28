using Godot;
using PluginManager.Editor;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree.Components
{
    public class FolderComp : Component
    {
        public bool Collapsed = false;

        public override string GetName() => "Folder";

        public override void ModifyTreeItem(TreeItem treeItem)
        {
            TreeEntity.Icon = (Collapsed) ? Resources.ICON_FOLDER_OPEN : Resources.ICON_FOLDER_CLOSE;
        }

        public override void GenerateProperties()
        {
            base.GenerateProperties();
            using (CheckBox checkbox = new())
            {
                checkbox.Text = "Collapsed";
                checkbox.Pressed = Collapsed;
                checkbox.Connect("toggled", this, nameof(OnCheckboxToggled));
                EditorServer.Instance.AddProperty(checkbox);
            }
            if (TreeEntity.GetComponent<DAWProperties>() is DAWProperties dawProperties)
                dawProperties.IsQueryVisible = (!Collapsed);
        }

        public void OnCheckboxToggled(bool pressed)
        {
            Collapsed = pressed;
            TreeEntity.DeferredGenerateProperties();
            TreeEntity.DeferredUpdateTreeItem();
        }

        public override Component Clone(Component newComponent = null)
        {
            FolderComp newComp = newComponent as FolderComp ?? new FolderComp();
            newComp.Collapsed = this.Collapsed;
            return newComp;
        }

        public override void Serialize(JObject jobj, TreeEntityLookup TEL)
        {
            base.Serialize(jobj, TEL);
            if (Collapsed)
                jobj.Add("collapsed", Collapsed);
        }

        public override void Deserialize(JObject jobj, TreeEntityLookup TEL)
        {
            base.Deserialize(jobj, TEL);
            Collapsed = jobj.GetValue("collapsed", false);
        }
    }
}
