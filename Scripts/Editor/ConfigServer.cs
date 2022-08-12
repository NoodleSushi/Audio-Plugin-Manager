using Godot;
using GDDictionary = Godot.Collections.Dictionary;
using GDArray = Godot.Collections.Array;

namespace PluginManager.Editor
{
    public class ConfigServer : Node
    {
        public const string SECTION = "main";
        public const string SAVE_PATH = "user://settings.cfg";
        public const string EXPORTERS_PATH = "user://exporters.json";
        private static ConfigServer _instance;
        public static ConfigServer Instance => _instance;
        private readonly ConfigFile configFile = new();
        private string lastSave = "";
        public GDArray Exporters = new();


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
            File exportersFile = new();
            if (exportersFile.FileExists(EXPORTERS_PATH))
            {
                exportersFile.Open(EXPORTERS_PATH, File.ModeFlags.Read);
                var result = JSON.Parse(exportersFile.GetAsText());
                exportersFile.Close();
                if (result.Error == Error.Ok && result.Result is GDArray arrayResult)
                    Exporters = arrayResult;
            }
        }

        private void Save()
        {
            File exportersFile = new();
            exportersFile.Open(EXPORTERS_PATH, File.ModeFlags.Write);
            exportersFile.StoreString(JSON.Print(Exporters));
            exportersFile.Close();
        }
    }
}
