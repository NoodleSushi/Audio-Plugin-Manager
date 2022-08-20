using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using PluginManager.Editor;
using PluginManager.PluginTree.Components;

namespace PluginManager.PluginTree
{
    public static class TreeEntityFactory
    {
        [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
        protected sealed class IdentifierAttribute : Attribute
        {
            private readonly string value;

            public IdentifierAttribute(string value)
            {
                this.value = value;
            }

            public string Value => value;
        }

        private static Dictionary<string, string> _identif2method;
        private static readonly Dictionary<string, TreeEntity> _identif2preset = new();

        public static TreeEntity CreateTreeEntityByIdentifier(string identifier)
        {
            _identif2method ??= typeof(TreeEntityFactory)
                .GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(IdentifierAttribute), false).Length > 0)
                .ToDictionary(m => m.GetCustomAttribute<IdentifierAttribute>().Value, m => m.Name);
            return (TreeEntity)typeof(TreeEntityFactory).GetMethod(_identif2method[identifier]).Invoke(null, null);
        }

        public static TreeEntity GetTreeEntityPreset(string identifier)
        {
            if (_identif2preset.ContainsKey(identifier))
            {
                return _identif2preset[identifier].Clone();
            }
            else
            {
                return CreateTreeEntityByIdentifier(identifier);
            }
        }

        public static void SetTreeEntityPreset(string identifier, TreeEntity treeEntity)
        {
            _identif2preset[identifier] = treeEntity.Clone();
        }

        public static void RemoveTreeEntityPreset(string identifier)
        {
            _identif2preset.Remove(identifier);
        }


        [IdentifierAttribute("Folder")]
        public static TreeFolder CreateFolder()
        {
            TreeFolder treeEntity = new() { Icon = Resources.ICON_FOLDER_CLOSE };
            Identifier identifier = treeEntity.AddComponent<Identifier>();
            identifier.Value = "Folder";
            Name nameComp = treeEntity.AddComponent<Name>();
            nameComp.NameString = "Folder";
            treeEntity.AddComponent<FolderComp>();
            treeEntity.AddComponent<DAWProperties>();
            treeEntity.AddComponent<TagCollection>();
            return treeEntity;
        }

        [IdentifierAttribute("Plugin")]
        public static TreeEntity CreatePlugin()
        {
            TreeEntity treeEntity = new() { Icon = Resources.ICON_NODE };
            Identifier identifier = treeEntity.AddComponent<Identifier>();
            identifier.Value = "Plugin";
            Name nameComp = treeEntity.AddComponent<Name>();
            nameComp.NameString = "Plugin";
            treeEntity.AddComponent<DAWProperties>();
            treeEntity.AddComponent<TagCollection>();
            return treeEntity;
        }

        [IdentifierAttribute("Reference")]
        public static TreeEntity CreateReference()
        {
            TreeEntity treeEntity = new();
            Identifier identifier = treeEntity.AddComponent<Identifier>();
            identifier.Value = "Reference";
            treeEntity.AddComponent<ReferenceData>();
            treeEntity.AddComponent<Name>();
            treeEntity.AddComponent<FolderComp>();
            treeEntity.AddComponent<DAWProperties>();
            treeEntity.AddComponent<TagCollection>();
            return treeEntity;
        }

        [IdentifierAttribute("Separator")]
        public static TreeEntity CreateSeparator()
        {
            TreeEntity treeEntity = new() { Icon = Resources.ICON_SEPARATOR };
            Identifier identifier = treeEntity.AddComponent<Identifier>();
            identifier.Value = "Separator";
            return treeEntity;
        }
    }
}
