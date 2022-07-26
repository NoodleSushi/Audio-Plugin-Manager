using Godot;
using PluginManager.Editor;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree.Components
{
    public class DAWProperties : BaseOptional
    {
        public enum DAWFlags
        {
            FLStudio = 1,
            Ableton = 2,
        }

        public const int DAWCount = 2;
        public static readonly Texture[] ICONS = new Texture[] { Resources.ICON_FL, Resources.ICON_LIVE };
        public string[] DAWQueries = new string[DAWCount] {"", ""};
        public int Flags = 0;
        public bool IsQueryVisible = true;

        public void ToggleFlag(bool press_state, int idx)
        {
            Flags = (Flags & ~(0b1 << idx)) | ((press_state ? 0b1 : 0b0) << idx);
        }

        public void ChangeQuery(string newQuery, int idx)
        {
            DAWQueries[idx] = newQuery;
        }

        public override string GetName() => "DAW Properties";

        public override string SerializeKey() => "daw";

        public override void GenerateProperties()
        {
            base.GenerateProperties();
            if (!Active)
                return;
            for (int idx = 0; idx < DAWCount; idx++)
            {
                using (TextureRect icon = new())
                {
                    icon.Texture = ICONS[idx];
                    icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                    EditorServer.Instance.AddProperty(icon);
                }

                using (CheckBox check = new())
                {
                    check.Text = "Exposed";
                    check.Pressed = ((Flags >> idx) & 1) > 0;
                    check.Connect(
                        "toggled",
                        this,
                        nameof(ToggleFlag),
                        new Godot.Collections.Array(idx)
                    );
                    EditorServer.Instance.AddProperty(check);
                }

                if (IsQueryVisible)
                {
                    LineEdit line = new()
                    {
                        Text = DAWQueries[idx]
                    };
                    line.Connect(
                        "text_changed",
                        this,
                        nameof(ChangeQuery),
                        new Godot.Collections.Array(idx)
                    );
                    EditorServer.Instance.AddProperty(line);
                }
            }
        }

        public override void Serialize(JObject jobj, TreeEntityLookup TEL)
        {
            base.Serialize(jobj, TEL);
            foreach (string str in DAWQueries)
            {
                if (str.Length > 0)
                {
                    jobj.Add("DAWqueries", new JArray(DAWQueries));
                    break;
                }
            }
            if (Flags > 0)
                jobj.Add("DAWflags", Flags);
        }

        public override void Deserialize(JObject jobj, TreeEntityLookup TEL)
        {
            base.Deserialize(jobj, TEL);
            if (jobj.ContainsKey("DAWqueries"))
            {
                JArray dawQueries = (JArray)jobj["DAWqueries"];
                for (int i = 0; i < DAWCount && i < dawQueries.Count; i++)
                {
                    DAWQueries[i] = (string)dawQueries[i];
                }
            }
            if (jobj.ContainsKey("DAWflags"))
            {
                Flags = (int)jobj["DAWflags"];
            }
        }

        public override Component Clone()
        {
            DAWProperties newComp = new()
            {
                Flags = this.Flags,
                DAWQueries = (string[])this.DAWQueries.Clone()
            };
            return newComp;
        }
    }
}
