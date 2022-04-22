using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Network.Shared;
using Player;
using UnityEngine;

public class WeaponPickup : MonoBehaviour {
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