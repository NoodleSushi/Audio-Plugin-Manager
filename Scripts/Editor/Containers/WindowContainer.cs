using Godot;
using GDDictionary = Godot.Collections.Dictionary;

namespace PluginManager.Editor.Containers
{
    public class WindowContainer : Control
    {
        private static WindowContainer _instance;
        public static WindowContainer Instance => _instance;

        public override void _Ready()
        {
            _instance = this;
        }

        public void DisplayOutput(string outputText = null)
        {
            AcceptDialog outputDialog = new();
            RichTextLabel outputDialogText = new();
            AddChild(outputDialog);
            outputDialog.WindowTitle = "Output";
            outputDialog.Resizable = true;
            outputDialog.AddChild(outputDialogText);
            outputDialogText.SetAnchorsPreset(LayoutPreset.Wide);
            outputDialogText.MarginLeft = 8;
            outputDialogText.MarginTop = 8;
            outputDialogText.MarginRight = -8;
            outputDialogText.MarginBottom = -36;

            if (outputText is not null)
                outputDialogText.Text = outputText;
            outputDialog.PopupCenteredRatio();
            Utils.MakePopupFreeable(outputDialog);
        }

        public void DisplayError(string errorText)
        {
            AcceptDialog errorDialog = new();
            AddChild(errorDialog);
            errorDialog.WindowTitle = "Error!";
            errorDialog.DialogText = errorText;
            errorDialog.PopupCenteredMinsize();
            Utils.MakePopupFreeable(errorDialog);
        }

        public void DisplayFileDialog(
            FileDialog.ModeEnum mode,
            Object target,
            string dir_method,
            string[] filters = null,
            string windowTitle = "File Dialog")
        {
            FileDialog fileDialog = new();
            AddChild(fileDialog);
            switch (mode)
            {
                case FileDialog.ModeEnum.OpenFile or FileDialog.ModeEnum.SaveFile:
                    fileDialog.Connect("file_selected", target, dir_method);
                    break;
                case FileDialog.ModeEnum.OpenFiles:
                    fileDialog.Connect("files_selected", target, dir_method);
                    break;
                case FileDialog.ModeEnum.OpenDir:
                    fileDialog.Connect("dir_selected", target, dir_method);
                    break;
            }
            fileDialog.Access = FileDialog.AccessEnum.Filesystem;
            fileDialog.Mode = mode;
            fileDialog.Filters = filters ?? new string[] { "" };
            fileDialog.WindowTitle = windowTitle;
            fileDialog.PopupCenteredRatio();
            Utils.MakePopupFreeable(fileDialog);
        }
    }
}
