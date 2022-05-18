using Unity.Netcode;
using Unity.Netcode.Transports.UNET;

namespace Core.Commands {
    public class ConnectCommand : ICommand {
        public void Execute(string[] args) {
            if (args == null || args.Length < 1) {
                Logger.Warning("Cannot connect, no IP or hostname provided ");
                return;
            }

            Logger.Info("Trying to connect to " + args[0]);
            if (args[0].Equals("localhost")) {
                args[0] = "127.0.0.1";
            }
            
            (NetworkManager.Singleton.NetworkConfig.NetworkTransport as UNetTransport).ConnectAddress = args[0];
            
            if (NetworkManager.Singleton.StartClient()) {
                Logger.Info("Client started...");
            }
            else {
                Logger.Info("Unable to start client...");
            }
        }

        public string Command() {
            return "connect";
        }
    }
}