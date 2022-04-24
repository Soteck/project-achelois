using Map.Maps.DevelopMap;
using Network.Shared;
using Player;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class ObjectiveDropZone : NetworkBehaviour {

    private void OnTriggerEnter(Collider other) {
        PlayableSoldier holder = other.gameObject.GetComponent<PlayableSoldier>();
        if (holder != null && holder.IsOwner) {
            if (holder.HasObjective()) {
                DevelopMapController.Instance.PlayerEnteredToDropZone(holder, this);
            }
        }
    }
}