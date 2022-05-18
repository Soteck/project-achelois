using Unity.Netcode;

namespace Core.Commands {
    public class ServerCommand : ICommand {
        public void Execute(string[] args) {
            if (args == null || args.Length < 1) {
                return;
            }

            if (args[0].Equals("start")) {
                if (NetworkManager.Singleton.StartServer()) {
                    Logger.Info("Server started...");
                }
                else {
                    Logger.Info("Unable to start server...");
                }
            }else if (args[0].Equals("host")) {
                
                // this allows the UnityMultiplayer and UnityMultiplayerRelay scene to work with and without
                // relay features - if the Unity transport is found and is relay protocol then we redirect all the 
                // traffic through the relay, else it just uses a LAN type (UNET) communication.
                // if (RelayManager.Instance.IsRelayEnabled) {
                //     await RelayManager.Instance.SetupRelay();
                // }

                if (NetworkManager.Singleton.StartHost()) {
                    Logger.Info("Host started...");
                }
                else {
                    Logger.Info("Unable to start host...");
                }
            }
            
        }

        public string Command() {
            return "server";
        }
    }
}