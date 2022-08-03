using Godot;

namespace PluginManager.Editor
{
    public static class Resources
    {
        public static readonly Texture ICON_FOLDER = GD.Load<Texture>("res://Icons/Folder.svg");
        public static readonly Texture ICON_FOLDER_CLOSE = GD.Load<Texture>("res://Icons/TreeFolderClose.png");
        public static readonly Texture ICON_FOLDER_OPEN = GD.Load<Texture>("res://Icons/TreeFolderOpen.png");
        public static readonly Texture ICON_NODE = GD.Load<Texture>("res://Icons/Node.svg");
        public static readonly Texture ICON_LIVE = GD.Load<Texture>("res://Icons/Live.png");
        public static readonly Texture ICON_FL = GD.Load<Texture>("res://Icons/FL.png");
        public static readonly Texture ICON_BOX_OPEN = GD.Load<Texture>("res://Icons/BoxOpen.svg");
        public static readonly Texture ICON_BOX_CLOSE = GD.Load<Texture>("res://Icons/BoxClose.svg");
        public static readonly Texture ICON_REMOVE = GD.Load<Texture>("res://Icons/Remove.svg");
        public static readonly Texture ICON_ADD = GD.Load<Texture>("res://Icons/Add.svg");
        public static readonly Texture ICON_SEPARATOR = GD.Load<Texture>("res://Icons/Separator.png");
        public static readonly Texture ICON_TAG = GD.Load<Texture>("res://Icons/Groups.svg");
        public static readonly Texture ICON_VISIBLE_ON = GD.Load<Texture>("res://Icons/VisibleOn.svg");
        public static readonly Texture ICON_VISIBLE_OFF = GD.Load<Texture>("res://Icons/VisibleOff.svg");
    }
}
