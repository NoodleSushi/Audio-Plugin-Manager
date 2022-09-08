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

            foreach (var type in TypeList)
            {
                Label label = new()
                {
                    Text = type.Name
                };
                vBoxContainer.AddChild(label);

                OptionButton option = new();
                option.AddItem("Ignore");
                option.AddItem("Activate");
                option.AddItem("Deactivate");
                option.AddItem("Deactivate On Default");
                vBoxContainer.AddChild(option);
            }
        }
    }
}