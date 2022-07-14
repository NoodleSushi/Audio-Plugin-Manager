using Godot;
using PluginManager.PluginTree;

namespace PluginManager.Editor
{
    public class EditorServer : Godot.Object
    {
        [Signal]
        public delegate void FocusedFolderChanged(TreeFolder newFocusedFolder);
        [Signal]
        public delegate void TreeEntityChanged();

        private static EditorServer _instance;
        private TreeFolder _focusedFolder;
        private VBoxContainer _propertiesContainer;
        private TreeEntity _selectedTreeEntity = null;
        public TreeFolder FocusedFolder => _focusedFolder;
        public TreeEntity SelectedTreeEntity
        {
            get => _selectedTreeEntity;
            set {
                _selectedTreeEntity = value;
                EmitSignal(nameof(TreeEntityChanged));
            }
        }
        public static EditorServer Instance
        {   
            get 
            {
                if (_instance == null)
                {
                    _instance = new();
                }
                return _instance;
            }
        }

        public EditorServer()
        {
            PluginServer.Instance.Connect(nameof(PluginServer.Cleared), this, nameof(OnServerCleared));
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
    }
}