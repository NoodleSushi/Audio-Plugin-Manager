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
        private List<TreeEntity> Clipboard;
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
            EditorServer.Instance.Connect(
                nameof(EditorServer.SelectedEntitiesAsked),
                this,
                nameof(OnEditorInsSelectedEntitiesAsked)
            );
            EditorServer.Instance.Connect(
                nameof(EditorServer.GroupedSelectedEntitiesAsked),
                this,
                nameof(OnEditorInsGroupedSelectedEntitiesAsked)
            );
            Tree = GetNode<TreeExtended>(TreePath);
            Tree.SelectMode = Godot.Tree.SelectModeEnum.Multi;
            Tree.CreateItem();
            Tree.Connect(nameof(TreeExtended.ItemDropped), this, nameof(OnTreeExtendedItemDropped));
            Tree.Connect(nameof(TreeExtended.ItemsDropped), this, nameof(OnTreeExtendedItemsDroppedDeferred));
            Tree.Connect("item_selected", this, nameof(OnTreeItemSelected));
            Tree.Connect("item_collapsed", this, nameof(OnTreeItemCollapsed));
            Tree.Connect("gui_input", this, nameof(OnTreeGuiInput));
            Tree.Connect("button_pressed", this, nameof(OnTreeButtonPressed));
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
            rootTreeItem.ContainTreeEntity(rootTreeFolder);

            Stack<(TreeItem treeItem, TreeFolder treeFolder, int idx)> trav = new();
            trav.Push((rootTreeItem, rootTreeFolder, 0));
            while (trav.Count > 0)
            {
                var branch = trav.Pop();
                if (branch.idx >= branch.treeFolder.Children.Count)
                    continue;
                TreeItem newTreeItem = Tree.CreateItem(branch.treeItem);
                TreeEntity childEntity = branch.treeFolder.Children[branch.idx++];
                newTreeItem.ContainTreeEntity(childEntity);
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
            }
            else
            {
                selectedEntity.AddChildAfter(treeEntity);
            }
            UpdateTree();
            selectedEntity.SelectTreeItem();
            return true;
        }

        public bool AddOnSelected(IEnumerable<TreeEntity> treeEntities)
        {
            if (EditorServer.Instance.FocusedFolder == null)
                return false;
            TreeEntity selectedEntity = GetSelectedEntity();
            if (selectedEntity == null)
            {
                EditorServer.Instance.FocusedFolder.AddChildren(treeEntities);
                UpdateTree();
                return true;
            }
            else if (selectedEntity is TreeFolder treeFolder)
            {
                treeFolder.AddChildren(treeEntities);
            }
            else
            {
                selectedEntity.AddChildrenAfter(treeEntities);
            }
            UpdateTree();
            treeEntities.LastOrDefault()?.SelectTreeItem();
            return true;
        }

        private TreeEntity GetSelectedEntity()
        {
            return Tree.GetSelected()?.GetTreeEntity();
        }

        private List<TreeEntity> GetGroupedSelectedEntities()
        {
            return Tree.GroupedSelectedMetadatas
                .OfType<TreeItemContainer>()
                .Select(x => x.Modifier)
                .ToList();
        }

        private void OnEditorInsFocusedFolderChanged(TreeFolder newFocusedFolder)
        {
            EditorServer.Instance.ClearProperties();
            EditorServer.Instance.SelectedTreeEntity = newFocusedFolder;
            UpdateTree();
        }

        private void OnEditorInsSelectedEntitiesAsked()
        {
            EditorServer.Instance.SelectedTreeEntities = Tree.SelectedMetadatas
                .OfType<TreeItemContainer>()
                .Select(x => x.Modifier)
                .ToList();
        }

        private void OnEditorInsGroupedSelectedEntitiesAsked()
        {
            EditorServer.Instance.GroupedSelectedTreeEntities = GetGroupedSelectedEntities();
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
                    landingEntity.AddChildBefore(heldEntity);
                    UpdateTree();
                    break;
                case 1:
                    landingEntity.AddChildAfter(heldEntity);
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
            List<TreeEntity> heldEntities = heldMetadatas
                .OfType<TreeItemContainer>()
                .Select(x => x.Modifier)
                .ToList();
            switch (dropSection)
            {
                case 0:
                    if (landingEntity is TreeFolder landingFolder)
                    {
                        landingFolder.AddChildren(heldEntities);
                        UpdateTree();
                    }
                    break;
                case -1:
                    if (landingEntity.Parent is not null)
                    {
                        landingEntity.AddChildrenBefore(heldEntities);
                        UpdateTree();
                    }
                    break;
                case 1:
                    if (landingEntity.Parent is not null)
                    {
                        landingEntity.AddChildrenAfter(heldEntities);
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
            if (item.GetTreeEntity() is TreeFolder treeFolder)
            {
                treeFolder.Collapsed = item.Collapsed;
            }
        }

        private void OnTreeGuiInput(InputEvent @event)
        {
            if (!Tree.HasFocus())
                return;
            if (@event is InputEventKey eventKey && eventKey.Pressed)
            {
                if (eventKey.Control)
                {
                    if (eventKey.Scancode == (int)KeyList.C)
                    {
                        if (eventKey.Shift)
                            OnCopyRefButtonPressed();
                        else
                            OnCopyButtonPressed();
                    }
                    else if (eventKey.Scancode == (int)KeyList.V)
                    {
                        OnPasteButtonPressed();
                    }
                    else if (eventKey.Scancode == (int)KeyList.X)
                    {
                        OnCutButtonPressed();
                    }
                }
                if (eventKey.Scancode == (int)KeyList.Delete)
                    OnDeleteButtonPressed();
            }
        }

        private void OnTreeButtonPressed(TreeItem item, int column, int id)
        {
            item.GetTreeEntity()?.ButtonPressed(column, id);
        }

        private void OnFolderButtonPressed()
        {
            AddOnSelected(TreeEntityFactory.GetTreeEntityPreset("Folder"));
        }

        private void OnPluginButtonPressed()
        {
            AddOnSelected(TreeEntityFactory.GetTreeEntityPreset("Plugin"));
        }

        private void OnSeparatorButtonPressed()
        {
            AddOnSelected(TreeEntityFactory.CreateSeparator());
        }

        private void OnDeleteButtonPressed()
        {
            IEnumerable<TreeEntity> treeItems = Tree.GroupedSelectedMetadatas
                .OfType<TreeItemContainer>()
                .Select(x => x.Modifier)
                .Where(x => x != EditorServer.Instance.FocusedFolder);

            foreach (TreeEntity treeItem in treeItems)
            {
                treeItem.Unparent();
            }

            UpdateTree();
            EditorServer.Instance.ClearProperties();
            EditorServer.Instance.SelectedTreeEntity = null;
        }

        private void OnCutButtonPressed()
        {
            _isCut = true;
            Clipboard = GetGroupedSelectedEntities();
        }

        private void OnCopyRefButtonPressed()
        {
            _isCut = false;
            Clipboard = GetGroupedSelectedEntities()
                .Where(x => x.GetComponent<Identifier>().Value is not "Reference" or "Separator")
                .Select(x =>
                {
                    TreeEntity folderRef = TreeEntityFactory.GetTreeEntityPreset("Reference");
                    ReferenceData refData = folderRef.GetComponent<ReferenceData>();
                    refData.TreeEntityRef = x;
                    refData.CopyFromRef();
                    return folderRef;
                })
                .ToList();
        }

        private void OnCopyButtonPressed()
        {
            _isCut = false;
            Clipboard = GetGroupedSelectedEntities();
        }

        private void OnPasteButtonPressed()
        {
            if (EditorServer.Instance.FocusedFolder is null || Clipboard.Count == 0)
                return;
            AddOnSelected(Clipboard.Select(x => x.Clone()));
            if (_isCut)
            {
                Clipboard.ForEach(x => x.Unparent());
                Clipboard.Clear();
            }
            UpdateTree();
        }
    }
}
