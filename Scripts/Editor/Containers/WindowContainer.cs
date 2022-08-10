using Godot;
using GDDictionary = Godot.Collections.Dictionary;

namespace PluginManager.Editor.Containers
{
    public class WindowContainer : Control
    {
        private static WindowContainer _instance;
        public static WindowContainer Instance => _instance;
        private readonly AcceptDialog outputDialog = new();
        private readonly RichTextLabel outputDialogText = new();
        private readonly AcceptDialog errorDialog = new();
        private readonly FileDialog fileDialog = new();

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

            AddChild(fileDialog);
            fileDialog.Access = FileDialog.AccessEnum.Filesystem;
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

        public void DisplayFileDialog(
            FileDialog.ModeEnum mode,
            Object target,
            string dir_method,
            string[] filters = null,
            string windowTitle = "File Dialog")
        {
            foreach (string signal in new string[] { "dir_selected", "file_selected", "files_selected" })
            {
                var list = fileDialog.GetSignalConnectionList(signal);
                for (var i = 0; i < list.Count; i++)
                {
                    var connection = (GDDictionary)list[i];
                    fileDialog.Disconnect(signal, (Object)connection["target"], (string)connection["method"]);
                }
            }
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
            fileDialog.Mode = mode;
            fileDialog.Filters = filters ?? new string[] { "" };
            fileDialog.WindowTitle = windowTitle;
            fileDialog.PopupCenteredRatio();
        }
    }
}
