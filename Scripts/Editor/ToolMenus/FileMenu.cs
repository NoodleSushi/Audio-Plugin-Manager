using Godot;
using PluginManager.PluginTree;
using PluginManager.Editor.Containers;

namespace PluginManager.Editor.ToolMenus
{
    public class FileMenu : MenuButtonExtended
    {
        private readonly string[] FILE_FILTERS = new string[] { "*.vstdb ; VST Database" };
        // private readonly FileDialog saveFileDialog = new();
        // private readonly FileDialog openFileDialog = new();
        private readonly PropertiesDialog propertiesDialog = new();

        public override void _Ready()
        {
            base._Ready();
            AddItem(nameof(NewButtonPressed));
            AddItem(nameof(OpenButtonPressed));
            AddSeparator();
            AddItem(nameof(SaveButtonPressed));
            AddSeparator();
            AddItem(nameof(ShowOutputButtonPressed));
            AddItem(nameof(ShowPropertiesButtonPressed));

            // saveFileDialog.Connect("file_selected", this, nameof(OnSaveFileDialogFileSelected));
            // openFileDialog.Connect("file_selected", this, nameof(OnOpenFileDialogFileSelected));
            CallDeferred(nameof(GenerateWindows));
        }

        private void GenerateWindows()
        {
            // saveFileDialog.Mode = FileDialog.ModeEnum.SaveFile;
            // saveFileDialog.Access = FileDialog.AccessEnum.Filesystem;
            // saveFileDialog.Filters = FILE_FILTERS;
            // saveFileDialog.WindowTitle = "Save VST Database";
            // WindowContainer.Instance.AddChild(saveFileDialog);

            // openFileDialog.Mode = FileDialog.ModeEnum.OpenFile;
            // openFileDialog.Access = FileDialog.AccessEnum.Filesystem;
            // openFileDialog.Filters = FILE_FILTERS;
            // openFileDialog.WindowTitle = "Open VST Database";
            // WindowContainer.Instance.AddChild(openFileDialog);

            WindowContainer.Instance.AddChild(propertiesDialog);
        }

        [PopupItemAttribute("New")]
        public void NewButtonPressed()
        {
            PluginServer.Instance.Clear();
        }

        [PopupItemAttribute("Open")]
        public void OpenButtonPressed()
        {
            WindowContainer.Instance.DisplayFileDialog(
                FileDialog.ModeEnum.OpenFile,
                this,
                nameof(OnOpenFileDialogFileSelected),
                FILE_FILTERS,
                "Open VST Database"
            );
            // openFileDialog.PopupCenteredRatio();
        }

        [PopupItemAttribute("Save")]
        public void SaveButtonPressed()
        {
            WindowContainer.Instance.DisplayFileDialog(
                FileDialog.ModeEnum.SaveFile,
                this,
                nameof(OnSaveFileDialogFileSelected),
                FILE_FILTERS,
                "Save VST Database"
            );
            // saveFileDialog.PopupCenteredRatio();
        }

        [PopupItemAttribute("Output")]
        public void ShowOutputButtonPressed()
        {
            WindowContainer.Instance.DisplayOutput();
        }

        [PopupItemAttribute("File Properties")]
        public void ShowPropertiesButtonPressed()
        {
            propertiesDialog.Popup();
        }

        private void OnOpenFileDialogFileSelected(string path)
        {
            bool success = true;
            string outputDialog = "An error occurred while reading the VSTDB file.\n\n";
            using (File file = new())
            {
                file.Open(path, File.ModeFlags.Read);
                try
                {
                    success = PluginServer.Instance.Deserialize(file.GetAsText());
                }
                catch (System.Exception error)
                {
                    success = false;
                    outputDialog += error.ToString();
                    PluginServer.Instance.Clear();
                }
                file.Close();
            }
            if (!success)
            {
                WindowContainer.Instance.DisplayOutput(outputDialog);
            }
        }

        private void OnSaveFileDialogFileSelected(string path)
        {
            bool success = true;
            string outputDialog = "An error occurred while saving as a VSTDB file.\n\n";
            File file = new();
            file.Open(path, File.ModeFlags.Write);
            try
            {
                file.StoreString(PluginServer.Instance.Serialize());
            }
            catch (System.Exception error)
            {
                success = false;
                outputDialog += error.ToString();
            }
            file.Close();
            if (!success)
            {
                WindowContainer.Instance.DisplayOutput(outputDialog);
            }
        }
    }
}
