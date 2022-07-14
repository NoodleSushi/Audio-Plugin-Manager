using Godot;

namespace PluginManager.Editor
{
    public class WindowContainer : Godot.Control
    {
        private static WindowContainer _instance;
        public static WindowContainer Instance => _instance;

        public override void _Ready()
        {
            _instance = this;
        }
    }
}