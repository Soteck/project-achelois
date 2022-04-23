using System.Collections.Generic;
using Controller;
using Core;
using Network.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
    public class PlayableSoldier : NetworkBehaviour {
        public int selectedWeapon = 0;
        public Transform activeWeapon;
        public Camera playerCamera;
        public Animator animator;

        private List<EquipableItemLogic> _storedItems = new List<EquipableItemLogic>();

        [SerializeField] private NetworkList<EquipableItemNetworkData> _networkItems;
        //TODO: Guardar una lista de objetos, con dos strings uno del ID y otro de metadatos
        // Sincronizar estos datos con instancias en local de EquipableItem guardadas en otro array

        [SerializeField] private NetworkVariable<int> networkActiveItem;

        [SerializeField] public NetworkVariable<float> networkHealth = new NetworkVariable<float>();

        private EquipableItemLogic _changeWeapon = null;
        private int localActiveItem = -1;
        public NetPlayerController playerController;

        protected PlayerInputActions inputActions;

        private WeaponSpawner weponSpawner;

        public void Awake() {
            inputActions = new PlayerInputActions();
            _networkItems = new NetworkList<EquipableItemNetworkData>();
            networkActiveItem = new NetworkVariable<int>();

            inputActions.Player.Disable();
            inputActions.Player.Scroll.performed += Scroll;

            weponSpawner = GetComponent<WeaponSpawner>();
            // foreach (EquipableItem item in activeWeapon.GetComponentsInChildren<EquipableItem>()) {
            //     if (!_storedItems.Contains(item)) {
            //         _storedItems.Add(InitEquipment(item, item.ToNetWorkData()));
            //     }
            // }
        }

        void Update() {
            if (IsSpawned) {
                if (IsClient && IsOwner) {
                    //ClientBeforeInput();
                    ClientInput();
                }

                if (IsServer) {
                    //ServerCalculations();
                }

                if (IsClient) {
                    ClientMovement();
                    //ClientVisuals();
                }
            }
        }

        private void ClientInput() {
            if (_changeWeapon != null) {
                UpdateActualEquippedWeaponServerRpc(_storedItems.IndexOf(_changeWeapon));
                _changeWeapon = null;
            }
        }

        private void ClientMovement() {
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
                        PickUp(netItem);
                    }
                }
            }

            if (networkActiveItem.Value > -1 && networkActiveItem.Value < _storedItems.Count &&
                networkActiveItem.Value != localActiveItem) {
                Equip(_storedItems[networkActiveItem.Value]);
            }

            if (_storedItems.Count > networkActiveItem.Value) {
                EquipableItemLogic activeItem = _storedItems[networkActiveItem.Value];
                if (activeItem != null) {
                    Transform activeItemTransform = activeItem.transform;
                    Transform activeWeaponTransform = activeWeapon.transform;
                    activeItemTransform.position = activeWeaponTransform.position;
                    activeItemTransform.rotation = activeWeaponTransform.rotation;
                }
            }
        }

        // Start is called before the first frame update
        public void Start() {
            //TODO: Equip base items
            // if (_storedItems.Count > 0) {
            //     UpdateActualEquippedWeaponServerRpc(0);
            // }

            animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.Normal;
        }

        private void Scroll(InputAction.CallbackContext context) {
            //Debug.Log(context);
            if (
                networkActiveItem != null
                && networkActiveItem.Value > -1
                && networkActiveItem.Value < _storedItems.Count
                //&& networkActiveItem.Value != localActiveItem
                && !_storedItems[networkActiveItem.Value].busy
                && context.phase == InputActionPhase.Performed) {
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


        public void PickUp(EquipableItemNetworkData netItem) {
            if (IsOwner) {
                PickupItemServerRpc(netItem, NetworkManager.Singleton.LocalClientId);
            }
        }

        private EquipableItemLogic InitEquipment(EquipableItemLogic item) {
            item.playerCamera = playerCamera;
            item.animator = animator;
            var itemTransform = item.transform;
            itemTransform.localPosition = Vector3.zero;
            itemTransform.localRotation = Quaternion.identity;
            item.gameObject.SetActive(false);

            return item;
        }

        private void Equip(EquipableItemLogic item) {
            UnEquip();
            if (item != null) {
                item.gameObject.SetActive(true);
                item.visual.gameObject.SetActive(true);
                localActiveItem = _storedItems.IndexOf(item);
                animator.Play("equip_" + item.item_id);
                if (IsOwner) {
                    item.EnableInputActions();
                }
            }
        }

        private void UnEquip() {
            if (localActiveItem > -1 && localActiveItem < _storedItems.Count) {
                EquipableItemLogic item = _storedItems[localActiveItem];
                if (item != null) {
                    item.gameObject.SetActive(false);
                    item.visual.gameObject.SetActive(false);
                }
            }
        }

        public EquipableItemLogic ActiveItem() {
            if (localActiveItem != -1) {
                return _storedItems[localActiveItem];
            }

            return null;
        }

        // ServerRpc only executed on server side

        [ServerRpc]
        private void UpdateActualEquippedWeaponServerRpc(int activeItem) {
            networkActiveItem.Value = activeItem;
        }

        [ServerRpc]
        private void PickupItemServerRpc(EquipableItemNetworkData itemMeta, ulong playerId) {
            if (_networkItems.Contains(itemMeta)) {
                return;
            }

            _networkItems.Add(itemMeta);
            EquipableItemLogic item = Instantiate(
                ItemPrefabFactory.PrefabById(
                    itemMeta.itemID.ToString()
                )
            );
            NetworkObject no = item.GetComponent<NetworkObject>();
            no.SpawnWithOwnership(playerId);
            no.TrySetParent(transform, false);
            //Add instance to the server list
            _storedItems.Add(InitEquipment(item));
            PickupItemClientRpc(no.NetworkObjectId, itemMeta.itemMeta);
        }

        // ClientRpc are executed on all client instances
        [ClientRpc]
        private void PickupItemClientRpc(ulong itemId, NetworkString itemMeta) {
            //TODO: play sound of pickup

            //ownerController.soldier.activeWeapon;
            NetworkObject spawnedItem = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemId];
            EquipableItemLogic item = spawnedItem.GetComponent<EquipableItemLogic>();
            if (IsOwner) {
                item.CallInitMetaData(itemMeta);
            }
            

            //Add instance to the client list
            if (!IsServer) {
                _storedItems.Add(InitEquipment(item));
            }

            EquipableItemVisual visualItem = Instantiate(item.visual, activeWeapon);
            item.AttachVisual(visualItem);

            if (_storedItems.Count == 1) {
                Equip(item);
            }
            else {
                visualItem.gameObject.SetActive(false);
            }

            animator.Rebind();
            animator.Play("equip_" + _storedItems[networkActiveItem.Value].item_id);
        }

        public void Enable() {
            gameObject.SetActive(true);
            inputActions.Player.Enable();
        }

        public void Disable() {
            gameObject.SetActive(false);
            inputActions.Player.Disable();
        }
    }
}