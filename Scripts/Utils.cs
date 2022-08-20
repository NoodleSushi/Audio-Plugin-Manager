using Godot;

namespace PluginManager
{
    public static class Utils
    {
        public static void MakeFreeable(this Popup popup)
        {
            popup.Connect("popup_hide", popup, "queue_free");
        }
    }
}