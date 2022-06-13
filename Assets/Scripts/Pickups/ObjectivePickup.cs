using Map.Maps.DevelopMap;
using Network.Shared;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace Pickups {
    [RequireComponent(typeof(NetworkObject))]
    public class ObjectivePickup : NetworkBehaviour {
        public NetworkVariable<NetworkString> objectiveCode = new NetworkVariable<NetworkString>();

        private void OnTriggerEnter(Collider other) {
            if (IsServer) {
                PlayableSoldier holder = other.gameObject.GetComponent<PlayableSoldier>();
                if (holder != null && !holder.HasObjective()) {
                    PlayerPickedUpObjectiveServerRpc(holder.NetworkObjectId, NetworkObjectId);
                }
            }
        }


        [ServerRpc]
        private void PlayerPickedUpObjectiveServerRpc(ulong playerId, ulong objectiveId) {
            DevelopMapController.Instance.ServerOnPlayerPickedUpObjective(playerId, objectiveId);
        }
    }
}