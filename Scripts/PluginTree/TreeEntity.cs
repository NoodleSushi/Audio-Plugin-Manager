using Godot;
using PluginManager.Editor;
using PluginManager.PluginTree.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

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
        public bool Dimmed = false;
        private readonly List<Component> _components = new();
        private readonly Dictionary<Type, Component> _componentMap = new();
        private bool _isUpdatingTreeItem = false;
        private bool _isGeneratingProperties = false;
        public ReadOnlyCollection<Component> Components => _components.AsReadOnly();

        virtual public void ModifyTreeItem(TreeItem treeItem)
        {
            foreach (Component component in _components)
            {
                if (component.ShallModifyTreeItem)
                    component.ModifyTreeItem(treeItem);
            }
            treeItem.SetIcon(0, Icon);
            treeItem.SetText(0, Label);
            Dimmed = (Parent is not null && Parent.Dimmed) || Dimmed;
            if (Dimmed)
                treeItem.SetCustomBgColor(0, new Color(Colors.Black, 0.25f));
            else
                treeItem.ClearCustomBgColor(0);
        }

        public void GenerateProperties()
        {
            EditorServer.Instance.ClearProperties();
            bool isFirst = false;
            foreach (Component component in _components)
            {
                if (component.Visible)
                {
                    if (!isFirst)
                        isFirst = true;
                    else
                        EditorServer.Instance.AddProperty(new HSeparator());
                    component.GenerateProperties();
                }
            }
            _isGeneratingProperties = false;
        }

        public void DeferredGenerateProperties()
        {
            if (!_isGeneratingProperties)
            {
                CallDeferred(nameof(GenerateProperties));
                _isGeneratingProperties = true;
            }
        }

        public T AddComponent<T>() where T : Component, new()
        {
            T newComponent = new()
            {
                TreeEntity = this
            };
            _components.Add(newComponent);
            _componentMap.Add(typeof(T), newComponent);
            return newComponent;
        }

        public void AddComponent(Component newComponent)
        {
            newComponent.TreeEntity = this;
            _components.Add(newComponent);
            _componentMap.Add(newComponent.GetType(), newComponent);
        }

        public T GetComponent<T>() where T : Component
        {
            if (_componentMap.TryGetValue(typeof(T), out Component comp))
                return (T)comp;
            return null;
        }

        public void UpdateTreeItem()
        {
            EmitSignal(nameof(ContentChanged));
            _isUpdatingTreeItem = false;
        }

        public void DeferredUpdateTreeItem()
        {
            if (!_isUpdatingTreeItem)
            {
                CallDeferred(nameof(UpdateTreeItem));
                _isUpdatingTreeItem = true;
            }
        }

        public void SelectTreeItem() => EmitSignal(nameof(SelectEmitted));

        public void Unparent() => Parent?.RemoveChild(this);

        virtual public JObject Serialize(TreeEntityLookup TEL)
        {
            JObject jobj = new();
            foreach (Component comp in _components)
            {
                comp.Serialize(jobj, TEL);
            }
            return jobj;
        }

        virtual public void Deserialize(JObject jobj, TreeEntityLookup TEL)
        {
            foreach (Component comp in _components)
            {
                comp.Deserialize(jobj, TEL);
            }
        }

        virtual public TreeEntity Clone(TreeEntity newTreeEntity = null)
        {
            if (newTreeEntity is null)
                newTreeEntity = new();
            newTreeEntity.Label = this.Label;
            newTreeEntity.Icon = this.Icon;
            foreach (Component comp in this.Components)
            {
                newTreeEntity.AddComponent(comp.Clone());
            }
            return newTreeEntity;
        }
    }
}
