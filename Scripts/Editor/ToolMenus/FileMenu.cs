using Godot;
using PluginManager.PluginTree;
using PluginManager.Editor.Containers;

namespace PluginManager.Editor.ToolMenus
{
    public class FileMenu : MenuButtonExtended
    {
        private readonly string[] FILE_FILTERS = new string[] { "*.vstdb ; VST Database" };

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
        }

        [PopupItemAttribute("Output")]
        public void ShowOutputButtonPressed()
        {
            WindowContainer.Instance.DisplayOutput();
        }

        [PopupItemAttribute("File Properties")]
        public void ShowPropertiesButtonPressed()
        {
            PropertiesDialog propertiesDialog = new();
            WindowContainer.Instance.AddChild(propertiesDialog);
            propertiesDialog.Popup();
            Utils.MakePopupFreeable(propertiesDialog);
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
