using Godot;
using System.Collections.Generic;
using PluginManager.PluginTree;

namespace PluginManager.Editor
{
    public class EditorServer : Node
    {
        [Signal]
        public delegate void FocusedFolderChanged(TreeFolder newFocusedFolder);
        [Signal]
        public delegate void TreeEntityChanged();
        [Signal]
        public delegate void CallFolderEditorRefresh();
        [Signal]
        public delegate void SelectedEntitiesAsked();
        [Signal]
        public delegate void GroupedSelectedEntitiesAsked();

        private static EditorServer _instance;
        private TreeFolder _focusedFolder;
        private VBoxContainer _propertiesContainer;
        private TreeEntity _selectedTreeEntity = null;
        public TreeFolder FocusedFolder => _focusedFolder;
        public TreeEntity SelectedTreeEntity
        {
            get => _selectedTreeEntity;
            set
            {
                _selectedTreeEntity = value;
                EmitSignal(nameof(TreeEntityChanged));
            }
        }
        private List<TreeEntity> _selectedTreeEntities = null;
        public List<TreeEntity> SelectedTreeEntities
        {
            get
            {
                EmitSignal(nameof(SelectedEntitiesAsked));
                return _selectedTreeEntities;
            }
            set => _selectedTreeEntities = value;
        }
        private List<TreeEntity> _groupedSelectedTreeEntities = null;
        public List<TreeEntity> GroupedSelectedTreeEntities
        {
            get
            {
                EmitSignal(nameof(GroupedSelectedEntitiesAsked));
                return _groupedSelectedTreeEntities;
            }
            set => _groupedSelectedTreeEntities = value;
        }
        private string _projectPath = "";
        public static EditorServer Instance => _instance;

        public override void _EnterTree()
        {
            _instance = this;
        }

        public override void _Ready()
        {
            PluginServer.Instance.Connect("Cleared", this, nameof(OnPluginServerCleared));
        }


        public EditorServer()
        {
            PluginServer.Instance.Connect(
                nameof(PluginServer.Cleared),
                this,
                nameof(OnServerCleared)
            );
        }

        public void RefreshFolderEditor()
        {
            EmitSignal(nameof(CallFolderEditorRefresh));
        }

        // Properties Container

        public void SetPropertiesContainer(VBoxContainer propertiesContainer)
        {
            _propertiesContainer = propertiesContainer;
        }

        public void ClearProperties()
        {
            foreach (Node child in _propertiesContainer.GetChildren())
                child.QueueFree();
        }

        public void AddProperty(Control property)
        {
            _propertiesContainer.AddChild(property);
        }

        // Folder Manipulation

        public void ChangeFocusedFolder(TreeFolder newFocusedFolder)
        {
            _focusedFolder = newFocusedFolder;
            SelectedTreeEntity = newFocusedFolder;
            EmitSignal(nameof(FocusedFolderChanged), newFocusedFolder);
        }

        public void UnfocusFolder() => ChangeFocusedFolder(null);

        private void OnServerCleared()
        {
            ClearProperties();
            UnfocusFolder();
        }

        // IO

        public bool IsProjectPathValid()
        {
            return _projectPath != "" && new Directory().FileExists(_projectPath);
        }

        public bool LoadProject(string path, out string message)
        {
            bool success = true;
            message = "";
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
                    message = error.ToString();
                    PluginServer.Instance.Clear();
                }
                file.Close();
            }
            if (success)
                _projectPath = path;
            return success;
        }

        public bool SaveProject(string path, out string message)
        {
            path ??= _projectPath;
            bool success = true;
            message = "";
            using (File file = new())
            {
                file.Open(path, File.ModeFlags.Write);
                try
                {
                    file.StoreString(PluginServer.Instance.Serialize());
                }
                catch (System.Exception error)
                {
                    success = false;
                    message = error.ToString();
                }
                file.Close();
            }
            if (success)
                _projectPath = path;
            return success;
        }

        // Signals

        public void OnPluginServerCleared()
        {
            _projectPath = "";
        }
    }
}
