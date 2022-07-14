using Godot;

namespace PluginManager
{
    public class TreeExtended : Tree
    {
        [Signal]
        public delegate void ItemDropped(TreeItem heldItem, TreeItem landingItem, int dropSection);
        public TreeItem _heldItem;
        [Export(PropertyHint.Flags, "On Item,In Between")]
        public int ActiveDropFlags = (int) DropModeFlagsEnum.Inbetween;
        [Export]
        public bool IsEditable = false;
        public TreeItem SelectedItem;
        public override void _Ready()
        {
            Connect("item_selected", this, nameof(OnItemSelected));
            DropModeFlags = (int)DropModeFlagsEnum.Disabled;
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
                    else if (!mb.Pressed && _heldItem != null)
                    {
                        if (pointedItem != _heldItem)
                        {
                            int dropSection = GetDropSectionAtPosition(mb.Position);
                            try
                            {
                                EmitSignal(nameof(ItemDropped), _heldItem, pointedItem, dropSection);
                            }
                            catch (System.Exception)
                            {
                                GD.Print("FUCK YOU");
                            }
                        }
                    }
                }
                if (!mb.Pressed)
                {
                    _heldItem = null;
                    DropModeFlags = (int)DropModeFlagsEnum.Disabled;
                }
            }
        }

        public void OnItemSelected()
        {
            if (IsEditable)
                SelectedItem = GetSelected();
        }
    }
}

