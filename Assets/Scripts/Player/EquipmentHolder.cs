﻿using System.Collections.Generic;
using Controller;
using Core;
using Network.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class EquipmentHolder : NetController {
    public int selectedWeapon = 0;
    public Transform activeWeapon;
    public Camera playerCamera;
    public Animator animator;

    private List<EquipableItem> _storedItems = new List<EquipableItem>();

    [SerializeField] private NetworkList<EquipableItemNetworkData> _networkItems;
    //TODO: Guardar una lista de objetos, con dos strings uno del ID y otro de metadatos
    // Sincronizar estos datos con instancias en local de EquipableItem guardadas en otro array

    [SerializeField] 
    private NetworkVariable<int> networkActiveItem;
    

    private EquipableItem _changeWeapon = null;
    private int localActiveItem = 0;

    public new void Awake() {
        base.Awake();
        _networkItems = new NetworkList<EquipableItemNetworkData>();
        networkActiveItem = new NetworkVariable<int>();
        inputActions.Player.Scroll.performed += Scroll;
        foreach (EquipableItem item in activeWeapon.GetComponentsInChildren<EquipableItem>()) {
            if (!_storedItems.Contains(item)) {
                _storedItems.Add(InitEquipment(item));
            }
        }
    }

    protected override void ServerCalculations() {
        //Empty
    }

    protected override void ClientBeforeInput() {
        //Empty
    }

    protected override void ClientInput() {
        if (_changeWeapon != null) {
            UpdateActualEquippedWeaponServerRpc(_storedItems.IndexOf(_changeWeapon));
            _changeWeapon = null;
        }
    }

    protected override void ClientMovement() {
        if (_networkItems.Count != _storedItems.Count) {
            foreach (var netItem in _networkItems) {
                bool exists = false;
                foreach (var storedItem in _storedItems) {
                    if (storedItem.item_id == netItem.itemID) {
                        exists = true;
                        break;
                    }
                }

                if (!exists) {
                    DoNewPickup(netItem);
                }
            }
        }

        if (networkActiveItem.Value != localActiveItem) {
            Equip(_storedItems[networkActiveItem.Value]);
        }
    }

    protected override void ClientVisuals() {
        //throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    public void Start() {
        if (_storedItems.Count > 0) {
            UpdateActualEquippedWeaponServerRpc(0);
        }

        animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.updateMode = AnimatorUpdateMode.Normal;
    }

    private void Scroll(InputAction.CallbackContext context) {
        //Debug.Log(context);
        if (networkActiveItem != null && !_storedItems[networkActiveItem.Value].busy && context.phase == InputActionPhase.Performed) {
            float scrollValue = context.ReadValue<float>();
            int previousSelectedWeapon = selectedWeapon;
            if (scrollValue > 0f) {
                if (selectedWeapon >= _storedItems.Count - 1) {
                    selectedWeapon = 0;
                }
                else {
                    ++selectedWeapon;
                }
            }

            if (scrollValue < 0f) {
                if (selectedWeapon <= 0) {
                    selectedWeapon = _storedItems.Count - 1;
                }
                else {
                    --selectedWeapon;
                }
            }

            /*if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                selectedWeapon = 0;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2)
            {
                selectedWeapon = 1;
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount >= 3)
            {
                selectedWeapon = 2;
            }

            if (Input.GetKeyDown(KeyCode.Alpha4) && transform.childCount >= 4)
            {
                selectedWeapon = 3;
            }*/

            if (previousSelectedWeapon != selectedWeapon) {
                _changeWeapon = _storedItems[selectedWeapon];
            }
        }
    }


    public void PickUp(EquipableItem itemPrefab) {
        if (!IsAlreadyEquipped(itemPrefab)) {
            ServerPickupItemServerRpc(itemPrefab.ToNetWorkData());
        }
    }

    private void DoNewPickup(EquipableItemNetworkData equipableItemNetworkData) {
        //TODO: play sound of pickup


        EquipableItem item = Instantiate(
            EquipmentPrefabFactory.GetPrefabByItemID(
                equipableItemNetworkData.itemID.ToString()
            )
        );
        _storedItems.Add(InitEquipment(item));
        if (_storedItems.Count == 1) {
            Equip(item);
        }

        animator.Rebind();
        animator.Play("equip_" + _storedItems[networkActiveItem.Value].item_id);
    }

    private bool IsAlreadyEquipped(EquipableItem itemPrefab) {
        foreach (EquipableItem item in _storedItems) {
            if (item.item_id == itemPrefab.item_id) {
                return true;
            }
        }

        return false;
    }

    private EquipableItem InitEquipment(EquipableItem item) {
        item.playerCamera = playerCamera;
        item.animator = animator;
        var itemTransform = item.transform;
        itemTransform.parent = activeWeapon;
        itemTransform.localPosition = Vector3.zero;
        itemTransform.localRotation = Quaternion.identity;
        item.gameObject.SetActive(false);

        return item;
    }

    private void Equip(EquipableItem item) {
        UnEquip();
        if (item != null) {
            item.gameObject.SetActive(true);
            localActiveItem = _storedItems.IndexOf(item);
            animator.Play("equip_" + item.item_id);
        }
    }

    private void UnEquip() { 
        _storedItems[localActiveItem]?.gameObject.SetActive(false);
    }

    [ServerRpc]
    private void UpdateActualEquippedWeaponServerRpc(int activeItem) {
        networkActiveItem.Value = activeItem;
    }

    [ServerRpc]
    private void ServerPickupItemServerRpc(EquipableItemNetworkData itemMeta) {
        _networkItems.Add(itemMeta);
    }
}