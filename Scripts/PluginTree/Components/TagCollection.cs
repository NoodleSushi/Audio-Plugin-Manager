using Godot;
using PluginManager.Editor;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace PluginManager.PluginTree.Components
{
    public class TagCollection : Component
    {
        private List<Tag> _tagList = new();
        public IList<Tag> TagList => _tagList.AsReadOnly();
        
        public void AddTag(Tag tag)
        {
            if (!HasTag(tag))
            {
                _tagList.Add(tag);
                tag.Connect(nameof(Tag.Deleting), this, nameof(RemoveTag), new Godot.Collections.Array(tag));
            }   
        }

        public bool HasTag(Tag tag)
        {
            return _tagList.Contains(tag);
        }

        public void RemoveTag(Tag tag)
        {
            _tagList.Remove(tag);
        }

        public void ToggleTag(Tag tag)
        {
            if (!HasTag(tag))
                AddTag(tag);
            else
                RemoveTag(tag);
        }

        public override void GenerateProperties()
        {
            // Server.Instance.AddProperty(lineEdit);
            Tree tree = new();
            tree.Connect("button_pressed", this, nameof(OnTreeButtonPressed));
            tree.HideRoot = true;
            tree.RectMinSize = new Vector2(0, 128);
            TreeItem root = tree.CreateItem();
            foreach (Tag tag in _tagList)
            {
                TreeItem newItem = tree.CreateItem(root);
                newItem.SetText(0, tag.Name);
                newItem.AddButton(0, Resources.ICON_REMOVE);
                newItem.SetMetadata(0, tag);
            }
            EditorServer.Instance.AddProperty(tree);
        }
        
        private void OnTreeButtonPressed(TreeItem item, int column, int id)
        { 
            if (item.GetMetadata(0) is Tag tag)
            {
                RemoveTag(tag);
                TreeEntity.GenerateProperties();
            }
        }

        public override void Serialize(JObject jobj)
        {
            if (_tagList.Count > 0)
            {
                JArray jTagList = new();
                foreach (Tag tag in _tagList)
                {
                    jTagList.Add(PluginServer.Instance.TagList.IndexOf(tag));
                }
                jobj.Add("tags", jTagList);
            }
        }

        public override void Deserialize(JObject jobj, TreeEntityLookup TEL)
        {
            if (jobj.ContainsKey("tags"))
            {
                JArray jTagList = (JArray) jobj["tags"];
                foreach (int jTagID in jTagList)
                {
                    _tagList.Add(PluginServer.Instance.TagList[jTagID]);
                }
            }
        }
    }
}
