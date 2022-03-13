using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public EquipableItem itemPrefab;

    private void OnTriggerEnter(Collider other)
    {
        EquipmentHolder holder = other.gameObject.GetComponent<EquipmentHolder>();
        if (holder)
        {
            holder.PickUp(itemPrefab);
        }
    }
}
