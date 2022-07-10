using Godot;
using System;
using PluginManager.PluginTree;


namespace PluginManager
{
    public static class Resources
    {
        public static readonly Texture ICON_FOLDER = GD.Load<Texture>("res://icons/Folder.svg");
        public static readonly Texture ICON_NODE = GD.Load<Texture>("res://icons/Node.svg");
        public static readonly Texture ICON_LIVE = GD.Load<Texture>("res://icons/Live.png");
        public static readonly Texture ICON_FL = GD.Load<Texture>("res://icons/FL.png");
        public static readonly Texture ICON_BOTH = GD.Load<Texture>("res://icons/Both.png");
        public static readonly Texture ICON_BOX_OPEN = GD.Load<Texture>("res://icons/BoxOpen.svg");
        public static readonly Texture ICON_BOX_CLOSE = GD.Load<Texture>("res://icons/BoxClose.svg");
        public static readonly Texture ICON_REMOVE = GD.Load<Texture>("res://icons/Remove.svg");
        public static readonly Texture ICON_ADD = GD.Load<Texture>("res://icons/Add.svg");
        public static readonly Texture ICON_SEPARATOR = GD.Load<Texture>("res://icons/Separator.png");
        public static readonly Texture ICON_TAG = GD.Load<Texture>("res://icons/Groups.svg");
        public static readonly Texture ICON_COPY = GD.Load<Texture>("res://icons/ActionCopy.svg");
        public static readonly Texture ICON_PASTE = GD.Load<Texture>("res://icons/ActionPaste.svg");
        public static readonly Texture ICON_VISIBLE_ON = GD.Load<Texture>("res://icons/VisibleOn.svg");
        public static readonly Texture ICON_VISIBLE_OFF = GD.Load<Texture>("res://icons/VisibleOff.svg");
    }
}

