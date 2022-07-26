using Godot;
using PluginManager.PluginTree;

namespace PluginManager.Editor
{
    public class ToolsMenu : MenuButtonExtended
    {
        private readonly PluginMakerDialog pluginMakerDialog = new();

        public override void _Ready()
        {
            base._Ready();
            AddItem(nameof(OnEnterPluginListPressed));
            CallDeferred(nameof(GenerateWindows));
        }

        private void GenerateWindows()
        {
            WindowContainer.Instance.AddChild(pluginMakerDialog);
        }

        [PopupItemAttribute("Generate Plugins")]
        public void OnEnterPluginListPressed()
        {
            pluginMakerDialog.Popup();
        }
    }
}