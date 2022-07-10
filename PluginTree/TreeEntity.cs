using Godot;
using System;
using System.Collections.Generic;

namespace PluginManager.PluginTree
{
    public class TreeEntity : Godot.Object
    {
        [Signal]
        public delegate void ContentChanged();
        [Signal]
        public delegate void SelectEmitted();
        public TreeFolder Parent;
        public string Label;
        public Texture Icon;
        private List<Component> _components = new();
        private Dictionary<Type, Component> _componentMap = new();

        virtual public void ModifyTreeItem(TreeItem treeItem)
        {
            foreach (Component component in _components)
            {
                component.ModifyTreeItem(treeItem);
            }
            treeItem.SetIcon(0, Icon);
            treeItem.SetText(0, Label);
        }

        public void GenerateProperties(VBoxContainer propertiesContainer)
        {
            foreach (Component component in _components)
            {
                component.GenerateProperties(propertiesContainer);
            }
        }

        public T AddComponent<T>() where T : Component, new()
        {
            T newComponent = new T();
            newComponent.TreeEntity = this;
            _components.Add(newComponent);
            _componentMap.Add(typeof(T), newComponent);
            return newComponent;
        }

        public T GetComponent<T>() where T : Component
        {
            Component comp;
            if (_componentMap.TryGetValue(typeof(T), out comp))
                return (T) comp;
            return null;
        }

        public void UpdateTreeItem()
        {
            EmitSignal(nameof(ContentChanged));
        }

        public void SelectTreeItem()
        {
            EmitSignal(nameof(SelectEmitted));
        }
    }
}