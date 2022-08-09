using Godot;
using PluginManager.PluginTree;
using PluginManager.Editor.Containers;

namespace PluginManager.Editor.ToolMenus
{
    public class EditorMenu : MenuButtonExtended
    {
        public override void _Ready()
        {
            base._Ready();
            AddItem(nameof(SettingsButtonPressed));
        }

        [PopupItemAttribute("Settings")]
        public void SettingsButtonPressed()
        {
        }
    }
}