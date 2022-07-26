using Godot;

namespace PluginManager.Editor
{
    public class WindowContainer : Godot.Control
    {
        private static WindowContainer _instance;
        public static WindowContainer Instance => _instance;
        private readonly Godot.AcceptDialog outputDialog = new();
        private readonly RichTextLabel outputDialogText = new();
        public override void _Ready()
        {
            _instance = this;
            CallDeferred(nameof(GenerateWindows));
        }

        private void GenerateWindows()
        {
            AddChild(outputDialog);
            outputDialog.WindowTitle = "Output";
            outputDialog.Resizable = true;
            outputDialog.AddChild(outputDialogText);
            outputDialogText.AnchorLeft = 0;
            outputDialogText.AnchorTop = 0;
            outputDialogText.AnchorRight = 1;
            outputDialogText.AnchorBottom = 1;
            outputDialogText.MarginLeft = 8;
            outputDialogText.MarginTop = 8;
            outputDialogText.MarginRight = -8;
            outputDialogText.MarginBottom = -36;
        }

        public void DisplayOutput(string outputText = null)
        {
            if (outputText is not null)
                outputDialogText.Text = outputText;
            outputDialog.PopupCenteredRatio();
        }
    }
}
