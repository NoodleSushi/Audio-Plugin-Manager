using Godot;

namespace PluginManager.Editor.ToolMenus
{
    // TODO: Develop File Properties Dialog
    public class PropertiesDialog : ConfirmationDialog
    {
        public override void _Ready()
        {
            WindowTitle = "Properties";
            PopupExclusive = true;
            Resizable = true;

            MarginContainer marginContainer = new();
            marginContainer.SizeFlagsHorizontal |= (int)SizeFlags.ExpandFill;
            marginContainer.SizeFlagsVertical |= (int)SizeFlags.ExpandFill;
            marginContainer.AddConstantOverride("margin_right", 8);
            marginContainer.AddConstantOverride("margin_top", 8);
            marginContainer.AddConstantOverride("margin_left", 8);
            marginContainer.AddConstantOverride("margin_bottom", 36);
            AddChild(marginContainer);

            VBoxContainer vBoxContainer = new();
            vBoxContainer.SizeFlagsVertical |= (int)SizeFlags.ExpandFill;
            marginContainer.AddChild(vBoxContainer);

            vBoxContainer.AddChild(new Label
            {
                Text = "Hello World IG"
            });

            Connect("confirmed", this, nameof(OnConfirmed));
        }

        private void OnConfirmed()
        {

        }

        public void Popup()
        {
            PopupCenteredRatio();
        }
    }
}