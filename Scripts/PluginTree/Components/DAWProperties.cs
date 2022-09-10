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
        public List<string> Queries = new();
        public int VisibleFlags = 0;
        public bool IsQueryVisible = true;

        public DAWProperties()
        {
            for (var i = 0; i < PluginServer.Instance.DAWCount; i++)
            {
                Queries.Add("");
            }
            VisibleFlags = (0b1 << PluginServer.Instance.DAWCount) - 1;
        }

        public void ToggleVisibleFlag(bool press_state, int idx)
        {
            VisibleFlags = (VisibleFlags & ~(0b1 << idx)) | ((press_state ? 0b1 : 0b0) << idx);
            VisibleFlags &= (0b1 << PluginServer.Instance.DAWCount) - 1;
        }

        public bool GetVisibleFlagState(int idx) => ((VisibleFlags >> idx) & 1) > 0;

        public void ChangeQuery(string newQuery, int idx)
        {
            Queries[idx] = newQuery;
        }

        public override string GetName() => "DAW Properties";

        public override string SerializeIdentifier() => "daw";

        protected override void OptionalGenerateProperties()
        {
            for (int idx = 0; idx < PluginServer.Instance.DAWCount; idx++)
            {
                using (Label label = new())
                {
                    label.Text = PluginServer.Instance.DAWList[idx];
                    EditorServer.Instance.AddProperty(label);
                }

                using (CheckBox check = new())
                {
                    check.Text = "Visible";
                    check.Pressed = GetVisibleFlagState(idx);
                    check.Connect(
                        "toggled",
                        this,
                        nameof(ToggleVisibleFlag),
                        new GDArray(idx)
                    );
                    EditorServer.Instance.AddProperty(check);
                }

                if (IsQueryVisible)
                {
                    LineEdit line = new()
                    {
                        Text = Queries[idx]
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
            if (Queries.Any(x => x.Length > 0))
                jobj.Add("queries", new JArray(Queries));
            if (VisibleFlags > 0)
                jobj.Add("flags", VisibleFlags);
        }

        protected override void OptionalDeserialize(JObject jobj, TreeEntityLookup TEL)
        {
            if (jobj.GetValue<JArray>("queries") is JArray dawQueries)
            {
                int dawCount = Math.Min(PluginServer.Instance.DAWCount, dawQueries.Count);
                for (int i = 0; i < dawCount; i++)
                {
                    if (dawQueries[i].ToObject<string>() is string dawQuery)
                        Queries[i] = dawQuery;
                }
            }
            VisibleFlags = jobj.GetValue<int>("flags", 0);
        }

        public override Component Clone(Component newComponent = null)
        {
            DAWProperties newComp = newComponent as DAWProperties ?? new DAWProperties();
            base.Clone(newComp);
            newComp.VisibleFlags = this.VisibleFlags;
            newComp.Queries = new(this.Queries);
            return newComp;
        }
    }
}
