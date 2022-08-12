using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace PluginManager.Editor.Containers
{
    public class TreeExtended : Tree
    {
        [Signal]
        public delegate void ItemDropped(Godot.Object heldMetadata, Godot.Object landingMetadata, int dropSection);
        [Signal]
        public delegate void ItemsDropped(IEnumerable<Godot.Object> heldMetadatas, Godot.Object landingMetadata, int dropSection);

        [Export(PropertyHint.Flags, "On Item,In Between")]
        public int ActiveDropFlags = (int)DropModeFlagsEnum.Inbetween;

        private TreeItem _heldItem;
        private TreeItem _selectedItem;
        private TreeItem _highestSelectedItem;
        private List<TreeItem> GroupedSelectedItems
        {
            get
            {
                List<TreeItem> selected = new();
                for (TreeItem treeItem = _highestSelectedItem; treeItem != null; treeItem = GetNextSelected(treeItem))
                {
                    bool shallAdd = !TraverseTest(treeItem.GetParent(), trav => selected.Contains(trav));
                    if (shallAdd)
                        selected.Add(treeItem);
                }
                return selected;
            }
        }
        public List<Godot.Object> GroupedSelectedMetadatas
        {
            get => GroupedSelectedItems.Select(treeItem => treeItem.GetMetadata(0) as Godot.Object).ToList();
        }

        public override void _Ready()
        {
            Connect("item_selected", this, nameof(OnItemSelected));
            Connect("multi_selected", this, nameof(OnMultiSelected));
            DropModeFlags = (int)DropModeFlagsEnum.Disabled;
        }

        public static bool TraverseTest(TreeItem treeItem, Func<TreeItem, bool> f)
        {
            for (TreeItem trav = treeItem; trav != null; trav = trav.GetParent())
            {
                if (f(trav))
                    return true;
            }
            return false;
        }

        public override void _GuiInput(InputEvent @event)
        {
            base._GuiInput(@event);
            if (@event is InputEventMouseButton mb && mb.ButtonIndex == (int)ButtonList.Left)
            {
                TreeItem pointedItem = GetItemAtPosition(mb.Position);
                if (pointedItem != null)
                {
                    if (mb.Pressed && pointedItem != GetRoot())
                    {
                        _heldItem = pointedItem;
                        DropModeFlags = ActiveDropFlags;
                    }
                    else if (!mb.Pressed && _heldItem != null && _heldItem != pointedItem)
                    {
                        ExecuteDrop(pointedItem, GetDropSectionAtPosition(mb.Position));
                    }
                }
                if (!mb.Pressed)
                {
                    _heldItem = null;
                    DropModeFlags = (int)DropModeFlagsEnum.Disabled;
                }
            }
        }

        private void ExecuteDrop(TreeItem pointedItem, int dropSection)
        {
            switch (SelectMode)
            {
                case Tree.SelectModeEnum.Single:
                    if (!TraverseTest(pointedItem, trav => trav == _heldItem))
                    {
                        EmitSignal(nameof(ItemDropped),
                                _heldItem.GetMetadata(0) as Godot.Object,
                                pointedItem.GetMetadata(0) as Godot.Object,
                                dropSection
                        );
                    }
                    break;
                case Tree.SelectModeEnum.Multi:
                    if (_highestSelectedItem is null)
                        break;
                    TreeItem pointedParent = (dropSection == 0) ? pointedItem : pointedItem.GetParent();
                    bool isSafe = true;
                    List<TreeItem> selected = GroupedSelectedItems;
                    foreach (TreeItem treeItem in selected)
                    {
                        if (treeItem == GetRoot() || treeItem == pointedItem || TraverseTest(pointedItem, trav => trav == treeItem))
                        {
                            isSafe = false;
                            break;
                        }
                    }
                    if (isSafe)
                        EmitSignal(nameof(ItemsDropped),
                                selected.Select(treeItem => treeItem.GetMetadata(0) as Godot.Object).ToList(),
                                pointedItem.GetMetadata(0) as Godot.Object,
                                dropSection
                        );
                    break;
            }
        }

        private void OnItemSelected()
        {
            _selectedItem = GetSelected();
        }

        private void OnMultiSelected(TreeItem item, int column, bool selected)
        {
            if (selected)
            {
                _selectedItem = item;
                EmitSignal("item_selected");
            }
            if (selected && (_highestSelectedItem is null || GetItemAreaRect(item).Position.y < GetItemAreaRect(_highestSelectedItem).Position.y))
                _highestSelectedItem = item;
            else if (!selected && item == _highestSelectedItem)
                _highestSelectedItem = null;
        }
    }
}
