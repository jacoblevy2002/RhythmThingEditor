using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;

namespace RhythmThingEditor.RhythmSystem
{
    public struct EditorConfig
    {
        public string GameInstall;
    }

    public static class EditorSettings
    {
        private const string CONFIG_NAME = "Config.json";
        private static EditorConfig? _editorConfig = null;
        public static EditorConfig MainConfig
        {
            get
            {

                return _editorConfig ?? throw new Exception("Null! This should NEVER happen!");
            }
        }
        public static bool Load()
        {
            if (_editorConfig == null)
            {
                if (File.Exists($"./{CONFIG_NAME}"))
                {
                    string configText = File.ReadAllText($"./{CONFIG_NAME}");
                    _editorConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<EditorConfig>(configText);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
        public static void Init(EditorConfig editorConfig)
        {
            _editorConfig = editorConfig;
            string config = Newtonsoft.Json.JsonConvert.SerializeObject(_editorConfig);
            File.WriteAllText($"./{CONFIG_NAME}", config);
            Window window = new MainWindow();
            window.Show();
            
        }
    }
}
