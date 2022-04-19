using Config;
using Enums;
using Network.Shared;
using Unity.Netcode;
using UnityEngine;
using Util;

namespace Player {
    
    [RequireComponent(typeof(NetworkObject))]
    public class Gun : EquipableItem {
        public GameObject magazine;

        public float damage = 10f;
        public float range = 100f;
        public float fireRate = 15f;
        public float impactForce = 30f;

        public float reloadTime = 2.5f;

        public int pickupStoredRounds = 150;
        public int pickupRounds = 30;
        public int magazineSize = 30;

        public ParticleSystem muzzleFlash;
        public GameObject impactEffect;
        public AudioClip shootSound;
        public AudioClip reloadSound;
        public AudioClip drySound;

        public TrailRenderer bulletTrailPrefab;


        private float _nextTimeToFire = 1f;

        //private int remainingRounds = 0;
        //private int storedRounds = 0;
        private float _reloadEndTime = 0f;
        //PlayerInputActions inputActions;


        //Variables sync between clients and server
        private readonly NetworkVariable<GunState> _networkGunState = new NetworkVariable<GunState>();
        private readonly NetworkVariable<int> _networkClipRemainingRounds = new NetworkVariable<int>();
        private readonly NetworkVariable<int> _networkStoredRemainingRounds = new NetworkVariable<int>();

        //Private variables for the client
        private GunState _localGunState;
        private AudioSource _shootSource;
        private AudioSource _reloadSource;
        private AudioSource _drySource;
        private bool _reloading = false;

        public new void Awake() {
            base.Awake();
            _shootSource = AudioUtil.AddAudio(gameObject, false, false, 1f, shootSound);
            _reloadSource = AudioUtil.AddAudio(gameObject, false, false, 1f, reloadSound);
            _drySource = AudioUtil.AddAudio(gameObject, false, false, 1f, drySound);
            inputActions.Player.Disable();
            // remainingRounds = pickupRounds;
            // storedRounds = pickupStoredRounds;
        }

        public override void OnGainedOwnership() {
            inputActions.Player.Enable();
        }


        protected override void ClientBeforeInput() {
            if (_reloadEndTime != 0 && _reloading && Time.time < _reloadEndTime) {
                //Reload ended but not yet notified
                _reloading = false;
                _reloadEndTime = 0;
                ReloadEndServerNRpc();
            }

            busy = _reloading && !(_nextTimeToFire != 0f && Time.time >= _nextTimeToFire);
        }


        protected override void ClientInput() {
            if (!busy) {
                if (inputActions.Player.Fire1.ReadValue<float>() > 0f) {
                    _nextTimeToFire = 0f;
                    ShootWeaponServerNRpc(playerCamera.transform.position, Time.time);
                }
                else if (inputActions.Player.Reload.ReadValue<float>() > 0f) {
                    ReloadServerNRpc(Time.time);
                }
            }
        }

        private void ReloadServerNRpc(float time) {
            
        }
        private void ReloadEndServerNRpc() {
            
        }
        private void ShootWeaponServerNRpc(Vector3 transformPosition, float time) {
            
        }
        private void DryClientNRpc() {
            
        }

        protected override void ClientMovement() {
            //empty
        }

        protected override void ServerCalculations() {
            //empty
        }

        protected override void InternalCallInitMetaData(EquipableItemNetworkData meta) {
            InitMetaDataServerRpc(meta);
        }



