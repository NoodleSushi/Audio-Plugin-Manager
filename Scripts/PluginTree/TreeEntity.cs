using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Godot;
using PluginManager.Editor;
using PluginManager.PluginTree.Components;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree
{
    public class TreeEntity : Resource
    {
        public enum BUTTON_ID
        {
            VISIBLE,
        };

        [Signal]
        public delegate void ContentChanged();

        [Signal]
        public delegate void SelectEmitted();
        public TreeFolder Parent;
        public string Label;
        public Texture Icon;
        public bool Dimmed = false;
        public bool Visible = true;
        private readonly List<Component> _components = new();
        private readonly Dictionary<Type, Component> _componentMap = new();
        private bool _isUpdatingTreeItem = false;
        private bool _isGeneratingProperties = false;
        public ReadOnlyCollection<Component> Components => _components.AsReadOnly();
        public string GDName => GetComponent<Name>().NameString;

        public virtual void ModifyTreeItem(TreeItem treeItem)
        {
            Dimmed = false;
            foreach (Component component in _components.Where(x => x.ShallModifyTreeItem))
            {
                component.ModifyTreeItem(treeItem);
            }
            treeItem.SetIcon(0, Icon);
            treeItem.SetText(0, Label);
            Texture visibilityIcon = Visible ? Resources.ICON_VISIBLE_ON : Resources.ICON_VISIBLE_OFF;
            if (Parent != null && treeItem.GetButtonById(0, (int)BUTTON_ID.VISIBLE) == -1)
                treeItem.AddButton(0, visibilityIcon, (int)BUTTON_ID.VISIBLE);
            treeItem.SetButton(0, treeItem.GetButtonById(0, (int)BUTTON_ID.VISIBLE), visibilityIcon);
            Dimmed = Dimmed || !Visible || (Parent is not null && Parent.Dimmed);
            if (Dimmed)
                treeItem.SetCustomBgColor(0, new Color(Colors.Black, 0.25f));
            else
                treeItem.ClearCustomBgColor(0);
        }

        public void GenerateProperties()
        {
            EditorServer.Instance.ClearProperties();
            bool isFirst = false;
            foreach (Component component in _components.Where(x => x.Visible))
            {
                if (!isFirst)
                    isFirst = true;
                else
                    EditorServer.Instance.AddProperty(new HSeparator());
                component.GenerateProperties();
            }
            Button makeDefaultButton = new() { Text = "Make Default" };
            Button resetDefaultButton = new() { Text = "Reset Default" };
            makeDefaultButton.Connect("pressed", this, nameof(OnDefaultButtonPressed));
            resetDefaultButton.Connect("pressed", this, nameof(OnResetButtonPressed));
            EditorServer.Instance.AddProperty(makeDefaultButton);
            EditorServer.Instance.AddProperty(resetDefaultButton);
            _isGeneratingProperties = false;
        }

        public void ButtonPressed(int column, int id)
        {
            if (id == (int)BUTTON_ID.VISIBLE)
                Visible = !Visible;
        }

        private void OnDefaultButtonPressed()
        {
            if (GetComponent<Identifier>() is not Identifier identifier)
                return;
            TreeEntityFactory.SetTreeEntityPreset(identifier.Value, this);
        }

        private void OnResetButtonPressed()
        {
            if (GetComponent<Identifier>() is not Identifier identifier)
                return;
            TreeEntityFactory.RemoveTreeEntityPreset(identifier.Value);
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

        public Component GetComponent(Type type)
        {
            if (_componentMap.TryGetValue(type, out Component comp))
                return comp;
            return null;
        }

        public bool HasComponent<T>() where T : Component
        {
            return _componentMap.ContainsKey(typeof(T));
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

        public void AddChildAfter(TreeEntity child)
        {
            Parent?.AddChildAfter(child, this);
        }

        public void AddChildrenAfter(IEnumerable<TreeEntity> children)
        {
            Parent?.AddChildrenAfter(children, this);
        }

        public void AddChildBefore(TreeEntity child)
        {
            Parent?.AddChildBefore(child, this);
        }

        public void AddChildrenBefore(IEnumerable<TreeEntity> children)
        {
            Parent?.AddChildrenBefore(children, this);
        }

        public virtual JObject Serialize(TreeEntityLookup TEL)
        {
            JObject jobj = new();
            jobj["visible"] = Visible;
            _components.ForEach(x => x.Serialize(jobj, TEL));
            return jobj;
        }

        public virtual void Deserialize(JObject jobj, TreeEntityLookup TEL)
        {
            Visible = jobj.GetValue("visible", true);
            _components.ForEach(x => x.Deserialize(jobj, TEL));
        }

        public virtual TreeEntity Clone(TreeEntity newTreeEntity = null)
        {
            if (newTreeEntity is null)
                newTreeEntity = new();
            newTreeEntity.Label = this.Label;
            newTreeEntity.Icon = this.Icon;
            newTreeEntity.Visible = this.Visible;
            _components.ForEach(x => newTreeEntity.AddComponent(x.Clone()));
            return newTreeEntity;
        }
    }
}
