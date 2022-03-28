using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public EquipableItem itemPrefab;

    public void Awake() {
        EquipmentPrefabFactory.Register(itemPrefab);
    }

    private void OnTriggerEnter(Collider other)
    {
        EquipmentHolder holder = other.gameObject.GetComponent<EquipmentHolder>();
        if (holder)
        {
            holder.PickUp(itemPrefab);
        }
    }
}
