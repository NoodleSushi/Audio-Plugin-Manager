using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using PluginManager.PluginTree.Components;

namespace PluginManager.PluginTree
{
    public static class TreeEntityFactory
    {
        [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
        sealed protected class IdentifierAttribute : Attribute
        {
            private readonly string value;

            public IdentifierAttribute(string value)
            {
                this.value = value;
            }

            public string Value => value;
        }

        private static Dictionary<string, string> _identif2method;
        public static TreeEntity CreateTreeEntityByIdentifier(string identifier)
        {
            if (_identif2method == null)
            {
                Dictionary<string, string> identif2method = new();
                var methods = typeof(TreeEntityFactory)
                    .GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(IdentifierAttribute), false).Length > 0)
                    .ToArray();
                foreach (MethodInfo method in methods)
                {
                    IdentifierAttribute identif = method.GetCustomAttribute<IdentifierAttribute>();
                    identif2method[identif.Value] = method.Name;
                }
                _identif2method = identif2method;
            }
            return (TreeEntity) typeof(TreeEntityFactory).GetMethod(_identif2method[identifier]).Invoke(null, null);
        }

        [IdentifierAttribute("Folder")]
        public static TreeFolder CreateFolder()
        {
            TreeFolder treeEntity = new()
            {
                Icon = Resources.ICON_FOLDER
            };
            Identifier identifier = treeEntity.AddComponent<Identifier>();
            identifier.value = "Folder";
            Name nameComp = treeEntity.AddComponent<Name>();
            nameComp.NameString = "Folder";
            treeEntity.AddComponent<DAWProperties>();
            treeEntity.AddComponent<TagCollection>();
            return treeEntity;
        }

        [IdentifierAttribute("Plugin")]
        public static TreeEntity CreatePlugin()
        {
            TreeEntity treeEntity = new()
            {
                Icon = Resources.ICON_NODE
            };
            Identifier identifier = treeEntity.AddComponent<Identifier>();
            identifier.value = "Plugin";
            Name nameComp = treeEntity.AddComponent<Name>();
            nameComp.NameString = "Plugin";
            treeEntity.AddComponent<DAWProperties>();
            treeEntity.AddComponent<TagCollection>();
            return treeEntity;
        }

        [IdentifierAttribute("Separator")]
        public static TreeEntity CreateSeparator()
        {
            TreeEntity treeEntity = new()
            {
                Icon = Resources.ICON_SEPARATOR
            };
            Identifier identifier = treeEntity.AddComponent<Identifier>();
            identifier.value = "Separator";
            return treeEntity;
        }
    }
}