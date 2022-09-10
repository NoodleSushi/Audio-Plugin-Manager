using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using PluginManager.Editor;
using Newtonsoft.Json.Linq;
using GDArray = Godot.Collections.Array;

namespace PluginManager.PluginTree.Components
{
    public class DAWProperties : BaseOptional
    {
        public List<string> DAWQueries = new();
        public int Flags = 0;
        public bool IsQueryVisible = true;

        public DAWProperties()
        {
            for (var i = 0; i < PluginServer.Instance.DAWCount; i++)
            {
                DAWQueries.Add("");
            }
            Flags = (0b1 << PluginServer.Instance.DAWCount) - 1;
        }

        public void ToggleFlag(bool press_state, int idx)
        {
            Flags = (Flags & ~(0b1 << idx)) | ((press_state ? 0b1 : 0b0) << idx);
            Flags &= (0b1 << PluginServer.Instance.DAWCount) - 1;
        }

        public bool GetFlagState(int idx) => ((Flags >> idx) & 1) > 0;

        public void ChangeQuery(string newQuery, int idx)
        {
            DAWQueries[idx] = newQuery;
        }

        public override string GetName() => "DAW Properties";

        public override string SerializeIdentifier() => "daw";

        public override void GenerateProperties()
        {
            base.GenerateProperties();
            if (!Active)
                return;
            for (int idx = 0; idx < PluginServer.Instance.DAWCount; idx++)
            {
                using (Label label = new())
                {
                    label.Text = PluginServer.Instance.DAWList[idx];
                    EditorServer.Instance.AddProperty(label);
                }

                using (CheckBox check = new())
                {
                    check.Text = "Exposed";
                    check.Pressed = GetFlagState(idx);
                    check.Connect(
                        "toggled",
                        this,
                        nameof(ToggleFlag),
                        new GDArray(idx)
                    );
                    EditorServer.Instance.AddProperty(check);
                }

                if (IsQueryVisible)
                {
                    LineEdit line = new()
                    {
                        Text = DAWQueries[idx]
                    };
                    if (TreeEntity.GetComponent<Name>() is Name nameComponent)
                    {
                        line.PlaceholderText = nameComponent.NameString;
                    }
                    line.Connect(
                        "text_changed",
                        this,
                        nameof(ChangeQuery),
                        new GDArray(idx)
                    );
                    EditorServer.Instance.AddProperty(line);
                }
            }
        }

        protected override void OptionalSerialize(JObject jobj, TreeEntityLookup TEL)
        {
            if (DAWQueries.Any(x => x.Length > 0))
                jobj.Add("DAWqueries", new JArray(DAWQueries));
            if (Flags > 0)
                jobj.Add("DAWflags", Flags);
        }

        protected override void OptionalDeserialize(JObject jobj, TreeEntityLookup TEL)
        {
            if (jobj.GetValue<JArray>("DAWqueries") is JArray dawQueries)
            {
                int dawCount = Math.Min(PluginServer.Instance.DAWCount, dawQueries.Count);
                for (int i = 0; i < dawCount; i++)
                {
                    if (dawQueries[i].ToObject<string>() is string dawQuery)
                        DAWQueries[i] = dawQuery;
                }
            }
            Flags = jobj.GetValue<int>("DAWflags", 0);
        }

        public override Component Clone(Component newComponent = null)
        {
            DAWProperties newComp = newComponent as DAWProperties ?? new DAWProperties();
            base.Clone(newComp);
            newComp.Flags = this.Flags;
            newComp.DAWQueries = new(this.DAWQueries);
            return newComp;
        }
    }
}
