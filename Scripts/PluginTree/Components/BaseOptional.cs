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

        private string GetSerializeKey()
        {
            return SerializeIdentifier() + "_active";
        }

        // protected JObject GetSerializeObject(JObject jobj)
        // {
        //     return jobj.GetOrMakeValue(SerializeIdentifier(), new JObject());
        // }

        public override void Serialize(JObject jobj, TreeEntityLookup TEL)
        {
            if (isOptional)
                jobj.Add(GetSerializeKey(), Active);
            // jobj = GetSerializeObject(jobj);
            // jobj.Add("optional", isOptional);
            // jobj.Add("active", Active);
        }

        public override void Deserialize(JObject jobj, TreeEntityLookup TEL)
        {
            if (jobj.ContainsKey(GetSerializeKey()))
            {
                isOptional = true;
                Active = jobj.GetValue(GetSerializeKey(), false);
            }
            // jobj = GetSerializeObject(jobj);
            // isOptional = jobj.GetValue("optional", false);
            // Active = jobj.GetValue("active", false);
        }
    }
}