        protected override void ClientVisuals() {
            if (_networkGunState.Value != _localGunState) {
                _localGunState = _networkGunState.Value;
                //TODO: Play state change
            }
        }
        //
        // [ServerRpc]
        // private void ReloadServerRpc(float timeStart) {
        //     if (_networkClipRemainingRounds.Value < magazineSize) {
        //         if (_networkStoredRemainingRounds.Value > 0) {
        //             ReloadStartClientRpc(timeStart + reloadTime);
        //         }
        //         else {
        //             DryClientRpc();
        //         }
        //     }
        // }
        //
        //
        // [ServerRpc]
        // private void ReloadEndServerRpc() {
        //     int rounds = magazineSize;
        //     if (rounds > _networkClipRemainingRounds.Value) {
        //         rounds = _networkClipRemainingRounds.Value;
        //     }
        //
        //     if (_networkClipRemainingRounds.Value > 0) {
        //         rounds -= _networkClipRemainingRounds.Value;
        //     }
        //
        //     _networkClipRemainingRounds.Value -= rounds;
        //     if (_networkClipRemainingRounds.Value < 1) {
        //         RemainingDryClientRpc();
        //     }
        //
        //     _networkClipRemainingRounds.Value += rounds;
        //     _reloadEndTime = 0f;
        // }
        //
        // [ServerRpc]
        // private void ShootWeaponServerRpc(Vector3 barrelPosition, float startShootTime) {
        //     if (_networkClipRemainingRounds.Value > 0) {
        //         _networkClipRemainingRounds.Value--;
        //
        //         if (Physics.Raycast(barrelPosition, playerCamera.transform.forward, out RaycastHit hit,
        //                 range)) {
        //             //Debug.Log(hit.transform.name);
        //             ShootWeaponHitClientRpc(barrelPosition, startShootTime + (1f / fireRate), hit.point, hit.normal);
        //
        //             Target target = hit.transform.GetComponent<Target>();
        //             if (target != null) {
        //                 target.TakeDamage(damage);
        //             }
        //
        //             if (hit.rigidbody != null) {
        //                 hit.rigidbody.AddForce(-hit.normal * impactForce);
        //             }
        //         }
        //         else {
        //             ShootWeaponClientRpc(barrelPosition, startShootTime + (1f / fireRate));
        //         }
        //
        //         if (_networkClipRemainingRounds.Value < 1) {
        //             DryClientRpc();
        //         }
        //     }
        //     else {
        //         DryClientRpc();
        //     }
        // }
        //
        [ServerRpc]
        private void InitMetaDataServerRpc(EquipableItemNetworkData meta) {
            //TODO: Improve this, ugly AF
             string[] data = meta.itemMeta.ToString().Split(',');
             _networkClipRemainingRounds.Value = int.Parse(data[0]);
             _networkStoredRemainingRounds.Value = int.Parse(data[1]);
            // _networkClipRemainingRounds.Value = 30;
            // _networkStoredRemainingRounds.Value = 150;
        }
        //
        // ClientRpc are executed on all client instances
        //
        // [ClientRpc]
        // private void ShootWeaponHitClientRpc(
        //     Vector3 barrelPosition, float nexShootTime, Vector3 hitPoint, Vector3 hitNormal) {
        //     muzzleFlash.Play();
        //     _shootSource.Play();
        //     _nextTimeToFire = nexShootTime;
        //     GameObject impactGo = Instantiate(impactEffect, hitPoint, Quaternion.LookRotation(hitNormal));
        //     Destroy(impactGo, 2f);
        //
        //     if (bulletTrailPrefab != null) {
        //         TrailRenderer bullet = Instantiate(bulletTrailPrefab, barrelPosition, Quaternion.identity);
        //         bullet.AddPosition(barrelPosition);
        //         bullet.transform.position = hitPoint;
        //     }
        // }
        //
        // [ClientRpc]
        // private void ShootWeaponClientRpc(Vector3 barrelPosition, float nexShootTime) {
        //     muzzleFlash.Play();
        //     _shootSource.Play();
        //     _nextTimeToFire = nexShootTime;
        // }
        //
        //
        // [ClientRpc]
        // private void DryClientRpc() {
        //     _drySource.Play();
        //     if (IsOwner && ConfigHolder.autoReload) {
        //         ReloadServerRpc(Time.time);
        //     }
        // }
        //
        // [ClientRpc]
        // private void RemainingDryClientRpc() {
        //     if (IsOwner) {
        //         _drySource.Play();
        //     }
        // }
        //
        // [ClientRpc]
        // private void ReloadStartClientRpc(float reloadEndTime) {
        //     _reloadEndTime = reloadEndTime;
        //     _reloadSource.Play();
        //     animator.SetTrigger("reload_weapon");
        //     busy = true;
        //     _reloading = true;
        // }

        //Output functions
        public override EquipableItemNetworkData ToNetWorkData() {
            return new EquipableItemNetworkData {
                itemID = (NetworkString) item_id,
                itemMeta = (NetworkString) $"{_networkClipRemainingRounds.Value},{_networkStoredRemainingRounds.Value}"
            };
        }

        public override string GetStatus() {
            return _networkClipRemainingRounds.Value + "/" + _networkStoredRemainingRounds.Value;
        }
    }
}