using PluginManager.Editor.Containers;

namespace PluginManager.Editor.ToolMenus
{
    public class ToolsMenu : MenuButtonExtended
    {
        public override void _Ready()
        {
            base._Ready();
            AddItem(nameof(OnEnterPluginListPressed));
        }

        [PopupItemAttribute("Generate Plugins")]
        public void OnEnterPluginListPressed()
        {
            PluginMakerDialog pluginMakerDialog = new();
            WindowContainer.Instance.AddChild(pluginMakerDialog);
            pluginMakerDialog.Popup();
            Utils.MakePopupFreeable(pluginMakerDialog);
        }
    }
}