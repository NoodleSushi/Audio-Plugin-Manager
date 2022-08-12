using Godot;
using System.Collections.Generic;
using PluginManager.PluginTree;
using PluginManager.PluginTree.Components;
using System.Linq;

namespace PluginManager.Editor.Containers
{
    public class FolderEditor : Control
    {
        [Export]
        private readonly NodePath TreePath;

        [Export]
        private readonly NodePath FolderButtonPath;

        [Export]
        private readonly NodePath PluginButtonPath;

        [Export]
        private readonly NodePath SeparatorButtonPath;

        [Export]
        private readonly NodePath DeleteButtonPath;
        [Export]
        private readonly NodePath CutButtonPath;
        [Export]
        private readonly NodePath CopyRefButtonPath;

        [Export]
        private readonly NodePath CopyButtonPath;

        [Export]
        private readonly NodePath PasteButtonPath;

        [Export]
        private readonly NodePath PropertiesPath;
        private TreeExtended Tree;
        private VBoxContainer PropertiesContainer;
        private TreeEntity Clipboard;
        private TreeEntity ClipboardOriginal;
        private bool _isCut = false;

        public override void _Ready()
        {
            EditorServer.Instance.Connect(
                nameof(EditorServer.FocusedFolderChanged),
                this,
                nameof(OnEditorInsFocusedFolderChanged)
            );
            EditorServer.Instance.Connect(
                nameof(EditorServer.CallFolderEditorRefresh),
                this,
                nameof(UpdateTree)
            );
            Tree = GetNode<TreeExtended>(TreePath);
            Tree.SelectMode = Godot.Tree.SelectModeEnum.Multi;
            Tree.CreateItem();
            Tree.Connect(nameof(TreeExtended.ItemDropped), this, nameof(OnTreeExtendedItemDropped));
            Tree.Connect(nameof(TreeExtended.ItemsDropped), this, nameof(OnTreeExtendedItemsDroppedDeferred));
            Tree.Connect("item_selected", this, nameof(OnTreeItemSelected));
            Tree.Connect("item_collapsed", this, nameof(OnTreeItemCollapsed));
            PropertiesContainer = GetNode<VBoxContainer>(PropertiesPath);
            EditorServer.Instance.SetPropertiesContainer(PropertiesContainer);
            GetNode<BaseButton>(FolderButtonPath).Connect("pressed", this, nameof(OnFolderButtonPressed));
            GetNode<BaseButton>(PluginButtonPath).Connect("pressed", this, nameof(OnPluginButtonPressed));
            GetNode<BaseButton>(SeparatorButtonPath).Connect("pressed", this, nameof(OnSeparatorButtonPressed));
            GetNode<BaseButton>(DeleteButtonPath).Connect("pressed", this, nameof(OnDeleteButtonPressed));
            GetNode<BaseButton>(CutButtonPath).Connect("pressed", this, nameof(OnCutButtonPressed));
            GetNode<BaseButton>(CopyRefButtonPath).Connect("pressed", this, nameof(OnCopyRefButtonPressed));
            GetNode<BaseButton>(CopyButtonPath).Connect("pressed", this, nameof(OnCopyButtonPressed));
            GetNode<BaseButton>(PasteButtonPath).Connect("pressed", this, nameof(OnPasteButtonPressed));
            UpdateTree();
        }

        private void UpdateTree()
        {
            // Don't display anything if no folder is selected
            Tree.GetRoot()?.Free();
            if (EditorServer.Instance.FocusedFolder == null)
            {
                Tree.HideRoot = true;
                Tree.Update();
                return;
            }
            // Generate Tree
            Tree.HideRoot = false;
            TreeItem rootTreeItem = Tree.CreateItem();
            TreeFolder rootTreeFolder = EditorServer.Instance.FocusedFolder;
            new TreeItemContainer(rootTreeItem, rootTreeFolder);

            Stack<(TreeItem treeItem, TreeFolder treeFolder, int idx)> trav = new();
            trav.Push((rootTreeItem, rootTreeFolder, 0));
            while (trav.Count > 0)
            {
                var branch = trav.Pop();
                if (branch.idx >= branch.treeFolder.Children.Count)
                    continue;
                TreeItem newTreeItem = Tree.CreateItem(branch.treeItem);
                TreeEntity childEntity = branch.treeFolder.Children[branch.idx++];
                new TreeItemContainer(newTreeItem, childEntity);
                trav.Push(branch);
                if (childEntity is TreeFolder newFolder)
                {
                    trav.Push((newTreeItem, newFolder, 0));
                }
            }
            Tree.Update();
        }

        public bool AddOnSelected(TreeEntity treeEntity)
        {
            if (EditorServer.Instance.FocusedFolder == null)
                return false;
            TreeEntity selectedEntity = GetSelectedEntity();
            if (selectedEntity == null)
            {
                EditorServer.Instance.FocusedFolder.AddChild(treeEntity);
                UpdateTree();
                return true;
            }
            else if (selectedEntity is TreeFolder treeFolder)
            {
                treeFolder.AddChild(treeEntity);
                UpdateTree();
                treeFolder.SelectTreeItem();
                return true;
            }
            else
            {
                selectedEntity.Parent.AddChildAfter(treeEntity, selectedEntity);
                UpdateTree();
                selectedEntity.SelectTreeItem();
                return true;
            }
        }

        private TreeEntity GetSelectedEntity()
        {
            return (Tree.GetSelected()?.GetMetadata(0) as TreeItemContainer)?.Modifier;
        }

