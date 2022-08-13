using System.Collections.Generic;
using CharacterController;
using Config;
using Core;
using Enums;
using Items;
using Map;
using Network.Shared;
using Unity.Netcode;
using Unity.Netcode.Editor;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;
using World;
using NetworkPlayer = Network.NetworkPlayer;

namespace Player {
    public class PlayableSoldier : NetworkBehaviour, IDamageableEntity {
        public float knockDownHealth = 75;
        public float timeToDisappearAfterDeath = 15f; //Seconds
        public int selectedWeapon = 0;
        public Transform activeWeapon;
        public Camera playerCamera;
        public Animator animator;

        private readonly List<EquipableItemLogic> _storedItems = new List<EquipableItemLogic>();

        [SerializeField]
        private NetworkList<EquipableItemNetworkData> _networkItems;

        [SerializeField]
        private NetworkVariable<int> networkActiveItem;

        [SerializeField]
        public NetworkVariable<float> networkHealth = new NetworkVariable<float>();

        [SerializeField]
        public NetworkVariable<float> networkEnergy = new NetworkVariable<float>();

        [SerializeField]
        public NetworkVariable<bool> networkInMenu = new NetworkVariable<bool>();

        [SerializeField]
        public NetworkVariable<NetworkString> networkObjective = new NetworkVariable<NetworkString>();

        [SerializeField]
        public NetworkVariable<bool> networkTexting = new NetworkVariable<bool>();


        private EquipableItemLogic _changeWeapon = null;
        private int _localActiveItem = -1;

        public NetPlayerController playerController;
        public NetworkObject networkObject;
        private NetworkPlayer _networkPlayer = null;
        private PlayerInputActions _inputActions;
        private float _knockDownHealth = -75;
        
        //Server variables
        private float _timeToDespawnObject = 0;
        private bool _deathNotified = false;


        public void Awake() {
            _inputActions = new PlayerInputActions();
            _networkItems = new NetworkList<EquipableItemNetworkData>();
            networkActiveItem = new NetworkVariable<int>();

            _inputActions.Player.Disable();
            _inputActions.Player.Scroll.performed += Scroll;

            _knockDownHealth = knockDownHealth * -1;

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
                    ServerCalculations();
                }

