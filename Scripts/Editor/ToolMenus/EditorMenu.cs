using Godot;
using GDArray = Godot.Collections.Array;
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
            var ExporterEditor = Resources.ExporterEditorScene.Instance<AcceptDialog>();
            WindowContainer.Instance.AddChild(ExporterEditor);
            ExporterEditor.PopupCenteredRatio();
            ExporterEditor.Call("serialize", ConfigServer.Instance.Exporters);
            ExporterEditor.Connect("confirmed", this, nameof(onExporterEditorFreed), new GDArray(ExporterEditor));
            Utils.MakePopupFreeable(ExporterEditor);
        }

        private void onExporterEditorFreed(Popup popup)
        {
            ConfigServer.Instance.Exporters = (GDArray)popup.Call("deserialize");
        }
    }
}