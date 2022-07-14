using Godot;
using System.Collections.Generic;
using PluginManager.PluginTree;


namespace PluginManager.Editor
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
            EditorServer.Instance.Connect(nameof(EditorServer.FocusedFolderChanged), this, nameof(OnEditorInsFocusedFolderChanged));
            Tree = GetNode<TreeExtended>(TreePath);
            Tree.CreateItem();
            Tree.Connect(nameof(TreeExtended.ItemDropped), this, nameof(OnTreeExtendedItemDropped));
            Tree.Connect("item_selected", this, nameof(OnTreeItemSelected));
            PropertiesContainer = GetNode<VBoxContainer>(PropertiesPath);
            EditorServer.Instance.SetPropertiesContainer(PropertiesContainer);
            GetNode<TextureButton>(FolderButtonPath).Connect("pressed", this, nameof(OnFolderButtonPressed));
            GetNode<TextureButton>(PluginButtonPath).Connect("pressed", this, nameof(OnPluginButtonPressed));
            GetNode<TextureButton>(SeparatorButtonPath).Connect("pressed", this, nameof(OnSeparatorButtonPressed));
            GetNode<TextureButton>(DeleteButtonPath).Connect("pressed", this, nameof(OnDeleteButtonPressed));
            UpdateTree();
        }

        private void UpdateTree()
        {
            Tree.GetRoot()?.Free();
            if (EditorServer.Instance.FocusedFolder == null)
            {
                Tree.HideRoot = true;
                Tree.Update();
                return;
            }
            Tree.HideRoot = false;
            TreeItem rootTreeItem = Tree.CreateItem();
            TreeFolder rootTreeFolder = EditorServer.Instance.FocusedFolder;
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
            if (EditorServer.Instance.FocusedFolder == null)
                return;
            TreeEntity selectedEntity = GetSelectedEntity();
            if (selectedEntity == null)
            {
                EditorServer.Instance.FocusedFolder.AddChild(treeEntity);
                UpdateTree();
            }
            else if (selectedEntity is TreeFolder treeFolder)
            {
                treeFolder.AddChild(treeEntity);
                UpdateTree();
                treeFolder.SelectTreeItem();
            }
            else
            {
                selectedEntity.Parent.AddChildAfter(treeEntity, selectedEntity);
                UpdateTree();
                selectedEntity.SelectTreeItem();
            }
        }

        private TreeEntity GetSelectedEntity()
        {
            return (Tree.GetSelected()?.GetMetadata(0) as TreeItemContainer)?.Modifier;
        }

        public void OnEditorInsFocusedFolderChanged(TreeFolder newFocusedFolder)
        {
            EditorServer.Instance.ClearProperties();
            EditorServer.Instance.SelectedTreeEntity = newFocusedFolder;
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
            EditorServer.Instance.ClearProperties();
            TreeEntity treeEntity = GetSelectedEntity();
            if (treeEntity != null)
            {
                treeEntity.GenerateProperties();
                EditorServer.Instance.SelectedTreeEntity = treeEntity;
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
            if (treeEntity is TreeEntity && treeEntity != EditorServer.Instance.FocusedFolder)
            {
                treeEntity.Parent.RemoveChild(treeEntity);
                UpdateTree();
                EditorServer.Instance.ClearProperties();
                EditorServer.Instance.SelectedTreeEntity = null;
            }
        }
    }
}

