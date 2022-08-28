using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Godot;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree
{
    public class TreeFolder : TreeEntity
    {
        private readonly List<TreeEntity> _children = new();
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

        public void SetChildren(IEnumerable<TreeEntity> newChildren)
        {
            foreach (TreeEntity child in newChildren)
            {
                AddChild(child);
            }
        }

        public void AddChild(TreeEntity child)
        {
            MakeChildParentThis(child);
            _children.Add(child);
        }

        public void AddChildren(IEnumerable<TreeEntity> children)
        {
            foreach (TreeEntity child in children)
            {
                AddChild(child);
            }
        }

        public void AddChildAfter(TreeEntity child, TreeEntity behindChild)
        {
            MakeChildParentThis(child);
            _children.Insert(Children.IndexOf(behindChild) + 1, child);
        }

        public void AddChildrenAfter(IEnumerable<TreeEntity> children, TreeEntity behindChildren)
        {
            TreeEntity latest = behindChildren;
            foreach (TreeEntity child in children)
            {
                latest.AddChildAfter(child);
                latest = child;
            }
        }

        public void AddChildBefore(TreeEntity child, TreeEntity afterChild)
        {
            MakeChildParentThis(child);
            _children.Insert(Children.IndexOf(afterChild), child);
        }

        public void AddChildrenBefore(IEnumerable<TreeEntity> children, TreeEntity afterChildren)
        {
            foreach (TreeEntity child in children)
            {
                AddChildBefore(child, afterChildren);
            }
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
                jobj.Add("children", new JArray(_children.Select(x => TEL.GetID(x))));
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
            if (jobj.GetValue<JArray>("children") is JArray children)
            {
                SetChildren(children.Select(v => TEL.GetTreeEntity((int)v)));
            }
            Collapsed = jobj.GetValue("editor_collapsed", false);
        }

        public override TreeEntity Clone(TreeEntity newTreeEntity = null)
        {
            TreeFolder newTreeFolder = new();
            if (newTreeEntity is TreeFolder)
                newTreeFolder = newTreeEntity as TreeFolder;
            newTreeFolder = base.Clone(newTreeFolder) as TreeFolder;
            newTreeFolder.Collapsed = this.Collapsed;
            newTreeFolder.SetChildren(_children.Select(x => x.Clone()));
            return newTreeFolder;
        }
    }
}
