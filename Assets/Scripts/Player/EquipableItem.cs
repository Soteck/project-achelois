using Controller;
using Network.Shared;
using Unity.Netcode;
using UnityEngine;

public abstract class EquipableItem : NetController {
    public Camera playerCamera;
    public string item_id;
    public Animator animator;
    public bool busy = false;
    public abstract EquipableItemNetworkData ToNetWorkData();
    protected abstract void InternalCallInitMetaData(EquipableItemNetworkData meta);

    public abstract string GetStatus();


    public void CallInitMetaData(EquipableItemNetworkData meta) {
        InternalCallInitMetaData(meta);
    }
}