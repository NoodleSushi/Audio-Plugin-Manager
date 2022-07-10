using Godot;
using System;
using PluginManager.PluginTree;


namespace PluginManager
{
    public class FolderTree : VBoxContainer
    {
        [Export]
        readonly private NodePath FolderButtonPath;
        [Export]
        readonly private NodePath DeleteButtonPath;
        [Export]
        readonly private NodePath TreePath;
        private TreeExtended Tree;

        public override void _Ready()
        {
            Tree = GetNode<TreeExtended>(TreePath);
            Tree.CreateItem();
            Tree.Connect(nameof(TreeExtended.ItemDropped), this, nameof(OnTreeExtendedItemDropped));
            Tree.Connect("item_selected", this, nameof(OnTreeExtendedItemSelected));
            GetNode<TextureButton>(FolderButtonPath).Connect("pressed", this, nameof(OnFolderButtonPressed));
            GetNode<TextureButton>(DeleteButtonPath).Connect("pressed", this, nameof(OnDeleteButtonPressed));
        }

        private void UpdateTree()
        {
            Tree.GetRoot()?.Free();
            TreeItem root = Tree.CreateItem();
            foreach (TreeFolder folder in Server.Instance.FolderList)
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
            TreeFolder newFolder = Server.Instance.CreateFolder();
            newFolder.GetComponent<Name>().NameString = "Folder" + Server.Instance.FolderList.Count;
            UpdateTree();
        }

        public void OnDeleteButtonPressed()
        {
            if (Tree.GetSelected()?.GetMetadata(0) is TreeFolder treeFolder)
            {
                Server.Instance.RemoveFolder(treeFolder);
                UpdateTree();
            }
        }

        public void OnTreeExtendedItemSelected()
        {
            if (Tree.GetSelected()?.GetMetadata(0) is TreeFolder treeFolder)
            {
                Server.Instance.ChangeFocusedFolder(treeFolder);
            }
        }

        private void OnTreeExtendedItemDropped(TreeItem heldItem, TreeItem landingItem, int dropSection)
        {
            Server.Instance.ReorderFolderList(
                (TreeFolder)heldItem.GetMetadata(0),
                (TreeFolder)landingItem.GetMetadata(0),
                dropSection
            );
            UpdateTree();
        }
    }
}

