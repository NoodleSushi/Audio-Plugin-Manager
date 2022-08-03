using Godot;
using PluginManager.PluginTree;
using PluginManager.PluginTree.Components;

namespace PluginManager.Editor.Containers
{
    public class FolderTree : VBoxContainer
    {
        [Export]
        private readonly NodePath FolderButtonPath;

        [Export]
        private readonly NodePath DeleteButtonPath;

        [Export]
        private readonly NodePath TreePath;
        private TreeExtended Tree;

        public override void _Ready()
        {
            Tree = GetNode<TreeExtended>(TreePath);
            Tree.HideRoot = true;
            Tree.CreateItem();
            Tree.Connect(nameof(TreeExtended.ItemDropped), this, nameof(OnTreeExtendedItemDropped));
            Tree.Connect("item_selected", this, nameof(OnTreeExtendedItemSelected));
            GetNode<BaseButton>(FolderButtonPath)
                .Connect("pressed", this, nameof(OnFolderButtonPressed));
            GetNode<BaseButton>(DeleteButtonPath)
                .Connect("pressed", this, nameof(OnDeleteButtonPressed));
            PluginServer.Instance.Connect(nameof(PluginServer.Cleared), this, nameof(UpdateTree));
            PluginServer.Instance.Connect(
                nameof(PluginServer.Deserialized),
                this,
                nameof(UpdateTree)
            );
        }

        private void UpdateTree()
        {
            Tree.GetRoot()?.Free();
            TreeItem root = Tree.CreateItem();
            foreach (TreeFolder folder in PluginServer.Instance.FolderList)
            {
                TreeItem treeItem = Tree.CreateItem(root);
                treeItem.SetText(0, folder.GetComponent<Name>().NameString);
                treeItem.SetIcon(0, Resources.ICON_FOLDER);
                treeItem.SetMetadata(0, folder);
            }
            Tree.Update();
        }

        public void OnFolderButtonPressed()
        {
            TreeFolder newFolder = PluginServer.Instance.CreateFolder();
            Name nameComponent = newFolder.GetComponent<Name>();
            nameComponent.NameString = "Folder" + PluginServer.Instance.FolderList.Count;
            nameComponent.Connect(
                nameof(PluginTree.Components.Name.NameChanged),
                this,
                nameof(OnFolderNameChanged)
            );
            UpdateTree();
        }

        public void OnDeleteButtonPressed()
        {
            if (Tree.GetSelected()?.GetMetadata(0) is TreeFolder treeFolder)
            {
                if (EditorServer.Instance.SelectedTreeEntity == treeFolder)
                {
                    EditorServer.Instance.ClearProperties();
                    EditorServer.Instance.SelectedTreeEntity = null;
                }
                EditorServer.Instance.UnfocusFolder();
                PluginServer.Instance.RemoveFolder(treeFolder);
                UpdateTree();
            }
        }

        public void OnTreeExtendedItemSelected()
        {
            if (Tree.GetSelected()?.GetMetadata(0) is TreeFolder treeFolder)
            {
                EditorServer.Instance.ChangeFocusedFolder(treeFolder);
                treeFolder.DeferredGenerateProperties();
            }
        }

        private void OnTreeExtendedItemDropped(Godot.Object heldMetadata, Godot.Object landingMetadata, int dropSection)
        {
            PluginServer.Instance.ReorderFolderList((TreeFolder)heldMetadata, (TreeFolder)landingMetadata, dropSection);
            UpdateTree();
        }

        public void OnFolderNameChanged(string newName)
        {
            UpdateTree();
        }
    }
}
