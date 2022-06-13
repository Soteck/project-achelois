using Map.Maps.DevelopMap;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace Pickups {
    [RequireComponent(typeof(NetworkObject))]
    public class ObjectiveDropZone : NetworkBehaviour {
        public string drop_zone_id;

        private void OnTriggerEnter(Collider other) {
            if (IsServer) {
                PlayableSoldier holder = other.gameObject.GetComponent<PlayableSoldier>();
                if (holder != null && holder.HasObjective()) {
                    PlayerEnteredToDropZoneServerRpc(holder.NetworkObjectId, NetworkObjectId);
                }
            }
        }


        [ServerRpc]
        private void PlayerEnteredToDropZoneServerRpc(ulong playerId, ulong objectiveId) {
            DevelopMapController.Instance.ServerOnPlayerEnteredToDropZone(playerId, objectiveId);
        }
    }
}