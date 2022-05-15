using Unity.Netcode;
using UnityEngine;
using NetworkPlayer = Network.NetworkPlayer;

namespace Util {
    public static class NetworkUtil {
        public static NetworkPlayer FindNetworkPlayerByOwnerId(ulong id) {
            NetworkPlayer ret = null;

            NetworkPlayer[] networkPlayers = Object.FindObjectsOfType<NetworkPlayer>();
            if (networkPlayers != null && networkPlayers.Length > 0) {
                foreach (NetworkPlayer networkPlayer in networkPlayers) {
                    if (networkPlayer.OwnerClientId == id) {
                        ret = networkPlayer;
                        break;
                    }
                }
            }
            
            return ret;
        }
    }
}