                if (IsClient) {
                    ClientMovement();
                    ClientVisuals();
                }
            }
        }

        private void ServerCalculations() {
            if (_timeToDespawnObject > 0 && Time.time > _timeToDespawnObject) {
                networkObject.Despawn();
            }

            //TODO: Any idea to make this less ugly?
            if (networkEnergy.Value <= ConfigHolder.maxEnergy) {
                networkEnergy.Value += Time.deltaTime * MapMaster.Instance.NetworkEnergyLoadRatio();
                if (networkEnergy.Value > ConfigHolder.maxEnergy) {
                    networkEnergy.Value = ConfigHolder.maxEnergy;
                }
            }
        }

        private void ClientVisuals() {

        }

        private void ClientInput() {
            if (_changeWeapon != null) {
                UpdateActualEquippedWeaponServerRpc(_storedItems.IndexOf(_changeWeapon));
                _changeWeapon = null;
            }

            if (_inputActions.Player.SelfKill.WasPerformedThisFrame()) {
                ClientGiveUpOrSelfKill();
            }

            if (_inputActions.Player.Jump.WasPerformedThisFrame() && IsKnockedDown()) {
                ClientGiveUpOrSelfKill();
            }
        }

        private void ClientMovement() {
            if (_networkItems.Count != _storedItems.Count) {
                foreach (EquipableItemNetworkData netItem in _networkItems) {
                    bool exists = false;
                    foreach (EquipableItemLogic storedItem in _storedItems) {
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
                networkActiveItem.Value != _localActiveItem) {
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
                    } else {
                        ++selectedWeapon;
                    }
                }

                if (scrollValue < 0f) {
                    if (selectedWeapon <= 0) {
                        selectedWeapon = _storedItems.Count - 1;
                    } else {
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


        public void InitClassRole() {
            networkHealth.Value = 100f;
            networkEnergy.Value = 20f;
            networkObjective.Value = Constants.OBJECTIVE_NONE;
            networkInMenu.Value = false;
            networkTexting.Value = false;
        }

        public void PickUp(EquipableItemNetworkData netItem) {
            if (IsOwner) {
                PickupItemServerRpc(netItem, NetworkManager.Singleton.LocalClientId);
            }
        }

        public void HealthPickUp() {
            if (IsOwner) {
                HealthPickupItemServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }

        public void AmmoPickUp() {
            if (IsOwner) {
                AmmoPickupItemServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }

        private EquipableItemLogic InitEquipment(EquipableItemLogic item) {
            item.playerCamera = playerCamera;
            item.animator = animator;
            var itemTransform = item.transform;
            itemTransform.localPosition = Vector3.zero;
            itemTransform.localRotation = Quaternion.identity;
            item.gameObject.SetActive(false);
            item.soldierOwner = this;

            return item;
        }

        private void Equip(EquipableItemLogic item) {
            UnEquip();
            if (item != null) {
                item.gameObject.SetActive(true);
                item.visual.gameObject.SetActive(true);
                _localActiveItem = _storedItems.IndexOf(item);
                animator.Play("equip_" + item.item_id);
                if (IsOwner) {
                    item.EnableInputActions();
                }
            }
        }

        private void UnEquip() {
            if (_localActiveItem > -1 && _localActiveItem < _storedItems.Count) {
                EquipableItemLogic item = _storedItems[_localActiveItem];
                if (item != null) {
                    item.gameObject.SetActive(false);
                    item.visual.gameObject.SetActive(false);
                }
            }
        }

        public EquipableItemLogic ActiveItem() {
            if (_localActiveItem != -1) {
                return _storedItems[_localActiveItem];
            }

            return null;
        }

        public void ClientGiveUpOrSelfKill() {
            GiveUpOrSelfKillServerRpc();
        }

        public void ServerTakeDamage(float amount) {
            DamageReceivedClientRpc();
            networkHealth.Value -= amount;
            if (IsKnockedDown()) {
                ServerNotifyKnockedDown();
            }else if (IsDead()) {
                ServerNotifyDeath();
            }

        }

        public bool ServerCanTakeDamage() {
            return !IsDead();
        }

        public GameTeam ServerGetTeam() {
            return networkPlayer.GetNetworkTeam();
        }

        private void ServerNotifyDeath() {
            if (!_deathNotified) {
                networkPlayer.ServerNotifyStateChange(PlayerState.PlayingDead);
                _timeToDespawnObject = Time.time + timeToDisappearAfterDeath;
                _deathNotified = true;
            }
        }

        private void ServerNotifyKnockedDown() {
            networkPlayer.ServerNotifyStateChange(PlayerState.PlayingKnockedDown);
        }

        private void ServerNotifyAlive() {
            //TODO: For revive
            
        }

        // ServerRpc only executed on server side
        [ServerRpc]
        public void GiveUpOrSelfKillServerRpc() {
            networkHealth.Value = _knockDownHealth - 1;
            ServerNotifyDeath();
        }

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
            EquipableItemLogic item = Instantiate(ItemPrefabFactory.PrefabById(itemMeta.itemID.ToString()));
            NetworkObject no = item.GetComponent<NetworkObject>();
            no.SpawnWithOwnership(playerId);
            no.TrySetParent(transform, false);
            //Add instance to the server list
            _storedItems.Add(InitEquipment(item));
            PickupItemClientRpc(no.NetworkObjectId, itemMeta.itemMeta);
        }
        
        [ServerRpc]
        private void HealthPickupItemServerRpc(ulong playerId) {
            NetPlayerController controller = NetworkUtil.FindNetPlayerControllerByOwnerId(playerId);
            controller.soldier.networkHealth.Value += 20;
        }
        
        [ServerRpc]
        private void AmmoPickupItemServerRpc(ulong playerId) {
            NetPlayerController controller = NetworkUtil.FindNetPlayerControllerByOwnerId(playerId);
            foreach (EquipableItemLogic equipableItemLogic in controller.soldier._storedItems) {
                equipableItemLogic.ServerAmmoPickup();
            }
        }

        // ClientRpc are executed on all client instances
        [ClientRpc]
        private void DamageReceivedClientRpc() {
            if (IsOwner) {
                Debug.Log("Damage received!!!");
            }
        }


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
            } else {
                visualItem.gameObject.SetActive(false);
            }

            animator.Rebind();
            animator.Play("equip_" + _storedItems[networkActiveItem.Value].item_id);
        }

        public void Enable() {
            gameObject.SetActive(true);
            _inputActions.Player.Enable();
        }

        public void Disable() {
            gameObject.SetActive(false);
            _inputActions.Player.Disable();
        }

        public bool IsTexting() {
            return networkTexting.Value;
        }

        public bool HasObjective() {
            return networkObjective.Value != Constants.OBJECTIVE_NONE;
        }

        public bool InMenu() {
            return networkInMenu.Value;
        }

        public bool IsKnockedDown() {
            float health = networkHealth.Value;
            return health <= 0 && health > _knockDownHealth;
        }

        public bool IsAlive() {
            return networkHealth.Value > 0;
        }

        public bool IsDead() {
            return networkHealth.Value <= _knockDownHealth;
        }
        
        public NetworkPlayer networkPlayer {
            get {
                if (_networkPlayer == null) {
                    _networkPlayer = NetworkUtil.FindNetworkPlayerByOwnerId(OwnerClientId);
                }

                return _networkPlayer;
            }

            set => _networkPlayer = value;
        }
    }
}