using Godot;
using PluginManager.Editor;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree
{
    public class Tag : Godot.Object
    {
        [Signal]
        public delegate void NameChanged(string newName);
        [Signal]
        public delegate void VisibilityChanged(bool visible);
        [Signal]
        public delegate void Deleting();
        private string _name;
        public string Name
        {
            get => _name;
            set {
                _name = value;
                EmitSignal(nameof(NameChanged), _name);
            }
        }
        private bool _visible = true;
        public bool Visible
        {
            get => _visible;
            set {
                _visible = value;
                EmitSignal(nameof(VisibilityChanged), _visible);
            }
        }
        public void NotifyDeletion()
        {
            EmitSignal(nameof(Deleting));
        }

        public void GenerateProperties()
        {
            LineEdit lineEdit = new LineEdit();
            EditorServer.Instance.AddProperty(lineEdit);
            lineEdit.Text = Name;
            lineEdit.Connect("text_entered", this, nameof(OnLineEditTextEntered));
        }

        public void OnLineEditTextEntered(string new_text)
        {
            Name = new_text;
        }

        public JObject Serialize()
        {
            JObject o = new();
            o.Add("name", _name);
            o.Add("visible", _visible);
            return o;
        }
    }
}
