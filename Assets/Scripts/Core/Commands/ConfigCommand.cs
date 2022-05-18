using Config;
using UnityEngine;

namespace Core.Commands {
    public class ConfigCommand : ICommand{
        public void Execute(string[] args) {
            if (args == null || args.Length < 1) {
                return;
            }

            if (args[0] == "write") {
                ConfigHolder.WriteConfig();
                Logger.Info("Configuration saved");
                return;
            }
            
            if (args.Length < 2) {
                return;
            }

            if (DoSetConfig(args)) {
                Logger.Info("Config applied");
            } else {
                Logger.Warning("Config not applied");
            }
            
        }

        private bool DoSetConfig(string[] args) {
            bool write = args.Length == 3;
            switch (args[0]) {
                case "input": {
                    switch (args[1]) {
                        case "sensitivity": {
                            if (write) {
                                ConfigHolder.mouseSensitivity = float.Parse(args[2]);
                            }
                               
                            Logger.Info("Mouse sensitivity set to: " + ConfigHolder.mouseSensitivity);
                            
                            return true;
                        }
                        case "invertMouse": {
                            if (write) {
                                ConfigHolder.invertMouse = bool.Parse(args[2]);
                            }
                            
                            Logger.Info("Invert mouse set to: " + ConfigHolder.invertMouse);
                            
                            return true;
                        }
                    }

                    break;
                }
            }
            
            return false;
        }

        public string Command() {
            return "config";
        }
    }
}