using Godot;
using System;

namespace PluginManager.PluginTree
{
    public abstract class Component : Godot.Object
    {
        public TreeEntity TreeEntity;
        virtual public void ModifyTreeItem(TreeItem treeItem) { }
        virtual public void GenerateProperties(VBoxContainer propertiesContainer) { }
    }
}
