using Network.Shared;
using Player;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class ObjectivePickup : NetworkBehaviour {
    public string itemId;
    public string itemMeta;


    private void OnTriggerEnter(Collider other) {
        PlayableSoldier holder = other.gameObject.GetComponent<PlayableSoldier>();
        if (holder) {
            EquipableItemNetworkData data = new EquipableItemNetworkData() {
                itemID = itemId,
                itemMeta = itemMeta
            };
            holder.PickUp(data);
        }
    }
}