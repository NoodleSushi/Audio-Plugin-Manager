using Godot;
using PluginManager.PluginTree;

namespace PluginManager.Editor
{
    public class FileMenu : MenuButtonExtended
    {
        private readonly string[] FILE_FILTERS = new string[] { "*.vstdb ; VST Database" };
        private readonly Godot.FileDialog saveFileDialog = new();
        private readonly Godot.FileDialog openFileDialog = new();

        public override void _Ready()
        {
            base._Ready();
            AddItem(nameof(NewButtonPressed));
            AddItem(nameof(OpenButtonPressed));
            AddSeparator();
            AddItem(nameof(SaveButtonPressed));
            // AddItem(nameof(TestButtonPressed));
            AddSeparator();
            AddItem(nameof(ShowOutputButtonPressed));
            // AddItem(nameof(OpenButtonPressed));
            // popup.AddSeparator();
            // popup.AddItem("Save");
            // popup.AddItem("Save As...");
            // popup.AddSeparator();
            // popup.AddItem("Export To Ableton");
            // popup.AddItem("Export To FLStudio");
            // popup.AddSeparator();
            // popup.AddItem("Show Output");
            saveFileDialog.Connect("file_selected", this, nameof(OnSaveFileDialogFileSelected));
            openFileDialog.Connect("file_selected", this, nameof(OnOpenFileDialogFileSelected));
            CallDeferred(nameof(GenerateWindows));
        }

        private void GenerateWindows()
        {
            saveFileDialog.Mode = Godot.FileDialog.ModeEnum.SaveFile;
            saveFileDialog.Access = Godot.FileDialog.AccessEnum.Filesystem;
            saveFileDialog.Filters = FILE_FILTERS;
            saveFileDialog.WindowTitle = "Save VST Database";
            WindowContainer.Instance.AddChild(saveFileDialog);

            openFileDialog.Mode = Godot.FileDialog.ModeEnum.OpenFile;
            openFileDialog.Access = Godot.FileDialog.AccessEnum.Filesystem;
            openFileDialog.Filters = FILE_FILTERS;
            openFileDialog.WindowTitle = "Open VST Database";
            WindowContainer.Instance.AddChild(openFileDialog);
        }

        [PopupItemAttribute("New")]
        public void NewButtonPressed()
        {
            PluginServer.Instance.Clear();
        }

        [PopupItemAttribute("Open")]
        public void OpenButtonPressed()
        {
            openFileDialog.PopupCenteredRatio();
        }

        [PopupItemAttribute("Save")]
        public void SaveButtonPressed()
        {
            saveFileDialog.PopupCenteredRatio();
        }

        // [PopupItemAttribute("Test")]
        // public void TestButtonPressed() { }
        [PopupItemAttribute("Show Output")]
        public void ShowOutputButtonPressed()
        {
            WindowContainer.Instance.DisplayOutput();
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
