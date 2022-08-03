using Godot;

namespace PluginManager.Editor.Containers
{
    public class WindowContainer : Godot.Control
    {
        private static WindowContainer _instance;
        public static WindowContainer Instance => _instance;
        private readonly Godot.AcceptDialog outputDialog = new();
        private readonly RichTextLabel outputDialogText = new();
        private readonly Godot.AcceptDialog errorDialog = new();

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
            outputDialogText.SetAnchorsPreset(LayoutPreset.Wide);
            outputDialogText.MarginLeft = 8;
            outputDialogText.MarginTop = 8;
            outputDialogText.MarginRight = -8;
            outputDialogText.MarginBottom = -36;

            AddChild(errorDialog);
            errorDialog.WindowTitle = "Error!";
        }

        public void DisplayOutput(string outputText = null)
        {
            if (outputText is not null)
                outputDialogText.Text = outputText;
            outputDialog.PopupCenteredRatio();
        }

        public void DisplayError(string errorText)
        {
            errorDialog.DialogText = errorText;
            errorDialog.PopupCenteredMinsize();
        }
    }
}
