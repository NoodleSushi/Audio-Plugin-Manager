using Godot;
using Newtonsoft.Json.Linq;
using PluginManager.Editor;

namespace PluginManager.PluginTree.Components
{
    public class BaseOptional : Component
    {
        public bool isOptional = false;
        public bool Active = true;

        virtual public string SerializeKey() => "";

        public override void GenerateProperties()
        {
            base.GenerateProperties();
            if (isOptional)
            {
                CheckBox checkBox = new()
                {
                    Text = "Activate",
                    Pressed = Active
                };
                checkBox.Connect("toggled", this, nameof(OnCheckBoxToggled));
                EditorServer.Instance.AddProperty(checkBox);
            }
        }

        private void OnCheckBoxToggled(bool pressed)
        {
            Active = pressed;
            TreeEntity.DeferredGenerateProperties();
            TreeEntity.DeferredUpdateTreeItem();
        }

        public override Component Clone()
        {
            BaseOptional newComp = new()
            {
                isOptional = this.isOptional,
                Active = this.Active
            };
            return newComp;
        }

        public override void Serialize(JObject jobj, TreeEntityLookup TEL)
        {
            if (isOptional && Active)
                jobj.Add(SerializeKey() + "active", true);
        }

        public override void Deserialize(JObject jobj, TreeEntityLookup TEL)
        {
            if (jobj.ContainsKey(SerializeKey() + "active"))
            {
                isOptional = true;
                Active = (bool) jobj[SerializeKey() + "active"];
            }
        }
    }
}
