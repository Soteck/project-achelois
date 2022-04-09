using System.IO;
using Core;
using SharpConfig;

namespace Config {
    public class ConfigHolder : Singleton<ConfigHolder> {
        private Configuration _cfg = new Configuration();
        private const string DefaultPlayerName = "Achelois player";
        private const float DefaultMouseSensitivity = 15f;
        private const bool DefaultAutoSave = true;
        private const bool DefaultAutoBalance = true;

        private void Start() {
            if (!File.Exists("config.cfg")) {
                Logger.Info("Setting up a default config since no file was found!");
                SetupCleanCfg();
                SaveConfig();
            }

            // Load the configuration.
            _cfg = Configuration.LoadFromFile("config.cfg");
        }


        private void SaveConfig()
        {
            Logger.Info("Saving Client config...");

            // Save the configuration.
            _cfg.SaveToFile ("config.cfg");
        }

        private void SetupCleanCfg()
        {
            _cfg["Player"]["Name"].StringValue = DefaultPlayerName;
            _cfg["Input"]["MouseSensitivity"].FloatValue = DefaultMouseSensitivity;
            _cfg["Configuration"]["AutoSave"].BoolValue = DefaultAutoSave;
            _cfg["Server"]["AutoBalance"].BoolValue = DefaultAutoBalance;
        }

        private void ShouldSave() {
            if (_cfg["Core"]["AutoSave"].BoolValue) {
                SaveConfig();
            }
        }
        
        public static void WriteConfig() {
            Instance.SaveConfig();
        }

        public static bool autoSave {
            get => Instance._cfg["Configuration"]["AutoSave"].BoolValue;

            set {
                Instance._cfg["Configuration"]["AutoSave"].BoolValue = value;
                Instance.ShouldSave();
            }
        }

        public static string playerName {
            get => Instance._cfg["Player"]["Name"].StringValue;

            set {
                Instance._cfg["Player"]["Name"].StringValue = value;
                Instance.ShouldSave();
            }
        }

        public static float mouseSensitivity {
            get => Instance._cfg["Input"]["MouseSensitivity"].FloatValue;

            set {
                Instance._cfg["Input"]["MouseSensitivity"].FloatValue = value;
                Instance.ShouldSave();
            }
        }

        public static bool autoBalance {
            get => Instance._cfg["Server"]["AutoBalance"].BoolValue;

            set {
                Instance._cfg["Server"]["AutoBalance"].BoolValue = value;
                Instance.ShouldSave();
            }
        }
    }
}