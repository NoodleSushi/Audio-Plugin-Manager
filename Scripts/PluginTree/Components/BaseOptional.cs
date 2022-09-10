using Godot;
using Newtonsoft.Json.Linq;
using PluginManager.Editor;

namespace PluginManager.PluginTree.Components
{
    public abstract class BaseOptional : Component
    {
        public bool isOptional = false;
        public bool Active = true;

        public virtual string SerializeIdentifier() => "";

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

        public override Component Clone(Component newComponent)
        {
            if (newComponent is not BaseOptional newComp)
                throw new System.Exception();
            newComp.isOptional = this.isOptional;
            newComp.Active = this.Active;
            return newComp;
        }

        private void GetSerializeJObject(ref JObject jobj)
        {
            jobj = jobj.GetValue(SerializeIdentifier(), new JObject());
        }

        protected abstract void OptionalSerialize(JObject jobj, TreeEntityLookup TEL);

        protected abstract void OptionalDeserialize(JObject jobj, TreeEntityLookup TEL);

        sealed public override void Serialize(JObject jobj, TreeEntityLookup TEL)
        {
            GetSerializeJObject(ref jobj);
            if (isOptional)
                jobj.Add("active", Active);
            OptionalSerialize(jobj, TEL);
        }

        sealed public override void Deserialize(JObject jobj, TreeEntityLookup TEL)
        {
            GetSerializeJObject(ref jobj);
            if (jobj.ContainsKey("active"))
            {
                isOptional = true;
                Active = jobj.GetValue("active", false);
            }
            OptionalDeserialize(jobj, TEL);
        }
    }
}
