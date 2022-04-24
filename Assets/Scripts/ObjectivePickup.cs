using Map.Maps.DevelopMap;
using Network.Shared;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(NetworkObject))]
public class ObjectivePickup : NetworkBehaviour {
    
    public NetworkVariable<NetworkString> objectiveCode = new NetworkVariable<NetworkString>();

    private void OnTriggerEnter(Collider other) {
        PlayableSoldier holder = other.gameObject.GetComponent<PlayableSoldier>();
        if (holder != null && holder.IsOwner) {
            DevelopMapController.Instance.PlayerPickedUpObjective(holder, this);
        }
    }
}