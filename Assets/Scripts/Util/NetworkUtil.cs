using CharacterController;
using Controller;
using Unity.Netcode;
using UnityEngine;
using NetworkPlayer = Network.NetworkPlayer;

namespace Util {
    public static class NetworkUtil {
        public static NetworkPlayer FindNetworkPlayerByOwnerId(ulong id) {
            NetworkPlayer[] networkPlayers = Object.FindObjectsOfType<NetworkPlayer>();
            if (networkPlayers != null && networkPlayers.Length > 0) {
                foreach (NetworkPlayer networkPlayer in networkPlayers) {
                    if (networkPlayer.OwnerClientId == id) {
                        return networkPlayer;
                    }
                }
            }
            
            return null;
        }
        
        public static NetFirstPersonController FindNetFirstPersonControllerByOwnerId(ulong ownerId) {
            NetFirstPersonController[] allControllers = Object.FindObjectsOfType<NetFirstPersonController>();
            foreach (NetFirstPersonController controller in allControllers) {
                if (controller.OwnerClientId == ownerId) {
                    return controller;
                }
            }

            return null;
        }
        
        public static NetPlayerController FindNetPlayerControllerByOwnerId(ulong ownerId) {
            NetPlayerController[] allControllers = Object.FindObjectsOfType<NetPlayerController>();
            foreach (NetPlayerController controller in allControllers) {
                if (controller.OwnerClientId == ownerId) {
                    return controller;
                }
            }

            return null;
        }
        
        public static NetSpectatorController FindNetSpectatorControllerByOwnerId(ulong ownerId) {
            NetSpectatorController[] allControllers = Object.FindObjectsOfType<NetSpectatorController>();
            foreach (NetSpectatorController controller in allControllers) {
                if (controller.OwnerClientId == ownerId) {
                    return controller;
                }
            }

            return null;
        }
    }
}