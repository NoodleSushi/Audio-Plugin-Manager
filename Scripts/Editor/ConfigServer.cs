using Godot;

namespace PluginManager.Editor
{
    public class ConfigServer : Node
    {
        public const string SECTION = "main";
        public const string SAVE_PATH = "user://settings.cfg";
        private static ConfigServer _instance;
        public static ConfigServer Instance => _instance;
        private readonly ConfigFile configFile = new();
        private string lastSave = "";


        public override void _EnterTree()
        {
            _instance = this;
            Load();
        }

        public override void _Notification(int what)
        {
            switch (what)
            {
                case NotificationWmQuitRequest:
                    Save();
                    break;
            }
        }

        private void Load()
        {
            Error error = configFile.Load(SAVE_PATH);
            if (error != Error.Ok)
            {
                configFile.SetValue(SECTION, "last_save", OS.GetSystemDir(OS.SystemDir.Desktop));
            }
            else
            {
                lastSave = (configFile.GetValue(SECTION, "last_save", "") as string) ?? OS.GetSystemDir(OS.SystemDir.Desktop);
            }
        }

        private void Save()
        {

        }
    }
}
