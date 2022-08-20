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
            AddItem(nameof(SaveAsButtonPressed));
            AddSeparator();
            AddItem(nameof(ExportButtonPressed));
            AddSeparator();
            AddItem(nameof(ShowOutputButtonPressed));
            AddItem(nameof(ShowPropertiesButtonPressed));
        }

        [PopupItemAttribute("New (Ctrl+N)")]
        public void NewButtonPressed()
        {
            PluginServer.Instance.Clear();
        }

        [PopupItemAttribute("Open (Ctrl+O)")]
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

        [PopupItemAttribute("Save (Ctrl+S)")]
        public void SaveButtonPressed()
        {
            if (EditorServer.Instance.IsProjectPathValid())
            {
                OnSaveFileDialogFileSelected(null);
            }
            else
            {
                WindowContainer.Instance.DisplayFileDialog(
                    FileDialog.ModeEnum.SaveFile,
                    this,
                    nameof(OnSaveFileDialogFileSelected),
                    FILE_FILTERS,
                    "Save VST Database"
                );
            }
        }

        [PopupItemAttribute("Save As... (Ctrl+Shift+S)")]
        public void SaveAsButtonPressed()
        {
            WindowContainer.Instance.DisplayFileDialog(
                FileDialog.ModeEnum.SaveFile,
                this,
                nameof(OnSaveFileDialogFileSelected),
                FILE_FILTERS,
                "Save VST Database"
            );
        }

        [PopupItemAttribute("Export")]
        public void ExportButtonPressed()
        {
            var Exporter = Resources.ExporterScene.Instance<AcceptDialog>();
            WindowContainer.Instance.AddChild(Exporter);
            Exporter.PopupCenteredRatio();
            Utils.MakePopupFreeable(Exporter);
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
            string outputDialog = "An error occurred while reading the VSTDB file.\n\n";
            bool success = EditorServer.Instance.LoadProject(path, out var errMsg);
            if (!success)
            {
                outputDialog += errMsg;
                WindowContainer.Instance.DisplayOutput(outputDialog);
            }
        }

        private void OnSaveFileDialogFileSelected(string path)
        {
            string outputDialog = "An error occurred while saving as a VSTDB file.\n\n";
            bool success = EditorServer.Instance.SaveProject(path, out var errMsg);
            if (!success)
            {
                outputDialog += errMsg;
                WindowContainer.Instance.DisplayOutput(outputDialog);
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey eventKey && eventKey.Control && eventKey.Pressed)
            {
                if (eventKey.Scancode == (int)KeyList.S)
                {
                    if (eventKey.Shift)
                    {
                        SaveAsButtonPressed();
                    }
                    else
                    {
                        SaveButtonPressed();
                    }
                }
                else if (eventKey.Scancode == (int)KeyList.N)
                {
                    NewButtonPressed();
                }
                else if (eventKey.Scancode == (int)KeyList.O)
                {
                    OpenButtonPressed();
                }
            }
        }
    }
}
