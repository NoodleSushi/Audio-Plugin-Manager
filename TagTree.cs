using Godot;
using System;
using PluginManager.PluginTree;


namespace PluginManager
{
    public class TagTree : VBoxContainer
    {
        [Export]
        readonly private NodePath TagButtonPath;
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
            GetNode<TextureButton>(TagButtonPath).Connect("pressed", this, nameof(OnTagButtonPressed));
            GetNode<TextureButton>(DeleteButtonPath).Connect("pressed", this, nameof(OnDeleteButtonPressed));
        }

        public void OnTagButtonPressed()
        {
            Tag newTag = Server.Instance.CreateTag();
            newTag.Name = "Tag" + Server.Instance.TagList.Count;
            UpdateTree();
        }

        public void OnDeleteButtonPressed()
        {
            if (Tree.GetSelected()?.GetMetadata(0) is Tag tag)
            {
                Server.Instance.RemoveTag(tag);
                UpdateTree();
            }
        }

        private void UpdateTree()
        {
            Tree.GetRoot()?.Free();
            TreeItem root = Tree.CreateItem();
            foreach (Tag tag in Server.Instance.TagList)
            {
                TreeItem treeItem = Tree.CreateItem(root);
                treeItem.SetText(0, tag.Name);
                treeItem.SetIcon(0, Resources.ICON_TAG);
                treeItem.SetMetadata(0, tag);
            }
            Tree.Update();
        }

        private void OnTreeExtendedItemDropped(TreeItem heldItem, TreeItem landingItem, int dropSection)
        {
            Server.Instance.ReorderTagList(
                (Tag)heldItem.GetMetadata(0),
                (Tag)landingItem.GetMetadata(0),
                dropSection
            );
            UpdateTree();
        }
    }
}

