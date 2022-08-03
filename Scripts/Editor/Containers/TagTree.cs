using Godot;
using PluginManager.PluginTree;
using PluginManager.PluginTree.Components;

namespace PluginManager.Editor.Containers
{
    public class TagTree : VBoxContainer
    {
        private const int BUTTON_VISIBILITY = 0;
        private const int BUTTON_ADD = 1;

        [Export]
        private readonly NodePath TagButtonPath;

        [Export]
        private readonly NodePath DeleteButtonPath;

        [Export]
        private readonly NodePath TreePath;
        private TreeExtended Tree;

        public override void _Ready()
        {
            Tree = GetNode<TreeExtended>(TreePath);
            Tree.CreateItem();
            Tree.Connect(nameof(TreeExtended.ItemDropped), this, nameof(OnTreeExtendedItemDropped));
            Tree.Connect("item_selected", this, nameof(OnTreeExtendedItemSelected));
            Tree.Connect("button_pressed", this, nameof(OnTreeButtonPressed));
            GetNode<BaseButton>(TagButtonPath).Connect("pressed", this, nameof(OnTagButtonPressed));
            GetNode<BaseButton>(DeleteButtonPath)
                .Connect("pressed", this, nameof(OnDeleteButtonPressed));
            PluginServer.Instance.Connect(nameof(PluginServer.Cleared), this, nameof(UpdateTree));
            PluginServer.Instance.Connect(
                nameof(PluginServer.Deserialized),
                this,
                nameof(UpdateTree)
            );
            EditorServer.Instance.Connect(
                nameof(EditorServer.FocusedFolderChanged),
                this,
                nameof(OnEditorInsFocusedFolderChanged)
            );
            EditorServer.Instance.Connect(
                nameof(EditorServer.TreeEntityChanged),
                this,
                nameof(UpdateTree)
            );
        }

        public void OnTagButtonPressed()
        {
            Tag newTag = PluginServer.Instance.CreateTag();
            newTag.Name = "Tag" + PluginServer.Instance.TagList.Count;
            newTag.Connect(nameof(Tag.NameChanged), this, nameof(OnTagNameChanged));
            UpdateTree();
        }

        public void OnDeleteButtonPressed()
        {
            if (Tree.GetSelected()?.GetMetadata(0) is Tag tag)
            {
                tag.NotifyDeletion();
                PluginServer.Instance.RemoveTag(tag);
                UpdateTree();
            }
        }

        private void UpdateTree()
        {
            Tree.GetRoot()?.Free();
            TreeItem root = Tree.CreateItem();
            foreach (Tag tag in PluginServer.Instance.TagList)
            {
                TreeItem treeItem = Tree.CreateItem(root);
                treeItem.SetText(0, tag.Name);
                treeItem.SetIcon(0, Resources.ICON_TAG);
                if (
                    EditorServer.Instance.SelectedTreeEntity is TreeEntity treeEntity
                    && treeEntity.GetComponent<TagCollection>() is TagCollection tagCollection
                )
                {
                    treeItem.AddButton(
                        0,
                        tagCollection.HasTag(tag) ? Resources.ICON_REMOVE : Resources.ICON_ADD,
                        BUTTON_ADD
                    );
                }
                treeItem.AddButton(
                    0,
                    tag.Visible ? Resources.ICON_VISIBLE_ON : Resources.ICON_VISIBLE_OFF,
                    BUTTON_VISIBILITY
                );
                treeItem.SetMetadata(0, tag);
            }
            Tree.Update();
        }

        private void OnTreeButtonPressed(TreeItem item, int column, int id)
        {
            if (item?.GetMetadata(0) is Tag tag)
            {
                switch (id)
                {
                    case BUTTON_VISIBILITY:
                        tag.Visible = !tag.Visible;
                        UpdateTree();
                        break;
                    case BUTTON_ADD:
                        if (
                            EditorServer.Instance.SelectedTreeEntity is TreeEntity treeEntity
                            && treeEntity.GetComponent<TagCollection>()
                                is TagCollection tagCollection
                        )
                        {
                            tagCollection.ToggleTag(tag);
                            treeEntity.DeferredGenerateProperties();
                            UpdateTree();
                        }
                        break;
                }
            }
        }

        private void OnTreeExtendedItemDropped(Godot.Object heldMetadata, Godot.Object landingMetadata, int dropSection)
        {
            PluginServer.Instance.ReorderTagList((Tag)heldMetadata, (Tag)landingMetadata, dropSection);
            UpdateTree();
        }

        public void OnTreeExtendedItemSelected()
        {
            if (Tree.GetSelected()?.GetMetadata(0) is Tag tag)
            {
                EditorServer.Instance.ClearProperties();
                tag.GenerateProperties();
            }
        }

        public void OnTagNameChanged(string _newName)
        {
            UpdateTree();
            Tree.Update();
        }

        public void OnEditorInsFocusedFolderChanged(TreeFolder newFocusedFolder)
        {
            UpdateTree();
        }
    }
}
