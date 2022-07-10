using Godot;
using System;
using System.Collections.Generic;

namespace PluginManager.PluginTree
{
    public static class TreeEntityFactory
    {
        public static TreeFolder CreateFolder()
        {
            TreeFolder treeEntity = new()
            {
                Icon = Resources.ICON_FOLDER
            };
            Name nameComp = treeEntity.AddComponent<Name>();
            nameComp.NameString = "Folder";
            treeEntity.AddComponent<TagCollection>();
            return treeEntity;
        }

        public static TreeEntity CreatePlugin()
        {
            TreeEntity treeEntity = new()
            {
                Icon = Resources.ICON_NODE
            };
            Name nameComp = treeEntity.AddComponent<Name>();
            nameComp.NameString = "Plugin";
            treeEntity.AddComponent<DAWProperties>();
            treeEntity.AddComponent<TagCollection>();
            return treeEntity;
        }

        public static TreeEntity CreateSeparator()
        {
            TreeEntity treeEntity = new()
            {
                Icon = Resources.ICON_SEPARATOR
            };
            return treeEntity;
        }
    }
}