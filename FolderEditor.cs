using Godot;
using System.Collections.Generic;
using PluginManager.PluginTree;


namespace PluginManager
{
    public class FolderEditor : Control
    {
        [Export]
        readonly private NodePath TreePath;
        [Export]
        readonly private NodePath FolderButtonPath;
        [Export]
        readonly private NodePath PluginButtonPath;
        [Export]
        readonly private NodePath SeparatorButtonPath;
        [Export]
        readonly private NodePath DeleteButtonPath;
        [Export]
        readonly private NodePath PropertiesPath;
        private TreeExtended Tree;
        private VBoxContainer PropertiesContainer;

        public override void _Ready()
        {
            Server.Instance.Connect(nameof(Server.FocusedFolderChanged), this, nameof(OnServerFocusedFolderChanged));
            Tree = GetNode<TreeExtended>(TreePath);
            Tree.CreateItem();
            Tree.Connect(nameof(TreeExtended.ItemDropped), this, nameof(OnTreeExtendedItemDropped));
            Tree.Connect("item_selected", this, nameof(OnTreeItemSelected));
            PropertiesContainer = GetNode<VBoxContainer>(PropertiesPath);
            GetNode<TextureButton>(FolderButtonPath).Connect("pressed", this, nameof(OnFolderButtonPressed));
            GetNode<TextureButton>(PluginButtonPath).Connect("pressed", this, nameof(OnPluginButtonPressed));
            GetNode<TextureButton>(SeparatorButtonPath).Connect("pressed", this, nameof(OnSeparatorButtonPressed));
            GetNode<TextureButton>(DeleteButtonPath).Connect("pressed", this, nameof(OnDeleteButtonPressed));
        }

        private void UpdateTree()
        {
            Tree.GetRoot()?.Free();
            if (Server.Instance.FocusedFolder == null)
            {
                Tree.HideRoot = true;
                Tree.Update();
                return;
            }
            Tree.HideRoot = false;
            TreeItem rootTreeItem = Tree.CreateItem();
            TreeFolder rootTreeFolder = Server.Instance.FocusedFolder;
            new TreeItemContainer(rootTreeItem, rootTreeFolder);

            Stack < (TreeItem treeItem, TreeFolder treeFolder, int idx) > trav = new();
            trav.Push((rootTreeItem, rootTreeFolder, 0));
            while (trav.Count > 0)
            {
                var branch = trav.Pop();
                if (branch.idx >= branch.treeFolder.Children.Count)
                    continue;
                TreeItem newTreeItem = Tree.CreateItem(branch.treeItem);
                TreeEntity childEntity = branch.treeFolder.Children[branch.idx++];
                trav.Push(branch);
                new TreeItemContainer(newTreeItem, childEntity);
                if (childEntity is TreeFolder newFolder)
                {
                    trav.Push((newTreeItem, newFolder, 0));
                }
            }
            Tree.Update();
        }

        public void AddOnSelected(TreeEntity treeEntity)
        {
            if (Server.Instance.FocusedFolder == null)
                return;
            TreeEntity selectedEntity = GetSelectedEntity();
            if (selectedEntity == null)
            {
                Server.Instance.FocusedFolder.AddChild(treeEntity);
                UpdateTree();
            }
            else if (selectedEntity is TreeFolder treeFolder)
            {
                treeFolder.AddChild(treeEntity);
                UpdateTree();
            }
            else
            {
                selectedEntity.Parent.AddChildAfter(treeEntity, selectedEntity);
                UpdateTree();
            }
        }

        private TreeEntity GetSelectedEntity()
        {
            return (Tree.GetSelected()?.GetMetadata(0) as TreeItemContainer)?.Modifier;
        }

        private void ClearPropertiesContainer()
        {
            foreach (Node child in PropertiesContainer.GetChildren())
            {
                child.QueueFree();
            }
        }

        public void OnServerFocusedFolderChanged(TreeFolder newFocusedFolder)
        {
            ClearPropertiesContainer();
            Server.Instance.SelectedTreeEntity = null;
            UpdateTree();
        }

        private void OnTreeExtendedItemDropped(TreeItem heldItem, TreeItem landingItem, int dropSection)
        {
            TreeEntity heldEntity = (heldItem.GetMetadata(0) as TreeItemContainer).Modifier;
            TreeEntity landingEntity = (landingItem.GetMetadata(0) as TreeItemContainer).Modifier;
            

            // Check if folder is assigned as a child of itself
            if (heldEntity is TreeFolder)
            {
                for (TreeEntity trav = landingEntity; trav.Parent != null; trav = trav.Parent)
                {
                    if (trav == heldEntity)
                        return;
                }
            }

            switch (dropSection)
            {
                case 0:
                    if (landingEntity is TreeFolder landingFolder)
                    {
                        landingFolder.AddChild(heldEntity);
                        UpdateTree();
                    }
                    break;
                case -1:
                    landingEntity.Parent.AddChildBefore(heldEntity, landingEntity);
                    UpdateTree();
                    break;
                case 1:
                    landingEntity.Parent.AddChildAfter(heldEntity, landingEntity);
                    UpdateTree();
                    break;
            }
        }

        public void OnTreeItemSelected()
        {
            ClearPropertiesContainer();
            TreeEntity treeEntity = GetSelectedEntity();
            if (treeEntity != null)
            {
                treeEntity.GenerateProperties(PropertiesContainer);
                Server.Instance.SelectedTreeEntity = treeEntity;
            }
        }

        public void OnFolderButtonPressed()
        {
            AddOnSelected(TreeEntityFactory.CreateFolder());
        }

        public void OnPluginButtonPressed()
        {
            AddOnSelected(TreeEntityFactory.CreatePlugin());
        }

        public void OnSeparatorButtonPressed()
        {
            AddOnSelected(TreeEntityFactory.CreateSeparator());
        }

        public void OnDeleteButtonPressed()
        {
            TreeEntity treeEntity = GetSelectedEntity();
            if (treeEntity is TreeEntity && treeEntity != Server.Instance.FocusedFolder)
            {
                treeEntity.Parent.RemoveChild(treeEntity);
                UpdateTree();
                ClearPropertiesContainer();
                Server.Instance.SelectedTreeEntity = null;
            }
        }
    }
}

