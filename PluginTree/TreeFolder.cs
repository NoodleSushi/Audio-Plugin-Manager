using Godot;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PluginManager.PluginTree
{
    public class TreeFolder : TreeEntity
    {
        private List<TreeEntity> _children = new();
        public ReadOnlyCollection<TreeEntity> Children
        {
            get => _children.AsReadOnly();
        }

        private void MakeChildParentThis(TreeEntity child)
        {
            if (child.Parent != null)
            {
                child.Parent.RemoveChild(child);
            }
            child.Parent = this;
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
    }
}