        private void OnEditorInsFocusedFolderChanged(TreeFolder newFocusedFolder)
        {
            EditorServer.Instance.ClearProperties();
            EditorServer.Instance.SelectedTreeEntity = newFocusedFolder;
            UpdateTree();
        }

        private void OnTreeExtendedItemDropped(Godot.Object heldMetadata, Godot.Object landingMetadata, int dropSection)
        {
            TreeEntity heldEntity = (heldMetadata as TreeItemContainer)?.Modifier;
            TreeEntity landingEntity = (landingMetadata as TreeItemContainer)?.Modifier;
            if ((heldEntity ?? landingEntity) is null)
                return;
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
                    landingEntity.Parent?.AddChildBefore(heldEntity, landingEntity);
                    UpdateTree();
                    break;
                case 1:
                    landingEntity.Parent?.AddChildAfter(heldEntity, landingEntity);
                    UpdateTree();
                    break;
            }
        }

        private void OnTreeExtendedItemsDroppedDeferred(List<Godot.Object> heldMetadatas, Godot.Object landingMetadata, int dropSection)
        {
            CallDeferred(nameof(OnTreeExtendedItemsDropped), heldMetadatas, landingMetadata, dropSection);
        }

        private void OnTreeExtendedItemsDropped(List<Godot.Object> heldMetadatas, Godot.Object landingMetadata, int dropSection)
        {
            if ((landingMetadata as TreeItemContainer)?.Modifier is not TreeEntity landingEntity)
                return;
            GD.Print(landingEntity.GetComponent<Name>().NameString);
            switch (dropSection)
            {
                case 0:
                    if (landingEntity is TreeFolder landingFolder)
                    {
                        foreach (TreeItemContainer heldMetadata in heldMetadatas.Cast<TreeItemContainer>())
                        {
                            if (heldMetadata?.Modifier is TreeEntity heldEntity)
                                landingFolder.AddChild(heldEntity);
                        }
                        UpdateTree();
                    }
                    break;
                case -1:
                    if (landingEntity.Parent is not null)
                    {
                        foreach (TreeItemContainer heldMetadata in heldMetadatas.Cast<TreeItemContainer>())
                        {
                            if (heldMetadata?.Modifier is TreeEntity heldEntity)
                                landingEntity.Parent.AddChildBefore(heldEntity, landingEntity);
                        }
                        UpdateTree();
                    }
                    break;
                case 1:
                    if (landingEntity.Parent is not null)
                    {
                        foreach (TreeItemContainer heldMetadata in heldMetadatas.Cast<TreeItemContainer>())
                        {
                            if (heldMetadata?.Modifier is TreeEntity heldEntity)
                                landingEntity.Parent.AddChildAfter(heldEntity, landingEntity);
                        }
                        UpdateTree();
                    }
                    break;
            }
        }

        private void OnTreeItemSelected()
        {
            EditorServer.Instance.ClearProperties();
            TreeEntity treeEntity = GetSelectedEntity();
            if (treeEntity != null)
            {
                treeEntity.DeferredGenerateProperties();
                EditorServer.Instance.SelectedTreeEntity = treeEntity;
            }
        }

        private void OnTreeItemCollapsed(TreeItem item)
        {
            if ((item.GetMetadata(0) as TreeItemContainer)?.Modifier is TreeFolder treeFolder)
            {
                treeFolder.Collapsed = item.Collapsed;
            }
        }

        private void OnFolderButtonPressed()
        {
            AddOnSelected(TreeEntityFactory.CreateFolder());
        }

        private void OnPluginButtonPressed()
        {
            AddOnSelected(TreeEntityFactory.CreatePlugin());
        }

        private void OnSeparatorButtonPressed()
        {
            AddOnSelected(TreeEntityFactory.CreateSeparator());
        }

        private void OnDeleteButtonPressed()
        {
            foreach (Godot.Object container in Tree.GroupedSelectedMetadatas)
            {
                if ((container as TreeItemContainer)?.Modifier is TreeEntity treeEntity)
                {
                    if (treeEntity != EditorServer.Instance.FocusedFolder)
                    {
                        treeEntity.Unparent();
                        UpdateTree();
                        EditorServer.Instance.ClearProperties();
                        EditorServer.Instance.SelectedTreeEntity = null;
                    }
                }
            }
        }

        private void OnCutButtonPressed()
        {
            _isCut = true;
            if (GetSelectedEntity() is TreeEntity treeEntity)
            {
                Clipboard = treeEntity.Clone();
                ClipboardOriginal = treeEntity;
            }
        }

        private void OnCopyRefButtonPressed()
        {
            if (GetSelectedEntity() is TreeEntity treeEntity && treeEntity.GetComponent<Identifier>().Value != "Reference")
            {
                _isCut = false;
                TreeEntity folderRef = TreeEntityFactory.CreateReference();
                folderRef.GetComponent<ReferenceData>().TreeEntityRef = treeEntity;
                folderRef.GetComponent<ReferenceData>().CopyFromRef();
                Clipboard = folderRef;
            }
        }

        private void OnCopyButtonPressed()
        {
            _isCut = false;
            if (GetSelectedEntity() is TreeEntity treeEntity)
            {
                Clipboard = treeEntity.Clone();
            }
        }

        private void OnPasteButtonPressed()
        {
            if (Clipboard is not null)
            {
                if (AddOnSelected(Clipboard) && _isCut)
                {
                    ClipboardOriginal?.Unparent();
                    UpdateTree();
                }
            }
        }
    }
}
