using Godot;
using PluginManager.Editor;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree.Components
{
    public class DAWProperties: Component
    {
        
        public enum DAWFlags
        {
            FLStudio = 1,
            Ableton = 2,
        }

        const int DAWCount = 2;
        private string[] _DAWQueries = new string[DAWCount];
        public int Flags = 0;

        private void ToggleFlag(bool press_state, int idx)
        {
            Flags = (Flags & ~(1 << idx)) + (press_state ? 1 : 0) << idx;
        }

        private void ChangeQuery(string newQuery, int idx)
        {
            _DAWQueries[idx] = newQuery;
        }

        public override void GenerateProperties()
        {
            Texture[] icons = new Texture[] { Resources.ICON_FL, Resources.ICON_LIVE};
            for (int idx = 0; idx < DAWCount; idx++)
            {
                TextureRect icon = new();
                icon.Texture = icons[idx];
                icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                EditorServer.Instance.AddProperty(icon);

                CheckBox check = new();
                check.Text = "Exposed";
                check.Pressed = ((Flags >> idx) & 1) > 0;
                check.Connect("toggled", this, nameof(ToggleFlag), new Godot.Collections.Array(idx));
                EditorServer.Instance.AddProperty(check);

                LineEdit line = new();
                line.Text = _DAWQueries[idx];
                line.Connect("text_changed", this, nameof(ChangeQuery), new Godot.Collections.Array(idx));
                EditorServer.Instance.AddProperty(line);
            }
        }

        public override void Serialize(JObject jobj)
        {
            foreach (string str in _DAWQueries)
            {
                if (str.Length > 0)
                {
                    jobj.Add("DAWqueries", new JArray(_DAWQueries));
                    break;
                }
            }
            if (Flags > 0)
                jobj.Add("DAWflags", Flags);
        }

        public override void Deserialize(JObject jobj, TreeEntityLookup TEL)
        {
            if (jobj.ContainsKey("DAWqueries"))
            {
                JArray dawqueries = (JArray) jobj["DAWQueries"];
                for (int i = 0; i < DAWCount && i < dawqueries.Count; i++)
                {
                    _DAWQueries[i] = (string) dawqueries[i];
                }
            }
            if (jobj.ContainsKey("DAWflags"))
            {
                Flags = (int) jobj["DAWflags"];
            }
        }
    }
}
