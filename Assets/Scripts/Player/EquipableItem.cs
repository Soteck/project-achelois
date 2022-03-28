using Network.Shared;
using UnityEngine;

public abstract class EquipableItem : MonoBehaviour {
    public Camera playerCamera;
    public string item_id;
    public Animator animator;
    public bool busy = false;
    public abstract EquipableItemNetworkData ToNetWorkData();
}