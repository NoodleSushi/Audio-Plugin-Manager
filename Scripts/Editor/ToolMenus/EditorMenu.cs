using Godot;
using PluginManager.Editor.Containers;

namespace PluginManager.Editor.ToolMenus
{
    public class EditorMenu : MenuButtonExtended
    {
        public override void _Ready()
        {
            base._Ready();
            AddItem(nameof(ExportersButtonPressed));
        }

        [PopupItemAttribute("Edit Exporters")]
        public void ExportersButtonPressed()
        {
        }
    }
}