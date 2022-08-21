using Godot;

namespace PluginManager.PluginTree
{
    public class TreeItemContainer : Resource
    {
        private readonly TreeItem _treeItem;
        public TreeItem TreeItem => _treeItem;
        private readonly TreeEntity _modifier;
        public TreeEntity Modifier => _modifier;

        public TreeItemContainer(TreeItem treeItem, TreeEntity modifier)
        {
            _treeItem = treeItem;
            _modifier = modifier;
            _modifier.ModifyTreeItem(_treeItem);
            _modifier.Connect(nameof(TreeEntity.ContentChanged), this, nameof(OnModifierContentChanged));
            _modifier.Connect(nameof(TreeEntity.SelectEmitted), this, nameof(OnModifierSelectEmitted));
            (_treeItem.GetMetadata(0) as TreeItemContainer)?.Free();
            _treeItem.SetMetadata(0, this);
        }

        void OnModifierContentChanged()
        {
            _modifier.ModifyTreeItem(_treeItem);
        }

        void OnModifierSelectEmitted()
        {
            _treeItem.Select(0);
        }
    }

    public static class TreeItemContainerExtensions
    {
        public static void ContainTreeEntity(this TreeItem treeItem, TreeEntity modifier)
        {
            new TreeItemContainer(treeItem, modifier);
        }

        public static TreeEntity GetTreeEntity(this TreeItem treeItem)
        {
            return (treeItem.GetMetadata(0) as TreeItemContainer)?.Modifier;
        }
    }
}
