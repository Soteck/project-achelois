using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Network.Shared;
using Player;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public EquipableItem itemPrefab;
    public string itemMeta;

    public void Awake() {
        EquipmentPrefabFactory.Register(itemPrefab);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayableSoldier holder = other.gameObject.GetComponent<PlayableSoldier>();
        if (holder) {
            EquipableItemNetworkData data = itemPrefab.ToNetWorkData();
            data.itemMeta = itemMeta;
            holder.PickUp(data);
        }
    }
}
