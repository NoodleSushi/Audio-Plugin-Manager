using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using PluginManager.PluginTree.Components;

namespace PluginManager.Editor.ToolMenus
{
    using TypeList = List<Type>;

    public class OptionalsActiveDialog : ConfirmationDialog
    {
        private enum ActiveOption
        {
            Ignore,
            Activate,
            Deactivate,
        }

        private TypeList _typeList = null;
        private TypeList TypeList
        {
            get
            {
                if (_typeList is null)
                {
                    _typeList = typeof(BaseOptional).Assembly.GetTypes()
                        .Where(x => x.IsSubclassOf(typeof(BaseOptional)))
                        .ToList();
                }
                return _typeList;
            }
        }
        private Dictionary<Type, ActiveOption> _typeActiveOptions = new();

        public override void _Ready()
        {
            WindowTitle = "Toggle Active of Optionals";
            PopupExclusive = true;
            Resizable = true;

            MarginContainer marginContainer = new();
            marginContainer.SizeFlagsHorizontal |= (int)Control.SizeFlags.ExpandFill;
            marginContainer.SizeFlagsVertical |= (int)Control.SizeFlags.ExpandFill;
            marginContainer.AddConstantOverride("margin_right", 8);
            marginContainer.AddConstantOverride("margin_top", 8);
            marginContainer.AddConstantOverride("margin_left", 8);
            marginContainer.AddConstantOverride("margin_bottom", 36);
            AddChild(marginContainer);

            VBoxContainer vBoxContainer = new();
            vBoxContainer.SizeFlagsVertical |= (int)Control.SizeFlags.ExpandFill;
            marginContainer.AddChild(vBoxContainer);



            foreach (Type type in TypeList)
            {
                _typeActiveOptions.Add(type, ActiveOption.Ignore);
                Label label = new() { Text = type.Name };
                vBoxContainer.AddChild(label);

                OptionButton optionButton = new();
                foreach (ActiveOption option in Enum.GetValues(typeof(ActiveOption)).Cast<ActiveOption>())
                {
                    optionButton.AddItem(option.ToString(), (int)option);
                }
                optionButton.Select(optionButton.GetItemIndex((int)_typeActiveOptions[type]));
                optionButton.Connect("item_selected", this, nameof(OnOptionButtonItemSelected), new(new SignalParamWrapper(optionButton), new SignalParamWrapper(type)));
                vBoxContainer.AddChild(optionButton);
            }

            Connect("confirmed", this, nameof(OnConfirmed));
        }

        private void OnConfirmed()
        {
            foreach (var folderRef in EditorServer.Instance.SelectedTreeEntities.Where(x => x.HasComponent<ReferenceData>()))
            {
                foreach (var type in TypeList)
                {
                    ActiveOption activeOption = _typeActiveOptions[type];
                    if (activeOption != ActiveOption.Ignore && folderRef.GetComponent(type) is BaseOptional Comp)
                    {
                        Comp.Active = activeOption == ActiveOption.Activate;
                    }
                }
            }
            EditorServer.Instance.RefreshFolderEditor();
        }

        // private void OnOptionButtonItemSelected(int index, OptionButton optionButton, Type type)
        private void OnOptionButtonItemSelected(int index, SignalParamWrapper _optionButton, SignalParamWrapper _type)
        {
            OptionButton optionButton = _optionButton.GetValue<OptionButton>();
            Type type = _type.GetValue<Type>();
            _typeActiveOptions[type] = (ActiveOption)optionButton.GetSelectedId();
        }

        public void Popup()
        {
            PopupCenteredRatio();
        }
    }
}