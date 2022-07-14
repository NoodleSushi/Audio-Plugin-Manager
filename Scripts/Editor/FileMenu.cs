using Godot;
using PluginManager.PluginTree;

namespace PluginManager.Editor
{
    public class FileMenu : MenuButtonExtended
    {
        private Godot.FileDialog saveFileDialog = new();
        private Godot.FileDialog openFileDialog = new();

        public override void _Ready()
        {
            base._Ready();
            AddItem(nameof(NewButtonPressed));
            AddItem(nameof(OpenButtonPressed));
            AddSeparator();
            AddItem(nameof(SaveButtonPressed));
            AddItem(nameof(TestButtonPressed));
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
            saveFileDialog.Filters = new string[]{"*.vstdb ; VST Database"};
            saveFileDialog.WindowTitle = "Save VST Database";
            WindowContainer.Instance.AddChild(saveFileDialog);

            openFileDialog.Mode = Godot.FileDialog.ModeEnum.OpenFile;
            openFileDialog.Access = Godot.FileDialog.AccessEnum.Filesystem;
            openFileDialog.Filters = new string[]{"*.vstdb ; VST Database"};
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
        
        [PopupItemAttribute("Test")]
        public void TestButtonPressed()
        {
            
        }

        private void OnOpenFileDialogFileSelected(string path)
        {
            File file = new();
            file.Open(path, File.ModeFlags.Read);
            PluginServer.Instance.Deserialize(file.GetAsText());
            file.Close();
        }

        private void OnSaveFileDialogFileSelected(string path)
        {
            File file = new();
            file.Open(path, File.ModeFlags.Write);
            file.StoreString(PluginServer.Instance.Serialize());
            file.Close();
            GD.Print(path);
        }
    }
}