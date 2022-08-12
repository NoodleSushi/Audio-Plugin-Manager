using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Godot;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree
{
    public class TreeFolder : TreeEntity
    {
        private List<TreeEntity> _children = new();
        public ReadOnlyCollection<TreeEntity> Children => _children.AsReadOnly();
        public bool Collapsed = false;

        public override void ModifyTreeItem(TreeItem treeItem)
        {
            base.ModifyTreeItem(treeItem);
            treeItem.Collapsed = Collapsed;
        }

        private void MakeChildParentThis(TreeEntity child)
        {
            child.Unparent();
            child.Parent = this;
        }

        public void SetChildren(List<TreeEntity> newChildren)
        {
            _children = newChildren;
        }

        public void AddChild(TreeEntity child)
        {
            MakeChildParentThis(child);
            _children.Add(child);
        }

        public void AddChildAfter(TreeEntity child, TreeEntity behindChild)
        {
            MakeChildParentThis(child);
            _children.Insert(Children.IndexOf(behindChild) + 1, child);
        }

        public void AddChildBefore(TreeEntity child, TreeEntity afterChild)
        {
            MakeChildParentThis(child);
            _children.Insert(Children.IndexOf(afterChild), child);
        }

        public void RemoveChild(TreeEntity child)
        {
            if (child.Parent == this)
            {
                child.Parent = null;
            }
            _children.Remove(child);
        }

        public void DeferredUpdateTreeItemChildren()
        {
            foreach (TreeEntity child in _children)
            {
                child.DeferredUpdateTreeItem();
                if (child is TreeFolder treeFolder)
                    treeFolder.DeferredUpdateTreeItemChildren();
            }
        }

        public override JObject Serialize(TreeEntityLookup TEL)
        {
            JObject jobj = base.Serialize(TEL);
            if (_children.Count > 0)
            {
                JArray childrenArray = new();
                _children.ForEach(x => childrenArray.Add(TEL.GetID(x)));
                jobj.Add("children", childrenArray);
            }
            if (Collapsed)
            {
                jobj.Add("editor_collapsed", Collapsed);
            }
            return jobj;
        }

        public override void Deserialize(JObject jobj, TreeEntityLookup TEL)
        {
            base.Deserialize(jobj, TEL);
            if (jobj.ContainsKey("children"))
            {
                foreach (int treeEntityID in jobj["children"].Select(v => (int)v))
                {
                    AddChild(TEL.GetTreeEntity(treeEntityID));
                }
            }

            if (jobj.ContainsKey("editor_collapsed"))
            {
                Collapsed = (bool)jobj["editor_collapsed"];
            }
        }

        public override TreeEntity Clone(TreeEntity newTreeEntity = null)
        {
            TreeFolder newTreeFolder = new();
            if (newTreeEntity is TreeFolder)
                newTreeFolder = newTreeEntity as TreeFolder;
            newTreeFolder = base.Clone(newTreeFolder) as TreeFolder;
            newTreeFolder.Collapsed = this.Collapsed;
            _children.ForEach(x => newTreeFolder.AddChild(x.Clone()));
            return newTreeFolder;
        }
    }
}
