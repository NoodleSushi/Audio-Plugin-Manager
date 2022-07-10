using Godot;
using System;

namespace PluginManager.PluginTree
{
    public class Tag : Godot.Object
    {
        [Signal]
        public delegate void Deleting();
        public string Name;
        public void NotifyDeletion()
        {
            EmitSignal(nameof(Deleting));
        }
    }
}
